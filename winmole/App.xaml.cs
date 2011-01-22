using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Windows;
using System.Diagnostics;
using winmole.Logic;

namespace winmole
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        ManagedWinapi.Hotkey hotKey;

        MainWindow mainWindow;

        IndexingService indexer;

        SearchService searcher;

        protected override void OnStartup(StartupEventArgs e)
        {
            Debug.Listeners.Add(new ConsoleTraceListener());


            hotKey = new ManagedWinapi.Hotkey();
            //hotKey.WindowsKey = true;
            hotKey.Shift = true;
            hotKey.KeyCode = System.Windows.Forms.Keys.Space ;
            hotKey.HotkeyPressed += new EventHandler(hotKey_HotkeyPressed);

            try
            {
                hotKey.Enabled = true;
            }
            catch (ManagedWinapi.HotkeyAlreadyInUseException)
            {
                MessageBox.Show("Could not register hotkey (win+space already in use)", "Register key error", MessageBoxButton.OK, MessageBoxImage.Error);

                ShutdownMode = System.Windows.ShutdownMode.OnLastWindowClose;
            }


            var paths = winmole.Properties.Settings.Default.IndexedPaths;

            ICollection<KeyValuePair<string, List<string>>> pathsAndExt = TransofrmSettingsPaths(paths);


            indexer = new IndexingService(pathsAndExt);

            searcher = new SearchService();
            mainWindow = new MainWindow(searcher);

            indexer.BuildIndex();

            base.OnStartup(e);

            if(ShutdownMode == System.Windows.ShutdownMode.OnLastWindowClose)
            {
                ShowMainWindow();
            }
        }

        private ICollection<KeyValuePair<string, List<string>>> TransofrmSettingsPaths(System.Collections.Specialized.StringCollection paths)
        {

            List<KeyValuePair<string, List<string>>> pathsAndExt = new List<KeyValuePair<string, List<string>>>(paths.Count);


            foreach (var item in paths)
            {
                int spacePos = item.IndexOf("|");
                string path = item.Substring(0, spacePos);
                path = path.Trim();

                string ext = item.Substring(spacePos+1);
                var extArr = ext.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

                for (int i = 0; i < extArr.Length; i++)
                {
                    extArr[i] = extArr[i].Trim();
                }


                pathsAndExt.Add(new KeyValuePair<string, List<string>>(path, extArr.ToList()));

            }


            return pathsAndExt;
        }

        void hotKey_HotkeyPressed(object sender, EventArgs e)
        {
            Debug.WriteLine("hotkey pressed");
            ShowMainWindow();

        }

        private void ShowMainWindow()
        {
            if (mainWindow == null)
                mainWindow = new MainWindow(searcher);

            //if (!mainWindow.IsVisible)
            //    mainWindow.ShowDialog();
            //else
            //    mainWindow.Activate();

            mainWindow.ActvateMainWindow();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            hotKey.Dispose();
            indexer.Dispose();
            indexer = null;
            searcher.Dispose();
            searcher = null;


            base.OnExit(e);
        }
        
    }
}
