#ifndef __N_BITSTREAM_H__
#define __N_BITSTREAM_H__

#include "RakNet/BitStream.h"

using namespace RakNet;

extern "C"
{
    BitStream* InitializeBitStream1();
    BitStream* InitializeBitStream2(const unsigned int);
    BitStream* InitializeBitStream3(unsigned char*, const unsigned int, bool);
    void DisposeBitStream(BitStream*&);

    signed char ReadInt8(BitStream*);
    unsigned char ReadUInt8(BitStream*);
    signed short ReadInt16(BitStream*);
    unsigned short ReadUInt16(BitStream*);
    signed int ReadInt32(BitStream*);
    unsigned int ReadUInt32(BitStream*);
    signed long long ReadInt64(BitStream*);
    unsigned long long int ReadUInt64(BitStream*);
    bool ReadBit(BitStream*);

    void WriteInt8(BitStream*, signed char);
    void WriteUInt8(BitStream*, unsigned char);
    void WriteInt16(BitStream*, signed short);
    void WriteUInt16(BitStream*, unsigned short);
    void WriteInt32(BitStream*, signed int);
    void WriteUInt32(BitStream*, unsigned int);
    void WriteInt64(BitStream*, signed long long);
    void WriteUInt64(BitStream*, unsigned long long int);
    void WriteBit(BitStream*, bool);
}

#endif
