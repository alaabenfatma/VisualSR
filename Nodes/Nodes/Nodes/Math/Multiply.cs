using System.ComponentModel.Composition;
using VisualSR.Controls;
using VisualSR.Core;
using VisualSR.Tools;

namespace Nodes.Nodes.Math
{
    [Export(typeof(Node))]
    public class Multiply : Node

    {
        private readonly UnrealControlsCollection.TextBox _a = new UnrealControlsCollection.TextBox();
        private readonly UnrealControlsCollection.TextBox _b = new UnrealControlsCollection.TextBox();
        private readonly VirtualControl Host;

        [ImportingConstructor]
        public Multiply([Import("host")] VirtualControl host, [Import("bool")] bool spontaneousAddition = false) : base(
            host, NodeTypes.Basic, spontaneousAddition)
        {
            Title = "Multiply";
            Description = "Multiplies A and B.";

            Category = "Math nodes";
            Host = host;
            AddObjectPort(this, "A", PortTypes.Input, RTypes.Numeric, false, _a);
            AddObjectPort(this, "B", PortTypes.Input, RTypes.Numeric, false, _b);
            AddObjectPort(this, "A * B", PortTypes.Output, RTypes.Numeric, true);
            MouseRightButtonDown += (sender, args) => GenerateCode();
            _a.TextChanged += (sender, args) =>
            {
                InputPorts[0].Data.Value = _a.Text;
                GenerateCode();
            };
            _b.TextChanged += (sender, args) =>
            {
                InputPorts[1].Data.Value = _b.Text;
                GenerateCode();
            };
            foreach (var port in InputPorts)
                port.DataChanged += (sender, args) => { GenerateCode(); };
            InputPorts[1].LinkChanged += (sender, args) =>
            {
                if (!InputPorts[1].Linked)
                    InputPorts[1].Data.Value = _b.Text;
            };
            InputPorts[0].LinkChanged += (sender, args) =>
            {
                if (!InputPorts[0].Linked)
                    InputPorts[0].Data.Value = _a.Text;
            };
            InputPorts[0].DataChanged += (s, e) =>
            {
                if (_a.Text != InputPorts[0].Data.Value)
                    _a.Text = InputPorts[0].Data.Value;
            };
            InputPorts[1].DataChanged += (s, e) =>
            {
                if (_b.Text != InputPorts[1].Data.Value)
                    _b.Text = InputPorts[1].Data.Value;
            };
        }

        public Multiply()
        {
        }


        public override string GenerateCode()
        {
            OutputPorts[0].Data.Value = "((" + InputPorts[0].Data.Value + ")*(" + InputPorts[1].Data.Value + "))";
            return OutputPorts[0].Data.Value;
        }

        public override Node Clone()
        {
            var node = new Multiply(Host, false);

            return node;
        }

        public override string Serialize()
        {
            NodeProperties = new NodeProperties(this);
            Cipher.SerializeToFile(@"multiply.txt", NodeProperties, true);
            return null;
        }

        public override string DeSerialize()
        {
            return null;
        }
    }
}