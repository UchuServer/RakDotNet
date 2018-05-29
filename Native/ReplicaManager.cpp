#include "ReplicaManager.h"

extern "C"
{
    EXPORT ReplicaManager* InitializeReplicaManager()
    {
        return new ReplicaManager();
    }

    EXPORT void DisposeReplicaManager(ReplicaManager*& ptr)
    {
        if (ptr)
        {
            delete ptr;
            ptr = NULL;
        }
    }

    EXPORT void ReplicaManagerSetAutoParticipateNewConnections(ReplicaManager* ptr, bool autoAdd)
    {
        ptr->SetAutoParticipateNewConnections(autoAdd);
    }

    EXPORT bool ReplicaManagerAddParticipant(ReplicaManager* ptr, SystemAddress* systemAddress)
    {
        return ptr->AddParticipant(*systemAddress);
    }

    EXPORT bool ReplicaManagerRemoveParticipant(ReplicaManager* ptr, SystemAddress* systemAddress)
    {
        return ptr->RemoveParticipant(*systemAddress);
    }

    EXPORT void ReplicaManagerConstruct(ReplicaManager* ptr, Replica* replica, bool copy, SystemAddress* systemAddress, bool broadcast)
    {
        ptr->Construct(replica, copy, *systemAddress, broadcast);
    }

    EXPORT void ReplicaManagerDestruct(ReplicaManager* ptr, Replica* replica, SystemAddress* systemAddress, bool broadcast)
    {
        ptr->Destruct(replica, *systemAddress, broadcast);
    }

    EXPORT void ReplicaManagerReferencePointer(ReplicaManager* ptr, Replica* replica)
    {
        ptr->ReferencePointer(replica);
    }

    EXPORT void ReplicaManagerDereferencePointer(ReplicaManager* ptr, Replica* replica)
    {
        ptr->DereferencePointer(replica);
    }

    EXPORT void ReplicaManagerSetScope(ReplicaManager* ptr, Replica* replica, bool inScope, SystemAddress* systemAddress, bool broadcast)
    {
        ptr->SetScope(replica, inScope, *systemAddress, broadcast);
    }

    EXPORT void ReplicaManagerSignalSerializeNeeded(ReplicaManager* ptr, Replica* replica, SystemAddress* systemAddress, bool broadcast)
    {
        ptr->SignalSerializeNeeded(replica, *systemAddress, broadcast);
    }

    EXPORT void ReplicaManagerSetSendChannel(ReplicaManager* ptr, unsigned char channel)
    {
        ptr->SetSendChannel(channel);
    }

    EXPORT void ReplicaManagerSetAutoConstructToNewParticipants(ReplicaManager* ptr, bool autoConstruct)
    {
        ptr->SetAutoConstructToNewParticipants(autoConstruct);
    }

    EXPORT void ReplicaManagerSetDefaultScope(ReplicaManager* ptr, bool scope)
    {
        ptr->SetDefaultScope(scope);
    }

    EXPORT void ReplicaManagerSetAutoSerializeInScope(ReplicaManager* ptr, bool autoSerialize)
    {
        ptr->SetAutoSerializeInScope(autoSerialize);
    }

    EXPORT void ReplicaManagerUpdate(ReplicaManager* ptr, RakPeerInterface* rakPeer)
    {
        ptr->Update(rakPeer);
    }

    EXPORT void ReplicaManagerEnableReplicaInterfaces(ReplicaManager* ptr, Replica* replica, unsigned char interfaceFlags)
    {
        ptr->EnableReplicaInterfaces(replica, interfaceFlags);
    }

    EXPORT void ReplicaManagerDisableReplicaInterfaces(ReplicaManager* ptr, Replica* replica, unsigned char interfaceFlags)
    {
        ptr->DisableReplicaInterfaces(replica, interfaceFlags);
    }

    EXPORT bool ReplicaManagerIsConstructed(ReplicaManager* ptr, Replica* replica, SystemAddress* systemAddress)
    {
        return ptr->IsConstructed(replica, *systemAddress);
    }

    EXPORT bool ReplicaManagerIsInScope(ReplicaManager* ptr, Replica* replica, SystemAddress* systemAddress)
    {
        return ptr->IsInScope(replica, *systemAddress);
    }

    EXPORT unsigned int ReplicaManagerGetReplicaCount(ReplicaManager* ptr)
    {
        return ptr->GetReplicaCount();
    }

    EXPORT Replica* ReplicaManagerGetReplicaAtIndex(ReplicaManager* ptr, unsigned int index)
    {
        return ptr->GetReplicaAtIndex(index);
    }

    EXPORT unsigned int ReplicaManagerGetParticipantCount(ReplicaManager* ptr)
    {
        return ptr->GetParticipantCount();
    }

    EXPORT SystemAddress* ReplicaManagerGetParticipantAtIndex(ReplicaManager* ptr, unsigned int index)
    {
        return &ptr->GetParticipantAtIndex(index);
    }

    EXPORT bool ReplicaManagerHasParticipant(ReplicaManager* ptr, SystemAddress* systemAddress)
    {
        return ptr->HasParticipant(*systemAddress);
    }
}