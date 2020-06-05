using System.ComponentModel.Composition;
using VisualSR.Controls;
using VisualSR.Core;

namespace Nodes.Nodes.R.RCore
{
    [Export(typeof(Node))]
    public class Library : Node
    {
        private readonly UnrealControlsCollection.TextBox _tb = new UnrealControlsCollection.TextBox();
        private readonly VirtualControl Host;

        [ImportingConstructor]
        public Library([Import("host")] VirtualControl host, [Import("bool")] bool spontaneousAddition = false) : base(
            host, NodeTypes.Method,
            spontaneousAddition)
        {
            Title = "Load a library";
            Description = "Loads a library from the installed packages.";
            Host = host;

            Category = "Core";
            AddExecPort(this, "", PortTypes.Input, "");
            AddExecPort(this, "", PortTypes.Output, "");
            AddObjectPort(this, "Library name", PortTypes.Input, RTypes.Character, false, _tb);
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
            return "Library('" + value + "')";
        }


        public override Node Clone()
        {
            var node = new Library(Host);

            return node;
        }
    }
}