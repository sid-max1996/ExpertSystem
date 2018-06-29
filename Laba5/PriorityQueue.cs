using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Laba5
{

    public struct L<T>
    {
        public T Element { get; set; }
        public int Key { get; set; }

        public int CompareTo(L<T> other)
        {
            return Key.CompareTo(other.Key);
        }
    }

    class PriorityQueue<T> : LinkedList<L<T>>
    {
        public void PriorityAdd(T obj, int key)
        {
            var newItem = new L<T> { Key = key, Element = obj };
            var item = First;

            while (item != null)
            {
                if (item.Value.CompareTo(newItem) > 0)
                {
                    AddBefore(item, newItem);
                    return;
                }
                item = item.Next;
            }

            AddLast(newItem);
        }

        public T Dequeue()
        {
            T res = this.First.Value.Element;
            Remove(this.First);
            return res;
        }

        public bool IsNotEmphty { get { return this.Count != 0; } }

    }

}
