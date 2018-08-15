using System;
using System.Collections.Generic;
using VisualSR.BasicNodes;
using VisualSR.Controls;
using VisualSR.Core;

namespace VisualSR.Tools
{
    public static class Hub
    {
        public static string WorkSpace;
        public static VirtualControl VirtualScriptsHost { get; set; }
        public static VariablesList VarialbesHost { get; set; }
        public static List<Node> LoadedExternalNodes { get; set; }
        public static VirtualControl CurrentHost { get; set; }

        public static bool LoadVirtualData(VirtualControl vc, string str)
        {
            try
            {
                for (var index = 0; index < vc.Nodes.Count; index++)
                {
                    var node = vc.Nodes[index];
                    node.Delete();
                }
                var data = Cipher.DeSerializeFromString<VirtualControlData>(str);
                for (var index = data.Nodes.Count - 1; index >= 0; index--)
                {
                    var copiednode = data.Nodes[index];
                    var typename = copiednode.Name;
                    Node newNode = null;
                    foreach (var node in LoadedExternalNodes)
                    {
                        if (node.ToString() != typename) continue;
                        newNode = node.Clone();
                        vc.AddNode(newNode, copiednode.X, copiednode.Y);
                        newNode.DeSerializeData(copiednode.InputData, copiednode.OutputData);
                        newNode.Id = copiednode.Id;
                        break;
                    }
                    if (newNode != null) continue;
                    var type = Type.GetType(typename);
                    if (type != null)
                    {
                        var instance = Activator.CreateInstance(type, vc, false);
                        vc.AddNode(instance as Node, copiednode.X, copiednode.Y);
                        var node = instance as Node;
                        if (node != null) node.Id = copiednode.Id;
                    }
                }
                foreach (var eConn in data.ExecutionConnectors)
                {
                    var start = vc.GetNode(eConn.StartNode_ID);
                    var end = vc.GetNode(eConn.EndNode_ID);
                    NodesManager.CreateExecutionConnector(vc, start.OutExecPorts[eConn.StartPort_Index],
                        end.InExecPorts[eConn.EndPort_Index]);
                }
                foreach (var oConn in data.ObjectConnectors)
                {
                    var start = vc.GetNode(oConn.StartNode_ID);
                    var end = vc.GetNode(oConn.EndNode_ID);
                    NodesManager.CreateObjectConnector(vc, start.OutputPorts[oConn.StartPort_Index],
                        end.InputPorts[oConn.EndPort_Index]);
                }
                foreach (var divider in data.SpaghettiDividers)
                {
                    var node = vc.GetNode(divider.EndNode_ID);
                    var port = node.InputPorts[divider.Port_Index];
                    var conn = port.ConnectedConnectors[0];
                    var spaghettiDivider = new SpaghettiDivider(vc, conn, false)
                    {
                        X = divider.X,
                        Y = divider.Y
                    };
                    vc.AddNode(spaghettiDivider, spaghettiDivider.X, spaghettiDivider.Y);
                }
            }
            catch (Exception)
            {
                return false;
            }
            foreach (var node in vc.Nodes)
                node.Refresh();
            vc.NeedsRefresh = true;

            return true;
        }
    }
}