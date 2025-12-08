using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _10_ThreadDispatcher
{
    internal class ThreadDispatcher : SynchronizationContext
    {
        ConcurrentQueue<(SendOrPostCallback callback, object state)> _workQueue = new ConcurrentQueue<(SendOrPostCallback, object)>();

        public override void Post(SendOrPostCallback d, object? state)
        {
            _workQueue.Enqueue((d, state));
        }

        public void Exec()
        {
            while (_workQueue.TryDequeue(out var work))
            {
                work.callback(work.state);
            }
        }
    }
}