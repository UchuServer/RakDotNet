#ifndef __THREAD_POOL_H
#define __THREAD_POOL_H

#include "RakMemoryOverride.h"
#include "DS_Queue.h"
#include "SimpleMutex.h"
#include "Export.h"
#include "RakThread.h"

#ifdef _MSC_VER
#pragma warning( push )
#endif

/// A simple class to create worker threads that processes a queue of functions with data.
/// This class does not allocate or deallocate memory.  It is up to the user to handle memory management.
/// InputType and OutputType are stored directly in a queue.  For large structures, if you plan to delete from the middle of the queue,
/// you might wish to store pointers rather than the structures themselves so the array can shift efficiently.
template <class InputType, class OutputType>
struct RAK_DLL_EXPORT ThreadPool : public RakNet::RakMemoryOverride
{
    ThreadPool();
    ~ThreadPool();

    /// Start the specified number of threads.
    /// \param[in] numThreads The number of threads to start
    /// \param[in] stackSize 0 for default (except on consoles).
    /// \param[in] _perThreadDataFactory User callback to return data stored per thread.  Pass 0 if not needed.
    /// \param[in] _perThreadDataDestructor User callback to destroy data stored per thread, created by _perThreadDataFactory.  Pass 0 if not needed.
    /// \return True on success, false on failure.
    bool StartThreads(int numThreads, int stackSize, void* (*_perThreadDataFactory)()=0, void (*_perThreadDataDestructor)(void*)=0);

    /// Stops all threads
    void StopThreads(void);

    /// Adds a function to a queue with data to pass to that function.  This function will be called from the thread
    /// Memory management is your responsibility!  This class does not allocate or deallocate memory.
    /// The best way to deallocate \a inputData is in userCallback.  If you call EndThreads such that callbacks were not called, you
    /// can iterate through the inputQueue and deallocate all pending input data there
    /// The best way to deallocate output is as it is returned to you from GetOutput.  Similarly, if you end the threads such that
    /// not all output was returned, you can iterate through outputQueue and deallocate it there.
    /// \param[in] workerThreadCallback The function to call from the thread
    /// \param[in] inputData The parameter to pass to \a userCallback
    void AddInput(OutputType (*workerThreadCallback)(InputType, bool *returnOutput, void* perThreadData), InputType inputData);

    /// Returns true if output from GetOutput is waiting.
    /// \return true if output is waiting, false otherwise
    bool HasOutput(void);

    /// Inaccurate but fast version of HasOutput.  If this returns true, you should still check HasOutput for the real value.
    /// \return true if output is probably waiting, false otherwise
    bool HasOutputFast(void);

    /// Returns true if input from GetInput is waiting.
    /// \return true if input is waiting, false otherwise
    bool HasInput(void);

    /// Inaccurate but fast version of HasInput.  If this returns true, you should still check HasInput for the real value.
    /// \return true if input is probably waiting, false otherwise
    bool HasInputFast(void);

    /// Gets the output of a call to \a userCallback
    /// HasOutput must return true before you call this function.  Otherwise it will assert.
    /// \return The output of \a userCallback.  If you have different output signatures, it is up to you to encode the data to indicate this
    OutputType GetOutput(void);

    /// Clears internal buffers
    void Clear(void);

    /// Lock the input buffer before calling the functions InputSize, InputAtIndex, and RemoveInputAtIndex
    /// It is only necessary to lock the input or output while the threads are running
    void LockInput(void);

    /// Unlock the input buffer after you are done with the functions InputSize, GetInputAtIndex, and RemoveInputAtIndex
    void UnlockInput(void);

    /// Length of the input queue
    unsigned InputSize(void);

    /// Get the input at a specified index
    InputType GetInputAtIndex(unsigned index);

    /// Remove input from a specific index.  This does NOT do memory deallocation - it only removes the item from the queue
    void RemoveInputAtIndex(unsigned index);

    /// Lock the output buffer before calling the functions OutputSize, OutputAtIndex, and RemoveOutputAtIndex
    /// It is only necessary to lock the input or output while the threads are running
    void LockOutput(void);
    
    /// Unlock the output buffer after you are done with the functions OutputSize, GetOutputAtIndex, and RemoveOutputAtIndex
    void UnlockOutput(void);

    /// Length of the output queue
    unsigned OutputSize(void);

    /// Get the output at a specified index
    OutputType GetOutputAtIndex(unsigned index);

