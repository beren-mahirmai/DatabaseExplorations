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

        public Int32 Count { get; private set; } = 0;

        public BinarySearchTree(IComparer<T> comparer) {
            Comparer = comparer;
        }

        public void Add(T newValue) {
            if(Root == null) {
                Count = 1;
                Root = new Node(newValue);
                return;
            } 
            Node current = Root;
            while(true) {
                Int32 compareResult = Comparer.Compare(current.Value, newValue);
                if(compareResult == 0) {
                    break; // already in the tree, so no op
                } else if(compareResult > 0) {
                    // current.Value > value
                    if(current.LowerValues == null) {
                        Count++;
                        current.LowerValues = new Node(newValue);
                        break;
                    } else {
                        current = current.LowerValues;
                        continue;
                    }
                } else {
                    // current.Value < value
                    if(current.HigherValues == null) {
                        Count++;
                        current.HigherValues = new Node(newValue);
                        break;
                    } else {
                        current = current.HigherValues;
                        continue;
                    }
                }
            }
        }

        public Boolean Contains(T value) {
            Node current = Root;
            while(true) {
                Int32 compareResult = Comparer.Compare(current.Value, value);
                if(compareResult == 0) {
                    return true;
                } else if(compareResult > 0) {
                    // current.Value > value
                    if(current.LowerValues == null) {
                        return false;
                    } else {
                        current = current.LowerValues;
                        continue;
                    }
                } else {
                    // current.Value < value
                    if(current.HigherValues == null) {
                        return false;
                    } else {
                        current = current.HigherValues;
                        continue;
                    }
                }
            }
        }

        public T[] ToSortedArray() {
            return new T[0];
            T[] result = new T[Count];
            if(Root == null) {
                return result;
            }
            Int32 index = 0;
            Node current = Root;
            while(true) {
                // if index == Count
                break;
                // if LowerValues.value is lowest, go down that path
                // if current.Value is lowest, take this value
                // if HigherValues.value is lowest, go down that path 
            }
        }

        public Path DeterminePath(Node node, T testValue) {
            if(node.LowerValues != null && testValue.Equals(default(T))) {
                // LowerValues path exists and we haven't settled on a first value yet
                return Path.Lower;
            }
            if(node.LowerValues != null && Comparer.Compare(node.LowerValues.Value, testValue) > 0) {
                // LowerValues path exists and it hasn't been traversed yet (higher than test value)
                return Path.Lower;
            }
            if(node.LowerValues == null && testValue.Equals(default(T))) {
                // No LowerValues and we haven't picked the first item, so this is that item
                return Path.Current;
            }
            if(Comparer.Compare(node.Value, testValue) >= 0) {
                // No LowerValues or LowerValues are already traversed and we haven't added the current item yet
                return Path.Current;
            }
            if(node.HigherValues == null) {
                // We have already handled the LowerValues path and current value, and there is no higher path
                return Path.Back;
            }
            if(node.HigherValues != null && Comparer.Compare(node.HigherValues.Value, testValue) > 0) {
                // There is a higher path and we haven't traversed it yet
                return Path.Higher;
            }

            // We have already traversed the low, current, and high paths
            return Path.Back;
        }

        public enum Path {
            Lower,
            Current,
            Higher,
            Back
        }

        public class Node {
            public T Value;
            public Node LowerValues;
            public Node HigherValues;

            public Node(T value) {
                Value = value;
            }
        }
    }
}
