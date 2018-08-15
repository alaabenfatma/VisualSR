/*Copyright 2018 ALAA BEN FATMA

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.*/
//Did not include it in the demos.
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using VisualSR.Core;
using VisualSR.Properties;

namespace VisualSR.Controls
{
    public class Notification : Button, INotifyPropertyChanged
    {
        private readonly VirtualControl host;

        public Notification(VirtualControl parent)
        {
            Background = Brushes.Cyan;
            Style = FindResource("Notification") as Style;
            host = parent;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void Show(string msg)
        {
            Loaded += (s, e) =>
            {
                var b = Template.FindName("b", this) as Border;
                var m = Template.FindName("Message", this) as TextBlock;
                m.Text = msg;
                b.SizeChanged += (ss, ee) =>
                {
                    Width = b.ActualWidth;
                    Height = b.ActualHeight;
                };

                VerticalContentAlignment = VerticalAlignment.Bottom;
                HorizontalAlignment = HorizontalAlignment.Right;

                Visibility = Visibility.Visible;
                var timer = new DispatcherTimer {Interval = new TimeSpan(0, 0, 3), IsEnabled = true};
                timer.Tick += (ts, te) =>
                {
                    host.Children.Remove(this);
                    timer.Stop();
                };
                host.Children.Add(this);
            };
        }

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}