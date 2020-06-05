using System.ComponentModel.Composition;
using VisualSR.Controls;
using VisualSR.Core;

namespace Nodes.Nodes.Characters
{
    [Export(typeof(Node))]
    public class Substr : Node

    {
        private readonly UnrealControlsCollection.TextBox _start = new UnrealControlsCollection.TextBox();
        private readonly UnrealControlsCollection.TextBox _stop = new UnrealControlsCollection.TextBox();
        private readonly UnrealControlsCollection.TextBox _t = new UnrealControlsCollection.TextBox();
        private readonly VirtualControl Host;

        [ImportingConstructor]
        public Substr([Import("host")] VirtualControl host, [Import("bool")] bool spontaneousAddition = false) : base(
            host, NodeTypes.Basic, spontaneousAddition)
        {
            Title = "Sub Text";
            Host = host;

            Category = "Characters";
            AddObjectPort(this, "Text", PortTypes.Input, RTypes.Character, false, _t);
            AddObjectPort(this, "Start Index", PortTypes.Input, RTypes.Numeric, false, _start);
            AddObjectPort(this, "Stop Index", PortTypes.Input, RTypes.Numeric, false, _stop);
            AddObjectPort(this, "Extracted value", PortTypes.Output, RTypes.Character, true);
            MouseRightButtonDown += (sender, args) => GenerateCode();
            _t.TextChanged += (sender, args) =>
            {
                InputPorts[0].Data.Value = _t.Text;
                GenerateCode();
            };
            _start.TextChanged += (sender, args) =>
            {
                InputPorts[1].Data.Value = _start.Text;
                GenerateCode();
            };
            _stop.TextChanged += (sender, args) =>
            {
                InputPorts[2].Data.Value = _stop.Text;
                GenerateCode();
            };
            foreach (var port in InputPorts)
                port.DataChanged += (sender, args) => { GenerateCode(); };
            InputPorts[1].LinkChanged += (sender, args) =>
            {
                if (!InputPorts[1].Linked)
                    InputPorts[1].Data.Value = _start.Text;
            };
            InputPorts[0].LinkChanged += (sender, args) =>
            {
                if (!InputPorts[0].Linked)
                    InputPorts[0].Data.Value = _t.Text;
            };
            InputPorts[2].LinkChanged += (sender, args) =>
            {
                if (!InputPorts[2].Linked)
                    InputPorts[2].Data.Value = _stop.Text;
            };
            InputPorts[0].DataChanged += (s, e) =>
            {
                if (_t.Text != InputPorts[0].Data.Value)
                    _t.Text = InputPorts[0].Data.Value;
            };
            InputPorts[1].DataChanged += (s, e) =>
            {
                if (_start.Text != InputPorts[1].Data.Value)
                    _start.Text = InputPorts[1].Data.Value;
            };
            InputPorts[2].DataChanged += (s, e) =>
            {
                if (_stop.Text != InputPorts[2].Data.Value)
                    _stop.Text = InputPorts[2].Data.Value;
            };
        }

        public Substr()
        {
        }


        public override string GenerateCode()
        {
            OutputPorts[0].Data.Value = "substr('" + _t.Text + "',start=" + _start.Text + ",stop=" + _stop.Text + ")";
            return OutputPorts[0].Data.Value;
        }

        public override Node Clone()
        {
            var node = new Substr(Host);

            return node;
        }
    }
}