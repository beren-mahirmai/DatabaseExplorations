using System;
using System.Collections;
using System.Collections.Generic;

namespace DBLib
{
    // This is a recursive algorithm, but recursion is bad in languages without tail call optimization.
    // We're using an iterative approach to avoid OutOfMemoryException in very large sets.
    // This implementation assume that values cannot be assigned twice, so the second time a value is
    // added the behavior is a no-op.
    public class BinarySearchTree<T>
    {
        private readonly IComparer<T> Comparer;
        private Node Root = null;

        public Int32 Count { get; private set; } = 0;

        public BinarySearchTree(IComparer<T> comparer) {
            Comparer = comparer;
        }

        // Adding to the tree is a recursive algorithm.  For each node compare the value being added 
        // with the current value, then step down either the 'low' or 'high' path in the tree to the
        // next node and try again.  If we need to go down a path, but the path is null, create a new
        // node their with the path in question.
        // If an attempt to add a duplicate is made, we eventually find it and return, resulting in
        // a no-op.
        public void Add(T newValue) {
            if(Root == null) {
                Count = 1;
                Root = new Node(newValue);
                return;
            } 
            Node current = Root;
            while(true) {
                Path path = GetPathForValueOnNode(current, newValue);
                if(path == Path.Current) { // we found a match, so do nothing
                    break;
                }
                Node nextNode = current.GetNodeForPath(path);
                if(nextNode == null) {
                    // This is the path the value should go on, but it is empty, so that's the node to set
                    current.SetValueForPath(newValue, path); 
                    Count++;
                    break;   
                }
                current = nextNode;
            }
        }

        // Checking the tree for a value is basically the same algorithm as adding, except we 
        // report true if we find a node with the value and false if we get to null branch
        // withiout encountering it.
        public Boolean Contains(T value) {
            Node current = Root;
            while(current != null) {
                Path path = GetPathForValueOnNode(current, value);
                if(path == Path.Current) {
                    return true;
                }
                current = current.GetNodeForPath(path);
            }
            return false;
        }

        // This takes the values, which are sorted in the tree but may be scattered about the
        // heap, and walks the tree and copies the values into an array in the correct order.
        public T[] ToSortedArray() {
            T[] result = new T[Count];
            if(Root == null) {
                return result;
            }
            Int32 index = 0;
            var nodeStack = new Stack<Node>();
            nodeStack.Push(Root);
            while(index < Count) {
                Path next = DeterminePath(nodeStack.Peek(), index >= 1 ? result[index-1] : default(T));
                if(next == Path.Lower) {
                    nodeStack.Push(nodeStack.Peek().LowerValues);
                } else if (next == Path.Higher) {
                    nodeStack.Push(nodeStack.Peek().HigherValues);
                } else if (next == Path.Current) {
                    result[index] = nodeStack.Peek().Value;
                    index++;
                } else { // next == Path.Back
                    nodeStack.Pop();
                }
            }
            return result;
        }

        // While walking a tree to find a value x, decide if we should look on the low or high branch,
        // or let us know if the supplied node is an exact match.
        private Path GetPathForValueOnNode(Node node, T x) {
            Int32 compareResult = Comparer.Compare(node.Value, x);
            return
                compareResult > 0 ? Path.Lower :
                compareResult < 0 ? Path.Higher :
                Path.Current;
        }

        // This method is intended to support ToSortedArray() by encapsulating the logic of deciding
        // whether to use the current value or check one of the branches further down the tree.
        // The 'testValue' is the value most recently written to the output array, we can assume
        // that anything equal to lower than that is already in the output array.  So between the
        // Lower branch, value, and higher branch for a node we always choose the lowest value that
        // is higher than 'testValue'.  If 'testValue' is higher than all three, we back out a layer.
        private Path DeterminePath(Node node, T testValue) {

            if(node.LowerValues != null && (testValue == null || testValue.Equals(default(T)))) {
                // LowerValues path exists and we haven't settled on a first value yet
                return Path.Lower;
            }
            if(node.LowerValues != null && Comparer.Compare(node.LowerValues.Value, testValue) > 0) {
                // LowerValues path exists and it hasn't been traversed yet (higher than test value)
                return Path.Lower;
            }
            if(node.LowerValues == null && (testValue == null || testValue.Equals(default(T)))) {
                // No LowerValues and we haven't picked the first item, so this is that item
                return Path.Current;
            }
            if(Comparer.Compare(node.Value, testValue) > 0) {
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

        // Indicates which path should be walked next to wort the tree.
        private enum Path {
            Lower,
            Current,
            Higher,
            Back
        }

        // Represents a single node of the tree, with a current value and two subtrees, one for values lower
        // than the node's value, one with the values which are higher.
        private class Node {
            public readonly T Value;
            public Node LowerValues {get; private set; }
            public Node HigherValues {get; private set;}

            public Node(T value) {
                Value = value;
            }

            // Sets the higher or lower value, assuming that subtree is null.
            public void SetValueForPath(T newValue, Path path) {
                if(path == Path.Lower && LowerValues == null) {
                    LowerValues = new Node(newValue);
                    return;
                }

                if(path == Path.Higher && HigherValues == null) {
                    HigherValues = new Node(newValue);
                    return;
                }
                throw new Exception($"Invalid attempt to set value, node='${Value}',  newValue='${newValue}', low='${LowerValues}', high='${HigherValues}'");
            }

            // Get a requested node reference.
            public Node GetNodeForPath(Path path) {
                if(path == Path.Back) {
                    throw new Exception("Invalid use of Path.Back");
                }
                return
                    path == Path.Lower ? LowerValues :
                    path == Path.Higher ? HigherValues :
                    path == Path.Current ? this :
                    throw new Exception("This should not have happened");
            }

            // This is only used to build exception messages.
            public override String ToString() {
                return $"Node:${Value}";
            }
        }
    }
}
