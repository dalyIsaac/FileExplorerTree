using Microsoft.WindowsAPICodePack.Dialogs;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
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

namespace TreeDirExplorer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void SelectFolder_Click(object sender, RoutedEventArgs e) => DoWork();

        private void Window_Loaded(object sender, RoutedEventArgs e) => DoWork();

        private void DoWork()
        {
            var dialog = new CommonOpenFileDialog()
            {
                IsFolderPicker = true
            };
            CommonFileDialogResult result = dialog.ShowDialog();
            string[] fileNameParts = dialog.FileName.Split('\\');
            FolderName.Text = fileNameParts.Last();
            FolderPath.Text = dialog.FileName;

            BackgroundWorker Worker = new BackgroundWorker() { WorkerReportsProgress = true };
            Worker.DoWork += Worker_DoWork;
            Worker.ProgressChanged += Worker_ProgressChanged;
            Worker.RunWorkerAsync(dialog.FileName);
        }

        private void Worker_DoWork(object sender, DoWorkEventArgs e)
        {
            string path = (string)e.Argument;
            GetItems(sender, path);
        }

        private void GetItems(object sender, string path, int tabIndex = 0)
        {
            foreach (string folderPath in Directory.GetDirectories(path))
            {
                (sender as BackgroundWorker).ReportProgress(0, new DirectoryItem(folderPath, tabIndex));
                GetItems(sender, folderPath, tabIndex + 1);
            }

            foreach (string filePath in Directory.GetFiles(path))
            {
                (sender as BackgroundWorker).ReportProgress(0, new DirectoryItem(filePath, tabIndex));
            }
        }

        private void Worker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            Tree.Items.Add(e.UserState);
        }
    }
}
