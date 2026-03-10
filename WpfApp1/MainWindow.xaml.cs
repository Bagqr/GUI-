using ICSharpCode.AvalonEdit.Document;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using MessageBox = System.Windows.MessageBox;

namespace WpfApp1
{
    public class EditorTab : INotifyPropertyChanged
    {
        private string _filePath;
        public ICSharpCode.AvalonEdit.TextEditor Editor { get; set; }

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
        private Dictionary<MenuItem, string> _originalHeaders = new Dictionary<MenuItem, string>();

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

            SaveOriginalHeaders();

            System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("ru-RU");
            System.Threading.Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo("ru-RU");
            UpdateUI();

            SetDefaultLanguageCheck();
        }

        private void SetDefaultLanguageCheck()
        {
            foreach (var mainItem in mainMenu.Items)
            {
                if (mainItem is MenuItem menuItem && menuItem.Header.ToString() == "Справка")
                {
                    foreach (var subItem in menuItem.Items)
                    {
                        if (subItem is MenuItem langMenu && langMenu.Header.ToString() == "Язык")
                        {
                            foreach (var langItem in langMenu.Items)
                            {
                                if (langItem is MenuItem mi && mi.Header.ToString() == "Русский")
                                {
                                    mi.IsChecked = true;
                                    return;
                                }
                            }
                        }
                    }
                }
            }
        }
        private ICSharpCode.AvalonEdit.TextEditor CurrentEditor => SelectedTab?.Editor;

        private void DumpVisualTree(DependencyObject obj, int level)
        {
            if (obj == null) return;
            string indent = new string(' ', level * 2);
            System.Diagnostics.Debug.WriteLine($"{indent}{obj.GetType().Name}");
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(obj); i++)
            {
                DumpVisualTree(VisualTreeHelper.GetChild(obj, i), level + 1);
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

            var editor = CurrentEditor;
            editor?.SelectAll();
        }