    /// Remove output from a specific index.  This does NOT do memory deallocation - it only removes the item from the queue
    void RemoveOutputAtIndex(unsigned index);

    /// Removes all items from the input queue
    void ClearInput(void);

    /// Removes all items from the output queue
    void ClearOutput(void);

    /// Are any of the threads working, or is input or output available?
    bool IsWorking(void);

    /// The number of currently active threads.
    int NumThreadsWorking(void);

    /// Have the threads been signaled to be stopped?
    bool WasStopped(void);

protected:
    // It is valid to cancel input before it is processed.  To do so, lock the inputQueue with inputQueueMutex,
    // Scan the list, and remove the item you don't want.
    SimpleMutex inputQueueMutex, outputQueueMutex, workingThreadCountMutex, runThreadsMutex;

    void* (*perThreadDataFactory)();
    void (*perThreadDataDestructor)(void*);

    // inputFunctionQueue & inputQueue are paired arrays so if you delete from one at a particular index you must delete from the other
    // at the same index
    DataStructures::Queue<OutputType (*)(InputType, bool *, void*)> inputFunctionQueue;
    DataStructures::Queue<InputType> inputQueue;

    DataStructures::Queue<OutputType> outputQueue;

    
    template <class ThreadInputType, class ThreadOutputType>
    friend RAK_THREAD_DECLARATION(WorkerThread);

    /*
#ifdef _WIN32
    friend unsigned __stdcall WorkerThread( LPVOID arguments );
#else
    friend void* WorkerThread( void* arguments );
#endif
    */

    /// \internal
    bool runThreads;
    /// \internal
    int numThreadsRunning;
    /// \internal
    int numThreadsWorking;
    /// \internal
    SimpleMutex numThreadsRunningMutex;
#ifdef _WIN32
    /// \internal
    HANDLE quitAndIncomingDataEvents[2];
#endif
};

#include "ThreadPool.h"
#include "RakSleep.h"
#ifdef _WIN32
#else
#include <unistd.h>
#endif

#ifdef _MSC_VER
#pragma warning(disable:4127)
#pragma warning( disable : 4701 )  // potentially uninitialized local variable 'inputData' used
#endif

template <class ThreadInputType, class ThreadOutputType>
RAK_THREAD_DECLARATION(WorkerThread)
/*
#ifdef _WIN32
unsigned __stdcall WorkerThread( LPVOID arguments )
#else
void* WorkerThread( void* arguments )
#endif
*/
{
    bool returnOutput;
    ThreadPool<ThreadInputType, ThreadOutputType> *threadPool = (ThreadPool<ThreadInputType, ThreadOutputType>*) arguments;
    ThreadOutputType (*userCallback)(ThreadInputType, bool *, void*);
    ThreadInputType inputData;
    ThreadOutputType callbackOutput;

    userCallback=0;

    void *perThreadData;
    if (threadPool->perThreadDataFactory)
        perThreadData=threadPool->perThreadDataFactory();
    else
        perThreadData=0;

    // Increase numThreadsRunning
    threadPool->numThreadsRunningMutex.Lock();
    ++threadPool->numThreadsRunning;
    threadPool->numThreadsRunningMutex.Unlock();


    while (1)
    {
#ifdef _WIN32
        if (userCallback==0)
        {
            // Wait for signaled event
            WaitForMultipleObjects(
                2,
                threadPool->quitAndIncomingDataEvents,
                false,
                INFINITE);
        }        
#endif

        threadPool->runThreadsMutex.Lock();
        if (threadPool->runThreads==false)
        {
            threadPool->runThreadsMutex.Unlock();
            break;
        }
        threadPool->runThreadsMutex.Unlock();

        threadPool->workingThreadCountMutex.Lock();
        ++threadPool->numThreadsWorking;
        threadPool->workingThreadCountMutex.Unlock();

        // Read input data
        userCallback=0;
        threadPool->inputQueueMutex.Lock();
        if (threadPool->inputFunctionQueue.Size())
        {
            userCallback=threadPool->inputFunctionQueue.Pop();
            inputData=threadPool->inputQueue.Pop();
        }
        threadPool->inputQueueMutex.Unlock();

        if (userCallback)
        {
            callbackOutput=userCallback(inputData, &returnOutput,perThreadData);
            if (returnOutput)
            {
                threadPool->outputQueueMutex.Lock();
                threadPool->outputQueue.Push(callbackOutput);
                threadPool->outputQueueMutex.Unlock();
            }            
        }

        threadPool->workingThreadCountMutex.Lock();
        --threadPool->numThreadsWorking;
        threadPool->workingThreadCountMutex.Unlock();

#ifndef _WIN32
        // If no input data available, and GCC, then sleep.
        if (userCallback==0)
            RakSleep(1000);
#endif
    }

    // Decrease numThreadsRunning
    threadPool->numThreadsRunningMutex.Lock();
    --threadPool->numThreadsRunning;
    threadPool->numThreadsRunningMutex.Unlock();

    if (threadPool->perThreadDataDestructor)
        threadPool->perThreadDataDestructor(perThreadData);

    return 0;
}
template <class InputType, class OutputType>
ThreadPool<InputType, OutputType>::ThreadPool()
{
    runThreads=false;
    numThreadsRunning=0;
}
template <class InputType, class OutputType>
ThreadPool<InputType, OutputType>::~ThreadPool()
{
    StopThreads();
    Clear();
}

