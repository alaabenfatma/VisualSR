/*Copyright 2018 ALAA BEN FATMA
Copyright Aug 8, 2016 DominicSinger
Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.*/
//Reference that helped me write this class: https://github.com/tumcms/TUM.CMS.VPLControl/blob/master/src/TUM.CMS.VplControl/Core/ZoomCanvas.cs

using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace VisualSR.Core
{
    public enum MouseMode
    {
        Nothing,
        Panning,
        Selection,
        PreSelectionRectangle,
        SelectionRectangle,
        DraggingPort,
        ResizingComment,
    }

    public enum WireMode
    {
        Nothing,
        FirstPortSelected
    }

    public class CanvasCamera : Canvas
    {
        public readonly ScaleTransform ScaleTransform = new ScaleTransform();
        public readonly TranslateTransform TranslateTransform = new TranslateTransform();
        public MouseMode MouseMode = MouseMode.Nothing;
        protected Point Origin;
        public ObservableCollection<Node> SelectedNodes = new ObservableCollection<Node>();
        protected Point Start;
        public WireMode WireMode = WireMode.Nothing;

        public CanvasCamera()
        {
            Style = FindResource("VirtualControlStyle") as Style;
            ApplyTemplate();
            MouseWheel += HandleMouseWheel;
            MouseDown += HandleMouseDown;
            PreviewMouseMove += HandleMouseMove;
            RenderTransformOrigin = new Point(0.5, 0.5);
        }

        public Point UIelementCoordinates(UIElement element)
        {
            var relativePoint = element.TransformToAncestor(this)
                .Transform(new Point(0, 0));
            return relativePoint;
        }

        public void AddChildren(UIElement child)
        {
            Initialize(child);
            if (!Children.Contains(child)) Children.Add(child);
        }

        public void AddChildren(UIElement child, double x, double y)
        {
            Initialize(child);
            if (!Children.Contains(child))
                Children.Add(child);
            SetLeft(child, x);
            SetTop(child, y);
        }

        public void Initialize(UIElement element)
        {
            if (element != null)
            {
                var transformGroup = new TransformGroup();
                transformGroup.Children.Add(ScaleTransform);
                transformGroup.Children.Add(TranslateTransform);
                element.RenderTransform = transformGroup;
                element.RenderTransformOrigin = new Point(0.5, 0.5);
            }
        }

        public void Reset()
        {
            // reset zoom
            ScaleTransform.ScaleX = 1.0;
            ScaleTransform.ScaleY = 1.0;

            // reset pan
            TranslateTransform.X = ActualWidth / 2;
            TranslateTransform.Y = ActualHeight / 2;
        }

        public void ZoomOut(int times = 1)
        {
            for (var i = 0; i < times; i++)
            {
                _zoom -= 0.05;
                if (_zoom < _zoomMin) _zoom = _zoomMin;
                if (_zoom > _zoomMax) _zoom = _zoomMax;
                var scaler = LayoutTransform as ScaleTransform;

                if (scaler == null)
                {
                    scaler = new ScaleTransform(01, 01, Mouse.GetPosition(this).X, Mouse.GetPosition(this).Y);
                    LayoutTransform = scaler;
                }
                var animator = new DoubleAnimation
                {
                    Duration = new Duration(TimeSpan.FromMilliseconds(500)),
                    To = _zoom
                };
                scaler.BeginAnimation(ScaleTransform.ScaleXProperty, animator);
                scaler.BeginAnimation(ScaleTransform.ScaleYProperty, animator);
            }
        }

        public void ZoomIn(int times = 1)
        {
            for (var i = 0; i < times; i++)
            {
                _zoom += 0.05;
                if (_zoom < _zoomMin) _zoom = _zoomMin;
                if (_zoom > _zoomMax) _zoom = _zoomMax;
                var scaler = LayoutTransform as ScaleTransform;

                if (scaler == null)
                {
                    scaler = new ScaleTransform(01, 01, Mouse.GetPosition(this).X, Mouse.GetPosition(this).Y);
                    LayoutTransform = scaler;
                }
                var animator = new DoubleAnimation
                {
                    Duration = new Duration(TimeSpan.FromMilliseconds(500)),
                    To = _zoom
                };
                scaler.CenterX = ActualWidth / 2;
                scaler.CenterY = ActualHeight / 2;
                scaler.BeginAnimation(ScaleTransform.ScaleXProperty, animator);
                scaler.BeginAnimation(ScaleTransform.ScaleYProperty, animator);
            }
        }

        #region Events

        private readonly double _zoomMax = 1.5;
        private readonly double _zoomMin = 0.7;
        private readonly double _zoomSpeed = 0.0005;
        private double _zoom = 0.9;

        protected virtual void HandleMouseWheel(object sender, MouseWheelEventArgs e)
        {
            _zoom += _zoomSpeed * e.Delta;
            if (_zoom < _zoomMin) _zoom = _zoomMin;
            if (_zoom > _zoomMax) _zoom = _zoomMax;
            var scaler = LayoutTransform as ScaleTransform;

            if (scaler == null)
            {
                scaler = new ScaleTransform(01, 01, Mouse.GetPosition(this).X, Mouse.GetPosition(this).Y);
                LayoutTransform = scaler;
            }

            var animator = new DoubleAnimation
            {
                Duration = new Duration(TimeSpan.FromMilliseconds(500)),
                To = _zoom
            };
            scaler.BeginAnimation(ScaleTransform.ScaleXProperty, animator);
            scaler.BeginAnimation(ScaleTransform.ScaleYProperty, animator);

            MouseMode = MouseMode.Nothing;
            e.Handled = true;
        }

        protected virtual void HandleMouseDown(object sender, MouseButtonEventArgs e)
        {
            Start = e.GetPosition(this);
            Origin = new Point(TranslateTransform.X, TranslateTransform.Y);
            if (MouseMode != MouseMode.Selection && e.ChangedButton == MouseButton.Middle)
            {
                Cursor = Cursors.Hand;
                MouseMode = MouseMode.Panning;
            }
        }

        protected virtual void HandleMouseUp(object sender, MouseButtonEventArgs e)
        {
            MouseMode = MouseMode.PreSelectionRectangle;
        }

        public Comment SelectedComment;

        protected virtual void HandleMouseMove(object sender, MouseEventArgs e)
        {
            var v = Start - e.GetPosition(this);

            if (MouseMode == MouseMode.Panning)
            {
                TranslateTransform.X = Origin.X - v.X;
                TranslateTransform.Y = Origin.Y - v.Y;

                for (var index = 0; index < Children.Count; index++)
                {
                    var element = Children[index];
                    if (element is Wire) continue;
                    if (element is Node)
                    {
                        var node = element as Node;
                        node.X = node.X - v.X;
                        node.Y = node.Y - v.Y;
                    }
                }
                Start = e.GetPosition(this);
            }
            else if (MouseMode == MouseMode.Selection)
            {
                for (var index = 0; index < SelectedNodes.Count; index++)
                {
                    var child = SelectedNodes[index];
                    child.X = child.X - v.X;
                    child.Y = child.Y - v.Y;
                }

                Start = e.GetPosition(this);
            }
            if (SelectedComment != null)
            {
                SelectedComment.X = SelectedComment.X - v.X;
                SelectedComment.Y = SelectedComment.Y - v.Y;
                Start = e.GetPosition(this);
            }
        }

        #endregion
    }
}
