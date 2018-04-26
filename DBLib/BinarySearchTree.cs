using System;
using System.Collections;
using System.Collections.Generic;

namespace DBLib
{
    public class BinarySearchTree<T>
    {
        private readonly IComparer<T> Comparer;

        private Node Root = null;

        public BinarySearchTree(IComparer<T> comparer) {
            Comparer = comparer;
        }

        public void Add(T value) {
            if(Root == null) {
                Root = new Node(value, Comparer);
                return;
            } 
            Root.Add(value);
        }

        public Boolean Contains(T obj) {
            throw new NotImplementedException();
        }

        public T[] ToSortedArray() {
            throw new NotImplementedException();
        }

        private class Node {
            public T Value;
            public Node LowerValues;
            public Node HigherValues;

            private readonly IComparer<T> Comparer;

            public Node(T value, IComparer<T> comparer) {
                Value = value;
                Comparer = comparer;
            }

            public void Add(T newValue) {
                if(Comparer.Compare(Value, newValue) > 0) {
                    // Value > newValue
                    if(LowerValues == null) {
                        LowerValues = new Node(newValue, Comparer);
                    } else {
                        LowerValues.Add(newValue);
                    }
                } else {
                    // Value <= newValue
                    if(HigherValues == null) {
                        HigherValues = new Node(newValue, Comparer);
                    } else {
                        HigherValues.Add(newValue);
                    }
                }
            }
        }
    }
}
