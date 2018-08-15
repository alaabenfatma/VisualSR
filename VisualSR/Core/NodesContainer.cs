/*Copyright 2018 ALAA BEN FATMA

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.*/
/*
 * This code is still under expirement
*/
//using System;
//using System.Collections.Generic;
//using System.Collections.ObjectModel;
//using System.ComponentModel;
//using System.Linq;
//using System.Threading.Tasks;
//using System.Windows;
//using System.Windows.Threading;
//using VisualSR.Compiler;
//using VisualSR.Tools;

//namespace VisualSR.Core
//{
//    public class NodesContainer : Node, INotifyPropertyChanged
//    {
//        private readonly List<MergedPorts> _inPortals = new List<MergedPorts>();
//        private readonly List<MergedPorts> _outPortals = new List<MergedPorts>();
//        private Point _startPoint = new Point(0, 0);
//        private double _x, _y;
//        private VirtualControl host;

//        public ObservableCollection<Node> Nodes = new ObservableCollection<Node>();

//        public NodesContainer(VirtualControl host, List<Node> listofNodes) :
//            base(host, NodeTypes.CollapsedRegion, false)
//        {
//            Task.Factory.StartNew(() =>
//            {
//                Dispatcher.BeginInvoke(DispatcherPriority.ApplicationIdle, new Action(() =>
//                {
//                    this.host = host;
//                    foreach (var node in listofNodes)
//                    {
//                        if (node.IsCollapsed) continue;
//                        Nodes.Add(node);
//                        Description += node.Title + " + ";
//                    }
//                    Title = "Collapsed region " + IndexOfThisRegion();
//                    AddExecPort(this, "", PortTypes.Input, "");
//                    AddExecPort(this, "", PortTypes.Output, "");
//                    DetermineExternalPorts(listofNodes);

//                    if (ExternalOutPort != null)
//                        NodesManager.CreateExecutionConnector(host, ExternalOutPort, InExecPorts[0]);
//                    if (ExternalInPort != null)
//                        NodesManager.CreateExecutionConnector(host, OutExecPorts[0], ExternalInPort);

//                    BuildPortals();
//                    PortalsCleaner();
//                    PushIntoTheBlackHole(listofNodes);
//                    PositionChanged += NodesContainer_PositionChanged;
//                    _startPoint.X = _x;
//                    _startPoint.Y = _y;

//                    host.AddNode(this, _x, _y);
//                    Dispatcher.BeginInvoke(DispatcherPriority.Render, new Action(() =>
//                    {
//                        if (InputPortsPanel.Children.Count == 0 || OutputPortsPanel.Children.Count == 0)
//                            Width += 70;
//                    }));
//                }));
//            });
//        }


//        private ExecPort ExternalInPort { get; set; }
//        private ExecPort ExternalOutPort { get; set; }
//        private Node Tail { get; set; }
//        private Node Head { get; set; }

//        private void NodesContainer_PositionChanged(object sender, EventArgs e)
//        {
//        }

//        private int IndexOfThisRegion() => 1 + host.Nodes.Count(node => node.Types == NodeTypes.CollapsedRegion);

//        private void DetermineExternalPorts(List<Node> selectedNodes)
//        {
//            try
//            {
//                var root = host.RootNode;
//                if (root.OutExecPorts.Count <= 0) return;
//                var nextNode = root.OutExecPorts[0].ConnectedConnectors[0].EndPort.ParentNode;
//                var stillHasMoreNodes = true;
//                while (stillHasMoreNodes)
//                {
//                    if (selectedNodes.Contains(nextNode))
//                        break;
//                    if (nextNode.OutExecPorts[0].ConnectedConnectors.Count > 0)
//                        nextNode = nextNode.OutExecPorts[0].ConnectedConnectors[0].EndPort.ParentNode;

