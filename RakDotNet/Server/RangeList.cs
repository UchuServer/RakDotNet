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

        public bool IsReadOnly => false;

        public void Add(int item)
        {
            throw new NotImplementedException();
        }

        public void Clear()
        {
            ranges.Clear();
        }

        public bool Contains(int item)
        {
            foreach (var r in ranges)
            {
                return r.Min <= item && item <= r.Max;
            }

            return false;
        }

        public void CopyTo(int[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        public IEnumerator<int> GetEnumerator()
        {
            var list = new List<int>();

            foreach (var r in ranges)
                list.AddRange(Enumerable.Range(r.Min, r.Max + 1));

            return list.GetEnumerator();
        }

        public bool Remove(int item)
        {
            throw new NotImplementedException();
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public override void Serialize(BitStream stream)
        {
            throw new NotImplementedException();
        }

        private struct Range
        {
            public int Min;
            public int Max;
        }
    }
}
