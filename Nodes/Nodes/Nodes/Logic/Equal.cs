using System.ComponentModel.Composition;
using VisualSR.Core;
using VisualSR.Tools;

namespace Nodes.Nodes.Logic
{
    [Export(typeof(Node))]
    public class Equal : Node

    {
        private readonly VirtualControl Host;

        [ImportingConstructor]
        public Equal([Import("host")] VirtualControl host, [Import("bool")] bool spontaneousAddition = false) : base(
            host, NodeTypes.Basic, spontaneousAddition)
        {
            Title = "==";
            Category = "Logic nodes";
            Host = host;

            AddObjectPort(this, "", PortTypes.Input, RTypes.Generic, false);
            AddObjectPort(this, "", PortTypes.Input, RTypes.Generic, false);
            AddObjectPort(this, "==", PortTypes.Output, RTypes.Logical, true);
            MouseRightButtonDown += (sender, args) => GenerateCode();
            foreach (var port in InputPorts)
                port.DataChanged += (sender, args) => { GenerateCode(); };
            InputPorts[1].LinkChanged += (sender, args) =>
            {
                if (!InputPorts[1].Linked)
                    InputPorts[1].Data.Value = "";
            };
            InputPorts[0].LinkChanged += (sender, args) =>
            {
                if (!InputPorts[0].Linked)
                    InputPorts[0].Data.Value = "";
            };
        }

        public Equal()
        {
        }


        public override string GenerateCode()
        {
            OutputPorts[0].Data.Value = "((" + InputPorts[0].Data.Value + ")==(" + InputPorts[1].Data.Value + "))";
            return OutputPorts[0].Data.Value;
        }

        public override Node Clone()
        {
            var node = new Equal(Host, false);

            return node;
        }

        public override string Serialize()
        {
            NodeProperties = new NodeProperties(this);
            Cipher.SerializeToFile(@"sum.txt", NodeProperties, true);
            return null;
        }

        public override string DeSerialize()
        {
            return null;
        }
    }
}