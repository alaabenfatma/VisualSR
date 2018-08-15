/*Copyright 2018 ALAA BEN FATMA

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.*/
using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using VisualSR.Tools;

namespace VisualSR.Core
{
    public class Comment : Control, IDisposable
    {
        private readonly VirtualControl _host;
        private StackPanel _p = new StackPanel();

        public Comment(ObservableCollection<Node> nodes, VirtualControl host)
        {
            CalculateSize(nodes);
            Style = FindResource("Comment") as Style;
            host.AddChildren(this, X, Y);
            Panel.SetZIndex(this, ZIndexes.CommentIndex);
            _host = host;
            MouseDown += Comment_MouseDown;
            MouseUp += Comment_MouseUp;

            Loaded += (e, r) =>
            {
                {
                    _p = (StackPanel) Template.FindName("CornerImage_Resize", this);
                    Canvas.SetLeft(_p, Width - 15);
                    Canvas.SetTop(_p, Height - 15);
                    _p.PreviewMouseDown += P_MouseDown;
                    _p.MouseEnter += (m, le) => { Cursor = Cursors.SizeNWSE; };
                    _p.MouseUp += P_MouseUp;
                    ContextMenu = DeleteComment();
                }
            };
            host.Comments.Add(this);
        }

        public string Summary { get; set; } = "Write your comment...";

        public double Y
        {
            get { return Canvas.GetTop(this); }
            set { Canvas.SetTop(this, value); }
        }

        public double X
        {
            get { return Canvas.GetLeft(this); }
            set { Canvas.SetLeft(this, value); }
        }


        public void Dispose()
        {
            _host.Children.Remove(this);
            _host.Comments.Remove(this);
            MouseUp -= Comment_MouseUp;
            _p.PreviewMouseDown -= P_MouseDown;
            _p.MouseEnter -= (m, le) => Cursor = Cursors.SizeNWSE;
            _p.MouseUp -= P_MouseUp;
        }

        private ContextMenu DeleteComment()
        {
            var cm = new ContextMenu();
            var delete = new TextBlock
            {
                Text = "Uncomment",
                Foreground = Brushes.WhiteSmoke
            };
            var uncomment = new MenuItem {Header = delete};
            uncomment.Click += (s, e) => { Dispose(); };
            cm.Items.Add(uncomment);
            return cm;
        }

        private void Comment_MouseDown(object sender, MouseButtonEventArgs e)
        {
            _host.MouseMode = MouseMode.Selection;
            _host.SelectedComment = this;
            _host.SelectedNodes.Clear();
            foreach (var node in _host.Nodes)
                if (node.X >= X &&
                    node.X + node.ActualWidth <= X + ActualWidth &&
                    node.Y >= Y &&
                    node.Y + node.ActualHeight <= Y + ActualHeight)
                    _host.SelectedNodes.Add(node);
        }

        private void Comment_MouseUp(object sender, MouseButtonEventArgs e)
        {
            Cursor = Cursors.Arrow;
            _host.SelectedComment = null;
            _host.MouseMode = MouseMode.Nothing;
        }

        public void LocateHandler()
        {
            Canvas.SetLeft(_p, Width - 15);
            Canvas.SetTop(_p, Height - 15);
        }

        private void P_MouseUp(object sender, MouseButtonEventArgs e)
        {
            Cursor = Cursors.Arrow;
        }


        private void P_MouseDown(object sender, MouseButtonEventArgs e)
        {
            _host.MouseMode = MouseMode.ResizingComment;
            _host.TempComment = this;
            e.Handled = true;
        }


        private void CalculateSize(ObservableCollection<Node> nodes)
        {
            if (nodes.Count > 0)
            {
                Y = nodes[0].Y;
                X = nodes[0].X;

                foreach (var node in nodes)
                {
                    if (node.Y < Y)
                        Y = node.Y;
                    if (node.X < X)
                        X = node.X;
                }
                Width = max_Width(nodes) + 30;
                Height = max_Height(nodes) + 40;
                X -= 20;
                Y -= 30;
            }
        }

        private double max_Width(ObservableCollection<Node> nodes)
        {
            var maxwidth = nodes[0].ActualWidth;
            foreach (var node in nodes)
                if (node.ActualWidth + node.X > maxwidth + X)
                    maxwidth += node.ActualWidth + node.X - (maxwidth + X);
            return maxwidth;
        }

        private double max_Height(ObservableCollection<Node> nodes)
        {
            var maxheight = nodes[0].ActualHeight;
            foreach (var node in nodes)
                if (node.ActualHeight + node.Y > maxheight + Y)
                    maxheight += node.ActualHeight + node.Y - (maxheight + Y);
            return maxheight;
        }
    }
}