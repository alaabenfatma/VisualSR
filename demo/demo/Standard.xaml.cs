using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Forms;
using VisualSR.Compiler;
using VisualSR.Tools;

namespace demo
{
    /// <summary>
    ///     Interaction logic for SaveLoad.xaml
    /// </summary>
    public partial class Standard : Window
    {
        public Standard()
        {
            Closing += Standard_Closing;

            InitializeComponent();
            var browser = new FolderBrowserDialog();
            var path = "";
            if (browser.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                path = browser.SelectedPath + @"\";
            else
                Close();
            Hub.WorkSpace = path;
            vars.Host = vc;
            compile.Click += (s, e) => Compile();
        }

        private void Standard_Closing(object sender, CancelEventArgs e)
        {
            e.Cancel = true;
            Visibility = Visibility.Hidden;
        }

        private void Compile()
        {
            File.WriteAllText(Hub.WorkSpace + "code.r", CodeMiner.Code(vc.RootNode));
            var path = Hub.WorkSpace + "virtualConsole.bat";
            var cmd = @"@echo off
title Console
rscript """ + Hub.WorkSpace + @"code.r""" + @" 

pause";

            File.WriteAllText(path, cmd);
            Process.Start(path);
        }
    }
}