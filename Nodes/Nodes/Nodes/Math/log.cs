using System;
using System.ComponentModel.Composition;
using VisualSR.Core;

namespace Nodes.Nodes.Math
{
    [Serializable]
    [Export(typeof(Node))]
    public class Log : Node
    {
        private readonly VirtualControl Host;

        [ImportingConstructor]
        public Log([Import("host")] VirtualControl host, [Import("bool")] bool spontaneousAddition = false) : base(host,
            NodeTypes.Basic,
            spontaneousAddition)
        {
            Host = host;
            Title = "Log";
            Category = "Math nodes";
            Description = "Calculates log(x).";
            AddObjectPort(this, "value", PortTypes.Input, RTypes.Numeric, false);
            AddObjectPort(this, "return log(value)", PortTypes.Output, RTypes.Numeric, true);
            InputPorts[0].DataChanged += (s, e) =>
            {
                OutputPorts[0].Data.Value = "log(" + InputPorts[0].Data.Value + ")";
            };
        }

        public override string GenerateCode()
        {
            return null;
        }

        public override Node Clone()
        {
            var node = new Log(Host);

            return node;
        }
    }
}