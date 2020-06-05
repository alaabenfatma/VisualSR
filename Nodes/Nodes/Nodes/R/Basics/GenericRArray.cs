using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Windows;
using VisualSR.Controls;
using VisualSR.Core;

namespace Nodes.Nodes.R.Basics
{
    [Export(typeof(Node))]
    public class GenericRArray : Node
    {
        private readonly List<string> _dataToCombine = new List<string>();
        private readonly VirtualControl Host;
        private int numOfElements = 2;

        [ImportingConstructor]
        public GenericRArray([Import("host")] VirtualControl host,
            [Import("bool")] bool spontaneousAddition = false) : base(
            host, NodeTypes.Method,
            spontaneousAddition)
        {
            Title = "Generic array";
            Host = host;
            Category = "Arrays";
            AddExecPort(this, "", PortTypes.Input, "");
            AddExecPort(this, "", PortTypes.Output, "");
            AddObjectPort(this, "[1]", PortTypes.Input, RTypes.Generic, true);
            AddObjectPort(this, "return value", PortTypes.Output, RTypes.ArrayOrFactorOrListOrMatrix, true);
            Loaded += RArray_Loaded;
        }

        private void RArray_Loaded(object sender, RoutedEventArgs e)
        {
            if (InputPortsControls.Children.Count != 0) return;

            var addpin = new UnrealControlsCollection.AddPin();
            Height += 25;
            addpin.Click += (s, p) =>
            {
                AddObjectPort(this, "[" + (numOfElements++) + "]", PortTypes.Input,
                    RTypes.Generic, false);
            };

            InputPortsControls.Children.Add(addpin);
        }

        public override void DynamicPortsGeneration(int n)
        {
            for (var i = 0; i < n - 1; i++)
                AddObjectPort(this, "[" + InputPorts.Count + 1 + "]", PortTypes.Input,
                    RTypes.Generic, false);
        }

        private void OnDataChanged(object sender, EventArgs eventArgs)
        {
            _dataToCombine.Clear();
            var s = "";
            foreach (ObjectsConnector conn in InputPorts[0].ConnectedConnectors)
            {
                _dataToCombine.Add(((ObjectPort) conn.StartPort).Data.Value);
                s += "\n" + ((ObjectPort) conn.StartPort).Data.Value;
            }
        }

        public override string GenerateCode()
        {
            throw new NotImplementedException();
        }

        public override Node Clone()
        {
            var node = new GenericRArray(Host, false);

            return node;
        }
    }
}