/*
 * This class has been written with the intention to write a nodes parser
 * May be supported in the new versions
*/
//using System;
//using System.Windows;
//using VisualSR.Core;

//namespace VisualSR.Compiler
//{
//    public class Builder
//    {
//        public static int Errors(VirtualControl host)
//        {
//            var collection = host.Nodes;
//            var errors = 0;
//            Node nodeToPoke = null;
//            foreach (var node in collection)
//                if (HasAnError(node))
//                {
//                    errors++;
//                    if (nodeToPoke == null)
//                        nodeToPoke = node;
//                }
//            if (nodeToPoke != null) MessageBox.Show(nodeToPoke.Title);
//            return errors;
//        }

//        private static bool HasAnError(Node node)
//        {
//            switch (node.Types)
//            {
//                case NodeTypes.Method:
//                    var portCollection = node.InputPorts;

//                    foreach (var port in portCollection)
//                        if (port.Linked)
//                            return false;
//                    break;
//                case NodeTypes.VariableSet:
//                {
//                    var port = node.InputPorts[0];

//                    if (port.Data.Value != "")
//                        return false;
//                    break;
//                }
//                case NodeTypes.Function:
//                {
//                    var port = node.OutputPorts[0];

//                    Console.WriteLine("Error has been detected:\nNode:{0}\nID:{1}\nValue:{2}", node, node.Id,
//                        port.Data.Value);
//                    if (port.Data.Value != "")
//                        return false;
//                    break;
//                }
//                case NodeTypes.Root:
//                {
//                    return false;
//                }
//            }
//            return true;
//        }
//    }
//}

