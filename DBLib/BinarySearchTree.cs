using System;
using System.Collections;
using System.Collections.Generic;

namespace DBLib
{
    public class BinarySearchTree<T>
    {
        IComparer<T> Comparer;

        public BinarySearchTree(IComparer<T> comparer) {
            Comparer = comparer;
        }

        public void Add(T obj) {
            throw new NotImplementedException();
        }

        public Boolean Contains(T obj) {
            throw new NotImplementedException();
        }

        public T[] ToSortedArray() {
            throw new NotImplementedException();
        }
    }
}
