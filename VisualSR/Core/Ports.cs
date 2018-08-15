/*Copyright 2018 ALAA BEN FATMA

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.*/
using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using VisualSR.Tools;

namespace VisualSR.Core
{
    public enum PortTypes
    {
        Input,
        Output
    }

    /// <summary>
    ///     The base class of all ports.
    /// </summary>
    public class Port : Button, INotifyPropertyChanged
    {
        private Brush _color;
        private bool _linked;
        private Point _origin;
        private string _text;
        public ConnectorsList ConnectedConnectors = new ConnectorsList();
        public int CountOutConnectors;
        public int index;

        public Port()
        {
            Focusable = false;
        }

        public Node ParentNode { get; set; }

        public PortTypes PortTypes { get; set; }

        public Point Origin
        {
            get { return _origin; }
            set
            {
                _origin = value;
                OnPropertyChanged();
            }
        }

        public string Text
        {
            get { return _text; }
            set
            {
                _text = value;
                OnPropertyChanged("Text");
            }
        }

        public bool Dragging { get; set; }
        public bool MultipleConnectionsAllowed { get; set; }
        public Type DataType { get; set; }
        public VirtualControl Host { get; set; }

        public Brush StrokeBrush
        {
            get { return _color; }
            set
            {
                _color = value;
                OnPropertyChanged("StrokeBrush");
            }
        }

