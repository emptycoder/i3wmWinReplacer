using System.Collections.Concurrent;

namespace WINReplacer
{
    public class FixedSizedQueue<T>
    {
        ConcurrentQueue<T> queue = new ConcurrentQueue<T>();
        private object lockObject = new object();
        readonly int limit;

        public FixedSizedQueue(int limit)
        {
            this.limit = limit;
        }

        public void Enqueue(T obj)
        {
            queue.Enqueue(obj);
            lock (lockObject)
            {
                T overflow;
                while (queue.Count > this.limit && queue.TryDequeue(out overflow)) ;
            }
        }

        public int Count()
        {
            return queue.Count;
        }

        public T[] ToArray()
        {
            return queue.ToArray();
        }
    }
}
