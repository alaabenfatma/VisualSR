using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VisualSR.Compiler;
using VisualSR.Controls;
using VisualSR.Core;

namespace Nodes.Nodes.R.Loops
{
    [Export(typeof(Node))]
    public class For : Node
    {
        private readonly UnrealControlsCollection.TextBox _rang = new UnrealControlsCollection.TextBox();
       private readonly VirtualControl Host;

        [ImportingConstructor]
        public For([Import("host")] VirtualControl host, [Import("bool")] bool spontaneousAddition = false) : base(
            host, NodeTypes.Method,
            spontaneousAddition)
        {
            Title = "For";
            Host = host;

            Category = "Loops";
            AddExecPort(this, "", PortTypes.Input, "");

            AddExecPort(this, "", PortTypes.Output, "Idle");
            AddExecPort(this, "Instructions", PortTypes.Output, "Instructions");
            AddObjectPort(this, "Element", PortTypes.Input, RTypes.Generic, false);
            AddObjectPort(this, "Range", PortTypes.Input, RTypes.Generic, false, _rang);
            Description = "For loop.";
            _rang.TextChanged += (s, e) => { InputPorts[1].Data.Value = _rang.Text; };
            InputPorts[1].DataChanged += (s, e) =>
            {
                if (_rang.Text != InputPorts[1].Data.Value)
                    _rang.Text = InputPorts[1].Data.Value;
            };
        }


        public override string GenerateCode()
        {
            var sb = new StringBuilder();
            sb.Append($"for({InputPorts?[0].Data.Value} in {InputPorts?[1].Data.Value})"+"{");
            sb.AppendLine();
            if (OutExecPorts[1].ConnectedConnectors.Count > 0)
                sb.AppendLine(CodeMiner.Code(OutExecPorts[1].ConnectedConnectors[0].EndPort.ParentNode));
            sb.AppendLine();
            sb.Append("}");
            return sb.ToString();
        }


        public override Node Clone()
        {
            var node = new For(Host, false);

            return node;
        }
    }
}
