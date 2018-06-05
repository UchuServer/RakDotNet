#ifndef __N_RAKPEERINTERFACE_H__
#define __N_RAKPEERINTERFACE_H__

#include "RakNet/RakNetworkFactory.h"
#include "RakNet/RakPeerInterface.h"

#include "Defines.h"

using namespace RakNet;

extern "C"
{
    EXPORT bool RakPeerInterfaceStartup(RakPeerInterface*, unsigned short, int, SocketDescriptor*);
    EXPORT void RakPeerInterfaceInitializeSecurity(RakPeerInterface*, const char*, const char*, const char*, const char*);
    EXPORT void RakPeerInterfaceDisableSecurity(RakPeerInterface*);
    EXPORT void RakPeerInterfaceSetMaximumIncomingConnections(RakPeerInterface*, unsigned short);
    EXPORT void RakPeerInterfaceSetIncomingPassword(RakPeerInterface*, const char*, int);
    EXPORT bool RakPeerInterfaceIsActive(RakPeerInterface*);
    EXPORT bool RakPeerInterfaceSend1(RakPeerInterface*, unsigned char*, const int, int, int, char, SystemAddress*, bool);
    EXPORT bool RakPeerInterfaceSend2(RakPeerInterface*, BitStream*, int, int, char, SystemAddress*, bool);
    EXPORT Packet* RakPeerInterfaceReceive(RakPeerInterface*);
    EXPORT void RakPeerInterfaceDeallocatePacket(RakPeerInterface*, Packet*);
    EXPORT void RakPeerInterfaceAttachPlugin(RakPeerInterface*, PluginInterface*);
    EXPORT void RakPeerInterfaceSetNetworkIDManager(RakPeerInterface*, NetworkIDManager*);
    EXPORT NetworkIDManager* RakPeerInterfaceGetNetworkIDManager(RakPeerInterface*);

    EXPORT RakPeerInterface* RakNetworkFactoryGetRakPeerInterface();
    EXPORT void RakNetworkFactoryDestroyRakPeerInterface(RakPeerInterface*);
}

#endif