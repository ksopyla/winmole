using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;
using System.Collections.ObjectModel;
using System.Text.RegularExpressions;

namespace winmole
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        Regex directoryPatrrern = new Regex(@"^(\w\:\\)");

        private ObservableCollection<Prompt> dataItems;

        public ObservableCollection<Prompt> DataItems
        {
            get
            {
                if (dataItems == null)
                {
                    return new ObservableCollection<Prompt>();
                }
                return dataItems;
            }
        }

        bool startTyping = false;
        private string typeACommandString="Type a command!";

        public MainWindow()
        {
            dataItems = new ObservableCollection<Prompt>();

            InitializeComponent();

        }

        private void TextBox_KeyUp(object sender, KeyEventArgs e)
        {
           
           
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
           // Width = tbCommand.Width;
            tbCommand.Focus();
        }

        private void tbCommand_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (!startTyping)
            {
                tbCommand.Text = "";
                startTyping = true;
            }

            if (e.Key == Key.Down && itcPrompt.Items.Count>0)
            {
                itcPrompt.Focus();
                itcPrompt.SelectedIndex = 0;
            }
            

        }

        private void tbCommand_PreviewKeyUp(object sender, KeyEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(tbCommand.Text))
            {
                startTyping = false;
                tbCommand.Text = typeACommandString;
            }
        }

        private void Window_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
                Close();
        }

        private void tbCommand_TextChanged(object sender, TextChangedEventArgs e)
        {
             string cmd = tbCommand.Text;
            
            if (directoryPatrrern.IsMatch(cmd))
            {
                DirectoryInfo drInfo = new DirectoryInfo(cmd);
                if (drInfo.Exists)
                {
                    var folders = (from f in drInfo.GetDirectories()
                                   select new Prompt(f.FullName)).Take(15);

                    dataItems.Clear();

                    foreach (var item in folders)
                    {
                        dataItems.Add(item);
                    }

                    
                }
            }
            else
                dataItems.Clear();
        }

        private void tbCommand_TextInput(object sender, TextCompositionEventArgs e)
        {

        }
    }
}
