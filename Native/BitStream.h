#ifndef __N_BITSTREAM_H__
#define __N_BITSTREAM_H__

#ifdef _MSC_VER
#define EXPORT __declspec(dllexport)
#elif defined(__GNUC__)
#define EXPORT __attribute__((visibility("default")))
#else
#define EXPORT
#endif

#include "RakNet/BitStream.h"

using namespace RakNet;

extern "C"
{
    EXPORT BitStream* InitializeBitStream1();
    EXPORT BitStream* InitializeBitStream2(const unsigned int);
    EXPORT BitStream* InitializeBitStream3(unsigned char*, const unsigned int, bool);
    EXPORT void DisposeBitStream(BitStream*&);

    EXPORT signed char ReadInt8(BitStream*);
    EXPORT unsigned char ReadUInt8(BitStream*);
    EXPORT signed short ReadInt16(BitStream*);
    EXPORT unsigned short ReadUInt16(BitStream*);
    EXPORT signed int ReadInt32(BitStream*);
    EXPORT unsigned int ReadUInt32(BitStream*);
    EXPORT signed long long ReadInt64(BitStream*);
    EXPORT unsigned long long int ReadUInt64(BitStream*);
    EXPORT bool ReadBit(BitStream*);

    EXPORT void WriteInt8(BitStream*, signed char);
    EXPORT void WriteUInt8(BitStream*, unsigned char);
    EXPORT void WriteInt16(BitStream*, signed short);
    EXPORT void WriteUInt16(BitStream*, unsigned short);
    EXPORT void WriteInt32(BitStream*, signed int);
    EXPORT void WriteUInt32(BitStream*, unsigned int);
    EXPORT void WriteInt64(BitStream*, signed long long);
    EXPORT void WriteUInt64(BitStream*, unsigned long long int);
    EXPORT void WriteBit(BitStream*, bool);

    EXPORT signed char ReadInt8Compressed(BitStream*);
    EXPORT unsigned char ReadUInt8Compressed(BitStream*);
    EXPORT signed short ReadInt16Compressed(BitStream*);
    EXPORT unsigned short ReadUInt16Compressed(BitStream*);
    EXPORT signed int ReadInt32Compressed(BitStream*);
    EXPORT unsigned int ReadUInt32Compressed(BitStream*);
    EXPORT signed long long ReadInt64Compressed(BitStream*);
    EXPORT unsigned long long int ReadUInt64Compressed(BitStream*);
    EXPORT bool ReadBitCompressed(BitStream*);

    EXPORT void WriteInt8Compressed(BitStream*, signed char);
    EXPORT void WriteUInt8Compressed(BitStream*, unsigned char);
    EXPORT void WriteInt16Compressed(BitStream*, signed short);
    EXPORT void WriteUInt16Compressed(BitStream*, unsigned short);
    EXPORT void WriteInt32Compressed(BitStream*, signed int);
    EXPORT void WriteUInt32Compressed(BitStream*, unsigned int);
    EXPORT void WriteInt64Compressed(BitStream*, signed long long);
    EXPORT void WriteUInt64Compressed(BitStream*, unsigned long long int);
    EXPORT void WriteBitCompressed(BitStream*, bool);
}

#endif
