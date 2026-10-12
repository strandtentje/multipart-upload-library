using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Diagnostics;

namespace Ziewaar.Web.UploaderServerSupport
{
    public class DataRateCollection
    {
        private int InCursor;
        private readonly long[] ByteCounts, TickCounts;
        private long LowStamp, HighStamp, DataMoved, TicksSpent;
        private int WorkingIndex, NaiveIndex;

        public DataRateCollection(int resolution)
        {
            this.ByteCounts = new long[resolution];
            this.TickCounts = new long[resolution];
            for (int i = 0; i < resolution; i++)
            {
                this.ByteCounts[i] = -1;
                this.TickCounts[i] = -1;
            }
        }

        public long Add(long byteCount)
        {
            if (InCursor < ByteCounts.Length)
            {
                ByteCounts[InCursor] = byteCount;
                TickCounts[InCursor] = Stopwatch.GetTimestamp();
                InCursor++;
                if (InCursor >= ByteCounts.Length)
                    InCursor = 0;

                LowStamp = long.MaxValue;
                HighStamp = 0;
                DataMoved = 0;
                for (NaiveIndex = InCursor; 
                    NaiveIndex < InCursor + ByteCounts.Length; NaiveIndex++)
                {
                    WorkingIndex = NaiveIndex % ByteCounts.Length;
                    if (ByteCounts[WorkingIndex] == -1)
                        continue;
                    LowStamp = Math.Min(LowStamp, TickCounts[WorkingIndex]);
                    HighStamp = Math.Max(HighStamp, TickCounts[WorkingIndex]);
                    if (WorkingIndex != InCursor)
                        DataMoved += ByteCounts[WorkingIndex];
                }
                TicksSpent = Math.Max(1, HighStamp - LowStamp);
                return (DataMoved * Stopwatch.Frequency) / TicksSpent;
            }
            else
            {
                return 0L;
            }
        }
    }
}