//                    else
//                        stillHasMoreNodes = false;
//                }
//                _x = nextNode.X;
//                _y = nextNode.Y;
//                Head = nextNode;
//                root = nextNode;
//                if (root.InExecPorts.Count > 0)
//                    ExternalOutPort = root.InExecPorts[0].ConnectedConnectors[0].StartPort.ParentNode.OutExecPorts[0];
//                stillHasMoreNodes = true;
//                while (stillHasMoreNodes)
//                {
//                    if (nextNode != null && nextNode.OutExecPorts.Count > 0)
//                        if (nextNode.OutExecPorts[0].ConnectedConnectors.Count > 0)
//                            if (!selectedNodes.Contains(
//                                nextNode.OutExecPorts[0].ConnectedConnectors[0].EndPort.ParentNode))
//                                break;
//                    if (nextNode != null && nextNode.OutExecPorts[0].ConnectedConnectors.Count > 0)
//                        nextNode = nextNode.OutExecPorts[0].ConnectedConnectors[0].EndPort.ParentNode;
//                    else
//                        stillHasMoreNodes = false;
//                }
//                Tail = nextNode;
//                if (nextNode != null)
//                    ExternalInPort = nextNode.OutExecPorts[0].ConnectedConnectors[0].EndPort.ParentNode.InExecPorts[0];

//            }
//            catch (Exception e)
//            {
//                Console.WriteLine(e);
//            }
//            }

//        private double max_Width(double x)
//        {
//            var maxwidth = Nodes[0].ActualWidth;
//            foreach (var node in Nodes)
//                if (node.ActualWidth + node.X > maxwidth + x)
//                    maxwidth += node.ActualWidth + node.X - (maxwidth + x);
//            return maxwidth;
//        }

//        private double max_Height(double y)
//        {
//            var maxheight = Nodes[0].ActualHeight;
//            foreach (var node in Nodes)
//                if (node.ActualHeight + node.Y > maxheight + y)
//                    maxheight += node.ActualHeight + node.Y - (maxheight + y);
//            return maxheight;
//        }


//        private void CalculateSize(ref double x, ref double y, ref double w, ref double h)
//        {
//            if (Nodes.Count > 0)
//            {
//                y = Nodes[0].Y;
//                x = Nodes[0].X;

//                foreach (var node in Nodes)
//                {
//                    if (node.Y < y)
//                        y = node.Y;
//                    if (node.X < x)
//                        x = node.X;
//                }
//                w = max_Width(x);
//                h = max_Height(y);
//            }
//        }

//        private void MakePreview()
//        {
//            //double x=0, y=0, w=0, h=0;
//            //CalculateSize(ref x, ref y, ref w, ref h);

//            //Bitmap image = new Bitmap((int)w, (int)h,
//            //    System.Drawing.Imaging.PixelFormat.Format32bppArgb);
//            //Graphics g = Graphics.FromImage(image);
//            //g.CopyFromScreen((int)x, (int)y,
//            //    0, -22,
//            //    new System.Drawing.Size((int)w, (int)h + 22),
//            //    CopyPixelOperation.SourceCopy);

//            //using (var m = new MemoryStream())
//            //{

//            //    image.Save(@"Data\Previews\" + Title.Replace(" ", string.Empty) + ".png", ImageFormat.Png);

//            //}

//            //StackPanel hintControl = Template.FindName("HintControls", this) as StackPanel;
//            //hintControl.Children.Add(new PreviewContainer(Title.Replace(" ", string.Empty)));
//        }

//        private void BuildPortals()
//        {
//            //Find the nodes that are trying to communicate from the outside of the 
//            //collapsed area
//            Task.Factory.StartNew(() =>
//            {
//                foreach (var node in Nodes)
//                {
//                    Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() =>
//                    {
//                        for (var index = 0; index < node.InputPorts.Count; index++)
//                        {
//                            var port = node.InputPorts[index];
//                            if (port.ConnectedConnectors.Count > 0)
//                                for (var i = 0; i < port.ConnectedConnectors.Count; i++)
//                                {
//                                    var conn = port.ConnectedConnectors[i];
//                                    if (!Nodes.Contains(conn.StartPort.ParentNode))
//                                    {
//                                        var sp = (ObjectPort) conn.StartPort;
//                                        if (conn.StartPort.ParentNode.Types == NodeTypes.SpaghettiDivider)
//                                        {
//                                            sp = conn.StartPort.ParentNode.InputPorts[0].ConnectedConnectors[0]
//                                                .StartPort as ObjectPort;
//                                            //conn.Delete();
//                                        }
//                                        if (sp.ParentNode.IsCollapsed == false)
//                                        {
//                                            AddObjectPort(this, "Portal input", PortTypes.Input,
//                                                ((ObjectPort) conn.EndPort).Data.Type, false);

//                                            NodesManager.CreateObjectConnector(host, sp,
//                                                InputPorts[InputPorts.Count - 1]);
//                                            var middle = InputPorts[InputPorts.Count - 1];
//                                            _inPortals.Add(new MergedPorts(
//                                                sp, ref middle, port));

