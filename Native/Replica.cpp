#include "Replica.h"

NativeReplica::NativeReplica()
{
}

ReplicaReturnResult NativeReplica::SendConstruction(RakNetTime a, SystemAddress b, unsigned int& c, BitStream* d, bool* e)
{
    if (constructCB)
        constructCB(&a, &b, c, d, *e);

    return REPLICA_PROCESSING_DONE;
}

ReplicaReturnResult NativeReplica::SendDestruction(BitStream*, SystemAddress, bool*)
{
    return REPLICA_PROCESSING_DONE;
}

ReplicaReturnResult NativeReplica::ReceiveDestruction(BitStream*, SystemAddress, RakNetTime)
{
    return REPLICA_PROCESSING_DONE;
}

ReplicaReturnResult NativeReplica::SendScopeChange(bool a, BitStream* b, RakNetTime c, SystemAddress d, bool* e)
{
    if (scopeChangeCB)
        scopeChangeCB(a, b, &c, &d, *e);

    return REPLICA_PROCESSING_DONE;
}

ReplicaReturnResult NativeReplica::ReceiveScopeChange(BitStream*, SystemAddress, RakNetTime)
{
    return REPLICA_PROCESSING_DONE;
}

ReplicaReturnResult NativeReplica::Serialize(bool* a, BitStream* b, RakNetTime c, PacketPriority* d, PacketReliability* e, RakNetTime f, SystemAddress g, unsigned int& h)
{
    if (serializeCB)
        serializeCB(a, b, &c, *d, *e, &f, &g, h);

    return REPLICA_PROCESSING_DONE;
}

ReplicaReturnResult NativeReplica::Deserialize(BitStream*, RakNetTime, RakNetTime, SystemAddress)
{
    return REPLICA_PROCESSING_DONE;
}

extern "C"
{
    EXPORT NativeReplica* InitializeNativeReplica()
    {
        return new NativeReplica();
    }

    EXPORT void DisposeNativeReplica(NativeReplica* ptr)
    {
        if (ptr)
            delete ptr;
    }

    EXPORT void NativeReplicaSetConstructCallback(NativeReplica* ptr, void(*construct)(RakNetTime*, SystemAddress*, unsigned int&, BitStream*, bool))
    {
        ptr->constructCB = construct;
    }

    EXPORT void NativeReplicaSetScopeChangeCallback(NativeReplica* ptr, void(*scopeChange)(bool, BitStream*, RakNetTime*, SystemAddress*, bool))
    {
        ptr->scopeChangeCB = scopeChange;
    }

    EXPORT void NativeReplicaSetSerializeCallback(NativeReplica* ptr, void(*serialize)(bool, BitStream*, RakNetTime*, int, int, RakNetTime*, SystemAddress*, unsigned int&))
    {
        ptr->serializeCB = serialize;
    }
}