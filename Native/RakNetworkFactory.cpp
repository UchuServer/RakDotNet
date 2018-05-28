#include "RakNetworkFactory.h"

extern "C"
{
    EXPORT RakPeerInterface* RakNetworkFactoryGetRakPeerInterface()
    {
        return RakNetworkFactory::GetRakPeerInterface();
    }

    EXPORT void RakNetworkFactoryDestroyRakPeerInterface(RakPeerInterface* ptr)
    {
        RakNetworkFactory::DestroyRakPeerInterface(ptr);
    }
}