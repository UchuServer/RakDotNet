#ifndef __N_RAKNET_TYEPS_H__
#define __N_RAKNET_TYPES_H__

#include "RakNet/RakNetTypes.h"

#include "Defines.h"

extern "C"
{
    EXPORT SocketDescriptor* InitializeSocketDescriptor1();
    EXPORT SocketDescriptor* InitializeSocketDescriptor2(unsigned short, const char*);
    EXPORT unsigned short SocketDescriptorGetPort(SocketDescriptor*);
    EXPORT char* SocketDescriptorGetHostAddress(SocketDescriptor*);

    EXPORT SystemAddress* InitializeSystemAddress();
    EXPORT unsigned int SystemAddressGetBinaryAddress(SystemAddress*);
    EXPORT unsigned short SystemAddressGetPort(SystemAddress*);
    EXPORT const char* SystemAddressToString(SystemAddress*, bool);
    EXPORT void SystemAddressSetBinaryAddress(SystemAddress*, const char*);
    
    EXPORT unsigned short PacketGetSystemIndex(Packet*);
    EXPORT SystemAddress* PacketGetSystemAddress(Packet*);
    EXPORT unsigned int PacketGetLength(Packet*);
    EXPORT unsigned int PacketGetBitSize(Packet*);
    EXPORT unsigned char* PacketGetData(Packet*);
}

#endif