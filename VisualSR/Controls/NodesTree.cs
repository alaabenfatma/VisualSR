/*Copyright 2018 ALAA BEN FATMA

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.*/
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using VisualSR.Core;
using VisualSR.Properties;
using VisualSR.Tools;

namespace VisualSR.Controls
{
    public class NodesTree : Control
    {
        private readonly List<string> _catNames = new List<string>
        {
            "Variables"
        };

        private readonly VirtualControl _host;
        private readonly PluginsManager _pluginsManager;
        private ObservableCollection<NodeItem> _backUpRoots = new ObservableCollection<NodeItem>();
        private List<Dictionary<int, string>> _categories;
        private TextBox _tb;
        private TreeView _tv;
        public new bool IsVisible;
        public ObservableCollection<NodeItem> Roots = new ObservableCollection<NodeItem>();

        public NodesTree(VirtualControl host)
        {
            Style = FindResource("NodesTree") as Style;
            _host = host;
            _pluginsManager = new PluginsManager(host);
            Task.Factory.StartNew(() =>
            {
                Dispatcher.BeginInvoke(DispatcherPriority.Send, new Action(() =>
                {
                    var loadedNewNodes = _pluginsManager.LoadPlugins();
                    if (!loadedNewNodes) return;
                    ClearAll();
                    BuildCategories();
                    BuildNodes();
                    RefreshBackUp();
                    Loaded += (o, e) =>
                    {
                        _tv = (TreeView) Template.FindName("Tv", this);
                        _tv.SelectedItemChanged += _tv_SelectedItemChanged;
                        _tb = (TextBox) Template.FindName("SearchTextBox", this);
                        _tb.TextChanged += (s, ev) =>
                            Filter(_tb.Text);
                        if (_tv.ItemsSource == null) _tv.ItemsSource = Roots;
                        _tb.PreviewKeyDown += OnPreviewKeyDown;
                        _tv.MouseDoubleClick += _tv_MouseDoubleClick;
                        ExpandAll();
                        _tb.Focus();
                    };
                }));
            });


            MouseLeave += (o, e) => Remove();
            Panel.SetZIndex(this, ZIndexes.NodesTreeIndex);
        }

        private NodeItem VariablesNode { get; set; }

        private void _tv_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var hostedNode = ((NodeItem) _tv.SelectedItem)?.HostedNode;
            if (hostedNode != null)
            {
                var node = hostedNode.Clone();
                var y = ((NodeItem) _tv.SelectedItem).NodeName;
                InsertNode(node);
                Remove();
            }
            e.Handled = true;
        }