//                                        }
//                                    }
//                                }
//                        }
//                    }));


//                    Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() =>
//                    {
//                        bool portIsBuilt = false;
//                        for (var index = 0; index < node.OutputPorts.Count; index++)
//                        {
//                            var port = node.OutputPorts[index];
//                            if (port.ConnectedConnectors.Count > 0)
//                            {
//                                var count = port.ConnectedConnectors.Count;
//                                for (var i = count - 1; i >= 0; i--)
//                                {
//                                    var conn = port.ConnectedConnectors[i];
//                                    if (!Nodes.Contains(conn.EndPort.ParentNode))
//                                    {
//                                        var ep = (ObjectPort)conn.EndPort;
//                                        if (conn.EndPort.ParentNode.Types == NodeTypes.SpaghettiDivider)
//                                        {
//                                            ep = conn.EndPort.ParentNode.InputPorts[0].ConnectedConnectors[0]
//                                                .EndPort as ObjectPort;
//                                            //conn.Delete();
//                                        }
//                                        if (ep != null &&
//                                            ep.ParentNode.IsCollapsed == false)
//                                        {

//                                            if (!portIsBuilt)
//                                                AddObjectPort(this, "Portal output", PortTypes.Output,
//                                                    ep.Data.Type, true);
//                                            portIsBuilt = true;
//                                            if (OutputPorts[OutputPorts.Count - 1].ConnectedConnectors.Count == 0)
//                                                NodesManager.CreateObjectConnector(host, port,
//                                                    OutputPorts[OutputPorts.Count - 1]);
//                                            port.DataChanged += (ss, ee) =>
//                                            {
//                                                OutputPorts[OutputPorts.Count - 1].Data.Value = port.Data.Value;
//                                            };
//                                            NodesManager.CreateObjectConnector(host, OutputPorts[OutputPorts.Count - 1],
//                                                ep);
//                                            OutputPorts[OutputPorts.Count - 1].DataChanged += (ss, ee) =>
//                                            {
//                                                ep.Data.Value = OutputPorts[OutputPorts.Count - 1].Data.Value;
//                                            };
//                                            var middle = OutputPorts[OutputPorts.Count - 1];
//                                          //  _outPortals.Add(new MergedPorts(middle, ref port, ep));
//                                        }
//                                    }
//                                }
//                            }
//                        }
//                    }));
//                }
//            });
//        }

//        //Hides all the nodes and their connections
//        private void PushIntoTheBlackHole(List<Node> list)
//        {
//            Task.Factory.StartNew(() =>
//            {
//                Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.ApplicationIdle, new Action(() =>
//                {
//                    foreach (var node in list)
//                    {
//                        node.Hide();
//                        node.IsCollapsed = true;
//                    }
//                }));
//            });
//        }

//        private void PullOutFromTheBlackHole()
//        {
//            Task.Factory.StartNew(() =>
//            {
//                Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Send, new Action(() =>
//                {
//                    foreach (var node in Nodes)
//                    {
//                        node.Visibility = Visibility.Visible;
//                        node.IsCollapsed = false;

//                        foreach (var port in node.InExecPorts)
//                        foreach (var conn in port.ConnectedConnectors)
//                            if (!conn.StartPort.ParentNode.IsCollapsed && !conn.EndPort.ParentNode.IsCollapsed)
//                                if (conn.Wire != null) conn.Wire.Visibility = Visibility.Visible;
//                        foreach (var port in node.OutExecPorts)
//                        foreach (var conn in port.ConnectedConnectors)
//                            if (!conn.StartPort.ParentNode.IsCollapsed && !conn.EndPort.ParentNode.IsCollapsed)
//                                if (conn.Wire != null) conn.Wire.Visibility = Visibility.Visible;
//                        foreach (var port in node.InputPorts)
//                        foreach (var conn in port.ConnectedConnectors)
//                            if (!conn.StartPort.ParentNode.IsCollapsed && !conn.EndPort.ParentNode.IsCollapsed)
//                            {
//                                conn.Wire.Visibility = Visibility.Visible;

