using System.Windows.Media;
using VisualSR.Core;

namespace VisualSR.BasicNodes
{
    public class ReturnNode : Node
    {
        public ReturnNode(VirtualControl host, bool spontaneousAddition = true) : base(host, NodeTypes.Return,
            spontaneousAddition)
        {
            Background = Brushes.WhiteSmoke;
            Title = "Return";
            Description = "This value will reflect the passed value as a return value.";
            AddExecPort(this, "", PortTypes.Input, "");
            AddObjectPort(this, "return value", PortTypes.Input, RTypes.Generic, false);
        }
        
    }
}