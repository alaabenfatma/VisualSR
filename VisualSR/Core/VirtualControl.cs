/*Copyright 2018 ALAA BEN FATMA

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.*/
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using VisualSR.BasicNodes;
using VisualSR.Controls;
using VisualSR.Tools;

namespace VisualSR.Core
{
    public class VirtualControlData
    {
        private readonly List<VolatileConnector> VolatileConnectors = new List<VolatileConnector>();
        public List<ExecutionConnectorProperties> ExecutionConnectors = new List<ExecutionConnectorProperties>();
        public List<NodeProperties> Nodes = new List<NodeProperties>();
        public List<ObjectConnectorProperties> ObjectConnectors = new List<ObjectConnectorProperties>();
        public List<SpaghDividerProperties> SpaghettiDividers = new List<SpaghDividerProperties>();

        public VirtualControlData()
        {
        }

        public VirtualControlData(VirtualControl vc)
        {
            for (var index = vc.Nodes.Count - 1; index >= 0; index--)
            {
                var node = vc.Nodes[index];
                if (node.Types != NodeTypes.Root && node.Types != NodeTypes.SpaghettiDivider)
                {
                    Nodes.Add(new NodeProperties(node));
                }
                else if (node.Types == NodeTypes.SpaghettiDivider)
                {
                    SpaghettiDividers.Add(new SpaghDividerProperties(node));
                    VolatileConnectors.Add(new VolatileConnector
                    {
                        X = node.X,
                        Y = node.Y,
                        Connector = ((SpaghettiDivider) node).ObjectConnector
                    });
                    (node as SpaghettiDivider).Delete();
                }
            }
            foreach (var conn in vc.ObjectConnectors)
                ObjectConnectors.Add(new ObjectConnectorProperties(conn));
            foreach (var conn in vc.ExecutionConnectors)
                ExecutionConnectors.Add(new ExecutionConnectorProperties(conn));

            foreach (var conn in VolatileConnectors)
            {
                var divider = new SpaghettiDivider(vc, conn.Connector, false);
                for (var index = 0; index < vc.ObjectConnectors.Count; index++)
                {
                    var c = vc.ObjectConnectors[index];
                    if (c == conn.Connector)
                        c.Delete();
                }
                conn.Connector.EndPort.ConnectedConnectors.ClearConnectors();
                vc.AddNode(divider, conn.X, conn.Y);
            }
        }

        private struct VolatileConnector
        {
            public double X;
            public double Y;
            public ObjectsConnector Connector;
        }
    }

    public enum VirtualControlType
    {
        Main,
        Function
    }

    public class VirtualControl : CanvasCamera
    {
        private readonly MouseClickEffect _mouseEffect = new MouseClickEffect();
        private readonly List<NodeProperties> CopiedNodes = new List<NodeProperties>();
        public readonly InnerLinkingMessageBox InnerMsg = new InnerLinkingMessageBox();
        public readonly NodesTree NodesTree;
        private readonly Search SearchForm;
        private Border _selectionZone;
        private Point _startPoint;
        private Node[] _tempNodes;
        private bool _wiresDisabled;
        private Point CenterOfGravity;
        public IList<Comment> Comments = new List<Comment>();
        public List<ExecutionConnector> ExecutionConnectors = new List<ExecutionConnector>();
        public bool NeedsRefresh;
        public IList<Node> Nodes = new List<Node>();
        public List<ObjectsConnector> ObjectConnectors = new List<ObjectsConnector>();
        public IList<VariableItem> Variables = new List<VariableItem>();

        public VirtualControl()
        {
            KeyboardNavigation.SetDirectionalNavigation(this, KeyboardNavigationMode.None);
            PreviewMouseDown += OnMouseDown;
            MouseMove += OnMouseMove;
            MouseUp += OnMouseUp;
            PreviewKeyDown += OnKeyDown;
            ContextMenu = VirtualContextMenu();
            AddChildren(InnerMsg);
            SelectedNodes.CollectionChanged += SelectedUiElementsOnCollectionChanged;
            AllowDrop = true;
            Drop += VirtualControl_Drop;
            var init = false;
            GotFocus += (s, e) => Hub.CurrentHost = this;
            SearchForm = new Search(this);
            Loaded += (sender, args) =>
            {
                if (!init)
                {
                    InitRoot();
                    init = true;
                }
            };
            NodesTree = new NodesTree(this);
        }

