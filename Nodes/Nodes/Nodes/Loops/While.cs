using System.ComponentModel.Composition;
using System.Text;
using VisualSR.Compiler;
using VisualSR.Core;

namespace Nodes.Nodes.R.Loops
{
    [Export(typeof(Node))]
    public class While : Node
    {
        private readonly VirtualControl Host;

        [ImportingConstructor]
        public While([Import("host")] VirtualControl host, [Import("bool")] bool spontaneousAddition = false) : base(
            host, NodeTypes.Method,
            spontaneousAddition)
        {
            Title = "While";
            Host = host;
            Category = "Loops";
            AddExecPort(this, "", PortTypes.Input, "");
            AddExecPort(this, "", PortTypes.Output, "Reset");
            AddExecPort(this, "Instructions", PortTypes.Output, "Instructions");
            AddObjectPort(this, "Condition", PortTypes.Input, RTypes.Logical, false);
            Description = "While loop.";
        }


        public override string GenerateCode()
        {
            var sb = new StringBuilder();
            sb.Append($"while({InputPorts?[0].Data.Value})" + "{");
            sb.AppendLine();
            if (OutExecPorts[1].ConnectedConnectors.Count > 0)
                sb.AppendLine(CodeMiner.Code(OutExecPorts[1].ConnectedConnectors[0].EndPort.ParentNode));
            sb.AppendLine();
            sb.Append("}");
            return sb.ToString();
        }


        public override Node Clone()
        {
            var node = new While(Host, false);
            return node;
        }
    }
}