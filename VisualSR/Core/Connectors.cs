/*Copyright 2018 ALAA BEN FATMA

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.*/
using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using VisualSR.BasicNodes;
using VisualSR.Properties;
using VisualSR.Tools;

namespace VisualSR.Core
{
    public enum ConnectorTypes
    {
        Root,
        Execution,
        Object
    }

    public class Connector : INotifyPropertyChanged, IDisposable
    {
        public string ID;
        public Point Sp, Ep;
        public ConnectorTypes Type;
        public Wire Wire = new Wire();

        public Connector(VirtualControl host, Port sPort, Port ePort)
        {
            Wire.ParentConnector = this;
            ID = Guid.NewGuid().ToString();
        }


        public VirtualControl Host { get; set; }
        public Port StartPort { get; set; }
        public Port EndPort { get; set; }

        public void Dispose()
        {
            Dispose(true);
        }

        public event PropertyChangedEventHandler PropertyChanged;


        public virtual void Delete(bool deletedByBrain = false)
        {
            Host.Children.Remove(Wire);
            Interlocked.Decrement(ref StartPort.CountOutConnectors);
            Interlocked.Decrement(ref EndPort.CountOutConnectors);

            if (StartPort.CountOutConnectors <= 1)
                StartPort.Linked = false;
            if (EndPort.CountOutConnectors <= 1)
                EndPort.Linked = false;

            EndPort.ConnectedConnectors.Remove(this);
            StartPort.ConnectedConnectors.Remove(this);
            Wire = null;
            StartPort.ParentNode.PropertyChanged -= ParentNodeOnPropertyChanged;
            EndPort.ParentNode.PropertyChanged -= ParentNodeOnPropertyChanged;
        }

        public void ParentNodeOnPropertyChanged(object sender, PropertyChangedEventArgs propertyChangedEventArgs)
        {
            StartPort.CalcOrigin();
            EndPort.CalcOrigin();
            Sp = PointsCalculator.PortOrigin(StartPort);
            Ep = PointsCalculator.PortOrigin(EndPort);
            Wire.StartPoint = Sp;
            Wire.EndPoint = Ep;
        }


        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged(string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected virtual void Dispose(bool really)
        {
            Host.Children.Remove(Wire);
        }
    }


    public class ExecutionConnector : Connector
    {
        public ExecutionConnector(VirtualControl host, ExecPort sPort, ExecPort ePort) : base(host, sPort, ePort)
        {
            Host = host;
            Type = ConnectorTypes.Execution;
            StartPort = sPort;
            ePort.Linked = true;
            sPort.Linked = true;
            EndPort = ePort;
            StartPort.CalcOrigin();
            EndPort.CalcOrigin();
            Sp = PointsCalculator.PortOrigin(StartPort);
            Ep = PointsCalculator.PortOrigin(EndPort);
            Wire.StartPoint = Sp;
            Wire.EndPoint = Ep;
            Host.Children.Add(Wire);
            sPort.ParentNode.PropertyChanged += ParentNodeOnPropertyChanged;
            ePort.ParentNode.PropertyChanged += ParentNodeOnPropertyChanged;
            Interlocked.Increment(ref StartPort.CountOutConnectors);
            Interlocked.Increment(ref EndPort.CountOutConnectors);
            Wire.ContextMenu = wireMenu();
        }

        private ContextMenu wireMenu()
        {
            var cm = new ContextMenu();

            var delete = new MenuItem {Header = "Delete", Foreground = Brushes.WhiteSmoke};
            delete.Click += (s, e) => Delete();
            cm.Items.Add(delete);
            return cm;
        }

        public override void Delete(bool deletedByBrain = false)
        {
            base.Delete();
            Host.ExecutionConnectors.Remove(this);
        }
    }

    public class ObjectsConnector : Connector
    {
        public ObjectsConnector(VirtualControl host, ObjectPort sPort, ObjectPort ePort) : base(host, sPort, ePort)
        {
            Wire.Background = sPort.StrokeBrush;
            Host = host;
            Type = ConnectorTypes.Object;
            StartPort = sPort;
            ePort.Linked = true;
            sPort.Linked = true;
            EndPort = ePort;
            StartPort.CalcOrigin();
            EndPort.CalcOrigin();
            Sp = PointsCalculator.PortOrigin(StartPort);
            Ep = PointsCalculator.PortOrigin(EndPort);
            Wire.StartPoint = Sp;
            Wire.EndPoint = Ep;
            ePort.Linked = true;
            sPort.Linked = true;
            Host.Children.Add(Wire);
            sPort.ParentNode.PropertyChanged += ParentNodeOnPropertyChanged;
            ePort.ParentNode.PropertyChanged += ParentNodeOnPropertyChanged;
            ePort.Data.Value = sPort.Data.Value;
            StartPort = sPort;
            EndPort = ePort;
            Wire.ContextMenu = wireMenu();
        }

