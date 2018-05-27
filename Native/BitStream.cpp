#include "BitStream.h"

extern "C"
{
    BitStream* InitializeBitStream1()
    {
        return new BitStream();
    }

    BitStream* InitializeBitStream2(const unsigned int initialBytesToAllocate)
    {
        return new BitStream(initialBytesToAllocate);
    }

    BitStream* InitializeBitStream3(unsigned char* data, const unsigned int length, bool copyData)
    {
        return new BitStream(data, length, copyData);
    }

    void DisposeBitStream(BitStream*& s)
    {
        if (s)
        {
            delete s;
            s = NULL;
        }
    }

    signed char ReadInt8(BitStream* s)
    {
        signed char o;
        s->Read(o);

        return o;
    }

    unsigned char ReadUInt8(BitStream* s)
    {
        unsigned char o;
        s->Read(o);

        return o;
    }

    signed short ReadInt16(BitStream* s)
    {
        signed short o;
        s->Read(o);

        return o;
    }

    unsigned short ReadUInt16(BitStream* s)
    {
        unsigned short o;
        s->Read(o);

        return o;
    }

    signed int ReadInt32(BitStream* s)
    {
        signed int o;
        s->Read(o);

        return o;
    }

    unsigned int ReadUInt32(BitStream* s)
    {
        unsigned int o;
        s->Read(o);

        return o;
    }

    signed long long ReadInt64(BitStream* s)
    {
        signed long long o;
        s->Read(o);

        return o;
    }

    unsigned long long int ReadUInt64(BitStream* s)
    {
        unsigned long long int o;
        s->Read(o);

        return o;
    }

    bool ReadBit(BitStream* s)
    {
        bool o;
        s->Read(o);

        return o;
    }

    void WriteInt8(BitStream* s, signed char i)
    {
        s->Write(i);
    }

    void WriteUInt8(BitStream* s, unsigned char i)
    {
        s->Write(i);
    }

    void WriteInt16(BitStream* s, signed short i)
    {
        s->Write(i);
    }

    void WriteUInt16(BitStream* s, unsigned short i)
    {
        s->Write(i);
    }

    void WriteInt32(BitStream* s, signed int i)
    {
        s->Write(i);
    }

    void WriteUInt32(BitStream* s, unsigned int i)
    {
        s->Write(i);
    }

    void WriteInt64(BitStream* s, signed long long i)
    {
        s->Write(i);
    }

    void WriteUInt64(BitStream* s, unsigned long long int i)
    {
        s->Write(i);
    }

    void WriteBit(BitStream* s, bool i)
    {
        s->Write(i);
    }
}
