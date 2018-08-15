/*Copyright 2018 ALAA BEN FATMA

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.*/
/* 
 * Similar to the list of variables; however, it is made to host functions. 
 * A function is nothing but a set of nodes
*/
//using System;
//using System.Collections.Generic;
//using System.ComponentModel;
//using System.Drawing;
//using System.Linq;
//using System.Windows;
//using System.Windows.Controls;
//using System.Windows.Input;
//using System.Windows.Media;
//using VisualSR.Core;
//using VisualSR.Properties;
//using Brushes = System.Windows.Media.Brushes;

//namespace VisualSR.Controls
//{
//    public class Gate : Control
//    {
//        public Gate()
//        {
//            Style = FindResource("GateProperties") as Style;
//        }
//    }
//    public class FunctionsList : Control
//    {
//        public VirtualControl Host;
//        public FunctionsList(VirtualControl host)
//        {
//            Style = FindResource("FunctionsList") as Style;
//            Loaded += (s, e) =>
//            {
//                var lv = (Template.FindName("Lv", this) as ListView);
//                var add = (Template.FindName("FunAdd", this) as Button);
//                var remove = (Template.FindName("FunRemove", this) as Button);
//                if (add != null) add.Click += (ss, ee) => { lv.Items.Add(new FunctionItem("") {Parent = lv}); };
//                if (remove != null) remove.Click += (ss, ee) => { lv.Items.Remove(lv.SelectedItem); };
//            };
//        }
//    }

//    public class FunctionItem : Control, INotifyPropertyChanged
//    {
//        public ListView Parent { get; set; }
//        private string _icon;
//        private string _name;
//        private int _ref;
//        public IList<string> IdOfNodes = new List<string>();
//        public List<NodeItem> NodesTreeItems = new List<NodeItem>();

//        int ValidName(string name)
//        {
//            int s = 0;
//            foreach(var item in Parent.Items)
//                if ((item as FunctionItem).Name == name)
//                    s++;
//            return s;
//        }
//        public FunctionItem(string name)
//        {
//            Icon = "MediaResources/FunctionsListIcons/Icons/Function.png";

//            Name = name;

//            Style = FindResource("FunctionItem") as Style;
//            Loaded += (s, e) =>
//            {
//                Rename();
//                (Template.FindName("RenameTB", this) as TextBox).KeyUp += (ss, ee) =>
//                {
//                    if (ee.Key == Key.Enter)
//                        ApplyNewName();
//                    else if (ee.Key == Key.Escape)
//                        Ignore();
//                };
//                (Template.FindName("RenameTB", this) as TextBox).LostFocus += (ss, ee) =>
//                {
//                    if (!((Template.FindName("RenameTB", this) as TextBox).IsFocused))
//                        ApplyNewName();
//                };

//                (Template.FindName("FunctionName", this) as TextBlock).MouseLeftButtonDown += (ss, ee) => Rename();
//                (Template.FindName("InputGateAdd", this) as Button).Click += (ss, ee) =>
//                {
//                    (Template.FindName("InputGatesLv", this) as ListView).Items.Add(new Gate());
//                }; (Template.FindName("OutputGateAdd", this) as Button).Click += (ss, ee) =>
//                {
//                    (Template.FindName("OutputGatesLv", this) as ListView).Items.Add(new Gate());
//                };
//            };
//        }

//        public string Code { get; set; }

//        /// <summary>
//        ///     Returns how many times we did set/get the value of this variable.
//        /// </summary>
//        public int Calls
//        {
//            get { return _ref; }
//            set
//            {
//                _ref = value;
//                OnPropertyChanged("Ref");
//            }
//        }

//        public string Ref => @"This function has been called " + Calls + " times.";

//        public string Name
//        {
//            get { return _name; }
//            set
//            {
//                _name = value;
//                OnPropertyChanged("Name");
//            }
//        }

//        public string Icon
//        {
//            get { return _icon; }
//            set
//            {
//                _icon = value;
//                OnPropertyChanged("Icon");
//            }
//        }

//        public event PropertyChangedEventHandler PropertyChanged;

//        private void Ignore()
//        {
//            var oldName = Name;
//            if (ValidName((Template.FindName("RenameTB", this) as TextBox).Text) >0)
//            {
//                Name = (Template.FindName("RenameTB", this) as TextBox).Text + ValidName((Template.FindName("RenameTB", this) as TextBox).Text);
//                var s = 1;
//                while (ValidName(Name) > 1)
//                {
//                    s++;
//                    Name = oldName+s;
//                }
//            }
//            (Template.FindName("RenameTB", this) as TextBox).Visibility = Visibility.Collapsed;
//            (Template.FindName("FunctionName", this) as TextBlock).Visibility = Visibility.Visible;
//            (Template.FindName("FunctionName", this) as TextBlock).Text = Name;
//        }

//        private void Rename()
//        {

//            (Template.FindName("RenameTB", this) as TextBox).Text =
//                Name;
//            (Template.FindName("FunctionName", this) as TextBlock).Visibility = Visibility.Collapsed;
//            (Template.FindName("RenameTB", this) as TextBox).Visibility = Visibility.Visible;
//            (Template.FindName("RenameTB", this) as TextBox).SelectAll();
//            (Template.FindName("RenameTB", this) as TextBox).Focus();
//        }

//        private void ApplyNewName()
//        {
//            if (ValidName((Template.FindName("RenameTB", this) as TextBox).Text)>1)
//            {

//                (Template.FindName("RenameTB", this) as TextBox).BorderBrush = Brushes.Red;
//                return;
//            }
//            (Template.FindName("RenameTB", this) as TextBox).BorderBrush = Brushes.LightBlue;

//            (Template.FindName("RenameTB", this) as TextBox).Visibility = Visibility.Collapsed;

//            (Template.FindName("FunctionName", this) as TextBlock).Visibility = Visibility.Visible;

//            (Template.FindName("FunctionName", this) as TextBlock).Text =
//                (Template.FindName("RenameTB", this) as TextBox).Text;
//            Name = (Template.FindName("RenameTB", this) as TextBox).Text;

//        }

//        public string SerializedMetaData()
//        {
//            return null;
//        }


//        [NotifyPropertyChangedInvocator]
//        protected virtual void OnPropertyChanged(string propertyName = null)
//        {
//            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
//        }
//    }
//}

