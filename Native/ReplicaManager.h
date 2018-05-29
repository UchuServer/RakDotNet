#ifndef __N_REPLICAMANAGER_H__
#define __N_REPLICAMANAGER_H__

#include "RakNet/ReplicaManager.h"

#include "Defines.h"

extern "C"
{
    EXPORT ReplicaManager* InitializeReplicaManager();
    EXPORT void DisposeReplicaManager(ReplicaManager*&);

    EXPORT void ReplicaManagerSetAutoParticipateNewConnections(ReplicaManager*, bool);
    EXPORT bool ReplicaManagerAddParticipant(ReplicaManager*, SystemAddress*);
    EXPORT bool ReplicaManagerRemoveParticipant(ReplicaManager*, SystemAddress*);
    EXPORT void ReplicaManagerConstruct(ReplicaManager*, Replica*, bool, SystemAddress*, bool);
    EXPORT void ReplicaManagerDestruct(ReplicaManager*, Replica*, SystemAddress*, bool);
    EXPORT void ReplicaManagerReferencePointer(ReplicaManager*, Replica*);
    EXPORT void ReplicaManagerDereferencePointer(ReplicaManager*, Replica*);
    EXPORT void ReplicaManagerSetScope(ReplicaManager*, Replica*, bool, SystemAddress*, bool);
    EXPORT void ReplicaManagerSignalSerializeNeeded(ReplicaManager*, Replica*, SystemAddress*, bool);
    EXPORT void ReplicaManagerSetSendChannel(ReplicaManager*, unsigned char);
    EXPORT void ReplicaManagerSetAutoConstructToNewParticipants(ReplicaManager*, bool);
    EXPORT void ReplicaManagerSetDefaultScope(ReplicaManager*, bool);
    EXPORT void ReplicaManagerSetAutoSerializeInScope(ReplicaManager*, bool);
    EXPORT void ReplicaManagerUpdate(ReplicaManager*, RakPeerInterface*);
    EXPORT void ReplicaManagerEnableReplicaInterfaces(ReplicaManager*, Replica*, unsigned char);
    EXPORT void ReplicaManagerDisableReplicaInterfaces(ReplicaManager*, Replica*, unsigned char);
    EXPORT bool ReplicaManagerIsConstructed(ReplicaManager*, Replica*, SystemAddress*);
    EXPORT bool ReplicaManagerIsInScope(ReplicaManager*, Replica*, SystemAddress*);
    EXPORT unsigned int ReplicaManagerGetReplicaCount(ReplicaManager*);
    EXPORT Replica* ReplicaManagerGetReplicaAtIndex(ReplicaManager*, unsigned int);
    EXPORT unsigned int ReplicaManagerGetParticipantCount(ReplicaManager*);
    EXPORT SystemAddress* ReplicaManagerGetParticipantAtIndex(ReplicaManager*, unsigned int);
    EXPORT bool ReplicaManagerHasParticipant(ReplicaManager*, SystemAddress*);
}

#endif