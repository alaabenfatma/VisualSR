using System.ComponentModel.Composition;
using VisualSR.Controls;
using VisualSR.Core;

namespace Nodes.Nodes.Math
{
    [Export(typeof(Node))]
    public class Sin : Node
    {
        private readonly UnrealControlsCollection.TextBox _tb = new UnrealControlsCollection.TextBox();
        private readonly VirtualControl Host;

        [ImportingConstructor]
        public Sin([Import("host")] VirtualControl host, [Import("bool")] bool spontaneousAddition = false) : base(host,
            NodeTypes.Basic,
            spontaneousAddition)
        {
            Host = host;
            Title = "Sin";

            Category = "Math nodes";
            Description = "Calculates sin(value).";
            AddObjectPort(this, "value", PortTypes.Input, RTypes.Numeric, true);
            AddObjectPort(this, "return sin(value)", PortTypes.Output, RTypes.Numeric, true);
            InputPorts[0].DataChanged += (s, e) =>
            {
                OutputPorts[0].Data.Value = "sin(" + InputPorts[0].Data.Value + ")";
            };
        }

        public override string GenerateCode()
        {
            return null;
        }

        public override Node Clone()
        {
            var node = new Sin(Host, false);

            return node;
        }
    }
}