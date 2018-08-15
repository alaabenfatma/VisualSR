/*Copyright 2018 ALAA BEN FATMA

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.*/
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Xml.Serialization;
using VisualSR.Controls;
using VisualSR.Properties;
using VisualSR.Tools;

namespace VisualSR.Core
{
    public enum NodeSymbols
    {
        Function,
        Variable,
        Event,
        Code
    }

    public enum NodeTypes
    {
        Root,
        Return,
        Basic,
        Event,
        Method,
        Function,
        VariableGet,
        VariableSet,
        Code,
        SpaghettiDivider,
        CollapsedRegion
    }


    public class Node : Control, INotifyPropertyChanged
    {
        //The host of the node
        public readonly VirtualControl Host;

        /// <summary>
        ///     The list of the <c>input</c> execution ports
        /// </summary>
        public readonly List<ExecPort> InExecPorts = new List<ExecPort>();

        /// <summary>
        ///     The list of the <c>input</c> ports
        /// </summary>
        public readonly List<ObjectPort> InputPorts = new List<ObjectPort>();

        /// <summary>
        ///     The list of the <c>Output</c> execution ports
        /// </summary>
        public readonly List<ExecPort> OutExecPorts = new List<ExecPort>();

        /// <summary>
        ///     The list of the <c>output</c> ports
        /// </summary>
        public readonly List<ObjectPort> OutputPorts = new List<ObjectPort>();

        private Border _backgroundBorder = new Border();

        private string _code;

        private bool _isselected;

        private string _title;

        public StackPanel InExecPortsPanel,
            OutExecPortsPanel,
            InputPortsPanel,
            OutputPortsPanel,
            InputPortsControls;

        public bool IsCollapsed = false;


        /// <summary>
        ///     The current position of the node
        /// </summary>
        public Point Position = new Point();

        //private readonly DependencyPropertyDescriptor LeftTrigger
        //    = DependencyPropertyDescriptor.FromProperty(
        //        Canvas.LeftProperty, typeof(Node)
        //    );


        /// <summary>
        ///     The node that will contain the ports and data
        /// </summary>
        /// <param name="host"></param>
        /// <param name="type"></param>
        /// <param name="spontaneousAddition"></param>
        public Node(VirtualControl host, NodeTypes type, bool spontaneousAddition = true)
        {
            Types = type;
            Host = host;
            Style = (Style) FindResource("NodeStyle");
            switch (type)
            {
                case NodeTypes.Event:
                    Background = Brushes.Red;
                    break;
                case NodeTypes.Basic:
                    Background = Brushes.Transparent;
                    break;
                case NodeTypes.Method:
                    Background = Brushes.DeepSkyBlue;
                    break;
                case NodeTypes.Function:
                    Background = Brushes.LightGreen;
                    break;
                case NodeTypes.VariableGet:
                    Background = Brushes.Black;
                    break;
                case NodeTypes.VariableSet:
                    Background = Brushes.LightGray;
                    break;
                case NodeTypes.Root:
                    break;
                case NodeTypes.Code:
                    break;
                case NodeTypes.SpaghettiDivider:
                    break;
                case NodeTypes.CollapsedRegion:
                    break;
            }
            var init = false;
            Loaded += (sender, args) =>
            {
                if (!init)
                {
                    FuncNodeInit();
                    init = true;
                }
            };
            Panel.SetZIndex(this, ZIndexes.NodeIndex);
            KeyboardNavigation.SetDirectionalNavigation(this, KeyboardNavigationMode.None);
            if (spontaneousAddition)
                host.AddNode(this, Mouse.GetPosition(host).X - 5, Mouse.GetPosition(host).Y - 5,
                    type == NodeTypes.Root);

            Id = Guid.NewGuid().ToString();
            Console.WriteLine(Id);
            IsVisibleChanged += Node_IsVisibleChanged;
            MouseDown += Node_MouseDown;
        }

        public Node()
        {
        }

        public string Category { get; set; } = "Misc";

        //private readonly DependencyPropertyDescriptor TopTrigger
        //    = DependencyPropertyDescriptor.FromProperty(
        //        Canvas.TopProperty, typeof(Node)
        //    );
        [XmlElement("Properties")]
        public NodeProperties NodeProperties { get; set; }

