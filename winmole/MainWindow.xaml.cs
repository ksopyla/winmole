﻿using System;
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
using System.Windows.Threading;

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

        IndexingService indexer;

        SearchService searcher;

        DispatcherTimer timer;


        public MainWindow()
        {

            Debug.Listeners.Add(new ConsoleTraceListener());
            dataItems = new ObservableCollection<Prompt>();


            indexer = new IndexingService();
            searcher = new SearchService();


            InitializeComponent();



            //timer = new DispatcherTimer();
            //timer.Interval = TimeSpan.FromSeconds(15);
            //timer.Tick += new EventHandler(timer_Tick);
            //timer.Start();
            

        }

        //void timer_Tick(object sender, EventArgs e)
        //{

        //    tblIndexing.Visibility = Visibility.Visible;
        //    System.Threading.Thread.Sleep(5 * 1000);
        //    tblIndexing.Visibility = Visibility.Collapsed;
        //}

       

       

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
           // Width = tbCommand.Width;

            indexer.BuildIndex();



            tbCommand.Focus();
        }

        private void tbCommand_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            Debug.WriteLine("--->tbCommand_PreviewKeyDown " + e.OriginalSource);

            if (!startTyping)
            {
                tbCommand.Text = "";
                startTyping = true;
               // e.Handled = true;
                return;
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
            Debug.WriteLine("--->tbCommand_PreviewKeyUp " + e.OriginalSource);

            if (string.IsNullOrWhiteSpace(tbCommand.Text))
            {
                startTyping = false;
                tbCommand.Text = typeACommandString;
            }
        }


        private void tbCommand_TextChanged(object sender, TextChangedEventArgs e)
        {
            Debug.WriteLine("--->tbCommand_TextChanged " + e.OriginalSource);

            if (!startTyping)
                return;
            
            string cmd = tbCommand.Text;
            if (string.IsNullOrEmpty(cmd))
            {
                e.Handled = true;
                return;
            }
            //find command type


            var items =  searcher.Search(tbCommand.Text);
            if (items.Count > 0)
            {
                dataItems.Clear();

                foreach (var item in items)
                {
                    dataItems.Add(item);
                }
            
            }

            //temporary find match directory
            //if (directoryPatrrern.IsMatch(cmd))
            //{
            //    DirectoryInfo drInfo = new DirectoryInfo(cmd);
            //    if (drInfo.Exists)
            //    {
            //        var folders = (from f in drInfo.GetDirectories()
            //                       select new Prompt(f.FullName)).Take(15);

            //        dataItems.Clear();

            //        foreach (var item in folders)
            //        {
            //            dataItems.Add(item);
            //        }

                    
            //    }
            //}
            //else
            //    dataItems.Clear();
        }

        private void itcPrompt_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            Debug.WriteLine("---->itcPrompt_previewKeyDown " + e.OriginalSource);

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
                    return true;
            }

            return false;
        }

       
        private void hostWindow_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            Debug.WriteLine("-->hostWindow_previewKeyUp " + e.OriginalSource);
            if (e.Key == Key.Escape)
            {
                e.Handled = true;
                Close();
            }
        }


        protected override void OnClosed(EventArgs e)
        {
            searcher.Dispose();
            indexer.Dispose();

            base.OnClosed(e);
        }
              
    }
}
