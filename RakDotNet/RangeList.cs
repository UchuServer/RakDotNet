using System;
using System.Collections;
using System.Collections.Generic;

namespace RakDotNet
{
    public class Range<T>
        where T : struct
    {
        public T Min { get; set; }
        public T Max { get; set; }
    }

    public abstract class RangeList<T> : ISerializable, IEnumerable<Range<T>>
        where T : struct
    {
        protected readonly List<Range<T>> _ranges;

        public int Count => _ranges.Count;

        protected RangeList()
        {
            _ranges = new List<Range<T>>();
        }

        public abstract void Add(T item);

        public void Clear()
            => _ranges.Clear();

        public IEnumerator<Range<T>> GetEnumerator()
            => _ranges.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator()
            => GetEnumerator();

        public abstract void Serialize(BitStream stream);

        public abstract void Deserialize(BitStream stream);
    }

    public class UIntRangeList : RangeList<uint>
    {
        public override void Add(uint item)
        {
            for (var i = 0; i < _ranges.Count; i++)
            {
                var range = _ranges[i];

                if (range.Min == item + 1)
                {
                    range.Min--;
                    return;
                }

                if (range.Min <= item)
                {
                    if (range.Max == item + 1)
                    {
                        range.Max++;

                        try
                        {
                            var nextRange = _ranges[++i];

                            if (nextRange.Min == item + 1)
                            {
                                range.Max = nextRange.Max;
                                _ranges.Remove(nextRange);
                            }
                        }
                        catch (IndexOutOfRangeException)
                        {
                        }

                        return;
                    }

                    if (range.Max >= item)
                        return;
                }
                else
                {
                    _ranges[i] = new Range<uint>
                    {
                        Min = item,
                        Max = item
                    };

                    return;
                }
            }

            _ranges.Add(new Range<uint>
            {
                Min = item,
                Max = item
            });
        }

        public override void Serialize(BitStream stream)
        {
            stream.WriteUShortCompressed((ushort) _ranges.Count);

            foreach (var range in _ranges)
            {
                var equalsMin = range.Min == range.Max;

                stream.WriteBit(equalsMin);
                stream.WriteUInt(range.Min);

                if (!equalsMin)
                    stream.WriteUInt(range.Max);
            }
        }

        public override void Deserialize(BitStream stream)
        {
            _ranges.Clear();

            var count = stream.ReadCompressedUShort();

            for (var i = 0; i < count; i++)
            {
                var equalsMin = stream.ReadBit();

                var min = stream.ReadUInt();
                var max = equalsMin ? min : stream.ReadUInt();

                _ranges.Add(new Range<uint>
                {
                    Min = min,
                    Max = max
                });
            }
        }
    }
}