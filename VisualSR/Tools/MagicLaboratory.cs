using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using VisualSR.Core;
using VisualSR.Properties;
using Color = System.Windows.Media.Color;

namespace VisualSR.Tools
{
    public enum FileTypes
    {
        Unknown,
        Image,
        Video,
        Executable,
        Data,
        Text,
        Code,
        Document,
        Audio
    }

    public enum ItemTypes
    {
        Node,
        Port,
        Connector
    }

    public static class ZIndexes
    {
        public static readonly byte CommentIndex = 0;
        public static readonly byte ConnectorIndex = 1;
        public static readonly byte NodeIndex = 2;
        public static readonly byte NodesTreeIndex = 3;
        public static readonly byte AboveAllIndex = 4;
    }

    public class MouseClickEffect : Control, INotifyPropertyChanged
    {
        private Color _effectColor;

        public MouseClickEffect()
        {
            Style = FindResource("MouseClickEffect") as Style;
            ApplyTemplate();
        }

        public Color EffectColor
        {
            get { return _effectColor; }
            set
            {
                _effectColor = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void Show(VirtualControl host, Color color)
        {
            EffectColor = color;
            ApplyTemplate();
            host.AddChildren(this, Mouse.GetPosition(host).X - 15, Mouse.GetPosition(host).Y - 15);
        }

        public void Remove(VirtualControl host)
        {
            host.Children.Remove(this);
        }

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged(string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public static class MagicLaboratory
    {
        //[return: MarshalAs(UnmanagedType.Bool)]
        //private static extern bool GetCursorPos(ref Win32Point pt);

        //[StructLayout(LayoutKind.Sequential)]
        //private struct Win32Point
        //{
        //    public int X;
        //    public int Y;
        //};
        //public static System.Windows.Point GetMousePosition()
        //{
        //    Win32Point w32Mouse = new Win32Point();
        //    GetCursorPos(ref w32Mouse);
        //    return new System.Windows.Point(w32Mouse.X, w32Mouse.Y);
        //}
        private static readonly Random random = new Random((int) DateTime.Now.Ticks);

        private static readonly Stack<string> DeletedFolders = new Stack<string>();

        public static void Blur(FrameworkElement control)
        {
            var storyboard = new Storyboard();
            var animation = new DoubleAnimation
            {
                From = 0,
                To = 5,
                AutoReverse = true
            };
            var effect = new BlurEffect();
            control.Effect = effect;
            storyboard.Children.Add(animation);
            Storyboard.SetTarget(storyboard, control.Effect);
            Storyboard.SetTargetProperty(storyboard, new PropertyPath("Radius"));
            storyboard.Begin();
        }

        public static void UnBlur(FrameworkElement control)
        {
            control.Effect = null;
        }

        public static void unLinkChild(UIElement child)
        {
            var parent = VisualTreeHelper.GetParent(child);
            var parentAsPanel = parent as Panel;
            parentAsPanel?.Children.Remove(child);
        }

        public static void CleanPath(ref string path)
        {
            path = path.Replace(Environment.NewLine, string.Empty);
        }

        public static string CorrectWindowsPath(string path)
        {
            return path.Replace(@"\", "/");
        }

        public static string RandomString(int size)
        {
            var builder = new StringBuilder();
            char ch;
            for (var i = 0; i < size; i++)
            {
                ch = Convert.ToChar(Convert.ToInt32(Math.Floor(26 * random.NextDouble() + 65)));
                builder.Append(ch);
            }

            return builder.ToString();
        }

        public static string DeleteAllDirectoriesForce(string head, bool async = true)
        {
            if (DeletedFolders.Peek() == "End.")
                foreach (var file in Directory.GetFiles(head))
                    File.Delete(file);

            foreach (var h in Directory.GetDirectories(head))
                DeleteAllDirectoriesForce(h);

            Directory.Delete(head);
            return null;
        }

        [DllImport("gdi32.dll", EntryPoint = "DeleteObject")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool DeleteObject([In] IntPtr hObject);

        public static ImageSource ImageSourceForBitmap(Bitmap bmp)
        {
            var handle = bmp.GetHbitmap();
            try
            {
                return Imaging.CreateBitmapSourceFromHBitmap(handle, IntPtr.Zero, Int32Rect.Empty,
                    BitmapSizeOptions.FromEmptyOptions());
            }
            finally
            {
                DeleteObject(handle);
            }
        }

        public static void GrayWiresOut(Wire wire)
        {
            wire.Opacity = .5;
        }

        public static void GrayWiresOut_Reverse(Wire wire)
        {
            wire.Opacity = 1;
        }

        public static bool ConvertFromString(string value)
        {
            return value != "FALSE";
        }
    }
}