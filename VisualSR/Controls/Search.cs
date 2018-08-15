/*Copyright 2018 ALAA BEN FATMA

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.*/
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using VisualSR.Core;
using VisualSR.Properties;
using VisualSR.Tools;

namespace VisualSR.Controls
{
    public class Search : Window, INotifyPropertyChanged
    {
        private readonly VirtualControl _host;
        private TextBlock clear;
        private TextBlock go;
        private ListView lv;

        private TextBox tb;

        public Search(VirtualControl host)
        {
            WindowStyle = WindowStyle.ToolWindow;
            _host = host;
            Style = FindResource("Search") as Style;
            Loaded += (s, e) =>
            {
                clear = Template.FindName("Clear", this) as TextBlock;
                go = Template.FindName("FindButton", this) as TextBlock;
                tb = Template.FindName("NodeName", this) as TextBox;
                lv = Template.FindName("FoundNodes", this) as ListView;
                clear.MouseLeftButtonUp += (ss, ee) => tb.Clear();
                go.MouseLeftButtonUp += Go_MouseLeftButtonUp;
                Topmost = true;
            };
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void Go_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            lv.Items.Clear();
            foreach (var node in _host.Nodes)
                if (node.Search(tb.Text) != null)
                {
                    var tv = new TreeView {Background = new SolidColorBrush(Color.FromArgb(35, 35, 35, 35))};
                    tv.Items.Add(node.Search(tb.Text));
                    lv.Items.Add(tv);
                }
        }

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class FoundItem : TreeViewItem
    {
        private readonly Path ctrl = new Path {Width = 15, Height = 15, Margin = new Thickness(0, -2, 0, 0)};

        private readonly TextBlock hint =
            new TextBlock {Foreground = Brushes.WhiteSmoke, Background = Brushes.Transparent};

        private string _hint;

        private Brush brush;

        public ItemTypes Type;

        public FoundItem(Brush b = null)
        {
            IsExpanded = true;
            Loaded += (s, e) =>
            {
                MouseDoubleClick += FoundItem_MouseDoubleClick;


                var sp = new StackPanel {Orientation = Orientation.Horizontal, MaxHeight = 20};
                sp.Children.Add(ctrl);
                sp.Children.Add(hint);
                Header = sp;
                if (Type == ItemTypes.Port)
                {
                    ctrl.Style = FindResource("ObjectPin") as Style;
                    ctrl.Stroke = b;
                }
                else
                {
                    ctrl.Style = FindResource("ExecPin") as Style;
                }
            };
        }

        public string Hint
        {
            get { return _hint; }
            set
            {
                _hint = value;
                if (hint != null) hint.Text = value;
            }
        }

        public Brush Brush
        {
            get { return brush; }
            set
            {
                brush = value;
                ctrl.Stroke = value;
                ctrl.Fill = value;
            }
        }

        public Node foundNode { get; set; }

        private void FoundItem_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            foundNode.Host.GoForNode(foundNode);
        }
    }
}