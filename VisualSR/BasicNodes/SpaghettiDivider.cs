using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Shapes;
using System.Windows.Threading;
using VisualSR.Core;
using VisualSR.Tools;

namespace VisualSR.BasicNodes
{
    public class SpaghettiDivider : Node
    {
        private readonly VirtualControl Host;

        public SpaghettiDivider(VirtualControl host, Connector connector, bool spontaneousAddition = true) : base(host,
            NodeTypes.SpaghettiDivider, spontaneousAddition)
        {
            ObjectConnector = connector as ObjectsConnector;
            Host = host;
            Connector = connector;
            Start = connector.StartPort as ObjectPort;
            End = connector.EndPort as ObjectPort;
            connector.Delete();
            Title = "";
            var inPort = Connector.EndPort as ObjectPort;
            var outPort = Connector.StartPort as ObjectPort;
            if (outPort != null)
                AddObjectPort(this, "", PortTypes.Input, outPort.Data.Type, true);
            Dispatcher.BeginInvoke(DispatcherPriority.Loaded, new Action(() =>
            {
                NodesManager.CreateObjectConnector(host, InputPorts[0], outPort);
                NodesManager.CreateObjectConnector(host, inPort, InputPorts[0]);
                if (Host.NodesTree.IsVisible)
                    Host.NodesTree.Remove();
                InputPorts[0].IsEnabled = false;
                if (InputPorts.Count > 0)
                    if (InputPorts[0].ConnectedConnectors.Count > 0)
                        Connector.StartPort = InputPorts[0].ConnectedConnectors[0].StartPort;
                InputPorts[0].DataChanged += (ss, ee) =>
                {
                    End.Data.Value = InputPorts[0].Data.Value;
                    foreach (var c in InputPorts[0].ConnectedConnectors)
                        c.Wire.HeartBeatsAnimation(false);
                };
                InputPorts[0].ConnectedConnectors.CollectionChanged += (ss, ee) => { };
                var hint = Template.FindName("NodeHint", this) as Path;
                if (hint != null) hint.Visibility = Visibility.Collapsed;
                Height -= 33;
                Width -= 13;
                Canvas.SetTop(InputPortsPanel, -8);
                Canvas.SetLeft(InputPortsPanel, 3);
            }));
        }

        public ObjectsConnector ObjectConnector { get; set; }

        private Connector Connector { get; }
        private ObjectPort Start { get; }
        private ObjectPort End { get; }

        public override void Delete(bool deletedByBrain = false)
        {
            try
            {
                Types = NodeTypes.Basic;
                if (InputPorts[0].ConnectedConnectors.Count > 0)
                    InputPorts[0].ConnectedConnectors[0].Wire.Visibility = Visibility.Collapsed;
                if (InputPorts[0].ConnectedConnectors.Count > 0)
                    InputPorts[0].ConnectedConnectors[0].Wire.Visibility = Visibility.Collapsed;
                InputPorts[0].ConnectedConnectors.Remove(Connector);
                InputPorts[0].ConnectedConnectors.ClearConnectors();
                if (Host.Children.Contains(Start.ParentNode))
                    if (Host.Children.Contains(End.ParentNode))
                        if (End.ConnectedConnectors.Count == 0)
                            NodesManager.CreateObjectConnector(Host, Start, End);
                Host.Children.Remove(this);
                Host.Nodes.Remove(this);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }
    }
}