        public bool Linked
        {
            get { return _linked; }
            set
            {
                _linked = value;
                OnPropertyChanged("Linked");
                Background = !_linked ? Brushes.Transparent : StrokeBrush;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void OnLinkChanged()
        {
            Linked = Linked;
        }


        public void CalcOrigin()
        {
            try
            {
                Origin = Host.UIelementCoordinates(this);
            }
            catch
            {
/*Ignored*/
            }
        }


        protected virtual void OnPropertyChanged(string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    /// <summary>
    ///     The port that handles R codes in a consucitive manner.
    /// </summary>
    public class ExecPort : Port
    {
        public ExecPort()
        {
        }

        public ExecPort(VirtualControl host, string name, PortTypes portType, string text, bool multipleConnectors)
        {
            Focusable = false;
            Text = text;
            Host = host;
            StrokeBrush = Brushes.AliceBlue;
            MultipleConnectionsAllowed = multipleConnectors;
            if (portType == PortTypes.Input)
            {
                Style = FindResource("InExecPortStyle") as Style;
                PortTypes = PortTypes.Input;
            }
            else
            {
                Style = FindResource("OutExecPortStyle") as Style;
                PortTypes = PortTypes.Output;
            }
            PreviewMouseDown += OnMouseLeftButtonDown;
            PreviewMouseUp += OnPreviewMouseUp;
            PreviewMouseMove += OnPreviewMouseMove;
            MouseLeave += (sender, args) => Host.HideLinkingPossiblity();
        }

        private void OnPreviewMouseMove(object o, MouseEventArgs mouseEventArgs)
        {
            if (Host.WireMode == WireMode.FirstPortSelected)

                Host.ShowLinkingPossiblity(LinkingPossiblity(), Mouse.GetPosition(Host));
        }

        private int LinkingPossiblity()
        {
            if (Host.TemObjectPort != null) return 0;
            if (Host.TemExecPort == null) return 0;
            if (PortTypes != Host.TemExecPort.PortTypes &&
                !Equals(ParentNode, Host.TemExecPort.ParentNode)) return 1;
            return 0;
        }

        private void OnPreviewMouseUp(object o, MouseButtonEventArgs mouseButtonEventArgs)
        {
            if (Host.WireMode != WireMode.FirstPortSelected) return;
            if (LinkingPossiblity() != 0)
                if (Host.TemExecPort.ConnectedConnectors.Count == 0)
                {
                    NodesManager.CreateExecutionConnector(Host, Host.TemExecPort, this);
                }
                else
                {
                    var thirdNode = Host.TemExecPort.ConnectedConnectors[0].EndPort.ParentNode;

                    if (thirdNode.OutExecPorts[0].ConnectedConnectors.Count > 0 &&
                        thirdNode.OutExecPorts[0].ConnectedConnectors[0].EndPort.ParentNode == ParentNode)
                        NodesManager.MiddleMan(Host, Host.TemExecPort.ParentNode, ParentNode, thirdNode);
                    NodesManager.CreateExecutionConnector(Host, Host.TemExecPort, this);
                    NodesManager.CreateExecutionConnector(Host, ParentNode.OutExecPorts[0],
                        thirdNode.InExecPorts[0]);
                }
            Host.Children.Remove(Host.TempConn);
            Host.TemExecPort = null;
            Host.WireMode = WireMode.Nothing;
            Host.MouseMode = MouseMode.Nothing;
            Host.HideLinkingPossiblity();
        }


        private void OnMouseLeftButtonDown(object o, MouseButtonEventArgs mouseButtonEventArgs)
        {
            if (mouseButtonEventArgs.LeftButton != MouseButtonState.Pressed) return;
            CalcOrigin();
            Host.TempConn = new Wire
            {
                Background = StrokeBrush,
                StartPoint = PointsCalculator.PortOrigin(this),
                EndPoint = PointsCalculator.PortOrigin(this)
            };
            Host.TemExecPort = this;
            Host.WireMode = WireMode.FirstPortSelected;
            Host.MouseMode = MouseMode.DraggingPort;
            Host.Children.Add(Host.TempConn);
            mouseButtonEventArgs.Handled = true;
        }
    }

    /// <summary>
    ///     The port that hosts data and manipulates it for further usage.
    /// </summary>
    public class ObjectPort : Port, INotifyPropertyChanged
    {
        private bool _linked;

        private StackPanel _panel = new StackPanel();
        private bool _parentNodeNeedsARefresh;
        private UIElement _pin;
        public Control Control = new Control();

        public ObjectPort(VirtualControl host, PortTypes portType, string text, RTypes type)
        {
            Text = text;
            Data = new RVariable(type);
            Host = host;
            // Focusable = false;
            Data.ParentPort = this;


            if (portType == PortTypes.Input)
            {
                if (type == RTypes.ArrayOrFactorOrListOrMatrix)
                    Style = (Style) FindResource("InArrayPortStyle");
                else
                    Style = (Style) FindResource("InObjectPortStyle");
                PortTypes = PortTypes.Input;
            }
            else
            {
                if (type == RTypes.ArrayOrFactorOrListOrMatrix)
                    Style = (Style) FindResource("OutArrayPortStyle");
                else
                    Style = (Style) FindResource("OutObjectPortStyle");
                PortTypes = PortTypes.Output;
            }
            Loaded += (sender, args) =>
            {
                Host = host;
                _panel = (StackPanel) Template.FindName("ControlsPanel", this);
                _pin = (UIElement) Template.FindName("Pin", this);
                if (_pin != null) _pin.PreviewMouseDown += OnMouseDown;
                if (_panel == null) return;
                if (Control != null)
                    try
                    {
                        _panel.Children.Remove(Control);
                        _panel.Children.Add(Control);
                    }
                    catch (Exception)
                    {
                        MagicLaboratory.unLinkChild(Control);
                        _panel.Children.Add(Control);
                    }
            };

            PreviewMouseUp += OnMouseUp;
            PreviewMouseMove += ObjectPort_PreviewMouseMove;
            MouseLeave += (sender, args) =>
            {
                Host.HideLinkingPossiblity();
                ParentNode.Refresh();
                args.Handled = true;
            };

            Control.Loaded += Control_Loaded;
            Control.SizeChanged += Control_SizeChanged;
        }


        public RVariable Data { get; set; }

        public new bool Linked
        {
            get { return _linked; }
            set
            {
                _linked = value;
                ParentNode?.Refresh();

                if (PortTypes != PortTypes.Output)
                    if (_linked)
                    {
                        Control.Visibility = Visibility.Collapsed;
                        _parentNodeNeedsARefresh = true;
                    }
                    else
                    {
                        Control.Visibility = Visibility.Visible;
                    }
                Background = !_linked ? Brushes.Transparent : StrokeBrush;
                OnPropertyChanged();
                OnLinkChanged();
            }
        }

        public new event PropertyChangedEventHandler PropertyChanged;

        private void Control_Loaded(object sender, RoutedEventArgs e)
        {
            ParentNode?.Refresh();
        }

        private void Control_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            ParentNode?.Refresh();
        }

        private void ObjectPort_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (Host.WireMode == WireMode.FirstPortSelected)
                Host.ShowLinkingPossiblity(LinkingPossiblity(), Mouse.GetPosition(Host));
            if (_parentNodeNeedsARefresh)
            {
                ParentNode?.Refresh();
                _parentNodeNeedsARefresh = false;
            }
            e.Handled = true;
        }

        public event EventHandler DataChanged;

        public void OnDataChanged()
        {
            DataChanged?.Invoke(this, new EventArgs());
            if (PortTypes != PortTypes.Output) return;
            for (var index = 0; index < ConnectedConnectors.Count; index++)
            {
                var inConnectedConnector = ConnectedConnectors[index];
                if (Equals(inConnectedConnector.StartPort, this))
                {
                    if (((ObjectPort) inConnectedConnector.EndPort).Data.Value != Data.Value)
                        ((ObjectPort) inConnectedConnector.EndPort).Data.Value = Data.Value;
                }
                else if (Equals(inConnectedConnector.EndPort, this))

                {
                    if (Data.Value != ((ObjectPort) inConnectedConnector.StartPort).Data.Value)
                        Data.Value = ((ObjectPort) inConnectedConnector.StartPort).Data.Value;
                }
            }
            for (var index = ConnectedConnectors.Count - 1; index >= 0; index--)
            {
                var conn = ConnectedConnectors[index];
                conn.Wire?.HeartBeatsAnimation(false);
            }
        }

        public event EventHandler LinkChanged;

        public void OnLinkChanged()
        {
            LinkChanged?.Invoke(this, new EventArgs());
        }

        public int LinkingPossiblity()
        {
            if (Host.TemExecPort != null) return 0;
            if (Host.TemObjectPort == null) return 0;
            if (Host.TemObjectPort.Data.Type != Data.Type && Host.TemObjectPort.ParentNode != ParentNode &&
                Host.TemObjectPort.PortTypes != PortTypes) return 2;
            if (Host.TemObjectPort.Data.Type == Data.Type && Host.TemObjectPort.ParentNode != ParentNode &&
                Host.TemObjectPort.PortTypes != PortTypes)
                return 1;

            return 0;
        }

        private void OnMouseUp(object o, MouseButtonEventArgs e)
        {
            var sport = Host.TemObjectPort;
            if (Host.WireMode != WireMode.FirstPortSelected) return;
            if (LinkingPossiblity() != 0)
            {
                for (var index = 0; index < ConnectedConnectors.Count; index++)
                {
                    var conn = ConnectedConnectors[index];
                    if (conn.EndPort.ParentNode.Types == NodeTypes.SpaghettiDivider)
                        conn.EndPort.ParentNode.Delete();
                    else if (conn.StartPort.ParentNode.Types == NodeTypes.SpaghettiDivider)
                        conn.StartPort.ParentNode.Delete();
                }
                NodesManager.CreateObjectConnector(Host, sport, this);
                OnLinkChanged();
            }
            else
            {
                Control.Focus();
            }


            Host.Children.Remove(Host.TempConn);
            Host.TemObjectPort = null;
            Host.WireMode = WireMode.Nothing;
            Host.MouseMode = MouseMode.Nothing;
            Host.HideLinkingPossiblity();
            ParentNode.Refresh();
            e.Handled = true;
        }

        private void OnMouseDown(object o, MouseButtonEventArgs mouseButtonEventArgs)
        {
            if (mouseButtonEventArgs.LeftButton != MouseButtonState.Pressed) return;
            CalcOrigin();
            Host.TempConn = new Wire
            {
                Background = StrokeBrush,
                StartPoint = PointsCalculator.PortOrigin(this),
                EndPoint = PointsCalculator.PortOrigin(this)
            };
            Host.TemObjectPort = this;
            Host.WireMode = WireMode.FirstPortSelected;
            Host.MouseMode = MouseMode.DraggingPort;
            Host.Children.Add(Host.TempConn);
            Host.TempConn.Background = Host.TemObjectPort.StrokeBrush;
            mouseButtonEventArgs.Handled = true;
        }

        protected override void OnPropertyChanged(string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}