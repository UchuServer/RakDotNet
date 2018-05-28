#ifndef __N_RAKNETWORKFACTORY_H__
#define __N_RAKNETWORKFACTORY_H__

#include "RakNet/RakNetworkFactory.h"
#include "RakNet/RakPeerInterface.h"

#include "Defines.h"

extern "C"
{
    EXPORT RakPeerInterface* RakNetworkFactoryGetRakPeerInterface();
    EXPORT void RakNetworkFactoryDestroyRakPeerInterface(RakPeerInterface*);
}

#endif