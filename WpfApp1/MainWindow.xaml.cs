using Microsoft.Win32;
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
            System.Windows.Forms.SaveFileDialog sfd = new System.Windows.Forms.SaveFileDialog();
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

    }
}