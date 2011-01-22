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
using System.Windows.Threading;
using System.Threading;
using winmole.ViewModel;
using winmole.Logic;
using winmole.Entities;
using System.ComponentModel;

namespace winmole
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        Regex directoryPatrrern = new Regex(@"^(\w\:\\)");


        TimeSpan lastKeyStroke = new TimeSpan();

       

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
        private string typeACommandString = "Type a command!";

       

        SearchService searcher;

        DispatcherTimer timer;

      

        BackgroundWorker worker;

        int intervalMs = 200;



        public MainWindow( SearchService searchSrv)
        {
            dataItems = new ObservableCollection<Prompt>();

           
            searcher = searchSrv;


            worker = new BackgroundWorker();

            worker.DoWork += new DoWorkEventHandler(worker_DoWork);
            worker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(worker_RunWorkerCompleted);

            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(intervalMs);
            timer.IsEnabled = false;
            timer.Tick += new EventHandler(timer_Tick);


            InitializeComponent();
           
        }


        void worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            dataItems.Clear();
            if (e.Error != null)
            {
                MessageBox.Show(e.Error.Message);

            }

            var promptItems = e.Result as IList<PromptItem>;

            Debug.WriteLine("add searching items");
            if (promptItems.Count > 0)
            {
                foreach (var item in promptItems)
                {
                    //dataItems.Add(item);
                    dataItems.Add(new Prompt() { Title = item.Name, FullPath = item.FullPath });
                }
            }
            
        }

        void worker_DoWork(object sender, DoWorkEventArgs e)
        {
            string query = (string)e.Argument;
            Debug.WriteLine("searching cmd= " + query);

           // e.Result = new List<PromptItem>();
            try
            {

                //temporary find match directory
                 if (directoryPatrrern.IsMatch(query))
                 {
                     string basePath = System.IO.Path.GetPathRoot(query);


                     string basePath2 = System.IO.Path.GetFullPath(query);

                     string basePath3 = System.IO.Path.GetDirectoryName(query);

                     int dirSeparatorPos = query.LastIndexOf(System.IO.Path.DirectorySeparatorChar);
                     string basePath4 = query.Substring(0, dirSeparatorPos+1);

                     string startDirName = query.Substring(dirSeparatorPos + 1);

                     DirectoryInfo drInfo = new DirectoryInfo(basePath4);
                     if (drInfo.Exists)
                     {
                         var folders = (from f in drInfo.GetDirectories(startDirName+"*")
                                        orderby f.Name ascending
                                        select new PromptItem(f.FullName)).Take(15).ToList();
                         
                         e.Result = folders;
                     }
                 }
                 else
                 {
                     var items = searcher.Search(query);
                     e.Result = items;
                 }
                
            }
            catch (Exception exp)
            {
                throw;

            }


        }


        void timer_Tick(object sender, EventArgs e)
        {

            DispatcherTimer dst = sender as DispatcherTimer;

            if (!worker.IsBusy)
            {
                Debug.WriteLine("worker run");
                worker.RunWorkerAsync(tbCommand.Text);
                timer.Stop();
            }
            else
            {
                Debug.WriteLine("worker busy");
            }

        }





        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            

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

            if (e.Key == Key.Down && itcPrompt.Items.Count > 0)
            {
                //  itcPrompt.Focus();

                // itcPrompt.SelectedIndex = itcPrompt.SelectedIndex+1;

                //if (itcPrompt.SelectedIndex < 0)
                //    itcPrompt.SelectedIndex = 0;
                timer.Stop();

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

            if (e.Key == Key.Return)
            {
                Launch(0);

            }

            //if(e.Key == Key.Tab){

            //    CompletePrompt(0);

            //}
        }

        private void tbCommand_TextChanged(object sender, TextChangedEventArgs e)
        {
            Debug.WriteLine("--->tbCommand_TextChanged " + e.OriginalSource);

            //stop update prompt list
            timer.Stop();

            if (!startTyping)
                return;

            string cmd = tbCommand.Text;
            if (string.IsNullOrWhiteSpace(cmd))
            {
                ClearPrompt();
                //e.Handled = true;
                return;
            }


            //find command type

            var nowTime = DateTime.Now.TimeOfDay;
            var timeKeyStroke = nowTime.Subtract(lastKeyStroke);

            Debug.WriteLine("<----last key press= {0}", timeKeyStroke.Milliseconds);

            lastKeyStroke = nowTime;
            if (cmd.Length < 2)
                return;
            //start timer to search
            timer.Start();


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

        private void ClearPrompt()
        {
            startTyping = false;
            tbCommand.Text = typeACommandString;
            tbCommand.Focus();
            dataItems.Clear();
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
                int selectedIndex = itcPrompt.SelectedIndex;

                Launch(selectedIndex);
                // tbCommand.Text = "";
                // startTyping = false;
                //this.Hide();
            }
           
            if (e.Key == Key.Tab)
            {
                int selectedIndex = itcPrompt.SelectedIndex;

                CompletePrompt(selectedIndex);
            }

        }

        private void Launch(int selectedIndex)
        {


            if (dataItems.Count <= selectedIndex)
                return;

            Prompt pr = dataItems[selectedIndex];

            if (pr != null)
            {
                //Process.Start(pr.ExecutePath);
                ProcessStartInfo prInfo = new ProcessStartInfo(pr.FullPath);
                prInfo.UseShellExecute = true;
                Process.Start(prInfo);

            }

            HideMainWindow();
        }

        private void CompletePrompt(int selectedIndex)
        {
            if (dataItems.Count <= selectedIndex)
                return;

             Prompt pr = dataItems[selectedIndex];

             if (pr != null)
             {
                 tbCommand.Text = pr.Title;
                 tbCommand.SelectionStart = pr.Title.Length;
                 tbCommand.Focus();
             }

        }

        /// <summary>
        /// Check if key is not editing key
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


        private void hostWindow_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            Debug.WriteLine("-->hostWindow_previewKeyUp " + e.OriginalSource);
            if (e.Key == Key.Escape)
            {

                HideMainWindow();

                e.Handled = true;
               // Close();
            }
            if (e.Key == Key.Tab)
            {
                if (itcPrompt.SelectedIndex > -1)
                {
                    CompletePrompt(itcPrompt.SelectedIndex);
                }
                else
                {
                    CompletePrompt(0);
                }
                e.Handled = true;
            }
        }

        private void HideMainWindow()
        {
            ClearPrompt();
            this.Hide();
        }



        protected override void OnClosing(CancelEventArgs e)
        {
            

            trayIcon.Dispose();

            base.OnClosing(e);
        }

        private void taskbarIcon_TrayLeftMouseUp(object sender, RoutedEventArgs e)
        {
            ActvateMainWindow();
        }

       

        private void miShowWindow_Click(object sender, RoutedEventArgs e)
        {
            ActvateMainWindow();
        }



        public void ActvateMainWindow()
        {
            if (!IsVisible)
            {
                Show();
            }

            this.Activate();
            tbCommand.Focus();
            tbCommand.SelectionStart = tbCommand.Text.Length;
        }

        private void miClose_Click(object sender, RoutedEventArgs e)
        {
            //Close();
            Application.Current.Shutdown();
        }

        private void hostWindow_Deactivated(object sender, EventArgs e)
        {

            Debug.WriteLine("-->Deactivate window");
            this.Hide();

            //this.Activate();
            //tbCommand.Focus();
            //HideMainWindow();
        }


       
    }
}