template <class InputType, class OutputType>
bool ThreadPool<InputType, OutputType>::StartThreads(int numThreads, int stackSize, void* (*_perThreadDataFactory)(), void (*_perThreadDataDestructor)(void *))
{
    (void) stackSize;

    runThreadsMutex.Lock();
    if (runThreads==true)
    {
        runThreadsMutex.Unlock();
        return false;
    }
    runThreadsMutex.Unlock();

#ifdef _WIN32
    quitAndIncomingDataEvents[0]=CreateEvent(0, true, false, 0);
    quitAndIncomingDataEvents[1]=CreateEvent(0, false, false, 0);
#endif

    perThreadDataFactory=_perThreadDataFactory;
    perThreadDataDestructor=_perThreadDataDestructor;

    runThreadsMutex.Lock();
    runThreads=true;
    runThreadsMutex.Unlock();

    numThreadsWorking=0;
    unsigned threadId = 0;
    (void) threadId;
    int i;
    for (i=0; i < numThreads; i++)
    {
        int errorCode = RakNet::RakThread::Create(WorkerThread<InputType, OutputType>, this);
        if (errorCode!=0)
        {
            StopThreads();
            return false;
        }
    }
    // Wait for number of threads running to increase to numThreads
    bool done=false;
    while (done==false)
    {
        RakSleep(50);
        numThreadsRunningMutex.Lock();
        if (numThreadsRunning==numThreads)
            done=true;
        numThreadsRunningMutex.Unlock();
    }

    return true;
}
template <class InputType, class OutputType>
void ThreadPool<InputType, OutputType>::StopThreads(void)
{
    runThreadsMutex.Lock();
    if (runThreads==false)
    {
        runThreadsMutex.Unlock();
        return;
    }

    runThreads=false;
    runThreadsMutex.Unlock();

#ifdef _WIN32
    // Quit event
    SetEvent(quitAndIncomingDataEvents[0]);
#endif

    // Wait for number of threads running to decrease to 0
    bool done=false;
    while (done==false)
    {
        RakSleep(50);
        numThreadsRunningMutex.Lock();
        if (numThreadsRunning==0)
            done=true;
        numThreadsRunningMutex.Unlock();
    }

#ifdef _WIN32
    CloseHandle(quitAndIncomingDataEvents[0]);
    CloseHandle(quitAndIncomingDataEvents[1]);
#endif
}
template <class InputType, class OutputType>
void ThreadPool<InputType, OutputType>::AddInput(OutputType (*workerThreadCallback)(InputType, bool *returnOutput, void* perThreadData), InputType inputData)
{
    inputQueueMutex.Lock();
    inputQueue.Push(inputData);
    inputFunctionQueue.Push(workerThreadCallback);
    inputQueueMutex.Unlock();

#ifdef _WIN32
    // Input data event
    SetEvent(quitAndIncomingDataEvents[1]);
#endif
}
template <class InputType, class OutputType>
bool ThreadPool<InputType, OutputType>::HasOutputFast(void)
{
    return outputQueue.IsEmpty()==false;
}
template <class InputType, class OutputType>
bool ThreadPool<InputType, OutputType>::HasOutput(void)
{
    bool res;
    outputQueueMutex.Lock();
    res=outputQueue.IsEmpty()==false;
    outputQueueMutex.Unlock();
    return res;
}
template <class InputType, class OutputType>
bool ThreadPool<InputType, OutputType>::HasInputFast(void)
{
    return inputQueue.IsEmpty()==false;
}
template <class InputType, class OutputType>
bool ThreadPool<InputType, OutputType>::HasInput(void)
{
    bool res;
    inputQueueMutex.Lock();
    res=inputQueue.IsEmpty()==false;
    inputQueueMutex.Unlock();
    return res;
}
template <class InputType, class OutputType>
OutputType ThreadPool<InputType, OutputType>::GetOutput(void)
{
    // Real output check
    OutputType output;
    outputQueueMutex.Lock();
    output=outputQueue.Pop();
    outputQueueMutex.Unlock();
    return output;
}
template <class InputType, class OutputType>
void ThreadPool<InputType, OutputType>::Clear(void)
{
    runThreadsMutex.Lock();
    if (runThreads)
    {
        runThreadsMutex.Unlock();
        inputQueueMutex.Lock();
        inputFunctionQueue.Clear();
        inputQueue.Clear();
        inputQueueMutex.Unlock();

        outputQueueMutex.Lock();
        outputQueue.Clear();
        outputQueueMutex.Unlock();
    }
    else
    {
        inputFunctionQueue.Clear();
        inputQueue.Clear();
        outputQueue.Clear();
    }
}
template <class InputType, class OutputType>
void ThreadPool<InputType, OutputType>::LockInput(void)
{
    inputQueueMutex.Lock();
}
template <class InputType, class OutputType>
void ThreadPool<InputType, OutputType>::UnlockInput(void)
{
    inputQueueMutex.Unlock();
}
template <class InputType, class OutputType>
unsigned ThreadPool<InputType, OutputType>::InputSize(void)
{
    return inputQueue.Size();
}
template <class InputType, class OutputType>
InputType ThreadPool<InputType, OutputType>::GetInputAtIndex(unsigned index)
{
    return inputQueue[index];
}
template <class InputType, class OutputType>
void ThreadPool<InputType, OutputType>::RemoveInputAtIndex(unsigned index)
{
    inputQueue.RemoveAtIndex(index);
    inputFunctionQueue.RemoveAtIndex(index);
}
template <class InputType, class OutputType>
void ThreadPool<InputType, OutputType>::LockOutput(void)
{
    outputQueueMutex.Lock();
}
template <class InputType, class OutputType>
void ThreadPool<InputType, OutputType>::UnlockOutput(void)
{
    outputQueueMutex.Unlock();
}
template <class InputType, class OutputType>
unsigned ThreadPool<InputType, OutputType>::OutputSize(void)
{
    return outputQueue.Size();
}
template <class InputType, class OutputType>
OutputType ThreadPool<InputType, OutputType>::GetOutputAtIndex(unsigned index)
{
    return outputQueue[index];
}
template <class InputType, class OutputType>
void ThreadPool<InputType, OutputType>::RemoveOutputAtIndex(unsigned index)
{
    outputQueue.RemoveAtIndex(index);
}
template <class InputType, class OutputType>
void ThreadPool<InputType, OutputType>::ClearInput(void)
{
    inputQueue.Clear();
    inputFunctionQueue.Clear();
}

