using System.Collections;
using System.Collections.Generic;

namespace RakDotNet
{
    public struct Range<T> where T : struct
    {
        public T Min { get; set; }
        public T Max { get; set; }
    }
    
    public abstract class RangeList<T> : Serializable, IEnumerable<Range<T>>
        where T : struct
    {
        protected readonly List<Range<T>> _ranges;

        public int Count => _ranges.Count;

        protected RangeList()
        {
            _ranges = new List<Range<T>>();
        }
        
        public void Clear()
            => _ranges.Clear();

        public IEnumerator<Range<T>> GetEnumerator()
            => _ranges.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator()
            => GetEnumerator();
    }

    public class UIntRangeList : RangeList<uint>
    {
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