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
using System.Windows.Shapes;

namespace demo
{
    /// <summary>
    /// Interaction logic for Basic.xaml
    /// </summary>
    public partial class Basic : Window
    {
        public Basic()
        {
            Loaded += MyWindow_Loaded;
          
        }
        private void MyWindow_Loaded(object sender, RoutedEventArgs e)
        {
            InitializeComponent();
        }
    }
}
