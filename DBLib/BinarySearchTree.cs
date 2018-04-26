using System;
using System.Collections;
using System.Collections.Generic;

namespace DBLib
{
    // This is a recursive algorithm, but recursion is bad in languages without tail call optimization.
    // We're using an iterative approach to avoid OutOfMemoryException in very large sets.
    public class BinarySearchTree<T>
    {
        private readonly IComparer<T> Comparer;

        private Node Root = null;

        public BinarySearchTree(IComparer<T> comparer) {
            Comparer = comparer;
        }

        public void Add(T newValue) {
            if(Root == null) {
                Root = new Node(newValue);
                return;
            } 
            Node current = Root;
            while(true) {
                if(Comparer.Compare(current.Value, newValue) > 0) {
                    // current.Value > newValue
                    if(current.LowerValues == null) {
                        current.LowerValues = new Node(newValue);
                        break;
                    } else {
                        current = current.LowerValues;
                        continue;
                    }
                } else {
                    // current.Value <= newValue
                    if(current.HigherValues == null) {
                        current.HigherValues = new Node(newValue);
                        break;
                    } else {
                        current = current.HigherValues;
                        continue;
                    }
                }
            }
        }

        private static Node FindClosestLeaf(Node root, T value, IComparer<T> comparer) {
            Node current = root;
            while(true) {
                if(comparer.Compare(current.Value, value) > 0) {
                    // current.Value > value
                    if(current.LowerValues == null) {
                        return current;
                    } else {
                        current = current.LowerValues;
                        continue;
                    }
                } else {
                    // current.Value <= value
                    if(current.HigherValues == null) {
                        return current;
                    } else {
                        current = current.HigherValues;
                        continue;
                    }
                }
            }        
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

            public Node(T value) {
                Value = value;
            }
        }
    }
}
