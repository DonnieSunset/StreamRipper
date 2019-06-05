using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StromReisser3000.Domain {
    public class PipeQueue<T> {
        private object _lck = new object();
        private int _capacity;
        private readonly Queue<T> _queue = new Queue<T>();

        public IEnumerable<T> Items { get { lock (_lck) return new List<T>(_queue); } }
        public int Capacity {
            get { return _capacity; }
            set {
                if(value <= 0) {
                    throw new ArgumentException("The capacity of the pipe queue cannot be zero or negative.", nameof(value));
                }

                _capacity = value;
                lock(_lck) {
                    TrimQueue();
                }
            }
        }

        public PipeQueue(int capacity) {
            Capacity = capacity;
        }

        public void Enqueue(T item) {
            if(item == null) {
                throw new ArgumentNullException(nameof(item));
            }

            lock(_lck) {
                TrimQueue();
                _queue.Enqueue(item);
            }
        }

        private void TrimQueue() {
            while(_queue.Count >= Capacity) {
                _queue.Dequeue();
            }
        }
    }
}
