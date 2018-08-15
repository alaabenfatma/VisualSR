using System.Windows.Media;
using VisualSR.Core;
using VisualSR.Tools;

namespace VisualSR.BasicNodes
{
    public class StartNode : Node
    {
        public StartNode(VirtualControl host, bool addDirectly = false) : base(host, NodeTypes.Root)
        {
            Background = Brushes.WhiteSmoke;
            Title = "Start";
            AddExecPort(this, "", PortTypes.Output, "Execute first node");
            Description = "The root node.";
        }

        public override string GenerateCode()
        {
            var path = Hub.WorkSpace.Replace(@"\", @"\\");
            return @"#Artificial code. Generated using VisualSR.
setwd(""" + path + @""")";
        }

        public override void Delete(bool deletedByBrain = false)
        {
            //Not a chance :-)
        }

        public override Node Clone()
        {
            return null;
        }
    }
}