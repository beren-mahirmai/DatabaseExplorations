using System;
using System.Collections;
using System.Collections.Generic;

namespace DBLib
{
    // Contains a set of key/value pairs, sorted by the key which is a string.
    // This is a recursive algorithm, but recursion is bad in languages without tail call optimization.
    // We're using an iterative approach to avoid OutOfMemoryException in very large sets.
    public class BinarySearchTree
    {
        private readonly IComparer<String> Comparer = StringComparer.CurrentCultureIgnoreCase;
        private Node Root = null;

        public Int32 Count { get; private set; } = 0;

        public BinarySearchTree() { }

        // Adding to the tree is a recursive algorithm.  For each node compare the key being added 
        // with the current key, then step down either the 'low' or 'high' path in the tree to the
        // next node and try again.  If we need to go down a path, but the path is null, create a new
        // node there with the path in question.
        // If an attempt to add a duplicate is made, we eventually find it and return, resulting in
        // a no-op.
        public void Set(String newKey, Object newValue) {
            if(Root == null) {
                Count = 1;
                Root = new Node(newKey, newValue);
                return;
            } 
            Node current = Root;
            while(true) {
                Path path = GetPathForValueOnNode(current, newKey);
                if(path == Path.Current) { // we found a match, so update the node
                    current.Value = newValue;
                    break;
                }
                Node nextNode = current.GetNodeForPath(path);
                if(nextNode == null) {
                    // This is the path the key should go on, but it is empty, so that's the node to set
                    current.SetValueForPath(newKey, newValue, path); 
                    Count++;
                    break;   
                }
                current = nextNode;
            }
        }

        // Checking the tree for a key is basically the same algorithm as adding, except we 
        // report true if we find a node with the key and false if we get to null branch
        // withiout encountering it.
        public Boolean Contains(String key) {
            Node current = Root;
            while(current != null) {
                Path path = GetPathForValueOnNode(current, key);
                if(path == Path.Current) {
                    return true;
                }
                current = current.GetNodeForPath(path);
            }
            return false;
        }

        // Retrieve a value stored in the BinarySearchTree based on it's key.
        public T Get<T>(String key) {
            Node current = Root;
            while(current != null) {
                Path path = GetPathForValueOnNode(current, key);
                if(path == Path.Current) {
                    return (T) current.Value;
                }
                current = current.GetNodeForPath(path);
            }
            throw new Exception($"'{key}' not found");
        }

        // This takes the pairs, which are sorted in the tree but may be scattered about the
        // heap, and walks the tree and copies the pairs into an array in the correct order.
        public KeyValuePair<String, Object>[] ToSortedArray() {
            var result = new KeyValuePair<String, Object>[Count];
            if(Root == null) {
                return result;
            }
            Int32 index = 0;
            var nodeStack = new Stack<Node>();
            nodeStack.Push(Root);
            while(index < Count) {
                Path next = DeterminePath(nodeStack.Peek(), index >= 1 ? result[index-1].Key : null);
                if(next == Path.Lower) {
                    nodeStack.Push(nodeStack.Peek().LowerValues);
                } else if (next == Path.Higher) {
                    nodeStack.Push(nodeStack.Peek().HigherValues);
                } else if (next == Path.Current) {
                    Node currentNode = nodeStack.Peek();
                    result[index] = new KeyValuePair<String, Object>(currentNode.Key, currentNode.Value);
                    index++;
                } else { // next == Path.Back
                    nodeStack.Pop();
                }
            }
            return result;
        }

        // While walking a tree to find a key x, decide if we should look on the low or high branch,
        // or let us know if the supplied node is an exact match.
        private Path GetPathForValueOnNode(Node node, String testKey) {
            Int32 compareResult = Comparer.Compare(node.Key, testKey);
            return
                compareResult > 0 ? Path.Lower :
                compareResult < 0 ? Path.Higher :
                Path.Current;
        }

        // This method is intended to support ToSortedArray() by encapsulating the logic of deciding
        // whether to use the current key or check one of the branches further down the tree.
        // The 'testValue' is the key most recently written to the output array, we can assume
        // that anything equal to lower than that is already in the output array.  So between the
        // Lower branch, key, and higher branch for a node we always choose the lowest key that
        // is higher than 'testValue'.  If 'testValue' is higher than all three, we back out a layer.
        private Path DeterminePath(Node node, String testKey) {
            return 
                node.LowerValues != null && testKey == null ? Path.Lower : // LowerValues path exists and we haven't settled on a first value yet
                node.LowerValues != null && Comparer.Compare(node.LowerValues.Key, testKey) > 0 ? Path.Lower : // LowerValues path exists and it hasn't been traversed yet (higher than test value)
                node.LowerValues == null && testKey == null ? Path.Current : // No LowerValues and we haven't picked the first item, so this is that item
                Comparer.Compare(node.Key, testKey) > 0 ? Path.Current : // No LowerValues or LowerValues are already traversed and we haven't added the current item yet
                node.HigherValues == null ? Path.Back : // We have already handled the LowerValues path and current value, and there is no higher path
                node.HigherValues != null && Comparer.Compare(node.HigherValues.Key, testKey) > 0 ? Path.Higher : // There is a higher path and we haven't traversed it yet
                Path.Back; // We have already traversed the low, current, and high paths
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
            public readonly String Key;
            public Object Value;
            public Node LowerValues {get; private set; }
            public Node HigherValues {get; private set;}

            public Node(String key, Object value) {
                Key = key;
                Value = value;
            }

            // Sets the higher or lower value, assuming that subtree is null.
            public void SetValueForPath(String newKey, Object newValue, Path path) {
                if(path == Path.Lower && LowerValues == null) {
                    LowerValues = new Node(newKey, newValue);
                    return;
                }

                if(path == Path.Higher && HigherValues == null) {
                    HigherValues = new Node(newKey, newValue);
                    return;
                }
                throw new Exception($"Invalid attempt to set value, node='${Key}',  newKey='${newKey}', low='${LowerValues}', high='${HigherValues}'");
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
                return $"Node:${Key}";
            }
        }
    }
}