        public string Description { get; set; } = string.Empty;

        //The ID of the node
        public string Id { get; set; }


        /// <summary>
        ///     The possible type of the node
        ///     <remarks>Event</remarks>
        ///     <remarks>Function</remarks>
        ///     <remarks>VariableGet</remarks>
        ///     <remarks>VariableSet</remarks>
        ///     <remarks>Code</remarks>
        /// </summary>
        public NodeTypes Types { get; set; }

        /// <summary>
        ///     Determin if the node is selected or not
        /// </summary>
        public bool IsSelected
        {
            get { return _isselected; }
            set
            {
                _isselected = value;
                if (_backgroundBorder != null)
                {
                    _backgroundBorder.BorderBrush = _isselected ? Brushes.Gold : Brushes.Black;
                    _backgroundBorder.BorderThickness = new Thickness(2);
                }
            }
        }

        public string Title
        {
            get { return _title; }
            set
            {
                _title = value;
                OnPropertyChanged("Title");
            }
        }

        /// <summary>
        ///     The top coordinate of this element with respect to the host canvas.
        /// </summary>
        public double Y
        {
            get { return Canvas.GetTop(this); }
            set
            {
                Canvas.SetTop(this, value);

                OnPropertyChanged();
                OnPositionChanged();
            }
        }

        /// <summary>
        ///     The left coordinate of this element with respect to the host canvas.
        /// </summary>
        public double X
        {
            get { return Canvas.GetLeft(this); }
            set
            {
                Canvas.SetLeft(this, value);
                OnPropertyChanged();
                OnPositionChanged();
            }
        }

        public string Code
        {
            get { return _code; }
            set
            {
                _code = value;
                OnPropertyChanged("Code");
            }
        }

        public Control BottomControl { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        private void Node_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (Visibility == Visibility.Hidden || Visibility == Visibility.Collapsed)
                try
                {
                    //Force to hide
                    Hide();
                }
                catch (Exception exception)
                {
                    MessageBox.Show(exception.ToString());
                }
        }

        public event EventHandler PositionChanged;

        public void OnPositionChanged()
        {
            PositionChanged?.Invoke(this, new EventArgs());
        }

        private void Node_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (!Keyboard.IsKeyDown(Key.LeftCtrl) && Host.SelectedNodes.Count < 2)
            {
                foreach (var node in Host.SelectedNodes)
                    node.IsSelected = false;
                Host.SelectedNodes.Clear();
            }

            if (!Host.SelectedNodes.Contains(this))
                if (Keyboard.IsKeyDown(Key.LeftCtrl))
                {
                    Host.SelectedNodes.Add(this);
                }
                else
                {
                    foreach (var node in Host.SelectedNodes)
                        node.IsSelected = false;

                    Host.SelectedNodes.Clear();
                    Host.SelectedNodes.Add(this);
                }


            Host.MouseMode = MouseMode.Selection;
            Cursor = Cursors.Hand;
        }

        public virtual string GenerateCode()
        {
            return null;
        }

        public virtual string Serialize()
        {
            return null;
        }

        public virtual string DeSerialize()
        {
            return null;
        }


        /// <summary>
        ///     Adds an execution port to the node
        /// </summary>
        /// <param name="node"></param>
        /// <param name="name"></param>
        /// <param name="porttype"></param>
        /// <param name="text"></param>
        public void AddExecPort(Node node, string name, PortTypes porttype, string text)
        {
            var port = new ExecPort(Host, name, porttype, text, false);
            if (port.PortTypes == PortTypes.Input)
            {
                port.index = InExecPorts.Count;
                InExecPorts.Add(port);
            }
            else
            {
                port.index = OutExecPorts.Count;

                OutExecPorts.Add(port);
            }
            port.ParentNode = this;
        }

