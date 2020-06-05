using System.ComponentModel.Composition;
using VisualSR.Controls;
using VisualSR.Core;
using VisualSR.Tools;

namespace Nodes.Nodes.Characters
{
    [Export(typeof(Node))]
    public class Grep : Node

    {
        private readonly UnrealControlsCollection.CheckBox _cb =
            new UnrealControlsCollection.CheckBox {IsChecked = true};

        private readonly UnrealControlsCollection.CheckBox _cbc =
            new UnrealControlsCollection.CheckBox {IsChecked = false};

        private readonly UnrealControlsCollection.TextBox _p = new UnrealControlsCollection.TextBox();
        private readonly UnrealControlsCollection.TextBox _t = new UnrealControlsCollection.TextBox();
        private readonly VirtualControl Host;

        [ImportingConstructor]
        public Grep([Import("host")] VirtualControl host, [Import("bool")] bool spontaneousAddition = false) : base(
            host, NodeTypes.Basic, spontaneousAddition)
        {
            Title = "Grep";
            Host = host;

            Category = "Characters";
            AddObjectPort(this, "Text", PortTypes.Input, RTypes.Character, false, _t);
            AddObjectPort(this, "Pattern", PortTypes.Input, RTypes.Generic, false, _p);
            AddObjectPort(this, "Case sensitive", PortTypes.Input, RTypes.Logical, false, _cbc);
            AddObjectPort(this, "Normal Text", PortTypes.Input, RTypes.Logical, false, _cb);
            AddObjectPort(this, "Extracted value", PortTypes.Output, RTypes.Character, true);
            MouseRightButtonDown += (sender, args) => GenerateCode();
            _t.TextChanged += (sender, args) =>
            {
                InputPorts[0].Data.Value = _t.Text;
                GenerateCode();
            };
            _p.TextChanged += (sender, args) =>
            {
                InputPorts[1].Data.Value = _p.Text;
                GenerateCode();
            };
            _cbc.Checked += (sender, args) =>
            {
                InputPorts[2].Data.Value = _cbc.IsChecked.ToString().ToUpper();
                GenerateCode();
            };
            _cb.Checked += (sender, args) =>
            {
                InputPorts[3].Data.Value = _cb.IsChecked.ToString().ToUpper();
                GenerateCode();
            };
            foreach (var port in InputPorts)
                port.DataChanged += (sender, args) => { GenerateCode(); };
            InputPorts[1].LinkChanged += (sender, args) =>
            {
                if (!InputPorts[1].Linked)
                    InputPorts[1].Data.Value = _p.Text;
            };
            InputPorts[0].LinkChanged += (sender, args) =>
            {
                if (!InputPorts[0].Linked)
                    InputPorts[0].Data.Value = _t.Text;
            };
            InputPorts[3].LinkChanged += (sender, args) =>
            {
                if (!InputPorts[3].Linked)
                    InputPorts[3].Data.Value = _cb.IsChecked.ToString().ToUpper();
            };
            InputPorts[2].LinkChanged += (sender, args) =>
            {
                if (!InputPorts[2].Linked)
                    InputPorts[2].Data.Value = _cbc.IsChecked.ToString().ToUpper();
            };
            InputPorts[0].DataChanged += (s, e) =>
            {
                if (_t.Text != InputPorts[0].Data.Value)
                    _t.Text = InputPorts[0].Data.Value;
            };
            InputPorts[1].DataChanged += (s, e) =>
            {
                if (_p.Text != InputPorts[1].Data.Value)
                    _p.Text = InputPorts[1].Data.Value;
            };
            InputPorts[3].DataChanged += (s, e) =>
            {
                if (_cb.IsChecked.ToString().ToUpper() != InputPorts[3].Data.Value)
                    _cb.IsChecked = MagicLaboratory.ConvertFromString(InputPorts[3].Data.Value);
            };
            InputPorts[2].DataChanged += (s, e) =>
            {
                if (_cb.IsChecked.ToString().ToUpper() != InputPorts[2].Data.Value)
                    _cbc.IsChecked = MagicLaboratory.ConvertFromString(InputPorts[2].Data.Value);
            };
        }


        public override string GenerateCode()
        {
            OutputPorts[0].Data.Value =
                $"grep({_t},{_p},ignore.case={_cbc.IsChecked.ToString().ToUpper()},fixed={_cb.IsChecked.ToString().ToUpper()})";
            return OutputPorts[0].Data.Value;
        }

        public override Node Clone()
        {
            var node = new Substr(Host);

            return node;
        }
    }
}