using Microsoft.WindowsAPICodePack.Dialogs;
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

namespace Trees
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

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            LoadItems();
        }

        private void LoadItems()
        {
            var dialog = new CommonOpenFileDialog()
            {
                IsFolderPicker = true
            };
            CommonFileDialogResult result = dialog.ShowDialog();
            string[] fileNameParts = dialog.FileName.Split('\\');
            FolderName.Text = fileNameParts.Last();
            FolderPath.Text = dialog.FileName;

            var service = new Service(dialog.FileName);
            viewsTreeView.ItemsSource = service.ItemList;
        }

        private void SelectFolder_Click(object sender, RoutedEventArgs e)
        {
            LoadItems();
        }
    }
}
