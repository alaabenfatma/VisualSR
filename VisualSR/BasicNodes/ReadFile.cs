using System;
using VisualSR.Controls;
using VisualSR.Core;
using VisualSR.Tools;

namespace VisualSR.BasicNodes
{
    public class ReadFile : Node
    {
        private readonly UnrealControlsCollection.TextBox _tb = new UnrealControlsCollection.TextBox {MaxWidth = 150};
        private readonly VirtualControl Host;

        public ReadFile(VirtualControl host, bool spontaneousAddition = false) : base(
            host, NodeTypes.Basic,
            spontaneousAddition)
        {
            Host = host;
            Title = "String";
            Description = "This node contains a list of charaters.";
            Category = "Basic";
            AddObjectPort(this, "return ", PortTypes.Output, RTypes.Character, true, _tb);
            OutputPorts[0].DataChanged += ReadFile_DataChanged;
            _tb.TextChanged += (sender, args) => OutputPorts[0].Data.Value = _tb.Text;
        }

        private void ReadFile_DataChanged(object sender, EventArgs e)
        {
            if (OutputPorts[0].Data.Value != _tb.Text)
                _tb.Text = MagicLaboratory.CorrectWindowsPath(OutputPorts[0].Data.Value);
        }

        public override string GenerateCode()
        {
            return null;
        }

        public override Node Clone()
        {
            var node = new ReadFile(Host, false);

            return node;
        }
    }
}