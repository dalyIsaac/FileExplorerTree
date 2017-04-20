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

        /// <summary>
        /// SelectFolder button click event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SelectFolder_Click(object sender, RoutedEventArgs e) => AskUserAndGetItems();

        /// <summary>
        /// Window loaded event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_Loaded(object sender, RoutedEventArgs e) => AskUserAndGetItems();

        /// <summary>
        /// Asks the user for the folder and gets the 
        /// </summary>
        private void AskUserAndGetItems()
        {
            var dialog = new CommonOpenFileDialog() { IsFolderPicker = true };
            dialog.ShowDialog();
            try
            {
                GetItems(dialog.FileName);
            }
            catch (Exception)
            {
            }
        }

        public AbortableBackgroundWorker Worker { get; set; }

        /// <summary>
        /// Gets all the directory items inside the specified directory
        /// </summary>
        private void GetItems(string FileName)
        {
            try
            {
                if (!(Directory.GetFiles(FileName).Length == 0 && Directory.GetDirectories(FileName).Length == 0))
                {
                    // Sets the title items
                    FolderPath.Text = FileName;

                    if (Worker != null)
                    {
                        if (Worker.IsBusy)
                        {
                            Worker.CancelAsync();
                            Worker.Abort();
                            Worker.Dispose();
                        }
                    }

                    Tree.Items.Clear();

                    Worker = new AbortableBackgroundWorker() { WorkerReportsProgress = true };
                    Worker.DoWork += Worker_DoWork;
                    Worker.ProgressChanged += Worker_ProgressChanged;
                    Worker.RunWorkerCompleted += Worker_Completed;
                    Worker.WorkerSupportsCancellation = true;
                    Worker.RunWorkerAsync(FileName); // Passes in the directory to search
                    MaxTabIndex = 0;
                }
            }
            catch (Exception)
            {
            }
        }

        /// <summary>
        /// Calls the GetItems method
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Worker_DoWork(object sender, DoWorkEventArgs e)
        {
            string path = (string)e.Argument;
            GetItems(sender, path);
        }

        /// <summary>
        /// Gets all the items in a specified directory, and adds them to the Tree
        /// </summary>
        /// <param name="sender">Used for adding the items to the Tree</param>
        /// <param name="path">Directory path</param>
        /// <param name="tabIndex">Number of indentations for an object</param>
        private void GetItems(object sender, string path, int tabIndex = 0)
        {
            try
            {
                foreach (string folderPath in Directory.GetDirectories(path)) // Gets all the directories in the specified directory
                {
                    (sender as AbortableBackgroundWorker).ReportProgress(0, new DirectoryItem(folderPath, tabIndex)); // Add the item to the tree
                    GetItems(sender, folderPath, tabIndex + 1); // Recursively search all the directories found
                    if (tabIndex > MaxTabIndex)
                    {
                        MaxTabIndex = tabIndex;
                    }
                }
            }
            catch (Exception)
            {
            }

            try
            {
                foreach (string filePath in Directory.GetFiles(path)) // Gets all the files in the specified directory
                {
                    (sender as AbortableBackgroundWorker).ReportProgress(0, new DirectoryItem(filePath, tabIndex)); // Add the item to the Tree
                }
            }
            catch (Exception)
            {
            }
        }

        /// <summary>
        /// Adds items to the Tree for the user to see
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Worker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            Tree.Items.Add(e.UserState);
        }

        /// <summary>
        /// Stores the maximum tab index of an item
        /// </summary>
        private static int MaxTabIndex = 0;

        /// <summary>
        /// Sets the column width for name
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Worker_Completed(object sender, RunWorkerCompletedEventArgs e)
        {
            try
            {
                int BiggestItemLength = Tree.Items.OfType<DirectoryItem>().OrderBy(x => x.Name.Length).Last().Name.Length;
                Column1.Width = MaxTabIndex * 24 + BiggestItemLength * 8;
            }
            catch (Exception)
            {
            }
        }

        private void GoUp_Click(object sender, RoutedEventArgs e)
        {
            var splitDir = FolderPath.Text.Split('\\').ToList();
            splitDir.RemoveAt(splitDir.Count - 1);
            string parentDir = String.Join("\\", splitDir);
            GetItems(parentDir);
        }

        /// <summary>
        /// Allows the user to navigate down a level
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ListViewItem_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            DirectoryItem item = (DirectoryItem)(sender as ListViewItem).Content;
            GetItems(item.Path);
        }

        private void EnterButton_Click(object sender, RoutedEventArgs e)
        {
            GetItems(FolderPath.Text);
        }

        private void FolderPath_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (Directory.Exists(FolderPath.Text))
            {
                ErrorMessage.Text = "";
                EnterButton.IsEnabled = true;
                GoUp.IsEnabled = true;
            }
            else
            {
                ErrorMessage.Text = "Folder not valid";
                EnterButton.IsEnabled = false;
                GoUp.IsEnabled = false;
            }
        }
    }
}