        public Node RootNode { get; set; }

        public Wire TempConn { get; set; }

        public ExecPort TemExecPort { get; set; }

        public ObjectPort TemObjectPort { get; set; }

        public Comment TempComment { get; set; }
        private Point BasisOrigin => new Point(ActualWidth / 2, ActualHeight / 2);

        private void VirtualControl_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                var files = (string[]) e.Data.GetData(DataFormats.FileDrop);

                for (var index = 0; index < files.Length; index++)
                {
                    var file = files[index];
                    var node = new ReadFile(this);
                    node.OutputPorts[0].Data.Value = file;
                    AddChildren(node, ActualWidth / 2, ActualHeight / 2);
                    node.X = node.X - node.ActualWidth;
                }
            }
        }

        public string SerializeAll()
        {
            var data = new VirtualControlData(this);
            return Cipher.SerializeToString(data);
        }

        private void SelectedUiElementsOnCollectionChanged(object sender,
            NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs)
        {
            if (notifyCollectionChangedEventArgs.NewItems == null) return;
            foreach (Node node in notifyCollectionChangedEventArgs.NewItems)
            {
                node.IsSelected = true;
                node.Focus();
            }
        }

        public Node GetNode(string guid)
        {
            foreach (var node in Nodes)
                if (node.Id == guid)
                    return node;
            return null;
        }

        #region  GameControllers

        private void OnKeyDown(object sender, KeyEventArgs keyEventArgs)
        {
            switch (keyEventArgs.Key)
            {
                case Key.Down:
                    for (var index = 0; index < SelectedNodes.Count; index++)
                    {
                        var node = SelectedNodes[index];
                        node.Y = node.Y + 5;
                        node.Focus();
                    }
                    break;
                case Key.Up:
                    for (var index = 0; index < SelectedNodes.Count; index++)
                    {
                        var node = SelectedNodes[index];
                        node.Y = node.Y - 5;
                        node.Focus();
                    }
                    break;
                case Key.Left:
                    for (var index = 0; index < SelectedNodes.Count; index++)
                    {
                        var node = SelectedNodes[index];
                        node.X = node.X - 5;
                        node.Focus();
                    }
                    break;
                case Key.Right:
                    for (var index = 0; index < SelectedNodes.Count; index++)
                    {
                        var node = SelectedNodes[index];
                        node.X = node.X + 5;
                        node.Focus();
                    }
                    break;
                case Key.Delete:
                    if (SelectedNodes.Contains(RootNode))
                        SelectedNodes.Remove(RootNode);
                    for (var index = 0; index < SelectedNodes.Count; index++)
                    {
                        var node = SelectedNodes[index];
                        Nodes.Remove(node);
                        if (node.IsCollapsed == false)
                            node.Delete();
                    }
                    break;

                case Key.C:
                    if (Keyboard.IsKeyDown(Key.LeftCtrl) && Keyboard.IsKeyDown(Key.LeftShift))
                    {
                        //This code is still under exp
                        //var collapsedRegion = new NodesContainer(this, SelectedNodes.ToList());
                    }
                    if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
                        Copy();
                    break;
                case Key.V:
                    if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
                        Paste();
                    break;
                case Key.Subtract:
                    if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
                    {
                        ZoomOut();
                        keyEventArgs.Handled = true;
                    }
                    break;
                case Key.Add:
                    if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
                    {
                        ZoomIn();
                        keyEventArgs.Handled = true;
                    }
                    break;
                case Key.A:
                    SelectedNodes.Clear();
                    if (Keyboard.FocusedElement is Node)

                        if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
                            if (Nodes != null)
                                for (var index = 0; index < Nodes.Count; index++)
                                {
                                    var node = Nodes[index];
                                    node.Focus();

                                    SelectedNodes.Add(node);
                                }

                    break;
                case Key.F:
                    if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
                        SearchForm.Show();
                    break;
                case Key.X:
                    if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
                    {
                        Copy();
                        DeleteSelected();
                    }
                    break;
                case Key.Escape:
                    if (Keyboard.FocusedElement is Node)
                        if (SelectedNodes.Count != 0)
                        {
                            for (var index = 0; index < SelectedNodes.Count; index++)
                            {
                                var node = SelectedNodes[index];
                                node.IsSelected = false;
                            }
                            SelectedNodes.Clear();
                        }

                    break;
            }
        }

        #endregion

        private void InitRoot()
        {
            var startNode = new StartNode(this);
            RootNode = startNode;
            RootNode.Id = "0";
            Task.Factory.StartNew(() =>
            {
                Dispatcher.BeginInvoke(DispatcherPriority.Render, new Action(() => { GoForNode(RootNode); }));
            });
        }

        public void ClearAllSelectedNodes()
        {
            for (var index = 0; index < SelectedNodes.Count; index++)
            {
                var node = SelectedNodes[index];
                node.IsSelected = false;
            }
            SelectedNodes.Clear();
        }

        private void OnMouseDown(object sender, MouseButtonEventArgs mouseButtonEventArgs)
        {
            if (Nodes?.Count > 0) Keyboard.Focus(Nodes[0]);
            if (mouseButtonEventArgs.ChangedButton == MouseButton.Left && mouseButtonEventArgs.ClickCount == 2)
            {
                if (!Children.Contains(NodesTree))
                    NodesTree.Show();
                MouseMode = MouseMode.PreSelectionRectangle;
            }
            else if (mouseButtonEventArgs.LeftButton == MouseButtonState.Pressed)
            {
                _startPoint = mouseButtonEventArgs.GetPosition(this);

                if (Children.Contains(_selectionZone))
                    Children.Remove(_selectionZone);

                _selectionZone = new Border
                {
                    BorderBrush = Brushes.WhiteSmoke,
                    BorderThickness = new Thickness(2),
                    CornerRadius = new CornerRadius(6)
                };

                SetLeft(_selectionZone, _startPoint.X);
                SetTop(_selectionZone, _startPoint.X);

                MouseMode = MouseMode.SelectionRectangle;
                WireMode = WireMode.Nothing;
            }
        }

        private void OnMouseMove(object sender, MouseEventArgs mouseEventArgs)
        {
            if (WireMode == WireMode.FirstPortSelected && MouseMode == MouseMode.DraggingPort)
            {
                TempConn.EndPoint = new Point(mouseEventArgs.GetPosition(this).X - 1,
                    mouseEventArgs.GetPosition(this).Y);
                if (_wiresDisabled) return;
                for (var index = 0; index < Children.Count - 1; index++)
                {
                    var uielement = Children[index];
                    if (uielement is Wire)
                        MagicLaboratory.GrayWiresOut(uielement as Wire);
                }
                _wiresDisabled = true;
                return;
            }
            if (mouseEventArgs.LeftButton == MouseButtonState.Released)
                if (_wiresDisabled)
                {
                    for (var index = 0; index < Children.Count - 1; index++)
                    {
                        var uielement = Children[index];
                        if (uielement is Wire)
                            MagicLaboratory.GrayWiresOut_Reverse(uielement as Wire);
                    }
                    _wiresDisabled = false;
                }

            if (mouseEventArgs.LeftButton != MouseButtonState.Released && MouseMode == MouseMode.SelectionRectangle)
            {
                if (!Children.Contains(_selectionZone))
                    AddChildren(_selectionZone);
                var pos = mouseEventArgs.GetPosition(this);
                var x = Math.Min(pos.X, _startPoint.X);
                var y = Math.Min(pos.Y, _startPoint.Y);
                var w = Math.Max(pos.X, _startPoint.X) - x;
                var h = Math.Max(pos.Y, _startPoint.Y) - y;
                _selectionZone.Width = w;
                _selectionZone.Height = h;
                SetLeft(_selectionZone, x);
                SetTop(_selectionZone, y);
                SelectionZoneWorkerOnDoWork();
                return;
            }
            if (MouseMode == MouseMode.ResizingComment && mouseEventArgs.LeftButton == MouseButtonState.Pressed)
            {
                Cursor = Cursors.SizeNWSE;
                var currentPoint = Mouse.GetPosition(this);
                if (currentPoint.Y - TempComment.Y > 0 && currentPoint.X - TempComment.X > 0)
                {
                    TempComment.Height = /*TempComment.Top +*/ currentPoint.Y - TempComment.Y;
                    TempComment.Width = /* TempComment.Left +*/ currentPoint.X - TempComment.X;
                    TempComment.LocateHandler();
                }
                else
                {
                    TempComment.Height = 32;
                    TempComment.Width = 32;
                    TempComment.LocateHandler();
                }
                return;
            }
            if (NeedsRefresh)
            {
                foreach (var node in Nodes)
                    node.Refresh();

                TemExecPort = null;
                TemObjectPort = null;
                NeedsRefresh = false;
            }
        }

        private void OnMouseUp(object sender, MouseButtonEventArgs mouseButtonEventArgs)
        {
            if (MouseMode == MouseMode.SelectionRectangle && Children.Contains(_selectionZone))
            {
                Children.Remove(_selectionZone);
                SelectedNodes.Clear();
                for (var index = 0; index < Nodes.Count; index++)
                {
                    var node = Nodes[index];
                    if (node.IsSelected) SelectedNodes.Add(node);
                }

                MouseMode = MouseMode.Nothing;
            }
            if (MouseMode == MouseMode.ResizingComment)
            {
                TempComment.Cursor = Cursors.Arrow;
                TempComment = null;
            }
            HideLinkingPossiblity();
            Cursor = Cursors.Arrow;
            if (WireMode == WireMode.FirstPortSelected && (TemExecPort != null || TemObjectPort != null))
            {
                NodesTree.Show();
                MouseMode = MouseMode.Nothing;
                WireMode = WireMode.Nothing;
            }
            else
            {
                Children.Remove(TempConn);
                MouseMode = MouseMode.Nothing;
                WireMode = WireMode.Nothing;
            }

            _mouseEffect.Show(this, Colors.DarkGray);
            Task.Factory.StartNew(() =>
            {
                Thread.Sleep(300);
                Application.Current.Dispatcher.BeginInvoke(new Action(() => { _mouseEffect.Remove(this); }));
            });
        }

        private void SelectionZoneWorkerOnDoWork()
        {
            Task.Factory.StartNew(() =>
            {
                Dispatcher.BeginInvoke(DispatcherPriority.Render, new Action(() =>
                {
                    foreach (var node in Nodes)
                        if (node.IsCollapsed == false)
                            if (_selectionZone != null)
                                if (node.X >= GetLeft(_selectionZone) &&
                                    node.X + node.ActualWidth <= GetLeft(_selectionZone) + _selectionZone.Width &&
                                    node.Y >= GetTop(_selectionZone) &&
                                    node.Y + node.ActualHeight <= GetTop(_selectionZone) + _selectionZone.Height)
                                    node.IsSelected = true;
                                else
                                    node.IsSelected = false;
                            else
                                node.IsSelected = false;
                }));
            });
        }

        public void ShowLinkingPossiblity(int possible, Point location)
        {
            InnerMsg.Visibility = Visibility.Visible;
            switch (possible)
            {
                case 0:
                    InnerMsg.MessageIcon = InnerLinkingMessageBox.InnerMessageIcon.False;
                    InnerMsg.Text = "Unlinkable.";
                    break;
                case 1:
                    InnerMsg.MessageIcon = InnerLinkingMessageBox.InnerMessageIcon.Correct;
                    InnerMsg.Text = "Linkable.";
                    break;
                case 2:
                    InnerMsg.MessageIcon = InnerLinkingMessageBox.InnerMessageIcon.Warning;
                    InnerMsg.Text = "Linkable (may require a cast).";
                    break;
            }


            SetTop(InnerMsg, location.Y);
            SetLeft(InnerMsg, location.X + 10);
            SetZIndex(InnerMsg, ZIndexes.AboveAllIndex);
        }

        public void HideLinkingPossiblity()
        {
            InnerMsg.Visibility = Visibility.Collapsed;
        }

        public void GoForNode(Node node)
        {
            node.X = Math.Truncate(node.X);
            node.Y = Math.Truncate(node.Y);
            var origin = BasisOrigin;
            origin.X -= node.ActualWidth - 10;
            origin.X = Math.Truncate(origin.X);
            origin.Y = Math.Truncate(origin.Y);
            var difference = Point.Subtract(origin, new Point(node.X, node.Y));
            var timer = new DispatcherTimer {Interval = new TimeSpan(0, 0, 0, 0, 01)};
            foreach (var n in Nodes)
            {
                if (difference.X < 0)
                    n.X += difference.X;
                else
                    n.X += difference.X;
                if (difference.Y < 0)
                    n.Y += difference.Y;
                else
                    n.Y += difference.Y;
                NeedsRefresh = true;
            }
            difference.X = 10;
            difference.Y = 0;
            if (difference.X != 0 || difference.Y != 0)
                timer.Start();
            timer.Tick += (s, e) =>
            {
                if (difference.X == 0 && difference.Y == 0)
                    timer.Stop();
                foreach (var n in Nodes)
                {
                    if (difference.X > 0)
                        n.X++;
                    else
                        n.X--;
                    if (difference.Y > 0)
                        n.Y++;
                    else
                        n.Y--;
                }
                if (difference.X > 0)
                    difference.X--;
                else
                    difference.X++;
                if (difference.Y > 0)
                    difference.Y--;
                else
                    difference.Y++;
            };
            SelectedNodes.Clear();
            SelectedNodes.Add(node);
        }

        public void Copy()
        {
            CopiedNodes.Clear();
            for (var index = SelectedNodes.Count - 1; index >= 0; index--)
            {
                var node = SelectedNodes[index];
                if (node.Types == NodeTypes.Root) continue;
                CopiedNodes.Add(new NodeProperties(node));
            }
            Clipboard.SetText(Cipher.SerializeToString(CopiedNodes));
        }

        public void Paste()
        {
            try
            {
                SelectedNodes.Clear();
                var dummyList = Cipher.DeSerializeFromString<List<NodeProperties>>(Clipboard.GetText());
                for (var index = dummyList.Count - 1; index >= 0; index--)
                {
                    var copiednode = dummyList[index];
                    var typename = copiednode.Name;
                    Node newNode = null;
                    foreach (var node in Hub.LoadedExternalNodes)
                    {
                        if (node.ToString() != typename) continue;
                        newNode = node.Clone();
                        AddNode(newNode, copiednode.X, copiednode.Y);
                        newNode.DeSerializeData(copiednode.InputData, copiednode.OutputData);
                        break;
                    }
                    if (newNode != null) continue;
                    var type = Type.GetType(typename);
                    try
                    {
                        var instance = Activator.CreateInstance(type, this, false);
                        AddNode(instance as Node, copiednode.X, copiednode.Y);
                    }
                    catch (Exception)
                    {
                        //Ignored
                    }
                }
            }
            catch
            {
                //ignored
            }
        }


        public void AddNode(Node child, double x, double y, bool addedByBrain = false)
        {
            AddChildren(child);
            SetLeft(child, x);
            SetTop(child, y);
            Nodes.Add(child);
            SelectedNodes.Clear();
            SelectedNodes.Add(child);
        }
        public void IndicateStatus()
        {
            var indicator = new Border
            {
                Width = ActualWidth,
                Height = ActualHeight,
                Background = Brushes.LawnGreen,
                Opacity = 0.1
            };
            AddChildren(indicator, 0, 0);
        }
        public void AddVariable(ref VariableItem variable, double x, double y)
        {
            var gsMenu = new GetSetMenu(ref variable) {Host = this};
            AddChildren(gsMenu, x, y);
        }
        private void DeleteSelected()
        {
            for (var index = 0; index < SelectedNodes.Count; index++)
            {
                var node = SelectedNodes[index];
                node.Delete();
            }
        }
        private ContextMenu VirtualContextMenu()
        {
            var cm = new ContextMenu();
            var add = new TextBlock {Text = "Add Node", Foreground = Brushes.WhiteSmoke};
            var addNode = new MenuItem {Header = add};
            addNode.Click += (s, e) => { NodesTree.Show(); };
            var delete = new TextBlock {Text = "Delete Selected", Foreground = Brushes.WhiteSmoke};
            var deleteNodes = new MenuItem {Header = delete};
            deleteNodes.Click += (s, e) => { DeleteSelected(); };
            var cut = new TextBlock {Text = "Cut Selected", Foreground = Brushes.WhiteSmoke};
            var cutNodes = new MenuItem {Header = cut};
            cutNodes.Click += (s, e) =>
            {
                Copy();
                DeleteSelected();
            };
            var copy = new TextBlock {Text = "Copy Selected", Foreground = Brushes.WhiteSmoke};
            var copyNodes = new MenuItem {Header = copy};
            copyNodes.Click += (s, e) => { Copy(); };
            var paste = new TextBlock {Text = "Paste Selected", Foreground = Brushes.WhiteSmoke};
            var pasteNodes = new MenuItem {Header = paste};
            pasteNodes.Click += (s, e) => { Paste(); };
            var comment = new TextBlock {Text = "Comment Selected", Foreground = Brushes.WhiteSmoke};
            var commentNodes = new MenuItem {Header = comment};
            commentNodes.Click += (s, e) =>
            {
                if (SelectedNodes.Count > 0)
                {
                    var comm = new Comment(SelectedNodes, this);
                }
            };
            cm.Items.Add(addNode);
            cm.Items.Add(deleteNodes);
            cm.Items.Add(new Separator {Foreground = Brushes.White});
            cm.Items.Add(cutNodes);
            cm.Items.Add(copyNodes);
            cm.Items.Add(pasteNodes);
            cm.Items.Add(new Separator {Foreground = Brushes.White});
            cm.Items.Add(commentNodes);
            return cm;
        }
    }

    public class GetSetMenu : Control
    {
        public GetSetMenu(ref VariableItem variable)
        {
            Style = (Style) FindResource("GetSetMenu");
            var item = variable;
            Loaded += (e, i) =>
            {
                ApplyTemplate();
                Variable = item;
                Get = (Button) Template.FindName("GetButton", this);
                Set = (Button) Template.FindName("SetButton", this);
                Get.Click += Get_Click;
                Set.Click += Set_Click;
                MouseLeave += (s, ee) =>
                    Host.Children.Remove(this);
            };
        }

        public VirtualControl Host { get; set; }
        private VariableItem Variable { get; set; }
        private Button Get { get; set; }
        private Button Set { get; set; }
        private Node Node { get; set; }
        private void Set_Click(object sender, RoutedEventArgs e)
        {
            Node = new Set(Host, Variable, false);
            Host.AddNode(Node, Canvas.GetLeft(this), Canvas.GetTop(this));
            Host.Children.Remove(this);
        }
        private void Get_Click(object sender, RoutedEventArgs e)
        {
            Node = new Get(Host, Variable, false);
            Host.AddNode(Node, Canvas.GetLeft(this), Canvas.GetTop(this));
            Host.Children.Remove(this);
        }
    }
}