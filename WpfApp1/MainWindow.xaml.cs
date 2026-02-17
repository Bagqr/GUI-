using System;
using System.Windows;
using System.Windows.Documents;
using System.IO;
using System.Windows.Forms;


namespace WpfApp1
{
    public partial class MainWindow : Window
    {
        string FileLocation = "";

        public MainWindow()
        {
            InitializeComponent();
        }

        private void exitApp_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void save_file_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "Text files(*.txt)|*.txt|C# files (*.cs)|*.cs|Xaml files (*.xaml)|*.xaml|All files (*.*)|*.*";
            sfd.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            //sfd.InitialDirectory = @"c:\";

            TextRange tr = new TextRange(richTextBox.Document.ContentStart, richTextBox.Document.ContentEnd);
            if (FileLocation == "")
            {
                if (sfd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    File.WriteAllText(sfd.FileName, tr.Text);
                }
                FileLocation = sfd.FileName;
            }
            else
            {
                File.WriteAllText(FileLocation, tr.Text);
            }
        }
        private void save_as_file_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "Text files(*.txt)|*.txt|C# files (*.cs)|*.cs|Xaml files (*.xaml)|*.xaml|All files (*.*)|*.*";
            sfd.InitialDirectory = @"c:\";
            TextRange tr = new TextRange(richTextBox.Document.ContentStart, richTextBox.Document.ContentEnd);
            if (sfd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                File.WriteAllText(sfd.FileName, tr.Text);
            }
        }

        private void open_file_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            TextRange tr = new TextRange(richTextBox.Document.ContentStart, richTextBox.Document.ContentEnd);
            if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                tr.Text = File.ReadAllText(ofd.FileName);
            }
        }
        private void new_file_Click(object sender, RoutedEventArgs e)
        {
            TextRange tr = new TextRange(richTextBox.Document.ContentStart, richTextBox.Document.ContentEnd);
            tr.Text = "";
            FileLocation = "";
        }
        private void back_Click(object sender, RoutedEventArgs e)
        {
            richTextBox.Undo();
        }
        private void front_Click(object sender, RoutedEventArgs e)
        {
            richTextBox.Redo();
        }
    }
}