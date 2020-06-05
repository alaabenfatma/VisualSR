using System.ComponentModel.Composition;
using VisualSR.Controls;
using VisualSR.Core;

namespace Nodes.Nodes.R.CSV
{
    [Export(typeof(Node))]
    public class ReadCSV : Node
    {
        private readonly UnrealControlsCollection.CheckBox _cb = new UnrealControlsCollection.CheckBox();
        private readonly UnrealControlsCollection.TextBox _tb = new UnrealControlsCollection.TextBox();
        private readonly UnrealControlsCollection.TextBox _tc = new UnrealControlsCollection.TextBox {MaxLength = 1};
        private readonly VirtualControl Host;

        [ImportingConstructor]
        public ReadCSV([Import("host")] VirtualControl host, [Import("bool")] bool spontaneousAddition = false) : base(
            host, NodeTypes.Basic,
            spontaneousAddition)
        {
            Title = "Read CSV";
            Description = "Reads a CSV file and return a data frame.";
            Host = host;

            Category = "CSV";

            AddObjectPort(this, "File Path", PortTypes.Input, RTypes.Character, false, _tb);
            AddObjectPort(this, "Headers", PortTypes.Input, RTypes.Logical, false, _cb);
            AddObjectPort(this, "Character of separation", PortTypes.Input, RTypes.Character, false, _tc);
            AddObjectPort(this, "return", PortTypes.Output, RTypes.DataFrame, false);

            _tb.TextChanged += (s, e) => { InputPorts[0].Data.Value = _tb.Text; };
            InputPorts[0].DataChanged += (s, e) =>
            {
                if (_tb.Text != InputPorts[0].Data.Value)
                    _tb.Text = InputPorts[0].Data.Value;
                GenerateCode();
            };
            InputPorts[2].DataChanged += (s, e) =>
            {
                if (_tc.Text != InputPorts[2].Data.Value)
                    _tc.Text = InputPorts[2].Data.Value;
                GenerateCode();
            };
            _cb.Checked += (s, e) =>
                GenerateCode();
        }

        public override string GenerateCode()
        {
            var value = InputPorts?[0].Data.Value;
            OutputPorts[0].Data.Value = "read.csv(file='" + value + "',header=" + _cb.IsChecked.ToString().ToUpper() +
                                        ",sep='" + _tc.Text + "')";
            return "# read CSV from" + value;
        }


        public override Node Clone()
        {
            var node = new ReadCSV(Host);

            return node;
        }
    }
}