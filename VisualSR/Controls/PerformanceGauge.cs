/*Copyright 2018 ALAA BEN FATMA

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.*/
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;
using VisualSR.Properties;

namespace VisualSR.Controls
{
    public enum PerfomanceState
    {
        Cold,
        Cool,
        Good,
        Normal,
        Heating,
        Hot,
        Risky
    }

    public class PerformanceGauge : Control, INotifyPropertyChanged
    {
        private readonly DispatcherTimer _backgroundRamCollector = new DispatcherTimer();

        private readonly PerformanceCounter _pc = new PerformanceCounter
        {
            CategoryName = "Process",
            CounterName = "Working Set - Private"
        };

        private readonly Process _proc = Process.GetCurrentProcess();
        private Path _cold;
        private Path _cool;
        private Color _coreColor;
        private Path _good;
        private Brush _heatColor;
        private Path _heating;
        private Path _hot;
        private double _memsize;
        private Path _normal;

        private Path[] _paths;
        private string _ram = "...";
        private Path _risky;
        public PerfomanceState States = PerfomanceState.Cold;

        public PerformanceGauge()
        {
            Style = FindResource("Gauge") as Style;
            ApplyTemplate();
            Loaded += (s, e) =>
            {
                _cold = Template.FindName("Cold", this) as Path;
                _cool = Template.FindName("Cool", this) as Path;
                _good = Template.FindName("Good", this) as Path;
                _normal = Template.FindName("Normal", this) as Path;
                _heating = Template.FindName("Heating", this) as Path;
                _hot = Template.FindName("Hot", this) as Path;
                _risky = Template.FindName("Risky", this) as Path;
                _paths = new[] {_cold, _cool, _good, _normal, _heating, _hot, _risky};
            };
            _backgroundRamCollector.Interval = new TimeSpan(0, 0, 0, 3);
            _backgroundRamCollector.IsEnabled = true;
            _backgroundRamCollector.Start();
            _backgroundRamCollector.Tick += (ts, te) => CountRam();
        }

        public Color CoreColor
        {
            get { return _coreColor; }
            set
            {
                _coreColor = value;
                OnPropertyChanged();
            }
        }

        public Brush HeatColor
        {
            get { return _heatColor; }
            set
            {
                _heatColor = value;
                OnPropertyChanged();
            }
        }

        public string Ram
        {
            get { return _ram; }
            set
            {
                _ram = value;

                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void ReBuild()
        {
            var x = _memsize;
            if (x <= 200)
            {
                if (!Equals(HeatColor, Brushes.Cyan))
                {
                    States = PerfomanceState.Cold;
                    HeatColor = Brushes.Cyan;
                    CoreColor = Colors.Cyan;
                    Highlighter(0, 1, HeatColor);
                    Highlighter(1, 7, Brushes.Black);
                }
            }
            else if (x <= 300 && x > 200)
            {
                if (!Equals(HeatColor, Brushes.DarkCyan))
                {
                    States = PerfomanceState.Cool;
                    HeatColor = Brushes.DarkCyan;
                    CoreColor = Colors.DarkCyan;
                    Highlighter(0, 2, HeatColor);
                    Highlighter(3, 7, Brushes.Black);
                }
            }
            else if (x <= 600 && x > 300)
            {
                if (!Equals(HeatColor, Brushes.CadetBlue))
                {
                    States = PerfomanceState.Good;
                    HeatColor = Brushes.CadetBlue;
                    CoreColor = Colors.CadetBlue;
                    Highlighter(0, 3, HeatColor);
                    Highlighter(4, 7, Brushes.Black);
                }
            }
            else if (x <= 900 && x > 600)
            {
                if (!Equals(HeatColor, Brushes.ForestGreen))
                {
                    States = PerfomanceState.Normal;
                    HeatColor = Brushes.ForestGreen;
                    CoreColor = Colors.ForestGreen;
                    Highlighter(0, 4, HeatColor);
                    Highlighter(5, 7, Brushes.Black);
                }
            }
            else if (x <= 1500 && x > 900)
            {
                if (!Equals(HeatColor, Brushes.HotPink))
                {
                    States = PerfomanceState.Heating;
                    HeatColor = Brushes.HotPink;
                    CoreColor = Colors.HotPink;
                    Highlighter(0, 5, HeatColor);
                    Highlighter(6, 7, Brushes.Black);
                }
            }
            else if (x <= 2000 && x > 1500)
            {
                if (!Equals(HeatColor, Brushes.Firebrick))
                {
                    States = PerfomanceState.Hot;
                    HeatColor = Brushes.Firebrick;
                    CoreColor = Colors.Firebrick;
                    Highlighter(0, 6, HeatColor);
                    Highlighter(7, 7, Brushes.Black);
                }
            }
            else if (x > 3000)
            {
                if (!Equals(HeatColor, Brushes.Firebrick))
                {
                    States = PerfomanceState.Risky;
                    HeatColor = Brushes.Red;
                    CoreColor = Colors.Red;
                    Highlighter(0, 7, HeatColor);
                }
            }
            OnPropertyChanged();
        }

        private void CountRam()
        {
            Task.Factory.StartNew(() =>
            {
                Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() =>
                {
                    //This code can be used to mimick a real-life experience
                    //memsize += 1;
                    //Ram = memsize + " MB";
                    //ReBuild();
                    _pc.InstanceName = _proc.ProcessName;
                    _memsize = Convert.ToDouble(_pc.NextValue() / 1048576);
                    Ram = _memsize.ToString("#.0") + " MB";
                    ReBuild();
                }));
            });
        }


        private void Highlighter(int begin, int end, Brush brush)
        {
            for (var i = begin; i < end; i++)
                if (_paths[i] != null)
                    _paths[i].Fill = brush;
        }

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged(string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}