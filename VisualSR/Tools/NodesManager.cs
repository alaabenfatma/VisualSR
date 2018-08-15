using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using VisualSR.Core;

namespace VisualSR.Tools
{
    public static class NodesManager
    {
        public static void TryCast(ref Connector oConnector)
        {
            if (((ObjectPort) oConnector.StartPort).StrokeBrush == ((ObjectPort) oConnector.EndPort).StrokeBrush
                &&
                ((ObjectPort) oConnector.StartPort).ParentNode.Types !=
                ((ObjectPort) oConnector.StartPort).ParentNode.Types) return;
            var myLinearGradientBrush =
                new LinearGradientBrush
                {
                    StartPoint = new Point(0, 0),
                    EndPoint = new Point(1, 0)
                };
            if (oConnector.StartPort != null)
            {
                var startPortBrush = oConnector.StartPort.Background as SolidColorBrush;
                if (startPortBrush != null)
                    myLinearGradientBrush.GradientStops.Add(
                        new GradientStop(startPortBrush.Color, 0.0));
                if (oConnector.EndPort != null)
                {
                    var endPortBrush = oConnector.EndPort.Background as SolidColorBrush;
                    if (startPortBrush != null)
                        if (endPortBrush != null)
                            myLinearGradientBrush.GradientStops.Add(
                                new GradientStop(endPortBrush.Color, .7));
                }
            }

            if (oConnector.Wire != null) oConnector.Wire.Background = myLinearGradientBrush;
        }

        /// <summary>
        ///     This function will decide which color matches the type of the variable.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        private static Brush ImperativeColor(string type = "Generic")
        {
            switch (type)
            {
                case "Generic":
                    return Brushes.LightGray;
                case "Character":
                    return Brushes.Magenta;
                case "Logical":
                    return Brushes.Red;
                case "Numeric":
                    return Brushes.LawnGreen;
                case "DataFrame":
                    return Brushes.Orange;
                default:
                    return Brushes.DodgerBlue;
            }
        }

        public static void ChangeColorOfVariableNode(ObjectPort op, string type = "Generic")
        {
            if (op.ParentNode.Types == NodeTypes.VariableSet)
                op.ParentNode.Background = ImperativeColor(type);
            if (op.Linked)
                switch (type)
                {
                    case "Generic":
                        op.StrokeBrush = ImperativeColor(type);
                        op.Background = ImperativeColor(type);
                        break;
                    case "Character":
                        op.StrokeBrush = ImperativeColor(type);
                        op.Background = ImperativeColor(type);

                        break;
                    case "Logical":
                        op.StrokeBrush = ImperativeColor(type);
                        op.Background = ImperativeColor(type);

                        break;
                    case "Numeric":
                        op.StrokeBrush = ImperativeColor(type);
                        op.Background = ImperativeColor(type);

                        break;
                    case "DataFrame":
                        op.StrokeBrush = ImperativeColor(type);
                        op.Background = ImperativeColor(type);

                        break;
                    default:

                        op.Background = ImperativeColor(type);
                        op.StrokeBrush = ImperativeColor(type);
                        break;
                }
            else
                switch (type)
                {
                    case "Generic":
                        op.StrokeBrush = ImperativeColor(type);

                        break;
                    case "Character":
                        op.StrokeBrush = ImperativeColor(type);

                        break;
                    case "Logical":
                        op.StrokeBrush = ImperativeColor(type);

                        break;
                    case "Numeric":
                        op.StrokeBrush = ImperativeColor(type);

                        break;
                    case "DataFrame":
                        op.StrokeBrush = ImperativeColor(type);

                        break;
                    default:

                        op.StrokeBrush = ImperativeColor(type);
                        break;
                }
        }

        public static void CreateExecutionConnector(VirtualControl host, ExecPort x, ExecPort y, string connId = "")
        {
            string id;
            if (connId == "")
                id = Guid.NewGuid().ToString();
            else id = connId;

            x.ConnectedConnectors.ClearConnectors();

            y.ConnectedConnectors.ClearConnectors();
            x.Linked = false;
            y.Linked = false;
            Connector conn;
            conn = x.PortTypes == PortTypes.Output
                ? new ExecutionConnector(host, x, y)
                : new ExecutionConnector(host, y, x);
            Panel.SetZIndex(conn.Wire, ZIndexes.ConnectorIndex);
            conn.EndPort.ConnectedConnectors.Add(conn);
            conn.StartPort.ConnectedConnectors.Add(conn);
            host.Children.Remove(host.TempConn);
            host.Children.Remove(host.TempConn);
            host.TemExecPort = null;
            host.WireMode = WireMode.Nothing;
            host.MouseMode = MouseMode.Nothing;
            host.HideLinkingPossiblity();
            host.ExecutionConnectors.Add((ExecutionConnector) conn);
            if (connId != "") conn.ID = connId;
        }