template <class InputType, class OutputType>
void ThreadPool<InputType, OutputType>::ClearOutput(void)
{
    outputQueue.Clear();
}
template <class InputType, class OutputType>
bool ThreadPool<InputType, OutputType>::IsWorking(void)
{
    bool isWorking;
//    workingThreadCountMutex.Lock();
//    isWorking=numThreadsWorking!=0;
//    workingThreadCountMutex.Unlock();

//    if (isWorking)
//        return true;

    // Bug fix: Originally the order of these two was reversed.
    // It's possible with the thread timing that working could have been false, then it picks up the data in the other thread, then it checks
    // here and sees there is no data.  So it thinks the thread is not working when it was.
    if (HasOutputFast() && HasOutput())
        return true;

    if (HasInputFast() && HasInput())
        return true;

    // Need to check is working again, in case the thread was between the first and second checks
    workingThreadCountMutex.Lock();
    isWorking=numThreadsWorking!=0;
    workingThreadCountMutex.Unlock();

    return isWorking;
}

template <class InputType, class OutputType>
int ThreadPool<InputType, OutputType>::NumThreadsWorking(void)
{
    return numThreadsWorking;
}

template <class InputType, class OutputType>
bool ThreadPool<InputType, OutputType>::WasStopped(void)
{
    bool b;
    runThreadsMutex.Lock();
    b = runThreads;
    runThreadsMutex.Unlock();
    return b;
}

#ifdef _MSC_VER
#pragma warning( pop )
#endif

#endif

