#include "BitStream.h"

extern "C"
{
    BitStream* InitializeBitStream1()
    {
        return new BitStream();
    }

    BitStream* InitializeBitStream2(const unsigned int initialBytesToAllocate)
    {
        return new BitStream(initialBytesToAllocate);
    }

    BitStream* InitializeBitStream3(unsigned char* data, const unsigned int length, bool copyData)
    {
        return new BitStream(data, length, copyData);
    }

    void DisposeBitStream(BitStream*& s)
    {
        if (s)
        {
            delete s;
            s = NULL;
        }
    }

    signed char BitStreamReadInt8(BitStream* s)
    {
        signed char o;
        s->Read(o);

        return o;
    }

    unsigned char BitStreamReadUInt8(BitStream* s)
    {
        unsigned char o;
        s->Read(o);

        return o;
    }

    signed short BitStreamReadInt16(BitStream* s)
    {
        signed short o;
        s->Read(o);

        return o;
    }

    unsigned short BitStreamReadUInt16(BitStream* s)
    {
        unsigned short o;
        s->Read(o);

        return o;
    }

    signed int BitStreamReadInt32(BitStream* s)
    {
        signed int o;
        s->Read(o);

        return o;
    }

    unsigned int BitStreamReadUInt32(BitStream* s)
    {
        unsigned int o;
        s->Read(o);

        return o;
    }

    signed long long BitStreamReadInt64(BitStream* s)
    {
        signed long long o;
        s->Read(o);

        return o;
    }

    unsigned long long int BitStreamReadUInt64(BitStream* s)
    {
        unsigned long long int o;
        s->Read(o);

        return o;
    }

    bool BitStreamReadBit(BitStream* s)
    {
        bool o;
        s->Read(o);

        return o;
    }

    void BitStreamWriteInt8(BitStream* s, signed char i)
    {
        s->Write(i);
    }

    void BitStreamWriteUInt8(BitStream* s, unsigned char i)
    {
        s->Write(i);
    }

    void BitStreamWriteInt16(BitStream* s, signed short i)
    {
        s->Write(i);
    }

    void BitStreamWriteUInt16(BitStream* s, unsigned short i)
    {
        s->Write(i);
    }

    void BitStreamWriteInt32(BitStream* s, signed int i)
    {
        s->Write(i);
    }

    void BitStreamWriteUInt32(BitStream* s, unsigned int i)
    {
        s->Write(i);
    }

    void BitStreamWriteInt64(BitStream* s, signed long long i)
    {
        s->Write(i);
    }

    void BitStreamWriteUInt64(BitStream* s, unsigned long long int i)
    {
        s->Write(i);
    }

    void BitStreamWriteBit(BitStream* s, bool i)
    {
        s->Write(i);
    }

    signed char BitStreamReadInt8Compressed(BitStream* s)
    {
        signed char o;
        s->ReadCompressed(o);

        return o;
    }

    unsigned char BitStreamReadUInt8Compressed(BitStream* s)
    {
        unsigned char o;
        s->ReadCompressed(o);

        return o;
    }

    signed short BitStreamReadInt16Compressed(BitStream* s)
    {
        signed short o;
        s->ReadCompressed(o);

        return o;
    }

    unsigned short BitStreamReadUInt16Compressed(BitStream* s)
    {
        unsigned short o;
        s->ReadCompressed(o);

        return o;
    }

    signed int BitStreamReadInt32Compressed(BitStream* s)
    {
        signed int o;
        s->ReadCompressed(o);

        return o;
    }

    unsigned int BitStreamReadUInt32Compressed(BitStream* s)
    {
        unsigned int o;
        s->ReadCompressed(o);

        return o;
    }

    signed long long BitStreamReadInt64Compressed(BitStream* s)
    {
        signed long long o;
        s->ReadCompressed(o);

        return o;
    }

    unsigned long long int BitStreamReadUInt64Compressed(BitStream* s)
    {
        unsigned long long int o;
        s->ReadCompressed(o);

        return o;
    }

    bool BitStreamReadBitCompressed(BitStream* s)
    {
        bool o;
        s->ReadCompressed(o);

        return o;
    }

    void BitStreamWriteInt8Compressed(BitStream* s, signed char i)
    {
        s->WriteCompressed(i);
    }

    void BitStreamWriteUInt8Compressed(BitStream* s, unsigned char i)
    {
        s->WriteCompressed(i);
    }

    void BitStreamWriteInt16Compressed(BitStream* s, signed short i)
    {
        s->WriteCompressed(i);
    }

    void BitStreamWriteUInt16Compressed(BitStream* s, unsigned short i)
    {
        s->WriteCompressed(i);
    }

    void BitStreamWriteInt32Compressed(BitStream* s, signed int i)
    {
        s->WriteCompressed(i);
    }

    void BitStreamWriteUInt32Compressed(BitStream* s, unsigned int i)
    {
        s->WriteCompressed(i);
    }

    void BitStreamWriteInt64Compressed(BitStream* s, signed long long i)
    {
        s->WriteCompressed(i);
    }

    void BitStreamWriteUInt64Compressed(BitStream* s, unsigned long long int i)
    {
        s->WriteCompressed(i);
    }

    void BitStreamWriteBitCompressed(BitStream* s, bool i)
    {
        s->WriteCompressed(i);
    }
}
