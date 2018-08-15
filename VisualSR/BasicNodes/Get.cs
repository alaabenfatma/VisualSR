using System;
using VisualSR.Controls;
using VisualSR.Core;
using VisualSR.Tools;

namespace VisualSR.BasicNodes
{
    public class Get : Node
    {
        private readonly VariableItem _item;
        private readonly VirtualControl Host;
        
        public Get(VirtualControl host, VariableItem variable, bool spontaneousAddition) : base(host,
            NodeTypes.VariableGet,
            spontaneousAddition)
        {
            Host = host;
            Title = string.Empty;
            AddObjectPort(this, "Return " + variable.Name, PortTypes.Output, RTypes.Generic, true);
            Description = @"Gets the value of " + variable.Name + ".";
            OutputPorts[0].Data.Value = variable.Name;
            if (variable.Type != null)
                NodesManager.ChangeColorOfVariableNode(OutputPorts[0], variable.Type);
            _item = variable;
            _item.Gets++;
            _item.DsOfNodes.Add(Id);
        }

        public void Update(string name)
        {
            OutputPorts[0].Data.Value = name;
        }
        public override string GenerateCode()
        {
            throw new NotImplementedException();
        }

        public override void Delete(bool deletedByBrain)
        {
            if (_item.Gets > 0) _item.Gets--;
            base.Delete();
        }

        public override Node Clone()
        {
            var node = new Get(Host, _item, false);
            return node;
        }
    }
}