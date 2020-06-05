using System.ComponentModel.Composition;
using System.Windows.Controls;
using VisualSR.Controls;
using VisualSR.Core;

namespace Nodes.Nodes.R.Basics
{
    [Export(typeof(Node))]
    public class Print : Node
    {
        private readonly UnrealControlsCollection.TextBox _tb = new UnrealControlsCollection.TextBox();
        private readonly VirtualControl Host;

        [ImportingConstructor]
        public Print([Import("host")] VirtualControl host, [Import("bool")] bool spontaneousAddition = false) : base(
            host, NodeTypes.Method,
            spontaneousAddition)
        {
            Title = "Print";
            Host = host;

            Category = "Console";
            AddExecPort(this, "", PortTypes.Input, "");

            AddExecPort(this, "", PortTypes.Output, "");
            AddObjectPort(this, "", PortTypes.Input, RTypes.Character, false, _tb);

            Description = "Prints out the input on the sceen.";
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
                return "print('" + value + "')";
            return "print(" + value + ")";
        }


        public override Node Clone()
        {
            var node = new Print(Host, false);

            return node;
        }
    }
}