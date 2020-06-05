using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using VisualSR.Controls;
using VisualSR.Core;

namespace Nodes.Nodes.Math
{
    [Serializable]
    [Export(typeof(Node))]
    public class Vector1D : Node
    {
        private readonly UnrealControlsCollection.TextBox _tb = new UnrealControlsCollection.TextBox();
        private readonly VirtualControl Host;

        [ImportingConstructor]
        public Vector1D([Import("host")] VirtualControl host, [Import("bool")] bool spontaneousAddition = false) : base(
            host, NodeTypes.Basic,
            spontaneousAddition)
        {
            Host = host;
            Title = "Vector 1D";
            Category = "Vectors";
            Description = "Contains a one-dimensional vector";
            AddObjectPort(this, "return value", PortTypes.Output, RTypes.Generic, true, _tb);
            _tb.TextChanged += (sender, args) => OutputPorts[0].Data.Value = _tb.Text;
        }

        public override string GenerateCode()
        {
            return null;
        }

        public override Node Clone()
        {
            var node = new Vector1D(Host, false);

            return node;
        }

        public override void DeSerializeData(List<string> input, List<string> output)
        {
            _tb.Text = output[0];
        }
    }
}