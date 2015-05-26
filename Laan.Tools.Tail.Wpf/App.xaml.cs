using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Windows;

using Microsoft.Shell;

namespace Laan.Tools.Tail
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application, ISingleInstanceApp
    {
        #region ISingleInstanceApp Members

        public bool SignalExternalCommandLineArgs(IList<string> args)
        {
            Application.Current.MainWindow.BringIntoView();

            IShell shell = AppBootstrapper.Container.GetExportedValue<IShell>();

            foreach (string arg in args.Skip(1))
                shell.FileNames.Add(arg);

            shell.BringLastTabToFront();

            return true;
        }

        #endregion
    }

    public class EntryPoint
    {
        private static string Unique = typeof(App).Namespace + "#20154afa-b92c-4e89-a9e9-a04e7ffe45a7";
                
        [STAThread]
        public static void Main()
        {
            if (!UserSettings.Load().Application.SingleInstance || SingleInstance<App>.InitializeAsFirstInstance(Unique))
            {
                var app = new App();
                app.InitializeComponent();
                app.Run();

                // Allow single instance code to perform cleanup operations
                SingleInstance<App>.Cleanup();
            }
            else
            {
                Environment.Exit(0);
            }
        }
    }
}
