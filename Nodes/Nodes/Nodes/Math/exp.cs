using System;
using System.ComponentModel.Composition;
using VisualSR.Core;

namespace Nodes.Nodes.Math
{
    [Serializable]
    [Export(typeof(Node))]
    public class Exp : Node
    {
        private readonly VirtualControl Host;

        [ImportingConstructor]
        public Exp([Import("host")] VirtualControl host, [Import("bool")] bool spontaneousAddition = false) : base(host,
            NodeTypes.Basic,
            spontaneousAddition)
        {
            Host = host;
            Title = "Exp";
            Category = "Math nodes";
            Description = "Calculates exp(x).";
            AddObjectPort(this, "value", PortTypes.Input, RTypes.Numeric, false);
            AddObjectPort(this, "return exp(value)", PortTypes.Output, RTypes.Numeric, true);
            InputPorts[0].DataChanged += (s, e) =>
            {
                OutputPorts[0].Data.Value = "exp(" + InputPorts[0].Data.Value + ")";
            };
        }

        public override string GenerateCode()
        {
            return null;
        }

        public override Node Clone()
        {
            var node = new Exp(Host);

            return node;
        }
    }
}