using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using ICSharpCode.AvalonEdit.Document;
using Microsoft.Win32;
using MessageBox = System.Windows.MessageBox;

namespace WpfApp1
{
    public class EditorTab : INotifyPropertyChanged
    {
        private string _filePath;
        public string FilePath
        {
            get => _filePath;
            set
            {
                _filePath = value;
                OnPropertyChanged(nameof(FileName)); 
            }
        }

        public string FileName => string.IsNullOrEmpty(FilePath) ? "Новый документ" : Path.GetFileName(FilePath);

        public TextDocument Document { get; } = new TextDocument();

        public ICommand CloseCommand { get; }

        public EditorTab()
        {
            CloseCommand = new RelayCommand(Close);
        }

        private void Close()
        {
            if (Application.Current.MainWindow is MainWindow mainWindow)
            {
                mainWindow.CloseTab(this);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }


    public class RelayCommand : ICommand
    {
        private readonly Action _execute;
        private readonly Func<bool> _canExecute;

        public RelayCommand(Action execute, Func<bool> canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        public bool CanExecute(object parameter) => _canExecute == null || _canExecute();
        public void Execute(object parameter) => _execute();
        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }
    }

    public partial class MainWindow : Window
    {
        public ObservableCollection<EditorTab> Tabs { get; } = new ObservableCollection<EditorTab>();

        private EditorTab _selectedTab;
        public EditorTab SelectedTab
        {
            get => _selectedTab;
            set
            {
                _selectedTab = value;
            }
        }

        public MainWindow()
        {
            InitializeComponent();

            this.DataContext = this;

            Tabs.Add(new EditorTab());
            SelectedTab = Tabs[0];
        }
        private ICSharpCode.AvalonEdit.TextEditor CurrentEditor
        {
            get
            {
                if (SelectedTab == null) return null;

                var tabItem = tabControl.ItemContainerGenerator.ContainerFromItem(SelectedTab) as TabItem;
                return tabItem != null ? FindVisualChild<ICSharpCode.AvalonEdit.TextEditor>(tabItem) : null;
            }
        }

        private static T FindVisualChild<T>(DependencyObject parent) where T : DependencyObject
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                if (child is T t) return t;
                var result = FindVisualChild<T>(child);
                if (result != null) return result;
            }
            return null;
        }

        public void CloseTab(EditorTab tab)
        {
            if (Tabs.Contains(tab))
            {
                Tabs.Remove(tab);
                if (Tabs.Count == 0)
                {
                    Tabs.Add(new EditorTab());
                }
            }
        }


        private void new_file_Click(object sender, RoutedEventArgs e)
        {
            Tabs.Add(new EditorTab());
            SelectedTab = Tabs[Tabs.Count - 1];
        }

        private void open_file_Click(object sender, RoutedEventArgs e)
        {
            var ofd = new OpenFileDialog
            {
                Filter = "Text files(*.txt)|*.txt|C# files (*.cs)|*.cs|Xaml files (*.xaml)|*.xaml|All files (*.*)|*.*",
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)
            };

            if (ofd.ShowDialog() == true)
            {
                var newTab = new EditorTab { FilePath = ofd.FileName };
                newTab.Document.Text = File.ReadAllText(ofd.FileName);
                Tabs.Add(newTab);
                SelectedTab = newTab;
            }
        }

        private void save_file_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedTab == null) return;

            if (string.IsNullOrEmpty(SelectedTab.FilePath))
            {
                save_as_file_Click(sender, e);
                return;
            }

            File.WriteAllText(SelectedTab.FilePath, SelectedTab.Document.Text);
        }

        private void save_as_file_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedTab == null) return;

            var sfd = new SaveFileDialog
            {
                Filter = "Text files(*.txt)|*.txt|C# files (*.cs)|*.cs|Xaml files (*.xaml)|*.xaml|All files (*.*)|*.*",
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)
            };

            if (sfd.ShowDialog() == true)
            {
                File.WriteAllText(sfd.FileName, SelectedTab.Document.Text);
                SelectedTab.FilePath = sfd.FileName;
            }
        }

        private void exitApp_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void back_Click(object sender, RoutedEventArgs e)
        {
            CurrentEditor?.Undo();
        }

        private void front_Click(object sender, RoutedEventArgs e)
        {
            CurrentEditor?.Redo();
        }

        private void cut_Click(object sender, RoutedEventArgs e)
        {
            CurrentEditor?.Cut();
        }

        private void copy_Click(object sender, RoutedEventArgs e)
        {
            CurrentEditor?.Copy();
        }

        private void paste_Click(object sender, RoutedEventArgs e)
        {
            CurrentEditor?.Paste();
        }

        private void delete_Click(object sender, RoutedEventArgs e)
        {
            if (CurrentEditor != null)
                CurrentEditor.SelectedText = "";
        }

        private void selectAll_Click(object sender, RoutedEventArgs e)
        {
            CurrentEditor?.SelectAll();
        }

        private void about_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("в разработке. метод about_Click");
        }

        private void questions_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("в разработке. метод questions_Click");
        }
    }
}