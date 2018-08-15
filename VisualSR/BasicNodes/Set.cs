using System;
using VisualSR.Controls;
using VisualSR.Core;
using VisualSR.Tools;

namespace VisualSR.BasicNodes
{
    public class Set : Node
    {
        private readonly VariableItem _item;
        private readonly VirtualControl Host;

        public Set(VirtualControl host, VariableItem variable, bool spontaneousAddition) : base(host,
            NodeTypes.VariableSet,
            spontaneousAddition)
        {
            Title = "Set";
            Host = host;

            AddExecPort(this, "", PortTypes.Input, "");
            AddExecPort(this, "", PortTypes.Output, "");
            AddObjectPort(this, variable.Name, PortTypes.Input, RTypes.Generic, false);
            Description = @"Sets the value of " + variable.Name + ".";
            if (variable.Type != null)
                NodesManager.ChangeColorOfVariableNode(InputPorts[0], variable.Type);
            variable.Sets++;
            _item = variable;
            _item.DsOfNodes.Add(Id);
            Console.WriteLine("Sets: " + variable.Gets);
        }

        public override string GenerateCode()
        {
            var variableName = _item.Name;

            return variableName + " <- " + InputPorts[0].Data.Value;
        }

        public override void Delete(bool deletedByBrain = false)
        {
            if (_item.Sets > 0) _item.Sets--;
            base.Delete();
        }

        public override Node Clone()
        {
            var node = new Set(Host, _item, false);
            return node;
        }
    }
}