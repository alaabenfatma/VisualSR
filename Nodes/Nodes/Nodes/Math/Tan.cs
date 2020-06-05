using System.ComponentModel.Composition;
using VisualSR.Core;

namespace Nodes.Nodes.Math
{
    [Export(typeof(Node))]
    public class Tan : Node
    {
        private readonly VirtualControl Host;

        [ImportingConstructor]
        public Tan([Import("host")] VirtualControl host, [Import("bool")] bool spontaneousAddition = false) : base(host,
            NodeTypes.Basic,
            spontaneousAddition)
        {
            Host = host;
            Title = "Tan";
            Category = "Math nodes";
            Description = "Calculates tan(x).";
            AddObjectPort(this, "value", PortTypes.Input, RTypes.Numeric, false);
            AddObjectPort(this, "return tan(value)", PortTypes.Output, RTypes.Numeric, true);
            InputPorts[0].DataChanged += (s, e) =>
            {
                OutputPorts[0].Data.Value = "tan(" + InputPorts[0].Data.Value + ")";
            };
        }

        public override string GenerateCode()
        {
            return null;
        }

        public override Node Clone()
        {
            var node = new Tan(Host);

            return node;
        }
    }
}