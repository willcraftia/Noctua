#region Using

using System;
using System.Collections.Generic;

#endregion

namespace Noctua
{
    public sealed class SideCollection<T> : IList<T>
    {
        #region Enumerator

        public struct Enumerator : IEnumerator<T>, System.Collections.IEnumerator
        {
            SideCollection<T> owner;

            int index;

            int version;

            T current;

            public T Current
            {
                get { return current; }
            }

            object System.Collections.IEnumerator.Current
            {
                get
                {
                    if (index == 0 || index == owner.Count + 1) throw new IndexOutOfRangeException("index");
                    return Current;
                }
            }

            internal Enumerator(SideCollection<T> owner)
            {
                this.owner = owner;
                index = 0;
                version = owner.version;
                current = default(T);
            }

            public void Dispose() { }

            public bool MoveNext()
            {
                if (version == owner.version && index < owner.Count)
                {
                    current = owner.items[index];
                    index++;
                    return true;
                }
                return MoveNextRare();
            }

            public void Reset()
            {
                if (version != owner.version) throw new InvalidOperationException("Version mismatch.");

                index = 0;
                current = default(T);
            }

            bool MoveNextRare()
            {
                if (version != owner.version) throw new InvalidOperationException("Version mismatch.");

                index = owner.Count + 1;
                current = default(T);
                return false;
            }
        }

        #endregion

        T[] items = new T[Side.Count];

        int version;

        public T this[int index]
        {
            get { return items[index]; }
            set
            {
                items[index] = value;
                version++;
            }
        }

        public int Count
        {
            get { return items.Length; }
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

        public T this[Side side]
        {
            get { return this[side.Index]; }
            set { this[side.Index] = value; }
        }

        public int IndexOf(T item)
        {
            return Array.IndexOf(items, item);
        }

        public void Insert(int index, T item)
        {
            throw new NotSupportedException();
        }

        public void RemoveAt(int index)
        {
            throw new NotSupportedException();
        }

        public void Add(T item)
        {
            throw new NotSupportedException();
        }

        public void Clear()
        {
            Array.Clear(items, 0, items.Length);
            version++;
        }

        public bool Contains(T item)
        {
            if ((object) item == null)
            {
                for (int i = 0; i < items.Length; i++)
                {
                    if ((object) items[i] == null) return true;
                }
                return false;
            }
            else
            {
                EqualityComparer<T> c = EqualityComparer<T>.Default;
                for (int i = 0; i < items.Length; i++)
                {
                    if (c.Equals(items[i], item)) return true;
                }
                return false;
            }
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            Array.Copy(items, 0, array, arrayIndex, items.Length);
        }

        public bool Remove(T item)
        {
            throw new NotSupportedException();
        }

        public IEnumerator<T> GetEnumerator()
        {
            return new Enumerator(this);
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return new Enumerator(this);
        }
    }
}