        private void TextEditor_Loaded(object sender, RoutedEventArgs e)
        {
            var editor = sender as ICSharpCode.AvalonEdit.TextEditor;
            if (editor?.DataContext is EditorTab tab)
            {
                tab.Editor = editor; 
            }
        }
        private void about_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("в разработке. метод about_Click");
        }

        private void questions_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("в разработке. метод questions_Click");
        }

        private void SetFontSize_Click(object sender, RoutedEventArgs e)
        {
            var menuItem = sender as MenuItem;
            if (menuItem?.Tag is string sizeStr && double.TryParse(sizeStr, out double size))
            {
                foreach (var tab in Tabs)
                {
                    if (tab.Editor != null)
                        tab.Editor.FontSize = size;
                }

                foreach (var item in ((MenuItem)menuItem.Parent).Items)
                {
                    if (item is MenuItem mi)
                        mi.IsChecked = false;
                }
                menuItem.IsChecked = true;
            }
        }
        private void SetCustomFontSize_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new Window
            {
                Title = resources.Language.FontSize,
                Width = 300,
                Height = 150,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Owner = this,
                WindowStyle = WindowStyle.ToolWindow,
                ResizeMode = ResizeMode.NoResize
            };

            var panel = new StackPanel { Margin = new Thickness(10) };
            panel.Children.Add(new TextBlock { Text = resources.Language.EnterSize, Margin = new Thickness(0, 0, 0, 5) });

            var textBox = new TextBox { Text = CurrentEditor?.FontSize.ToString() ?? "12" };
            textBox.SelectAll();
            panel.Children.Add(textBox);

            var btnPanel = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Right, Margin = new Thickness(0, 10, 0, 0) };
            var okBtn = new Button { Content = resources.Language.OK, Width = 60, Height = 22, Margin = new Thickness(0, 0, 5, 0), IsDefault = true };
            var cancelBtn = new Button { Content = resources.Language.Cancel, Width = 60, Height = 22, IsCancel = true };

            btnPanel.Children.Add(okBtn);
            btnPanel.Children.Add(cancelBtn);
            panel.Children.Add(btnPanel);
            dialog.Content = panel;

            okBtn.Click += (s, args) =>
            {
                if (double.TryParse(textBox.Text, out double size))
                {
                    size = Math.Max(6, Math.Min(48, size));
                    foreach (var tab in Tabs)
                        if (tab.Editor != null)
                            tab.Editor.FontSize = size;

                    if (mainMenu.Items[2] is MenuItem textMenu &&
                        textMenu.Items[7] is MenuItem fontSizeMenu)
                    {
                        foreach (var menuItem in fontSizeMenu.Items)
                        {
                            if (menuItem is MenuItem mi && mi.Header.ToString() != resources.Language.Other)
                            {
                                mi.IsChecked = false;
                            }
                        }
                    }

                    dialog.Close();
                }
                else
                {
                    MessageBox.Show(resources.Language.ErrorInvalidNumber, resources.Language.FontSizeTitle,
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    textBox.Focus();
                    textBox.SelectAll();
                }
            };

            dialog.ShowDialog();
        }
        private void SetRussian_Click(object sender, RoutedEventArgs e)
        {
            System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("ru-RU");
            System.Threading.Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo("ru-RU");

            UpdateUI();

            ((MenuItem)sender).IsChecked = true;
            foreach (var item in ((MenuItem)((MenuItem)sender).Parent).Items)
            {
                if (item is MenuItem mi && mi != sender)
                    mi.IsChecked = false;
            }
        }

        private void SetEnglish_Click(object sender, RoutedEventArgs e)
        {
            System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en-US");
            System.Threading.Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo("en-US");

            UpdateUI();

            ((MenuItem)sender).IsChecked = true;
            foreach (var item in ((MenuItem)((MenuItem)sender).Parent).Items)
            {
                if (item is MenuItem mi && mi != sender)
                    mi.IsChecked = false;
            }
        }
        private void SetMongolian_Click(object sender, RoutedEventArgs e)
        {
            System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("mn-MN");
            System.Threading.Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo("mn-MN");

            UpdateUI();

            ((MenuItem)sender).IsChecked = true;
            foreach (var item in ((MenuItem)((MenuItem)sender).Parent).Items)
            {
                if (item is MenuItem mi && mi != sender)
                    mi.IsChecked = false;
            }
        }
        private void UpdateUI()
        {
            this.Title = resources.Language.WindowTitle;

            UpdateMenuItems(mainMenu);

            UpdateDataGridColumns();

            UpdateToolTips();
        }
        private void UpdateDataGridColumns()
        {
            if (DataGrid1 == null || DataGrid1.Columns.Count < 4)
                return;

            DataGrid1.Columns[0].Header = resources.Language.File;       
            DataGrid1.Columns[1].Header = resources.Language.Position;  
            DataGrid1.Columns[2].Header = resources.Language.Code;      
            DataGrid1.Columns[3].Header = resources.Language.Error;     
        }
        private void UpdateToolTips()
        {
            create.ToolTip = resources.Language.New;
            open.ToolTip = resources.Language.Open;
            save.ToolTip = resources.Language.Save;
            back.ToolTip = resources.Language.Undo;
            front.ToolTip = resources.Language.Redo;
            copy.ToolTip = resources.Language.Copy;
            paste.ToolTip = resources.Language.Paste;
            question.ToolTip = resources.Language.HelpContent;
            about.ToolTip = resources.Language.About;
        }
        
        private void UpdateMenuItems(ItemsControl itemsControl)
        {
            foreach (var item in itemsControl.Items)
            {
                if (item is MenuItem menuItem)
                {
                    if (_originalHeaders.TryGetValue(menuItem, out string originalHeader))
                    {
                        string translated = GetTranslation(originalHeader);

                        if (menuItem.Header.ToString() != translated)
                        {
                            menuItem.Header = translated;
                            System.Diagnostics.Debug.WriteLine($"Переведено: {originalHeader} -> {translated}");
                        }
                    }
                    else
                    {
                        _originalHeaders[menuItem] = menuItem.Header.ToString();
                        System.Diagnostics.Debug.WriteLine($"Экстренно сохранён: {menuItem.Header}");
                    }

                    if (menuItem.Items.Count > 0)
                        UpdateMenuItems(menuItem);
                }
            }
        }
        private void SaveOriginalHeaders()
        {
            SaveOriginalHeadersRecursive(mainMenu);
        }

        private void SaveOriginalHeadersRecursive(ItemsControl itemsControl)
        {
            foreach (var item in itemsControl.Items)
            {
                if (item is MenuItem menuItem)
                {
                    if (!_originalHeaders.ContainsKey(menuItem))
                    {
                        _originalHeaders[menuItem] = menuItem.Header.ToString();
                        System.Diagnostics.Debug.WriteLine($"Сохранён оригинал: {menuItem.Header}");
                    }

                    if (menuItem.Items.Count > 0)
                        SaveOriginalHeadersRecursive(menuItem);
                }
            }
        }
        private string GetTranslation(string russianText)
        {
            var translationMap = new Dictionary<string, string>
    {
        {"Файл", "File"},
        {"Правка", "Edit"},
        {"Текст", "Text"},
        {"Пуск", "Start"},
        {"Справка", "Help"},
        
        {"Новый файл", "New"},
        {"Создать", "New"},
        {"Открыть", "Open"},
        {"Сохранить", "Save"},
        {"Сохранить как", "SaveAs"},
        {"Выход", "Exit"},
        
        {"Назад", "Undo"},
        {"Заново", "Redo"},
        {"Вырезать", "Cut"},
        {"Копировать", "Copy"},
        {"Вставить", "Paste"},
        {"Удалить", "Delete"},
        {"Выделить всё", "SelectAll"},
        
        {"Постановка задачи", "TaskStatement"},
        {"Грамматика", "Grammar"},
        {"Классификация грамматики", "GrammarClassification"},
        {"Метод анализа", "AnalysisMethod"},
        {"Тестовый пример", "TestExample"},
        {"Список литературы", "References"},
        {"Исходный код программы", "SourceCode"},
        {"Размер шрифта", "FontSize"},

        {"Другой...", "Other"},
        
        {"Вызов справки", "HelpContent"},
        {"О программе", "About"},
        {"Язык", "MenuLanguage"},
        {"Русский", "Russian"},
        {"English", "English"},
        {"Монгольский", "Mongolian"},
        
        {"Позиция", "Position"},
        {"Код", "Code"},
        {"Ошибка", "Error"},
    };

            if (translationMap.ContainsKey(russianText))
            {
                var resource = resources.Language.ResourceManager.GetString(translationMap[russianText]);
                return resource ?? russianText;
            }

            return russianText;
        }
    }
}