        /// <summary>
        ///     Adds an object port to the node
        /// </summary>
        /// <param name="node"></param>
        /// <param name="text">Title of the port</param>
        /// <param name="porttype">Type of the port (Input,Output)</param>
        /// <param name="type">Type of the data of the port</param>
        /// <para name="multicon">Multiple connections in one port</para>
        /// <param name="multiconnections"></param>
        /// <param name="control">The control that will host the data <c>optional</c></param>
        public void AddObjectPort(Node node, string text, PortTypes porttype, RTypes type, bool multiconnections,
            Control control)
        {
            var port = new ObjectPort(Host, porttype, text, type)
            {
                Control = control,
                MultipleConnectionsAllowed = multiconnections,
                ParentNode = this
            };
            if (port.PortTypes == PortTypes.Input)
            {
                port.Style = (Style) FindResource("InObjectPortStyle");
                port.index = InputPorts.Count;

                InputPorts.Add(port);
            }
            else
            {
                port.Style = (Style) FindResource("OutObjectPortStyle");
                port.index = OutputPorts.Count;

                OutputPorts.Add(port);
            }
        }

        public void AddObjectPort(Node node, string text, PortTypes porttype, RTypes type, bool multiconnections)
        {
            var port = new ObjectPort(Host, porttype, text, type)
            {
                MultipleConnectionsAllowed = multiconnections
            };
            port.ParentNode = node;
            if (port.PortTypes == PortTypes.Input)
            {
                InputPorts?.Add(port);
                InputPortsPanel?.Children.Add(port);
            }
            else
            {
                OutputPorts?.Add(port);
                OutputPortsPanel?.Children.Add(port);
            }
            OnPropertyChanged();
        }

        /// <summary>
        ///     Initializes the node and adds all the functionalities
        /// </summary>
        public void FuncNodeInit()
        {
            ApplyTemplate();
            _backgroundBorder = (Border) Template.FindName("BackgroundBorder", this);
            IsSelected = true;
            InExecPortsPanel = (StackPanel) Template.FindName("InExecPorts", this);
            OutExecPortsPanel = (StackPanel) Template.FindName("OutExecPorts", this);
            InputPortsPanel = (StackPanel) Template.FindName("InputPorts", this);
            InputPortsControls = (StackPanel) Template.FindName("InputPortsControl", this);
            OutputPortsPanel = (StackPanel) Template.FindName("OutputPorts", this);
            BottomControl = (Control) Template.FindName("BottomControl", this);
            foreach (var port in InExecPorts)
                InExecPortsPanel?.Children.Add(port);
            foreach (var port in OutExecPorts)
                OutExecPortsPanel?.Children.Add(port);
            foreach (var port in InputPorts)
                if (!InputPortsPanel.Children.Contains(port))
                    InputPortsPanel?.Children.Add(port);
            foreach (var port in OutputPorts)
                if (!OutputPortsPanel.Children.Contains(port))
                    try
                    {
                        OutputPortsPanel.Children.Add(port);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                        throw;
                    }
            var inPortsPanel = (StackPanel) Template.FindName("InputPorts", this);
            var outPortsPanel = (StackPanel) Template.FindName("OutputPorts", this);

            if (OutExecPortsPanel != null) OutExecPortsPanel.SizeChanged += (sender, args) => OnChangedSize();

            if (inPortsPanel != null) inPortsPanel.SizeChanged += (sender, args) => OnChangedSize();
            if (outPortsPanel != null) outPortsPanel.SizeChanged += (sender, args) => OnChangedSize();
            OnChangedSize();
        }

        public void Refresh()
        {
            X += 0.000001;
        }

