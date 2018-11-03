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

    public abstract class RangeList<T> : ISerializable, IEnumerable<T>
        where T : struct
    {
        protected readonly List<Range<T>> Ranges;

        public int RangeCount => Ranges.Count;
        public abstract int Count { get; }
        public abstract int HoleCount { get; }

        protected RangeList()
        {
            Ranges = new List<Range<T>>();
        }

        public abstract void Add(T item);

        public abstract IEnumerable<T> GetHoles();

        public void Clear()
            => Ranges.Clear();

        public abstract IEnumerator<T> GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator()
            => GetEnumerator();

        public abstract void Serialize(BitStream stream);

        public abstract void Deserialize(BitStream stream);
    }

    public class UIntRangeList : RangeList<uint>
    {
        public override int Count
        {
            get
            {
                var length = 0u;

                foreach (var range in Ranges)
                {
                    length += range.Max - range.Min + 1;
                }

                return (int) length;
            }
        }

        public override int HoleCount
        {
            get
            {
                var holes = 0u;
                var lastMax = 0u;

                for (var i = 0; i < Ranges.Count; i++)
                {
                    var range = Ranges[i];

                    if (i != 0)
                        holes += range.Min - lastMax - 1;

                    lastMax = range.Max;
                }

                return (int) holes;
            }
        }

        public override IEnumerable<uint> GetHoles()
        {
            var lastMax = 0u;

            for (var i = 0; i < Ranges.Count; i++)
            {
                var range = Ranges[i];

                if (i != 0)
                {
                    for (var ii = lastMax + 1; ii < range.Min; ii++)
                    {
                        yield return ii;
                    }
                }

                lastMax = range.Max;
            }
        }

        public override IEnumerator<uint> GetEnumerator()
        {
            foreach (var range in Ranges)
            {
                for (var i = range.Min; i <= range.Max; i++)
                {
                    yield return i;
                }
            }
        }

        public override void Add(uint item)
        {
            for (var i = 0; i < Ranges.Count; i++)
            {
                var range = Ranges[i];

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
                            var nextRange = Ranges[++i];

                            if (nextRange.Min == item + 1)
                            {
                                range.Max = nextRange.Max;
                                Ranges.Remove(nextRange);
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
                    Ranges[i] = new Range<uint>
                    {
                        Min = item,
                        Max = item
                    };

                    return;
                }
            }

            Ranges.Add(new Range<uint>
            {
                Min = item,
                Max = item
            });
        }

        public override void Serialize(BitStream stream)
        {
            stream.WriteUShortCompressed((ushort) Ranges.Count);

            foreach (var range in Ranges)
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
            Ranges.Clear();

            var count = stream.ReadCompressedUShort();

            for (var i = 0; i < count; i++)
            {
                var equalsMin = stream.ReadBit();

                var min = stream.ReadUInt();
                var max = equalsMin ? min : stream.ReadUInt();

                Ranges.Add(new Range<uint>
                {
                    Min = min,
                    Max = max
                });
            }
        }
    }
}