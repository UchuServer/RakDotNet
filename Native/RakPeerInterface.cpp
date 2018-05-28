#include "RakPeerInterface.h"

extern "C"
{
    EXPORT bool RakPeerInterfaceStartup(RakPeerInterface* ptr, unsigned short maxConnections, int threadSleepTimer, SocketDescriptor* socketDescriptor)
    {
        return ptr->Startup(maxConnections, threadSleepTimer, socketDescriptor, 1);
    }

    EXPORT void RakPeerInterfaceInitializeSecurity(RakPeerInterface* ptr, const char* pubKeyE, const char* pubKeyN, const char* privKeyP, const char* privKeyQ)
    {
        ptr->InitializeSecurity(pubKeyE, pubKeyN, privKeyP, privKeyQ);
    }

    EXPORT void RakPeerInterfaceDisableSecurity(RakPeerInterface* ptr)
    {
        ptr->DisableSecurity();
    }

    EXPORT void RakPeerInterfaceSetMaximumIncomingConnections(RakPeerInterface* ptr, unsigned short connections)
    {
        ptr->SetMaximumIncomingConnections(connections);
    }

    EXPORT void RakPeerInterfaceSetIncomingPassword(RakPeerInterface* ptr, const char* password, int length)
    {
        ptr->SetIncomingPassword(password, length);
    }

    EXPORT bool RakPeerInterfaceIsActive(RakPeerInterface* ptr)
    {
        return ptr->IsActive();
    }

    EXPORT bool RakPeerInterfaceSend1(RakPeerInterface* ptr, unsigned char* data, const int length, int priority, int reliability, char orderingChannel, SystemAddress* systemAddress, bool broadcast)
    {
        return ptr->Send((const char*) data, length, (PacketPriority) priority, (PacketReliability) reliability, orderingChannel, *systemAddress, broadcast);
    }

    EXPORT bool RakPeerInterfaceSend2(RakPeerInterface* ptr, BitStream* bitStream, int priority, int reliability, char orderingChannel, SystemAddress* systemAddress, bool broadcast)
    {
        return ptr->Send(bitStream, (PacketPriority) priority, (PacketReliability) reliability, orderingChannel, *systemAddress, broadcast);
    }

    EXPORT Packet* RakPeerInterfaceReceive(RakPeerInterface* ptr)
    {
        return ptr->Receive();
    }

    EXPORT void RakPeerInterfaceDeallocatePacket(RakPeerInterface* ptr, Packet* packet)
    {
        ptr->DeallocatePacket(packet);
    }
}