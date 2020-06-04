using System.IO;
using System.Windows;
using VisualSR.Tools;
using Microsoft.Win32;

namespace demo
{
    /// <summary>
    /// Interaction logic for SaveLoad.xaml
    /// </summary>
    public partial class SaveLoad : Window
    {
        public SaveLoad()
        {
            InitializeComponent();
            save.Click += (s, e) => Save();
            load.Click += (s, e) => Load();
        }

        private void Save()
        {
            var sfd = new SaveFileDialog();
            sfd.Filter = "CDE file (*.cde)|*.cde";
            sfd.FileName = "ProjectName";
            var save = sfd.ShowDialog();
            if (save == true)
                File.WriteAllText(sfd.FileName, vControl.SerializeAll());
        }

        private void Load()
        {
            var ofd = new OpenFileDialog();
            ofd.Filter = "CDE file (*.cde)|*.cde";
            var save = ofd.ShowDialog();
            if (save == true)
                Hub.LoadVirtualData(vControl, File.ReadAllText(ofd.FileName));
        }
    }
}