        public static void CreateExecutionConnector(VirtualControl host, string xid, int xindex, string yid, int yindex,
            string connId = "")
        {
            ExecPort x = null, y = null;
            foreach (var node in host.Nodes)
                if (node.Id == xid)
                    x = node.OutExecPorts[xindex];
                else if (node.Id == yid)
                    y = node.InExecPorts[yindex];
            if (y == null || x == null) return;
            x.ConnectedConnectors.ClearConnectors();

            y.ConnectedConnectors.ClearConnectors();
            x.Linked = false;
            y.Linked = false;
            Connector conn;

            conn = x.PortTypes == PortTypes.Output
                ? new ExecutionConnector(host, x, y)
                : new ExecutionConnector(host, y, x);
            Panel.SetZIndex(conn.Wire, ZIndexes.ConnectorIndex);

            conn.EndPort.ConnectedConnectors.Add(conn);
            conn.StartPort.ConnectedConnectors.Add(conn);
            host.Children.Remove(host.TempConn);
            host.Children.Remove(host.TempConn);
            host.TemExecPort = null;
            host.WireMode = WireMode.Nothing;
            host.MouseMode = MouseMode.Nothing;
            host.HideLinkingPossiblity();
            host.ExecutionConnectors.Add((ExecutionConnector) conn);
            if (connId != "") conn.ID = connId;
        }

        public static void DeleteExecConnection(VirtualControl host, string id, bool deletedByBrain = false)
        {
            for (var index = 0; index < host.ExecutionConnectors.Count; index++)
            {
                var conn = host.ExecutionConnectors[index];
                if (conn.ID == id)
                    conn.Delete(deletedByBrain);
            }
        }

        public static void CreateObjectConnector(VirtualControl host, ObjectPort x, ObjectPort y, string connID = "",
            bool nullifyTempConn = true)
        {
            if (!y.MultipleConnectionsAllowed)
                if (y.ConnectedConnectors.Count > 0)
                {
                    y.ConnectedConnectors.ClearConnectors();
                    y.Linked = false;
                    if (x != null) x.Linked = false;
                }
            Connector conn = x.PortTypes == PortTypes.Output
                ? new ObjectsConnector(host, x, y)
                : new ObjectsConnector(host, y, x);
            conn.EndPort.ConnectedConnectors.Add(conn);
            conn.StartPort.ConnectedConnectors.Add(conn);
            conn.StartPort.CountOutConnectors++;
            conn.EndPort.CountOutConnectors++;
            host.TemObjectPort = null;
            TryCast(ref conn);
            y.Data.Value = x.Data.Value;
            host.ObjectConnectors.Add(conn as ObjectsConnector);
            if (connID != "")
                conn.ID = connID;
        }

        public static void CreateObjectConnector(VirtualControl host, string xid, int xindex, string yid, int yindex,
            string connID = "",
            bool nullifyTempConn = true)
        {
            ObjectPort x = null, y = null;
            foreach (var node in host.Nodes)
                if (node.Id == xid)
                    x = node.OutputPorts[xindex];
                else if (node.Id == yid)
                    y = node.InputPorts[yindex];
            if (!y.MultipleConnectionsAllowed)
                if (y.ConnectedConnectors.Count > 0)
                {
                    y.ConnectedConnectors.ClearConnectors();
                    y.Linked = false;
                    if (x != null) x.Linked = false;
                }
            Connector conn = x.PortTypes == PortTypes.Output
                ? new ObjectsConnector(host, x, y)
                : new ObjectsConnector(host, y, x);
            conn.EndPort.ConnectedConnectors.Add(conn);
            conn.StartPort.ConnectedConnectors.Add(conn);
            conn.StartPort.CountOutConnectors++;
            conn.EndPort.CountOutConnectors++;
            host.TemObjectPort = null;
            TryCast(ref conn);
            y.Data.Value = x.Data.Value;
            host.ObjectConnectors.Add(conn as ObjectsConnector);
            if (connID != "")
                conn.ID = connID;
        }

        public static void MoveTo(Node node, Point newP)
        {
            var oldP = new Point();
            oldP.X = node.X;
            oldP.Y = node.Y;

            var anim1 = new DoubleAnimation(oldP.X, newP.X, TimeSpan.FromSeconds(3));
            var anim2 = new DoubleAnimation(oldP.Y, newP.Y, TimeSpan.FromSeconds(3));

            node.BeginAnimation(Canvas.LeftProperty, anim1);
            node.BeginAnimation(Canvas.TopProperty, anim2);
        }

        public static void NodeLocationTrigger(Node node)
        {
            node.X = node.X;
            node.Y = node.Y;
        }

        public static void MiddleMan(VirtualControl host, Node a, Node b, Node c)
        {
            if (c.OutExecPorts[0].ConnectedConnectors[0].EndPort.ParentNode == a)
            {
                CreateExecutionConnector(host, a.OutExecPorts[0], b.InExecPorts[0]);

                CreateExecutionConnector(host, b.OutExecPorts[0],
                    c.InExecPorts[0]);
                if (b.OutExecPorts[0].ConnectedConnectors.Count > 0)
                    MiddleMan(host, c, b, b.OutExecPorts[0].ConnectedConnectors[0].EndPort.ParentNode);
            }
        }
    }
}