#include "RakNetTypes.h"

extern "C"
{
    EXPORT SocketDescriptor* InitializeSocketDescriptor1()
    {
        return new SocketDescriptor();
    }

    EXPORT SocketDescriptor* InitializeSocketDescriptor2(unsigned short port, const char* hostAddress)
    {
        return new SocketDescriptor(port, hostAddress);
    }

    EXPORT unsigned short SocketDescriptorGetPort(SocketDescriptor* ptr)
    {
        return ptr->port;
    }

    EXPORT char* SocketDescriptorGetHostAddress(SocketDescriptor* ptr)
    {
        return ptr->hostAddress;
    }

    EXPORT SystemAddress* InitializeSystemAddress()
    {
        return new SystemAddress();
    }

    EXPORT unsigned int SystemAddressGetBinaryAddress(SystemAddress* ptr)
    {
        return ptr->binaryAddress;
    }

    EXPORT unsigned short SystemAddressGetPort(SystemAddress* ptr)
    {
        return ptr->port;
    }

    EXPORT const char* SystemAddressToString(SystemAddress* ptr, bool port)
    {
        return ptr->ToString(port);
    }

    EXPORT void SystemAddressSetBinaryAddress(SystemAddress* ptr, const char* address)
    {
        ptr->SetBinaryAddress(address);
    }

    EXPORT unsigned short PacketGetSystemIndex(Packet* ptr)
    {
        return ptr->systemIndex;
    }

    EXPORT SystemAddress* PacketGetSystemAddress(Packet* ptr)
    {
        return &ptr->systemAddress;
    }

    EXPORT unsigned int PacketGetLength(Packet* ptr)
    {
        return ptr->length;
    }

    EXPORT unsigned int PacketGetBitSize(Packet* ptr)
    {
        return ptr->bitSize;
    }

    EXPORT unsigned char* PacketGetData(Packet* ptr)
    {
        return ptr->data;
    }
}