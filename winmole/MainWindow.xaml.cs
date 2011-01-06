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
using System.Diagnostics;

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
              //  itcPrompt.Focus();

              // itcPrompt.SelectedIndex = itcPrompt.SelectedIndex+1;

                //if (itcPrompt.SelectedIndex < 0)
                //    itcPrompt.SelectedIndex = 0;
                if (itcPrompt.SelectedIndex > -1)
                {
                    ListBoxItem lbi = itcPrompt.ItemContainerGenerator.ContainerFromIndex(itcPrompt.SelectedIndex) as ListBoxItem;
                    if (lbi != null)
                    {
                        lbi.Focus();
                    }
                }
                else
                    itcPrompt.Focus();
                //itcPrompt.SelectedIndex = itcPrompt.SelectedIndex + 1;
                
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
            

            //find command type



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

        private void itcPrompt_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (!IsNotEditingKey(e))
            {
                tbCommand.Focus();
                tbCommand.SelectionStart = tbCommand.Text.Length;
            }
            else if (e.Key == Key.Return)
            {
                Prompt pr = itcPrompt.SelectedValue as Prompt;
                if(pr!=null)
                Process.Start(pr.ExecutePath);
                tbCommand.Text = "";
                //this.Hide();
            }

        }

        /// <summary>
        /// Check if key is 
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        private static bool IsNotEditingKey(KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Up:
                case Key.Down:
                case Key.PageDown:
                case Key.PageUp:
                case Key.Home:
                case Key.End:
                case Key.Return:
                    return true;
            }

            return false;
        }

        private void itcPrompt_GotFocus(object sender, RoutedEventArgs e)
        {
            string source = e.OriginalSource.ToString();
            var arr = source.Split(new char[]{' '});
            source = arr[0];
            int p = source.LastIndexOf(".");
            source = source.Substring(p);

        }

        private void itcPrompt_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void itcPrompt_TextInput(object sender, TextCompositionEventArgs e)
        {

        }

        private void itcPrompt_KeyUp(object sender, KeyEventArgs e)
        {

        }

        private void itcPrompt_PreviewKeyUp(object sender, KeyEventArgs e)
        {

        }

       
    }
}