        private void OnChangedSize()
        {
            var container = (Canvas) Template.FindName("Container", this);
            if (Types != NodeTypes.VariableGet)
                Width = InputPortsPanel.ActualWidth + OutExecPortsPanel.ActualWidth + OutputPortsPanel.ActualWidth +
                        InExecPortsPanel.ActualWidth + 10;
            else
                Width = OutputPortsPanel.ActualWidth + 10;
            var title = (Label) Template.FindName("NodeTitle", this);
            Height = 30;
            if (Types == NodeTypes.VariableGet)
            {
                Canvas.SetTop(OutputPortsPanel, -8);
                Height += 4;
                _backgroundBorder.BorderThickness = new Thickness(3);
                Width += 30;
                var hint = Template.FindName("NodeHint", this) as Path;
                if (hint != null) hint.Visibility = Visibility.Collapsed;
            }
            else if (Types != NodeTypes.VariableGet)
            {
                Height += OutputPortsPanel.ActualHeight > InputPortsPanel.ActualHeight
                    ? OutputPortsPanel.ActualHeight
                    : InputPortsPanel.ActualHeight;
                Height += OutExecPortsPanel.ActualHeight > InExecPortsPanel.ActualHeight
                    ? OutExecPortsPanel.ActualHeight
                    : InExecPortsPanel.ActualHeight;
                Canvas.SetTop(OutputPortsPanel, 25 + OutExecPortsPanel.ActualHeight);
                Canvas.SetTop(InputPortsPanel, 25 + OutExecPortsPanel.ActualHeight);
                if (InputPortsControls.Children.Count > 0)
                {
                    Height += InputPortsControls.ActualHeight;
                    Canvas.SetLeft(InputPortsControls,
                        ActualWidth - (OutExecPorts.Count != 0
                            ? OutputPortsPanel.ActualWidth
                            : OutExecPortsPanel.ActualWidth));
                }
                if (Types != NodeTypes.Root)
                    Canvas.SetLeft(title, container.ActualWidth / 2 - title.ActualWidth / 2);
            }
            else if (Types == NodeTypes.VariableSet)
            {
                Height -= 30;
                Canvas.SetTop(OutExecPortsPanel, -2);
                Canvas.SetTop(InExecPortsPanel, -2);
                Canvas.SetTop(InputPortsPanel, 25);
                Canvas.SetTop(title, 4);
                var hint = Template.FindName("NodeHint", this) as Path;
                if (hint != null) hint.Visibility = Visibility.Collapsed;
                Canvas.SetLeft(title, container.ActualWidth / 2 - title.ActualWidth / 2);
            }
            else if (Types == NodeTypes.Basic)
            {
                Height -= 20;
                title.Visibility = Visibility.Collapsed;
                Canvas.SetTop(InputPortsPanel, 4);
                Canvas.SetTop(OutputPortsPanel, 5);
            }
            else if (Types == NodeTypes.SpaghettiDivider)
            {
                Width = 30;
                Background = Brushes.Transparent;
                title.Visibility = Visibility.Collapsed;
                Canvas.SetLeft(InputPortsPanel, 3);
                Canvas.SetTop(InputPortsPanel, -9);
                Canvas.SetTop(OutputPortsPanel, -9);
                Canvas.SetLeft(OutputPortsPanel, 5);
                var hint = Template.FindName("NodeHint", this) as Path;
                if (hint != null) hint.Visibility = Visibility.Collapsed;
                title.Visibility = Visibility.Collapsed;
                Height -= 20;
                BottomControl = null;
                Height = 0;
            }
            if (BottomControl != null)
                Height += BottomControl.ActualHeight;
            Canvas.SetTop(InputPortsControls, Canvas.GetTop(OutputPortsPanel) + OutputPortsPanel.ActualHeight);
            Refresh();
            OnPropertyChanged();
        }

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged(string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public virtual Node Clone()
        {
            return null;
        }

        public virtual FoundItem Search(string key)
        {
            if (IsCollapsed) return null;
            var KEY = key.ToUpper();
            var fi = new FoundItem();
            if (Title.ToUpper().Contains(KEY))
            {
                fi.foundNode = this;
                fi.Hint = Title;
                fi.Type = ItemTypes.Node;
            }
            foreach (var port in InputPorts)
                if (port.IsVisible)
                {
                    if (port.Control is TextBox)
                    {
                        if (((TextBox) port.Control).Text.ToUpper().Contains(KEY))
                        {
                            if (fi.Hint != Title)
                                fi.Hint = Title;
                            fi.Items.Add(new FoundItem
                            {
                                Hint = $"   In input :{(port.Control as TextBox).Text}",
                                Type = ItemTypes.Port,
                                foundNode = this
                            });
                        }
                    }
                    else
                    {
                        var textBox = port.Control as UnrealControlsCollection.TextBox;
                        if (textBox == null || !textBox.Text.ToUpper().Contains(KEY)) continue;
                        var box = port.Control as UnrealControlsCollection.TextBox;
                        if (box != null)
                            if (fi.Hint != Title)
                                fi.Hint = Title;
                        fi.Items.Add(new FoundItem(port.Background)
                        {
                            Hint = $"In input :{box.Text}",
                            Type = ItemTypes.Port,
                            foundNode = this,
                            Brush = port.StrokeBrush
                        });
                    }
                }
                else
                {
                    var textBox = port.Control as UnrealControlsCollection.TextBox;
                    if (textBox == null || !textBox.Text.ToUpper().Contains(KEY)) continue;
                    var box = port.Control as UnrealControlsCollection.TextBox;
                    if (box != null)
                        if (fi.Hint != Title)
                            fi.Hint = Title;
                    fi.Items.Add(new FoundItem(port.Background)
                    {
                        Hint = $"In input :{box.Text}",
                        Type = ItemTypes.Port,
                        foundNode = this,
                        Brush = port.StrokeBrush
                    });
                }
            foreach (var port in OutputPorts)
                if (port.IsVisible)
                {
                    if (port.Control is TextBox)
                    {
                        if (((TextBox) port.Control).Text.ToUpper().Contains(KEY))
                        {
                            if (fi.Hint != Title)
                                fi.Hint = Title;
                            fi.Items.Add(new FoundItem
                            {
                                Hint = $"   In input :{(port.Control as TextBox).Text}",
                                Type = ItemTypes.Port,
                                foundNode = this
                            });
                        }
                    }
                    else
                    {
                        var textBox = port.Control as UnrealControlsCollection.TextBox;
                        if (textBox == null || !textBox.Text.ToUpper().Contains(KEY)) continue;
                        var box = port.Control as UnrealControlsCollection.TextBox;
                        if (box != null)
                            if (fi.Hint != Title)
                                fi.Hint = Title;
                        fi.Items.Add(new FoundItem(port.Background)
                        {
                            Hint = $"In input :{box.Text}",
                            Type = ItemTypes.Port,
                            foundNode = this,
                            Brush = port.StrokeBrush
                        });
                    }
                }
                else
                {
                    var textBox = port.Control as UnrealControlsCollection.TextBox;
                    if (textBox == null || !textBox.Text.ToUpper().Contains(KEY)) continue;
                    var box = port.Control as UnrealControlsCollection.TextBox;
                    if (box != null)
                        if (fi.Hint != Title)
                            fi.Hint = Title;
                    fi.Items.Add(new FoundItem(port.Background)
                    {
                        Hint = $"In input :{box.Text}",
                        Type = ItemTypes.Port,
                        foundNode = this,
                        Brush = port.StrokeBrush
                    });
                }
            if (fi.Hint != Title)
                return null;
            fi.foundNode = this;
            return fi;
        }


        public void Hide()
        {
            foreach (var port in InExecPorts)
            foreach (var conn in port.ConnectedConnectors)
                conn.Wire.Visibility = Visibility.Hidden;
            foreach (var port in OutExecPorts)
            foreach (var conn in port.ConnectedConnectors)
                conn.Wire.Visibility = Visibility.Hidden;
            foreach (var port in InputPorts)
                for (var index = port.ConnectedConnectors.Count - 1; index >= 0; index--)
                {
                    var conn = port.ConnectedConnectors[index];
                    if (conn.StartPort.ParentNode.Types == NodeTypes.SpaghettiDivider)
                        conn.StartPort.ParentNode.Delete();
                    if (conn.Wire != null) conn.Wire.Visibility = Visibility.Hidden;
                }
            foreach (var port in OutputPorts)
                for (var index = port.ConnectedConnectors.Count - 1; index >= 0; index--)
                {
                    var conn = port.ConnectedConnectors[index];

                    if (conn.EndPort.ParentNode.Types == NodeTypes.SpaghettiDivider)
                        conn.EndPort.ParentNode.Delete();
                    if (conn.Wire != null) conn.Wire.Visibility = Visibility.Hidden;
                }
            Visibility = Visibility.Collapsed;
        }

        public virtual void Delete(bool deletedByBrain = false)
        {
            for (var index = 0; index < OutExecPorts.Count; index++)
            {
                var x = OutExecPorts[index];
                x.ConnectedConnectors.ClearConnectors();
            }
            for (var index = 0; index < InExecPorts.Count; index++)
            {
                var x = InExecPorts[index];
                x.ConnectedConnectors.ClearConnectors();
            }
            for (var index = 0; index < OutputPorts.Count; index++)
            {
                var x = OutputPorts[index];
                for (var i = 0; i < x.ConnectedConnectors.Count; i++)
                {
                    var conn = x.ConnectedConnectors[i];
                    if (conn.EndPort.ParentNode.Types == NodeTypes.SpaghettiDivider)
                        conn.EndPort.ParentNode.Delete();
                }
                x.ConnectedConnectors.ClearConnectors();

                Host.Children.Remove(x.Control);
            }
            for (var index = 0; index < InputPorts.Count; index++)
            {
                var x = InputPorts[index];
                for (var i = 0; i < x.ConnectedConnectors.Count; i++)
                {
                    var conn = x.ConnectedConnectors[i];
                    if (conn.StartPort.ParentNode.Types == NodeTypes.SpaghettiDivider)
                        if (Types != NodeTypes.SpaghettiDivider)
                            conn.StartPort.ParentNode.Delete();
                }

                x.ConnectedConnectors.ClearConnectors();
                if (x.Control != null)
                    Host.Children.Remove(x.Control);
            }
            if (Host.Children.Contains(this))
                Host.Children.Remove(this);

            Host.Nodes.Remove(this);
            Host.NeedsRefresh = true;
            InExecPortsPanel = null;
            OutExecPortsPanel = null;
            InputPortsPanel = null;
            OutputPortsPanel = null;
            GC.SuppressFinalize(this);
        }

        public virtual void DeSerializeData(List<string> input, List<string> output)
        {
            //Not always called, the call of this function is only relevant when the set of 
            //ports can be modified at the run-time.
            DynamicPortsGeneration(input.Count);
            for (var index = 0; index < input.Count; index++)
            {
                var item = input[index];
                if (InputPorts.Count > index)
                {
                    var port = InputPorts[index];
                    port.Data.Value = item;
                }
            }
            for (var index = 0; index < output.Count; index++)
            {
                var item = output[index];
                var port = OutputPorts[index];
                port.Data.Value = item;
            }
        }

        public virtual void DynamicPortsGeneration(int n)
        {
        }

        public void Dispose(bool disposing)
        {
        }
    }

