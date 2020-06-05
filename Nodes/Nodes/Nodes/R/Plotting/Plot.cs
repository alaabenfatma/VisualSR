using System.ComponentModel.Composition;
using VisualSR.Controls;
using VisualSR.Core;

namespace Nodes.Nodes.R.Plotting
{
    [Export(typeof(Node))]
    public class Plot : Node
    {
        private readonly UnrealControlsCollection.TextBox _tb = new UnrealControlsCollection.TextBox();
        private readonly VirtualControl Host;

        [ImportingConstructor]
        public Plot([Import("host")] VirtualControl host, [Import("bool")] bool spontaneousAddition = false) : base(
            host, NodeTypes.Method,
            spontaneousAddition)
        {
            Title = "Plot";
            Host = host;

            Category = "Plotting";
            AddExecPort(this, "", PortTypes.Input, "");

            AddExecPort(this, "", PortTypes.Output, "");
            AddObjectPort(this, "", PortTypes.Input, RTypes.Generic, false, _tb);

            Description = "Uses R's built-in plotting function to generate a plot out of the entered data.";
            _tb.TextChanged += (s, e) => { InputPorts[0].Data.Value = _tb.Text; };
            InputPorts[0].DataChanged += (s, e) =>
            {
                if (_tb.Text != InputPorts[0].Data.Value)
                    _tb.Text = InputPorts[0].Data.Value;
            };
        }

        public override string GenerateCode()
        {
            var value = InputPorts?[0].Data.Value;

            if (InputPorts != null && !InputPorts[0].Linked)
                return "plot('" + value + "')";
            return "plot(" + value + ")";
        }


        public override Node Clone()
        {
            var node = new Plot(Host, false);

            return node;
        }
    }
}