using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.ComponentModel.Composition.Primitives;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;

using Caliburn.Micro;

using MahApps.Metro;

namespace Laan.Tools.Tail
{
    public class AppBootstrapper : Bootstrapper<IShell>
    {

        internal static CompositionContainer Container;
        private ISystemSettings _systemSettings;
        private IUserSettings _userSettings;

        private void LogError(Exception exception)
        {
            const string eventSource = "LaanTail";

            try
            {
                var elog = new EventLog();
                if (!EventLog.SourceExists(eventSource))
                    EventLog.CreateEventSource(eventSource, eventSource);

                elog.Source = eventSource;
                elog.EnableRaisingEvents = true;
                elog.WriteEntry(exception.ToString(), EventLogEntryType.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show(exception.ToString(), "Error: " + ex.Message);
            }

            MessageBox.Show(exception.ToString(), "Error: " + exception.Message);
        }

        private ISystemSettings SystemSettings
        {
            get
            {
                if (_systemSettings == null)
                    _systemSettings = Win.SystemSettings.Load();

                return _systemSettings;
            }
        }

        private IUserSettings UserSettings
        {
            get
            {
                if (_userSettings == null)
                    _userSettings = Win.UserSettings.Load();

                return _userSettings;
            }
        }

        protected override void BuildUp(object instance)
        {
            Container.SatisfyImportsOnce(instance);
        }
        
        /// <summary>
        /// By default, we are configured to use MEF
        /// </summary>
        protected override void Configure()
        {
            var catalog = new AggregateCatalog(
                AssemblySource.Instance.Select(x => new AssemblyCatalog(x)).OfType<ComposablePartCatalog>()
            );

            Container = new CompositionContainer(catalog);
            var batch = new CompositionBatch();

            batch.AddExportedValue<IWindowManager>(new WindowManager());
            batch.AddExportedValue<IEventAggregator>(new EventAggregator());
            batch.AddExportedValue(Container);
            batch.AddExportedValue(catalog);
            batch.AddExportedValue<IUserSettings>(UserSettings);
            batch.AddExportedValue<ISystemSettings>(SystemSettings);
            
            Container.Compose(batch);

            //LogManager.GetLog = type => new DebugLogger(type);
        }

        protected override IEnumerable<object> GetAllInstances(Type serviceType)
        {
            return Container.GetExportedValues<object>(AttributedModelServices.GetContractName(serviceType));
        }

        protected override object GetInstance(Type serviceType, string key)
        {
            string contract = string.IsNullOrEmpty(key) ? AttributedModelServices.GetContractName(serviceType) : key;
            var exports = Container.GetExportedValues<object>(contract);

            if (exports.Count() > 0)
                return exports.First();

            throw new Exception(string.Format("Could not locate any instances of contract {0}.", contract));
        }
        
        protected override void OnExit(object sender, EventArgs e)
        {
            SystemSettings.Save();
            base.OnExit(sender, e);
        }

        protected override void OnStartup(object sender, System.Windows.StartupEventArgs e)
        {
            base.OnStartup(sender, e);

            Application.DispatcherUnhandledException += (s, ex) => LogError(ex.Exception);

            Environment.CurrentDirectory = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);

            Accent accent = ThemeManager.DefaultAccents.First(a => a.Name == UserSettings.Appearance.AccentColor);
            ThemeManager.ChangeTheme(Application.MainWindow, accent, UserSettings.Appearance.Theme);

            IShell shell = Container.GetExportedValue<IShell>();
            UserSettings.Shell = shell;
            ISystemSettings settings = Container.GetExportedValue<ISystemSettings>();
            int storedActiveIndex = settings.ActiveTabIndex;

            if (UserSettings.Application.RememberOpenFiles)
                settings.OpenFiles.Apply(shell.FileNames.Add);

            if (e.Args.Length > 0)
            {
                var newFiles = e.Args.Except(shell.FileNames).ToList();
                newFiles.Apply(shell.FileNames.Add);
                shell.ActiveTabIndex = shell.FileNames.IndexOf(e.Args.Last());
            }
            else
            {
                shell.ActiveTabIndex = storedActiveIndex;
            }
        }
    }
}