    [XmlType("Node")]
    public class NodeProperties
    {
        public List<object> InnerData = new List<object>();
        public List<string> InputData = new List<string>();
        public List<string> OutputData = new List<string>();

        public NodeProperties()
        {
        }

        public NodeProperties(Node node)
        {
            Name = node.ToString();
            NodeType = node.Types.ToString();
            Id = node.Id;
            X = node.X;
            Y = node.Y;
            foreach (var iPort in node.InputPorts)
                InputData.Add(iPort.Data.Value);
            foreach (var oPort in node.OutputPorts)
                OutputData.Add(oPort.Data.Value);
        }


        public string Name { get; set; }
        public string Id { get; set; }
        public string NodeType { get; set; }
        public double X { get; set; }
        public double Y { get; set; }
    }

    public class SpaghDividerProperties
    {
        public SpaghDividerProperties()
        {
        }

        public SpaghDividerProperties(Node sd)
        {
            X = sd.X;
            Y = sd.Y;
            if (sd.InputPorts.Count > 0)
                if (sd.InputPorts[0].ConnectedConnectors.Count > 1)
                {
                    EndNode_ID = sd.InputPorts[0].ConnectedConnectors[1].EndPort.ParentNode.Id;

                    for (var index = 0;
                        index < sd.InputPorts[0].ConnectedConnectors[1].EndPort.ParentNode.InputPorts.Count;
                        index++)
                    {
                        var port = sd.InputPorts[0].ConnectedConnectors[1].EndPort.ParentNode.InputPorts[index];
                        if (Equals(port, sd.InputPorts[0].ConnectedConnectors[1].EndPort))
                        {
                            Port_Index = index;
                            break;
                        }
                    }
                }
        }

        public string EndNode_ID { get; set; }
        public int Port_Index { get; set; }
        public double X { get; set; }
        public double Y { get; set; }
    }
}