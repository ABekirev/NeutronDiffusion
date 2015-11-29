using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace NeutronDiffusion.Logic
{
    class NeutronThreadsWrapper
    {
        private readonly List<List<Neutron>> _neutronChunks;
        private int _tasksCount;
        private readonly ManualResetEvent _tasksDone = new ManualResetEvent(false);

        public NeutronThreadsWrapper(List<Neutron> neutrons)
        {
            _neutronChunks = neutrons.ChunkBy(100);
            _tasksCount = _neutronChunks.Count;
        }

        private void ThreadPoolCallback(Object threadContext)
        {
            var neutrons = (List<Neutron>)threadContext;
            neutrons.ForEach(neutron => neutron.Move());
            if (Interlocked.Decrement(ref _tasksCount) == 0)
            {
                _tasksDone.Set();
            }
        }

        public void LaunchCalculations()
        {
            _neutronChunks.ForEach(chunk => ThreadPool.QueueUserWorkItem(ThreadPoolCallback, chunk));
            _tasksDone.WaitOne();
        }

    }

    public static class ListExtensions
    {
        public static List<List<T>> ChunkBy<T>(this List<T> source, int chunkSize)
        {
            return source
                .Select((x, i) => new { Index = i, Value = x })
                .GroupBy(x => x.Index / chunkSize)
                .Select(x => x.Select(v => v.Value).ToList())
                .ToList();
        }
    }
}