        private void _tv_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            Task.Factory.StartNew(() =>
            {
                Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() =>
                {
                    if (e.OldValue != null)
                    {
                        var eOld = (NodeItem) e.OldValue;
                        eOld.ShowToolTip = false;
                    }
                    if (e.NewValue != null)
                    {
                        var eNew = (NodeItem) e.NewValue;

                        if (eNew.NodesCount == 0 && eNew.Description != "") eNew.ShowToolTip = true;
                    }
                }));
            });
        }

        private void BuildCategories()
        {
            _categories = new List<Dictionary<int, string>>();

            foreach (var node in _pluginsManager.LoadedNodes)
                if (!_catNames.Contains(node.Category))
                    _catNames.Add(node.Category);
            if (VariablesTreeRootIndex() != -1)
                _catNames.Remove("Variables");
            for (var index = 0; index < _catNames.Count; index++)
            {
                var name = _catNames[index];
                var dic = new Dictionary<int, string> {{index, name}};
                _categories.Add(dic);
            }
            for (var index = 0; index < _categories.Count; index++)
            {
                var item = _categories[index];
                var name = item[index];
                if (name != null) Roots.Add(new NodeItem("", name));
            }
        }

        private void BuildNodes()
        {
            Task.Factory.StartNew(() =>
            {
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    foreach (var node in _pluginsManager.LoadedNodes)
                        for (var index = 0; index < Roots.Count; index++)
                            if (node.Category == Roots[index].NodeName)
                                Roots[index].Nodes.Add(new NodeItem("", node.Clone()));
                }));
            });
        }

        public int VariablesTreeRootIndex()
        {
            for (var i = 0; i < Roots.Count; i++)
            {
                var root = Roots[i];
                if (root.NodeName == "Variables")
                    return i;
            }
            return -1;
        }


        public void RefreshBackUp()
        {
            var x = new ObservableCollection<NodeItem>(Roots.ToList());
            _backUpRoots.Clear();
            _backUpRoots = x;
        }

        private int GetTotalNodes(NodeItem nodes)
        {
            return nodes.NodesCount + nodes.Nodes.Sum(GetTotalNodes);
        }

        private void InsertNode(Node node)
        {
            Task.Factory.StartNew(() =>
            {
                Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Render, new Action(() =>
                {
                    _host.AddNode(node, Canvas.GetLeft(this), Canvas.GetTop(this));

                    node.Dispatcher.BeginInvoke(DispatcherPriority.Loaded, new Action(() =>
                    {
                        if (_host.TemExecPort != null && node.InExecPorts.Count > 0)
                            if (_host.TemExecPort.ConnectedConnectors.Count > 0)
                            {
                                string id1 = Guid.NewGuid().ToString(), id2 = Guid.NewGuid().ToString();

                                var thirdNode = _host.TemExecPort.ConnectedConnectors[0].EndPort.ParentNode;
                                NodesManager.CreateExecutionConnector(_host, _host.TemExecPort, node.InExecPorts[0],
                                    id1);
                                NodesManager.CreateExecutionConnector(_host, node.OutExecPorts[0],
                                    thirdNode.InExecPorts[0], id2);
                            }
                            else
                            {
                                NodesManager.CreateExecutionConnector(_host, _host.TemExecPort, node.InExecPorts[0]);
                            }
                        else if (_host.TemObjectPort != null && node.InputPorts.Count > 0)
                            NodesManager.CreateObjectConnector(_host, _host.TemObjectPort, node.InputPorts[0]);
                    }));
                }));
            });
        }

        private void OnPreviewKeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Enter:
                    var hostedNode = ((NodeItem) _tv.SelectedItem)?.HostedNode;
                    if (hostedNode != null)
                    {
                        var node = hostedNode.Clone();
                        var y = ((NodeItem) _tv.SelectedItem).NodeName;
                        InsertNode(node);
                        Remove();
                    }
                    e.Handled = true;
                    break;
                case Key.Left:
                    Collapse();
                    _tb.Focus();

                    e.Handled = true;
                    break;
                case Key.Right:
                    Expand();
                    _tb.Focus();

                    e.Handled = true;
                    break;
                case Key.Up:
                    SelectPrevious();
                    _tb.Focus();
                    e.Handled = true;
                    break;
                case Key.Down:
                    SelectNext();
                    _tb.Focus();
                    e.Handled = true;
                    break;
            }
        }

        public void Remove()
        {
            _host.Children.Remove(_host.TempConn);
            _host.Children.Remove(this);
            Task.Factory.StartNew(() =>
            {
                Dispatcher.BeginInvoke(DispatcherPriority.ApplicationIdle, new Action(() =>
                {
                    _tb.Clear();
                    IsVisible = false;
                    _host.NeedsRefresh = true;
                }));
            });
        }

        public void Show()
        {
            _host.ClearAllSelectedNodes();
            _host.AddChildren(this, Mouse.GetPosition(_host).X - 25, Mouse.GetPosition(_host).Y - 25);
            IsVisible = true;
        }

        private void ClearAll()
        {
            if (VariablesTreeRootIndex() != -1) VariablesNode = Roots[VariablesTreeRootIndex()];
            Roots.Clear();
            _backUpRoots.Clear();
            if (VariablesNode != null)
            {
                Roots.Add(VariablesNode);
                _backUpRoots.Add(VariablesNode);
            }
            _categories?.Clear();
        }

        private void Filter(string filterstring)
        {
            Task.Factory.StartNew(() =>
            {
                Dispatcher.BeginInvoke(DispatcherPriority.Input, new Action(() =>
                {
                    filterstring = filterstring.ToUpper();
                    foreach (var root in _backUpRoots)
                    {
                        if (root.BackUpNodes.Count == 0)
                        {
                            root.BackUpNodes = new List<NodeItem>();
                            foreach (var item in root.Nodes)
                                root.BackUpNodes.Add(item);
                        }
                        if (filterstring != "")
                        {
                            foreach (var item in root.BackUpNodes)
                                if (!item.NodeName.ToUpper().Contains(filterstring))
                                {
                                    root.Nodes.Remove(item);
                                }
                                else
                                {
                                    if (!item.NodeName.ToUpper().Contains(filterstring)) continue;
                                    root.Nodes.Remove(item);
                                    root.Nodes.Add(item);
                                }
                        }
                        else
                        {
                            root.Nodes.Clear();
                            foreach (var item in root.BackUpNodes)
                                root.Nodes.Add(item);
                        }
                    }
                    Roots.Sort();
                    SelectNext();
                    Expand();
                    _tb.Focus();
                }));
            });
        }

        public void ExpandAll()
        {
            var x = Roots.Count;

            for (var i = 0; i < x; i++)
                SelectNext();
            for (var i = x; i > 0; i--)
            {
                Expand();
                SelectPrevious();
            }
        }

        private void SelectNext()
        {
            _tv.Focus();
            var key = Key.Down;
            var target = Keyboard.FocusedElement;
            var routedEvent = Keyboard.KeyDownEvent;

            if (Keyboard.PrimaryDevice.ActiveSource != null)
                target.RaiseEvent(
                    new KeyEventArgs(
                            Keyboard.PrimaryDevice,
                            Keyboard.PrimaryDevice.ActiveSource,
                            0,
                            key)
                        {RoutedEvent = routedEvent}
                );
        }

        private void SelectPrevious()
        {
            _tv.Focus();
            var key = Key.Up;
            var target = Keyboard.FocusedElement;
            var routedEvent = Keyboard.KeyDownEvent;

            if (Keyboard.PrimaryDevice.ActiveSource != null)
                target.RaiseEvent(
                    new KeyEventArgs(
                            Keyboard.PrimaryDevice,
                            Keyboard.PrimaryDevice.ActiveSource,
                            0,
                            key)
                        {RoutedEvent = routedEvent}
                );
        }

        private void Expand()
        {
            _tv.Focus();
            var key = Key.Right;
            var target = Keyboard.FocusedElement;
            var routedEvent = Keyboard.KeyDownEvent;

            if (Keyboard.PrimaryDevice.ActiveSource != null)
                target.RaiseEvent(
                    new KeyEventArgs(
                            Keyboard.PrimaryDevice,
                            Keyboard.PrimaryDevice.ActiveSource,
                            0,
                            key)
                        {RoutedEvent = routedEvent}
                );
        }

        private void Collapse()
        {
            _tv.Focus();
            var key = Key.Left;
            var target = Keyboard.FocusedElement;
            var routedEvent = Keyboard.KeyDownEvent;

            if (Keyboard.PrimaryDevice.ActiveSource != null)
                target.RaiseEvent(
                    new KeyEventArgs(
                            Keyboard.PrimaryDevice,
                            Keyboard.PrimaryDevice.ActiveSource,
                            0,
                            key)
                        {RoutedEvent = routedEvent}
                );
        }
    }

    public class NodeItem : IComparable, INotifyPropertyChanged
    {
        private string _desc;
        private string _iconuri;

        private string _name;
        private bool _showToolTip;
        public List<NodeItem> BackUpNodes = new List<NodeItem>();
        public ObservableCollection<NodeItem> Nodes = new ObservableCollection<NodeItem>();

        public NodeItem(string icon, string name, string description = "")
        {
            NodeName = name;
            Description = description;
        }

        public NodeItem(string icon, Node node)
        {
            NodeName = node.Title;
            Description = node.Description;
            HostedNode = node;
        }

        public NodeItem(string icon, string name, Node node)
        {
            NodeName = name;
            Description = node.Description;
            HostedNode = node;
        }

        public bool ShowToolTip
        {
            get { return _showToolTip; }
            set
            {
                _showToolTip = value;
                OnPropertyChanged();
            }
        }

        public string Description
        {
            get { return _desc; }
            set
            {
                _desc = value;
                OnPropertyChanged();
            }
        }

        public int NodesCount => Nodes.Count;

        public Node HostedNode { get; set; }
        public bool HasIcon { get; set; } = true;

        public string Icon { get; set; }

        public string NodeName
        {
            get { return _name; }
            set
            {
                _name = value;
                OnPropertyChanged("NodeName");
            }
        }

        public IList Children => Nodes;

        public int CompareTo(object o)
        {
            var a = this;
            var b = (NodeItem) o;
            return b.NodesCount.CompareTo(a.NodesCount);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged(string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}