using System;
using System.ComponentModel.Composition;
using VisualSR.Core;

namespace Nodes.Nodes.Math
{
    [Serializable]
    [Export(typeof(Node))]
    public class Tolower : Node
    {
        private readonly VirtualControl Host;

        [ImportingConstructor]
        public Tolower([Import("host")] VirtualControl host, [Import("bool")] bool spontaneousAddition = false) : base(
            host,
            NodeTypes.Basic,
            spontaneousAddition)
        {
            Host = host;
            Title = "Text to lower";
            Category = "Characters";
            Description = "Converts a set of characters to lowercased characters.";
            AddObjectPort(this, "Text", PortTypes.Input, RTypes.Character, false);
            AddObjectPort(this, "return ", PortTypes.Output, RTypes.Character, true);
            InputPorts[0].DataChanged += (s, e) =>
            {
                OutputPorts[0].Data.Value = "tolower(" + InputPorts[0].Data.Value + ")";
            };
        }

        public override string GenerateCode()
        {
            return null;
        }

        public override Node Clone()
        {
            var node = new Tolower(Host);

            return node;
        }
    }
}