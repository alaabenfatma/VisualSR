/*Copyright 2018 ALAA BEN FATMA

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.*/
/*
 * Brain is an UNDO/REDO framework
 * It is still under exp
*/
//using System;
//using System.Collections.Generic;
//using System.Windows.Forms;
//using VisualSR.Tools;

//namespace VisualSR.Core
//{

//    public class VirtualStack<T> : Stack<T>
//    {
//        public Stack<T> RedoList = new Stack<T>();
//        public event EventHandler CollectionChanged;
//        public new Record Pop()
//        {
//            if (Count == 0) return null;
//            var x = base.Pop();
//            RedoList.Push(x);
//            OnCollectionChanged();
//            return x as Record;
//        }

//        public void Push(T item, bool redoing)
//        {
//            Push(item);
//            if (!redoing)
//                RedoList.Clear();
//            OnCollectionChanged();
//        }

//        protected virtual void OnCollectionChanged()
//        {
//            CollectionChanged?.Invoke(this, EventArgs.Empty);
//        }
//    }

//    public enum PossibleEvents
//    {
//        //VirtualControl related events
//        MoveNodes,
//        DeleteNode,
//        AddNode,
//        MakeAConnection,
//        DeleteAConnection,
//        AddAPin,
//        //VariablesList
//        AddAVariable,
//        DeleteAVarible,
//        RenameAVarible,
//        ChangeTypeOfAVariable,
//        //ContentsBrowser
//        MakeNewItem,
//        RenameItem,
//        DeleteItem,
//        //FunctionsList : not supported yet
//    }

//    public class UndoAddNode : Record
//    {

//        public UndoAddNode(VirtualControl host,Node Node) : base(() =>
//        {
//            Node.Delete(true);
//        }, () =>
//        {
//            var props = new NodeProperties(Node);
//            var capture = new CaptureData {Data = props};
//            var data = capture.Data as NodeProperties;
//            var typename = data.Name;
//            Node newNode = null;
//            foreach (var node in Hub.LoadedExternalNodes)
//            {
//                if (node.ToString() != typename) continue;
//                newNode = node.Clone();
//                host.AddNode(newNode, data.X, data.Y,true);
//                newNode.DeSerializeData(data.InputData, data.OutputData);
//                newNode.Id = data.Id;
//                break;
//            }
//            if (newNode != null) return;
//            var type = Type.GetType(typename);
//            if (type != null)
//            {
//                var instance = Activator.CreateInstance(type, host, false);
//                host.AddNode(instance as Node, data.X, data.Y,true);
//                var node = instance as Node;
//                if (node != null) node.Id = data.Id;
//            }
//        }, "Add Node")
//        {
//        }
//    }
//    public class CaptureData
//    {
//        public object Data;
//    }
//    public class Record
//    {

//        public Action Command;
//        public Action Opposite;
//        public string Description;
//        public Record(Action command,Action opposite, string description)
//        {
//            Command = command;
//            Opposite = opposite;
//            Description = description;
//            Opposite();
//        }


//        public void Undo()
//        {

//            Command();
//        }
//    }

//    public class Brain
//    {
//        /// <summary>
//        ///     This stack will contain all the previous states of a specific
//        ///     control based on some serialized data.
//        /// </summary>
//        public VirtualStack<Record> UndoList = new VirtualStack<Record>();

//        public void AddToUndo(Record item, bool redoing=false)
//        {
//            UndoList.Push(item, redoing);
//        }

//        public void Undo()
//        {
//            if (UndoList.Count == 0) return;
//            var x = UndoList.Pop();

//                x.Undo();
//        }

//        public void Redo()
//        {
//            if (UndoList.RedoList.Count == 0)
//            {
//                MessageBox.Show("redo empty.");
//                return;
//            }
//             AddToUndo(UndoList.RedoList.Peek() as Record, true);
//             UndoList.RedoList.Pop().Opposite();
//        }
//    }
//}

