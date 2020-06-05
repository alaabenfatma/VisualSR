using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Text;
using System.Windows;
using VisualSR.Controls;
using VisualSR.Core;

namespace Nodes.Nodes.R.Basics
{
    [Export(typeof(Node))]
    public class CharacterRVector : Node
    {
        private readonly List<string> _dataToCombine = new List<string>();
        private readonly VirtualControl Host;

        [ImportingConstructor]
        public CharacterRVector([Import("host")] VirtualControl host,
            [Import("bool")] bool spontaneousAddition = false) : base(
            host, NodeTypes.Method,
            spontaneousAddition)
        {
            Title = "Characters Vector";
            Host = host;
            Category = "Vectors";
            AddExecPort(this, "", PortTypes.Input, "");
            AddExecPort(this, "", PortTypes.Output, "");
            AddObjP();
            AddObjectPort(this, "return value", PortTypes.Output, RTypes.ArrayOrFactorOrListOrMatrix, true);
            Loaded += RArray_Loaded;
        }

        private void AddObjP()
        {
            var newtb = new UnrealControlsCollection.TextBox();
            AddObjectPort(this, "[" + (InputPorts.Count + 1) + "]", PortTypes.Input,
                RTypes.Character, false);
            var x = InputPorts.Count - 1;

            newtb.TextChanged += (ss, ee) => { InputPorts[x].Data.Value = newtb.Text; };
            InputPorts[x].Control = newtb;
            InputPorts[x].DataChanged += (s, e) =>
            {
                if (newtb.Text != InputPorts[x].Data.Value)
                    newtb.Text = InputPorts[x].Data.Value;
            };
        }

        public override void DynamicPortsGeneration(int n)
        {
            for (var i = 0; i < n - 1; i++)
                AddObjP();
        }

        private void RArray_Loaded(object sender, RoutedEventArgs e)
        {
            if (InputPortsControls.Children.Count != 0) return;

            var addpin = new UnrealControlsCollection.AddPin();
            Height += 25;
            addpin.Click += (s, p) => { AddObjP(); };
            InputPortsControls.Children.Add(addpin);
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
            var sb = new StringBuilder();
            sb.Append("c(");
            foreach (var ip in InputPorts)
                sb.Append($"'{ip.Data.Value}',");
            sb.Append(')');
            var code = sb.ToString().Replace(",)", ")");
            OutputPorts[0].Data.Value = code;
            return "#Generated a vector of characters : " + code;
        }

        public override Node Clone()
        {
            var node = new CharacterRVector(Host, false);

            return node;
        }
    }
}