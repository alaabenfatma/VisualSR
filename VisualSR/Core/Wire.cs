/*Copyright 2018 ALAA BEN FATMA

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.*/
using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using VisualSR.Properties;
using VisualSR.Tools;

namespace VisualSR.Core
{
    public class Wire : Control, INotifyPropertyChanged
    {
        private readonly PointAnimationUsingPath beatsAnimation = new PointAnimationUsingPath();

        private readonly Storyboard heart = new Storyboard();
        private Point _epoint;
        private Point _mpoint1;
        private Point _mpoint2;
        private Point _spoint;

        public Wire()
        {
            Style = (Style) FindResource("VirtualWire");
            Background = Brushes.WhiteSmoke;
            Focusable = false;
            Loaded += (s, e) =>
            {
                IsVisibleChanged += (ss, ee) =>
                {
                    if (Visibility != Visibility.Visible)
                        stopAnimation();
                };
            };


            beatsAnimation.Completed += BeatsAnimation_Completed;
            heart.Children.Add(beatsAnimation);
        }

        public Connector ParentConnector { get; set; }

        public Point StartPoint
        {
            get { return _spoint; }
            set
            {
                _spoint = value;
                OnPropertyChanged("StartPoint");
                MiddlePoint1 = _spoint;
            }
        }

        public Point MiddlePoint1
        {
            get { return _mpoint1; }
            set
            {
                _mpoint1 = new Point(value.X + 70, value.Y);
                OnPropertyChanged("MiddlePoint1");
            }
        }

        public Point MiddlePoint2
        {
            get { return _mpoint2; }
            set
            {
                OnPropertyChanged("MiddlePoint2");
                _mpoint2 = new Point(value.X - 70, value.Y);
            }
        }

        public Point EndPoint
        {
            get { return _epoint; }
            set
            {
                _epoint = value;

                MiddlePoint2 = _epoint;
                OnPropertyChanged("EndPoint");
            }
        }


        public event PropertyChangedEventHandler PropertyChanged;

        public void HeartBeatsAnimation(bool forever = true)
        {
            try
            {
                var path = Template.FindName("Wire", this) as Path;
                beatsAnimation.PathGeometry = path.Data.GetFlattenedPathGeometry();
                beatsAnimation.Duration = TimeSpan.FromSeconds(1);
                (Template.FindName("BeatContainer", this) as Path).Visibility = Visibility.Visible;
                beatsAnimation.RepeatBehavior = forever ? RepeatBehavior.Forever : new RepeatBehavior(1);
                (Template.FindName("Beat", this) as EllipseGeometry).BeginAnimation(EllipseGeometry.CenterProperty,
                    beatsAnimation);
            }
            catch (Exception)
            {
                //Ignored
            }
        }

        private void BeatsAnimation_Completed(object sender, EventArgs e)
        {
            stopAnimation();
        }

        private void stopAnimation()
        {
            (Template.FindName("Beat", this) as EllipseGeometry).BeginAnimation(EllipseGeometry.CenterProperty, null);
            (Template.FindName("BeatContainer", this) as Path).Visibility = Visibility.Collapsed;
        }

        public void ParentNodeOnPropertyChanged(Port s, Port e)
        {
            var sp = PointsCalculator.PortOrigin(s);
            var ep = PointsCalculator.PortOrigin(e);
            StartPoint = sp;
            EndPoint = ep;
        }

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged(string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}