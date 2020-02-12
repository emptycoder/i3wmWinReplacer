using System.Collections.Concurrent;

namespace WINReplacer
{
    public class FixedSizedQueue<T>
    {
        ConcurrentQueue<T> queue;
        private object lockObject = new object();

        public FixedSizedQueue()
        {
            queue = new ConcurrentQueue<T>();
        }

        public FixedSizedQueue(ConcurrentQueue<T> queue)
        {
            this.queue = queue;
        }

        public void Enqueue(T obj)
        {
            queue.Enqueue(obj);
            lock (lockObject)
            {
                T overflow;
                while (queue.Count > WIN.ControlsCount && queue.TryDequeue(out overflow)) ;
            }
        }

        public bool Contains(T app)
        {
            foreach (T item in queue)
            {
                if (item.Equals(app))
                {
                    return true;
                }
            }
            return false;
        }

        public int Count()
        {
            return queue.Count;
        }

        public T[] ToArray()
        {
            return queue.ToArray();
        }

        public ConcurrentQueue<T> GetQueue()
        {
            return queue;
        }
    }
}
