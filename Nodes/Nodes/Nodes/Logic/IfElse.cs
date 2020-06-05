using System;
using System.ComponentModel.Composition;
using System.Text;
using VisualSR.Compiler;
using VisualSR.Core;

namespace Nodes.Nodes.Logic
{
    [Export(typeof(Node))]
    public class IfElse : Node

    {
        private readonly VirtualControl Host;

        [ImportingConstructor]
        public IfElse([Import("host")] VirtualControl host, [Import("bool")] bool spontaneousAddition = false) : base(
            host, NodeTypes.Method,
            spontaneousAddition)
        {
            Types = NodeTypes.Event;
            Host = host;
            Title = "IfElse";
            Category = "Logic nodes";
            AddExecPort(this, "", PortTypes.Input, "");
            AddObjectPort(this, "Condition", PortTypes.Input, RTypes.Logical, false);
            AddExecPort(this, "", PortTypes.Output, "True");
            AddExecPort(this, "", PortTypes.Output, "False");
            AddExecPort(this, "", PortTypes.Output, "Default");
            re_Arrange();
        }

        private void re_Arrange()
        {
            var aux = OutExecPorts[0];
            OutExecPorts[0] = OutExecPorts[2];
            OutExecPorts[2] = aux;
            aux = OutExecPorts[1];
            OutExecPorts[1] = OutExecPorts[2];
            OutExecPorts[2] = aux;
        }

        public override string GenerateCode()
        {
            var sb = new StringBuilder();
            sb.AppendLine();
            sb.Append("if(" + InputPorts[0].Data.Value + "){");
            try
            {
                if (OutExecPorts[1] != null)
                    sb.Append(CodeMiner.Code(OutExecPorts[1].ConnectedConnectors[0].EndPort.ParentNode));
            }
            catch (Exception)
            {
                //Ignored
            }
            sb.Append("}");
            sb.Append("else{");
            try
            {
                if (OutExecPorts[1] != null)
                    sb.Append(CodeMiner.Code(OutExecPorts[2].ConnectedConnectors[0].EndPort.ParentNode));
            }
            catch (Exception)
            {
                //Ignored
            }
            sb.Append("}");
            sb.AppendLine();
            return sb.ToString();
        }

        public override Node Clone()
        {
            var node = new IfElse(Host, false);

            return node;
        }
    }
}