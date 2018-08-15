/*Copyright 2018 ALAA BEN FATMA

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.*/
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using VisualSR.Properties;

namespace VisualSR.Controls
{
    public class InnerLinkingMessageBox : Control, INotifyPropertyChanged
    {
        public enum InnerMessageIcon
        {
            Correct,
            Warning,
            False
        }

        public const string BaseUri = @"VisualSR;component/MediaResources/";
        private string _text;
        private string _uri;
        public InnerMessageIcon MessageIcon = InnerMessageIcon.Correct;

        public InnerLinkingMessageBox()
        {
            Style = FindResource("InnerMessageBox") as Style;
            Height = 25;
            Visibility = Visibility.Collapsed;
        }

        public string Text
        {
            get { return _text; }
            set
            {
                _text = value;
                OnPropertyChanged("Text");
                OnPropertyChanged("Uri");

                Loaded += (s, e) =>
                {
                    (Template.FindName("BorderIcon", this) as Border).Style = FindResource("TickBorder") as Style;
                };
                BringIntoView();
            }
        }

        public string Uri
        {
            get
            {
                if (MessageIcon == InnerMessageIcon.Correct)
                    _uri = BaseUri + "tick.png";
                else if (MessageIcon == InnerMessageIcon.False)
                    _uri = BaseUri + "Cross.png";
                else
                    _uri = BaseUri + "Warning.png";

                return _uri;
            }
            set
            {
                _uri = value;

                OnPropertyChanged("Uri");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged(string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}