//                                ((ObjectPort) conn.EndPort).Data.Value = ((ObjectPort) conn.StartPort).Data.Value;
//                            }
//                        foreach (var port in node.OutputPorts)
//                        foreach (var conn in port.ConnectedConnectors)
//                            if (!conn.StartPort.ParentNode.IsCollapsed && !conn.EndPort.ParentNode.IsCollapsed)
//                            {
//                                conn.Wire.Visibility = Visibility.Visible;
//                                ((ObjectPort) conn.EndPort).Data.Value = ((ObjectPort) conn.StartPort).Data.Value;
//                            }
//                        node.IsCollapsed = false;
//                    }
//                }));
//            });
//        }

//        private void RebuildPrimitiveConnections()
//        {
//            Task.Factory.StartNew(() =>
//            {
//                Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Send, new Action(() =>
//                {
//                    if (InExecPorts[0].ConnectedConnectors.Count > 0)
//                    {
//                        var inPort = InExecPorts[0].ConnectedConnectors[0].StartPort;

//                        NodesManager.CreateExecutionConnector(host, (ExecPort) inPort, Head.InExecPorts[0]);
//                        Head = null;
//                    }
//                    if (OutExecPorts[0].ConnectedConnectors.Count > 0)
//                    {
//                        var outPort = OutExecPorts[0].ConnectedConnectors[0].EndPort;
//                        NodesManager.CreateExecutionConnector(host, Tail.OutExecPorts[0], (ExecPort) outPort);
//                        Tail = null;
//                    }
//                    var distance = Point.Subtract(new Point(X, Y), _startPoint);
//                    foreach (var node in Nodes)
//                    {
//                        node.X += distance.X;
//                        node.Y += distance.Y;
//                    }
//                }));
//            });
//        }

//        private void Relink()
//        {
//            foreach (var port in OutputPorts)
//            {
//                if (port.ConnectedConnectors.Count > 1)
//                {
//                    var rootPort = port.ConnectedConnectors[0].StartPort;
//                    for (var index = port.ConnectedConnectors.Count - 1; index >= 0; index--)
//                    {
//                        if (port.ConnectedConnectors.Count > index)
//                        {
//                            var conn = port.ConnectedConnectors[index];
//                            if (conn != null)
//                                NodesManager.CreateObjectConnector(host, (ObjectPort) rootPort, (ObjectPort) conn.EndPort);
//                        }
//                    }
//                    port.ConnectedConnectors.ClearConnectors();
//                }
//            }
//        }
//        private void Expand()
//        {
//            Relink();
//            MessageBox.Show("lol");
//            PullOutFromTheBlackHole();
//            RebuildPrimitiveConnections();

//        }

//        private void PortalsCleaner()
//        {
//            foreach (var port in InputPorts)
//            foreach (var conn in port.ConnectedConnectors)
//                if (conn.StartPort.ParentNode.Types == NodeTypes.SpaghettiDivider)
//                    conn.StartPort.ParentNode.Delete();
//            foreach (var port in InputPorts)
//            foreach (var conn in port.ConnectedConnectors)
//                if (conn.EndPort.ParentNode.Types == NodeTypes.SpaghettiDivider)
//                    conn.EndPort.ParentNode.Delete();
//        }

//        public override string GenerateCode()
//        {
//            Console.WriteLine(Head);
//            return CodeMiner.Code(Head);
//        }

//        public override void Delete(bool deletedByBrain)
//        {
//            Expand();
//            foreach (var node in host.SelectedNodes)
//                MessageBox.Show(node.ToString());
//            Task.Factory.StartNew(() =>
//            {
//                Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.ApplicationIdle,
//                    new Action(() => { base.Delete(); }));
//            });
//        }

//        private class MergedPorts
//        {
//            public MergedPorts(ObjectPort start, ref ObjectPort middle, ObjectPort end)
//            {
//                Start = start;
//                MiddlePort = middle;
//                End = end;
//                //  End.Data.Value = Start.Data.Value;
//                //    middle.LinkChanged += End_DataChanged;
//                start.OnDataChanged();
//                middle.OnDataChanged();
//                end.OnDataChanged();
//            }

//            private ObjectPort Start { get; }
//            private ObjectPort MiddlePort { get; }
//            private ObjectPort End { get; }


//            private void End_DataChanged(object sender, EventArgs e)
//            {
//                if (MiddlePort.Linked)
//                    End.Data = MiddlePort.Data;
//                else
//                    End.Data.Value = "";
//            }
//        }

//        public class PreviewContainer : VirtualControl
//        {
//        }
//    }
//}

