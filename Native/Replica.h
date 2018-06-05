#ifndef __N_REPLICA_H__
#define __N_REPLICA_H__

#include "RakNet/Replica.h"
#include "RakNet/BitStream.h"

#include "Defines.h"

using namespace RakNet;

class NativeReplica : public Replica
{
public:
    void(*constructCB)(RakNetTime*, SystemAddress*, unsigned int&, BitStream*, bool);
    void(*scopeChangeCB)(bool, BitStream*, RakNetTime*, SystemAddress*, bool);
    void(*serializeCB)(bool, BitStream*, RakNetTime*, int, int, RakNetTime*, SystemAddress*, unsigned int&);

    NativeReplica();

    ReplicaReturnResult SendConstruction(RakNetTime, SystemAddress, unsigned int&, BitStream*, bool*);
    ReplicaReturnResult SendDestruction(BitStream*, SystemAddress, bool*);
    ReplicaReturnResult ReceiveDestruction(BitStream*, SystemAddress, RakNetTime);
    ReplicaReturnResult SendScopeChange(bool, BitStream*, RakNetTime, SystemAddress, bool*);
    ReplicaReturnResult ReceiveScopeChange(BitStream*, SystemAddress, RakNetTime);
    ReplicaReturnResult Serialize(bool*, BitStream*, RakNetTime, PacketPriority*, PacketReliability*, RakNetTime, SystemAddress, unsigned int&);
    ReplicaReturnResult Deserialize(BitStream*, RakNetTime, RakNetTime, SystemAddress);
    int GetSortPriority(void) const
    {
        return 0;
    }
};

extern "C"
{
    EXPORT NativeReplica* InitializeNativePacket();
    EXPORT void DisposeNativePacket(NativeReplica*);

    EXPORT void NativePacketSetConstructCallback(NativeReplica*, void(*)(RakNetTime*, SystemAddress*, unsigned int&, BitStream*, bool));
    EXPORT void NativePacketSetScopeChangeCallback(NativeReplica*, void(*)(bool, BitStream*, RakNetTime*, SystemAddress*, bool));
    EXPORT void NativePacketSetSerializeCallback(NativeReplica*, void(*)(bool, BitStream*, RakNetTime*, int, int, RakNetTime*, SystemAddress*, unsigned int&));
}

#endif