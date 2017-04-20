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
            GoUpCommand.InputGestures.Add(new KeyGesture(Key.Up, ModifierKeys.Alt));
            EditURLBarCommand.InputGestures.Add(new KeyGesture(Key.E, ModifierKeys.Control));
            EditURLBarCommand.InputGestures.Add(new KeyGesture(Key.L, ModifierKeys.Control));
            EnterCommand.InputGestures.Add(new KeyGesture(Key.Enter));
            OpenFolderCommand.InputGestures.Add(new KeyGesture(Key.O, ModifierKeys.Control));
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
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (Properties.Settings.Default.LastLocation != null)
            {
                GetItems(Properties.Settings.Default.LastLocation);
            }
        }

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

        /// <summary>
        /// Backgroundworker for multithreading
        /// </summary>
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
                    Properties.Settings.Default.LastLocation = FileName;
                    Properties.Settings.Default.Save();
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
            ListViewItem item = new ListViewItem { Content = e.UserState };
            Tree.Items.Add(item);
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
                var ListViewItems = Tree.Items.OfType<ListViewItem>();
                var DirectoryItems = from item in ListViewItems select (DirectoryItem)item.Content;
                var ordered = DirectoryItems.OrderBy(x => x.Name.Length);
                var BiggestLength = ordered.Last().Name.Length;
                Column1.Width = MaxTabIndex * 24 + BiggestLength * 8;
            }
            catch (Exception)
            {
            }
        }

        /// <summary>
        /// Command to go up
        /// </summary>
        public static RoutedCommand GoUpCommand = new RoutedCommand();

        /// <summary>
        /// Goes up a level after a keyboard shortcut is executed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void GoUp_Executed(object sender, ExecutedRoutedEventArgs e) => GoUp();

        /// <summary>
        /// Goes up a level
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void GoUp_Click(object sender, RoutedEventArgs e) => GoUp();

        /// <summary>
        /// Goes up a level
        /// </summary>
        private void GoUp()
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

            try
            {
                Directory.GetFiles(item.Path);
                GetItems(item.Path);
            }
            catch (Exception)
            {
                System.Diagnostics.Process.Start(item.Path);

            }
        }

        /// <summary>
        /// Command to go to the specified directory
        /// </summary>
        public static RoutedCommand EnterCommand = new RoutedCommand();

        /// <summary>
        /// Goes to the specified directory if the directory is valid and the enter key has been pressed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void EnterCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if(EnterButton.IsEnabled && FolderPath.IsFocused)
            {
                GetItems(FolderPath.Text);
            }
        }

        /// <summary>
        /// Goes to the specified directory
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void EnterButton_Click(object sender, RoutedEventArgs e) => GetItems(FolderPath.Text);

        /// <summary>
        /// Checks if the folder is valid
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FolderPath_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (Directory.Exists(FolderPath.Text))
            {
                ErrorMessage.Text = "";
                EnterButton.IsEnabled = true;
                GoUpButton.IsEnabled = true;
            }
            else
            {
                ErrorMessage.Text = "Folder not valid";
                EnterButton.IsEnabled = false;
                GoUpButton.IsEnabled = false;
            }
        }

        /// <summary>
        /// Command to edit the URL bar
        /// </summary>
        public static RoutedCommand EditURLBarCommand = new RoutedCommand();

        /// <summary>
        /// Goes to the URL bar after the keyboard shortcut is given
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void EditURLBarCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            FolderPath.CaretIndex = FolderPath.Text.Length; // Moves the cursor to the end
            FolderPath.Focus();
        }

        /// <summary>
        /// Command to open the Windows folder dialog to select a new folder
        /// </summary>
        public static RoutedCommand OpenFolderCommand = new RoutedCommand();

        /// <summary>
        /// Ask the user and get items
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OpenFolderCommand_Executed(object sender, ExecutedRoutedEventArgs e) => AskUserAndGetItems();

        /// <summary>
        /// Minimizes/maximizes a folder in the tree structure
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ListViewItem_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            ListViewItem itemClickedListViewItem = sender as ListViewItem;
            DirectoryItem itemClicked = (DirectoryItem)itemClickedListViewItem.Content;
            if (itemClickedListViewItem != null && itemClicked.ParentPath != null)
            {
                foreach (var item in Tree.Items)
                {
                    var temp = (DirectoryItem)(((ListViewItem)item).Content);
                    string itemClickedPath = itemClicked.Path;
                    if (temp.Path != itemClicked.Path && temp.ParentPath.Contains(itemClickedPath))
                    {
                        if (((ListViewItem)item).Visibility == Visibility.Visible)
                        {
                            ((ListViewItem)item).Visibility = Visibility.Collapsed;
                        }
                        else
                        {
                            ((ListViewItem)item).Visibility = Visibility.Visible;
                        }
                    }
                }
            }
        }
    }
}
