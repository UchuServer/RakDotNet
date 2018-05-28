using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RakDotNet
{
    public class RangeList : Serializable, ICollection<int>
    {
        private readonly List<Range> ranges;

        public RangeList()
        {
            ranges = new List<Range>();
        }

        public int Count
        {
            get
            {
                var len = 0;

                foreach (var r in ranges)
                    len += r.Max - r.Min + 1;

                return len;
            }
        }

        public IEnumerable<int> Holes
        {
            get
            {
                int? lastMax = null;

                foreach (var r in ranges)
                {
                    if (lastMax != null)
                        foreach (var i in Enumerable.Range((int)lastMax - 1, r.Min))
                            yield return i;


                    lastMax = r.Max;
                }
            }
        }

        public int HoleCount
        {
            get
            {
                var holes = 0;
                int? lastMax = null;

                foreach (var r in ranges)
                {
                    if (lastMax != null)
                        holes += r.Min - (int)lastMax - 1;

                    lastMax = r.Max;
                }

                return holes;
            }
        }

        public bool IsReadOnly => false;

        public void Add(int item)
        {
            var iter = ranges.GetEnumerator();

            while (iter.MoveNext())
            {
                var r = iter.Current;

                if (r.Min == item + 1)
                {
                    r.Min--;
                    return;
                }

                if (r.Min <= item)
                {
                    if (r.Max == item - 1)
                    {
                        r.Max++;

                        if (iter.MoveNext())
                        {
                            var n = iter.Current;

                            if (n.Min == item + 1)
                            {
                                r.Max = n.Max;

                                ranges.Remove(n);
                            }
                        }

                        return;
                    }
                }
                else
                {
                    ranges.Insert(ranges.IndexOf(r), new Range { Min = item, Max = item });
                    return;
                }
            }

            ranges.Add(new Range { Min = item, Max = item });
        }

        public void Clear()
        {
            ranges.Clear();
        }

        public bool Contains(int item)
        {
            foreach (var r in ranges)
                if (r.Min <= item && item <= r.Max)
                    return true;

            return false;
        }

        public void CopyTo(int[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        public IEnumerator<int> GetEnumerator()
        {
            foreach (var r in ranges)
                foreach (var i in Enumerable.Range(r.Min, r.Max + 1))
                    yield return i;
        }

        public bool Remove(int item)
        {
            throw new NotImplementedException();
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public override void Serialize(BitStream stream)
        {
            stream.WriteUInt16((ushort)ranges.Count);

            foreach (var r in ranges)
            {
                stream.WriteBit(r.Min == r.Max);
                stream.WriteUInt32((uint)r.Min);

                if (r.Min != r.Max)
                    stream.WriteUInt32((uint)r.Max);
            }
        }

        public new static RangeList Deserialize(BitStream stream)
        {
            var list = new RangeList();
            var count = stream.ReadUInt16Compressed();

            for (var i = 0; i < count; i++)
            {
                var maxEqualsMin = stream.ReadBit();
                var min = stream.ReadUInt32();
                var max = maxEqualsMin ? min : stream.ReadUInt32();

                list.ranges.Add(new Range { Min = (int)min, Max = (int)max });
            }

            return list;
        }

        private struct Range
        {
            public int Min;
            public int Max;
        }
    }
}