        private ContextMenu wireMenu()
        {
            var cm = new ContextMenu();
            var divide = new MenuItem {Header = "Divide", Foreground = Brushes.WhiteSmoke};
            divide.Click += (s, e) =>
            {
                if (StartPort.ParentNode.Types != NodeTypes.SpaghettiDivider &&
                    EndPort.ParentNode.Types != NodeTypes.SpaghettiDivider)
                    Task.Factory.StartNew(() =>
                    {
                        Wire.Dispatcher.BeginInvoke(DispatcherPriority.SystemIdle,
                            new Action(() =>
                            {
                                var divider = new SpaghettiDivider(Host, this, false);
                                Host.AddNode(divider, Mouse.GetPosition(Host).X, Mouse.GetPosition(Host).Y);
                                e.Handled = true;
                            }));
                    });
                e.Handled = true;
            };
            var delete = new MenuItem {Header = "Delete", Foreground = Brushes.WhiteSmoke};
            delete.Click += (s, e) => Delete();
            cm.Items.Add(divide);
            cm.Items.Add(delete);
            return cm;
        }

        public override void Delete(bool deletedByBrain = false)
        {
            if (StartPort.ParentNode.Types == NodeTypes.SpaghettiDivider)
            {
                try
                {
                    StartPort.ParentNode.Delete();

                    return;
                }
                catch (Exception)
                {
                    //Ignored
                }
                return;
            }
            if (EndPort.ParentNode.Types == NodeTypes.SpaghettiDivider)
            {
                try
                {
                    EndPort.ParentNode.Delete();

                    return;
                }
                catch (Exception)
                {
                    //Ignored
                }
                return;
            }

            if (Host.Children.Contains(Wire)) Host.Children.Remove(Wire);
            Interlocked.Decrement(ref StartPort.CountOutConnectors);
            Interlocked.Decrement(ref EndPort.CountOutConnectors);
            EndPort.ConnectedConnectors.Remove(this);
            StartPort.ConnectedConnectors.Remove(this);
            if (StartPort.CountOutConnectors == 0)
                ((ObjectPort) StartPort).Linked = false;
            if (EndPort.CountOutConnectors == 0)
                ((ObjectPort) EndPort).Linked = false;
            Wire = null;
            StartPort.ParentNode.PropertyChanged -= ParentNodeOnPropertyChanged;
            EndPort.ParentNode.PropertyChanged -= ParentNodeOnPropertyChanged;
            Host.ObjectConnectors.Remove(this);
        }
    }

    public class ObjectConnectorProperties
    {
        public ObjectConnectorProperties()
        {
        }

        public ObjectConnectorProperties(ObjectsConnector connector)
        {
            StartNode_ID = connector.StartPort.ParentNode.Id;
            EndNode_ID = connector.EndPort.ParentNode.Id;
            for (var index = 0; index < connector.StartPort.ParentNode.OutputPorts.Count; index++)
            {
                var port = connector.StartPort.ParentNode.OutputPorts[index];
                if (Equals(port, connector.StartPort))
                    StartPort_Index = index;
            }
            for (var index = 0; index < connector.EndPort.ParentNode.InputPorts.Count; index++)
            {
                var port = connector.EndPort.ParentNode.InputPorts[index];
                if (Equals(port, connector.EndPort))
                    EndPort_Index = index;
            }
        }

        public string StartNode_ID { get; set; }
        public string EndNode_ID { get; set; }
        public int StartPort_Index { get; set; }
        public int EndPort_Index { get; set; }
    }

    public class ExecutionConnectorProperties
    {
        public ExecutionConnectorProperties()
        {
        }

        public ExecutionConnectorProperties(ExecutionConnector connector)
        {
            StartNode_ID = connector.StartPort.ParentNode.Id;
            EndNode_ID = connector.EndPort.ParentNode.Id;
            for (var index = 0; index < connector.StartPort.ParentNode.OutExecPorts.Count; index++)
            {
                var port = connector.StartPort.ParentNode.OutExecPorts[index];
                if (Equals(port, connector.StartPort))
                    StartPort_Index = index;
            }
            for (var index = 0; index < connector.EndPort.ParentNode.InExecPorts.Count; index++)
            {
                var port = connector.EndPort.ParentNode.InExecPorts[index];
                if (Equals(port, connector.EndPort))
                    EndPort_Index = index;
            }
        }

        public string StartNode_ID { get; set; }
        public string EndNode_ID { get; set; }
        public int StartPort_Index { get; set; }
        public int EndPort_Index { get; set; }
    }
}