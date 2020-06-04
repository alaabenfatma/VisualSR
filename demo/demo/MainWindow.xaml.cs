using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace demo
{
    /// <summary>
    ///     Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            Closed += (s, e) => { Application.Current.Shutdown(); };
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var basic = new Basic();
            basic.ShowDialog();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            var vars = new VarsList();
            vars.ShowDialog();
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            var saveload = new SaveLoad();
            saveload.ShowDialog();
        }

        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            var stan = new Standard();
            stan.ShowDialog();
        }

        private void Button_Click_4(object sender, RoutedEventArgs e)
        {
            var perf = new Performance();
            perf.ShowDialog();
        }

        private void Button_Click_5(object sender, RoutedEventArgs e)
        {
            var advanced = new Advanced();
            advanced.ShowDialog();
        }
    }
}
