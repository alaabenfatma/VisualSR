using System.ComponentModel.Composition;
using VisualSR.Controls;
using VisualSR.Core;

namespace Nodes.Nodes.Logic
{
    [Export(typeof(Node))]
    public class LogicalNode : Node
    {
        private readonly UnrealControlsCollection.CheckBox _cb = new UnrealControlsCollection.CheckBox();
        private readonly VirtualControl Host;

        [ImportingConstructor]
        public LogicalNode([Import("host")] VirtualControl host,
            [Import("bool")] bool spontaneousAddition = false) : base(host, NodeTypes.Basic,
            spontaneousAddition)
        {
            Title = "Bool";
            Host = host;

            Category = "Logic nodes";
            AddObjectPort(this, "return value", PortTypes.Output, RTypes.Logical, true, _cb);
            _cb.GotFocus += (sender, args) =>
            {
                _cb.IsChecked = !_cb.IsChecked;
                OutputPorts[0].Data.Value = _cb.IsChecked.Value.ToString();
            };
        }

        public override string GenerateCode()
        {
            return null;
        }

        public override Node Clone()
        {
            var node = new LogicalNode(Host, false);

            return node;
        }
    }
}