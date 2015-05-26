using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Reactive.Linq;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

using Caliburn.Micro;

using MahApps.Metro;

using Microsoft.Win32;

namespace Laan.Tools.Tail
{
    public class BufferedObservableCollection<T> : ObservableCollection<T>
    {
        public void AddRange(IEnumerable<T> range)
        {
            foreach (var item in range)
            {
                Items.Add(item);
            }

            OnPropertyChanged(new PropertyChangedEventArgs("Count"));
            OnPropertyChanged(new PropertyChangedEventArgs("Item[]"));
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }

        public void Reset(IEnumerable<T> range)
        {
            Items.Clear();
            AddRange(range);
        }
    }

    [Export(typeof(IShell))]
    public class ShellViewModel : PropertyChangedBase, IShell, IDisposable
    {
        private readonly FileSystemWatcher _configWatcher;

        private bool _canFindNextItem;
        private int _currentRow;
        private TailService _currentService;
        private bool _findShow;
        private string _findText;
        private TabFileItem _selectedFile;
        private string _status;
        private WindowState _windowState;

        public ShellViewModel()
        {
            Buffer = new BufferedObservableCollection<BufferItem>();
            TabFileItems = new ObservableCollection<TabFileItem>();
            FileNames = new ObservableCollection<string>();
            FileNames.CollectionChanged += FileNamesCollectionChanged;

            _configWatcher = new FileSystemWatcher(Path.GetDirectoryName(Win.UserSettings.ConfigFile));
            _configWatcher.EnableRaisingEvents = true;
            _configWatcher.Changed += ConfigChanged;
        }

        ~ShellViewModel()
        {
            _configWatcher.Changed -= ConfigChanged;
        }

        private void ConfigChanged(object sender, FileSystemEventArgs e)
        {
            UserSettings = Win.UserSettings.Load();
            UserSettings.Shell = this;
            Execute.OnUIThread(ApplyConfiguration);
        }

        private void FileNamesCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (UserSettings.Application.RememberOpenFiles)
                SystemSettings.OpenFiles = FileNames;

            if (FileNames == null)
            {
                SelectedTab = null;
                return;
            }

            if (e.Action != NotifyCollectionChangedAction.Add)
                return;

            using (new CursorManager())
            {
                foreach (var fileName in FileNames)
                {
                    if (!TabFileItems.Any(tab => tab.FileName == fileName))
                        LoadFile(fileName);
                }
            }
        }

        //public bool CanReload
        //{
        //    get { return _selectedFile != null; }
        //}

        private async Task<IList<BufferItem>> GetBuffer()
        {
            var buffer = await _currentService.Buffer();
                
            return buffer
                .Select(b => new BufferItem(UserSettings) { Line = b })
                .ToList();
        }

        private T GetComponent<T>(string name) where T : class
        {
            return Application.Current.MainWindow.FindName(name) as T;
        }

        private TabFileItem LoadFile(string fileName)
        {
            var service = new TailService(UserSettings, fileName);
            service.Changed += (sender, e) => Execute.OnUIThread(() => TailChanged(e));

            var item = new TabFileItem(this, fileName, Path.GetFileNameWithoutExtension(fileName), service);
            item.FollowTail = UserSettings.Tail.AutoFollow;
            TabFileItems.Add(item);

            return item;
        }

        private void MoveTab(int delta)
        {
            var index = (TabFileItems.IndexOf(SelectedTab) + delta) % TabFileItems.Count;
            SelectedTab = TabFileItems[index];
        }

        private void Reset()
        {
            foreach (var tabFileItem in TabFileItems)
            {
                tabFileItem.ResetChange();

                if (tabFileItem.ChangeCount > 0 && tabFileItem == SelectedTab)
                    Reload(false);
            }
        }

        private BufferItem SearchNext(int startRow)
        {
            var next = Buffer
                .Skip(startRow)
                .SkipWhile(b => !b.HasHighlighter)
                .FirstOrDefault();

            if (next != null)
                CurrentRow = Buffer.IndexOf(next);

            return next;
        }

        private void SearchText(string findText)
        {
            Color foundColor = Colors.BlueViolet;

            Regex regex;
            try
            {
                regex = new Regex(findText, RegexOptions.IgnoreCase);
            }
            catch
            {
                return;
            }

            foreach (var buffer in Buffer)
            {
                var matched = regex.IsMatch(buffer.Line);
                var color = matched ? foundColor : Colors.Black;

                buffer.BackgroundColor = new SolidColorBrush(color);
                buffer.ForegroundColor = new SolidColorBrush(Colors.White);
                buffer.HasHighlighter = matched;
            }
            SearchNext(CurrentRow);
        }

        private void SetFont()
        {
            var lb = BufferListBox;
            lb.FontFamily = new FontFamily(UserSettings.Appearance.FontFamily);
            lb.FontSize = UserSettings.Appearance.FontSize;
        }

        private void TailChanged(TailChangedEventArgs e)
        {
            var tab = TabFileItems.FirstOrDefault(item => item.FileName == e.FileName);
            if (tab != null)
            {
                tab.ApplyChange();
                Reset();
            }
        }

        private ListBox BufferListBox
        {
            get { return GetComponent<ListBox>("Buffer"); }
        }

        public void ApplyConfiguration()
        {
            try
            {
                var currentRow = SelectedTab != null ? SelectedTab.CurrentRow : 0;

                Accent accent = ThemeManager.DefaultAccents.First(a => a.Name == UserSettings.Appearance.AccentColor);
                ThemeManager.ChangeTheme(Application.Current.MainWindow, accent, UserSettings.Appearance.Theme);
                TabFileItems.Apply(tab => tab.TailService.ApplyConfiguration(UserSettings));

                SetFont();
                Reload(force: true);
                Buffer.Apply(b => b.ApplyConfiguration(UserSettings));
                
                if (SelectedTab != null)
                    SelectedTab.CurrentRow = currentRow;
                
                Refresh();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "LaanTail - Error Updating Settings - please check the XML and try again");
            }
        }

        public void BringLastTabToFront()
        {
            ActiveTabIndex = FileNames.IndexOf(FileNames.Last());
        }

        //public bool CanClear()
        //{
        //    try
        //    {
        //        if (SelectedFile == null)
        //            return false;
        //        
        //        FileInfo file = new FileInfo(SelectedFile.FileName);
        //        return !file.Attributes.HasFlag(FileAttributes.ReadOnly);
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine(ex);
        //        return false;
        //    }
        //}

        public void Clear()
        {
            if (SelectedTab == null)
                return;

            FileInfo file = new FileInfo(SelectedTab.FileName);
            if (file.Attributes.HasFlag(FileAttributes.ReadOnly))
                return;

            var dialogResult = MessageBox.Show(
                "Are you sure you want to clear this file?",
                "LaanTail - Clear File",
                MessageBoxButton.YesNo
            );

            if (dialogResult != MessageBoxResult.Yes)
                return;

            File.WriteAllText(SelectedTab.FileName, "");
            CurrentRow = Buffer.Count - 1;
        }

        public void Close()
        {
            Close(SelectedTab);
        }

        public void Close(TabFileItem tabItem)
        {
            int index = TabFileItems.IndexOf(tabItem);
            FileNames.Remove(tabItem.FileName);
            TabFileItems.Remove(tabItem);
            tabItem.TailService.Dispose();

            if (TabFileItems.Count == 0)
                Buffer.Clear();

            SelectedTab = TabFileItems.Any() ? TabFileItems[Math.Min(index, TabFileItems.Count - 1)] : null;
        }

        public void CloseAll()
        {
            using (new CursorManager())
            {
                TabFileItems.Clear();
                SelectedTab = null;
                Buffer.Clear();
            }
        }

        public void CloseOthers(TabFileItem tabItem)
        {
            using (new CursorManager())
            {
                TabFileItems
                    .Where(tab => tab != tabItem)
                    .Apply(tab => Close(tab));
            }
        }

        //public bool CanCopy
        //{
        //    get { return Buffer != null && Buffer.Any(b => b.IsSelected); }
        //}

        public void Copy()
        {
            using (new CursorManager())
            {
                try
                {
                    var text = Buffer
                        .Where(b => b.IsSelected)
                        .Select(b => b.Line)
                        .Join(Environment.NewLine);
                    
                    Clipboard.SetText(text);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "LaanTail - Windows Clipboard Error");
                }
            }
        }

        public void Find()
        {
            FindShow = true;
            CanFindNextItem = true;
        }

        public void FindClose()
        {
            FindShow = false;
        }

        public void FindNext()
        {
            if (!String.IsNullOrEmpty(FindText))
            {
                var next = SearchNext(CurrentRow + 1);
                if (next == null)
                    next = SearchNext(0);

                CanFindNextItem = next != null;
            }
        }

        public void Follow()
        {
            if (SelectedTab == null)
                return;

            SelectedTab.FollowTail = !SelectedTab.FollowTail;
        }

        public void NextTab()
        {
            MoveTab(1);
        }

        public void OnFileDrag(DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
                e.Effects = DragDropEffects.All;
            else
                e.Effects = DragDropEffects.None;
        }

        public void OnFileDrop(DragEventArgs e)
        {
            var files = (string[])e.Data.GetData(DataFormats.FileDrop, false);
            files.Apply(FileNames.Add);
            SelectedTab = TabFileItems.Last();
        }

        public void Open()
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Multiselect = true;
            dialog.Filter = "Log Files (*.log)|*.log|Any File (*.*)|*.*";

            if (dialog.ShowDialog() != true)
                return;

            dialog.FileNames.Apply(FileNames.Add);
            SelectedTab = TabFileItems.Last();
        }

        public void PreviousTab()
        {
            MoveTab(-1);
        }

        public void Quit()
        {
            Application.Current.MainWindow.Close();
        }

        public void Reload()
        {
            Reload(true);
        }

        public async void Reload(bool force)
        {
            if (SelectedTab == null)
                return;

            if (!force && !SelectedTab.FollowTail && Buffer.Any() && SelectedTab.CurrentRow != Buffer.Count - 1)
                return;

            var items = await GetBuffer();

            Buffer.Reset(items);
            CurrentRow = Buffer.Count - 1;
        }

        public void SelectAll()
        {
            Buffer.Apply(b => b.IsSelected = true);
        }

        public void Settings()
        {
            Process.Start(new ProcessStartInfo()
            {
                FileName = Win.UserSettings.ConfigFile,
                UseShellExecute = true,
                Verb = "Edit"
            });
        }

        //public void OnMouseWheel(MouseWheelEventArgs e)
        //{
        //    if (!_ctrlPressed)
        //        return;

        //    MessageBox.Show("!");
        //}

        //public void OnKeyDown(KeyboardEventArgs e)
        //{
        //    _ctrlPressed = e.KeyboardDevice.Modifiers == ModifierKeys.Control;
        //}

        public void TabClick(TabFileItem tabItem, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Middle)
                Close(tabItem);
        }

        public void WordWrap()
        {
            UserSettings.WordWrap = !UserSettings.WordWrap;
        }

        public int ActiveTabIndex
        {
            get { return SelectedTab != null ? TabFileItems.IndexOf(SelectedTab) : -1; }
            set
            {
                if (value >= 0 && value < TabFileItems.Count)
                    SelectedTab = TabFileItems[value];
            }
        }

        public BufferedObservableCollection<BufferItem> Buffer { get; set; }

        public bool CanFindNextItem
        {
            get { return _canFindNextItem; }
            set
            {
                _canFindNextItem = value;
                NotifyOfPropertyChange(() => CanFindNextItem);
            }
        }

        public int CurrentRow
        {
            get { return _currentRow; }
            set
            {
                _currentRow = value;
                if (SelectedTab != null)
                    SelectedTab.CurrentRow = value;

                NotifyOfPropertyChange(() => CurrentRow);
                BufferListBox.ScrollIntoView(BufferListBox.SelectedItem);
            }
        }
 
        public ObservableCollection<string> FileNames { get; set; }

        public int FindHeight { get { return FindShow ? 20 : 0; } }

        public bool FindShow
        {
            get { return _findShow; }
            set
            {
                _findShow = value;
                if (!_findShow)
                    FindText = "";
                else 
                    GetComponent<TextBox>("FindText").Focus();

                NotifyOfPropertyChange(() => FindShow);
                NotifyOfPropertyChange(() => FindHeight);
            }
        }

        public string FindText
        {
            get { return _findText; }
            set
            {
                _findText = value;
                NotifyOfPropertyChange(() => FindText);
                
                if (FindText != "")
                    SearchText(FindText);
                else 
                    Buffer.Apply(b => b.ApplyConfiguration(UserSettings));
            }
        }

        public TabFileItem SelectedTab
        {
            get { return _selectedFile; }
            set
            {
                if (_selectedFile == value)
                    return;

                using (new CursorManager())
                {
                    _selectedFile = value;
                    NotifyOfPropertyChange(() => SelectedTab);
                    //NotifyOfPropertyChange(() => CanReload);
                    //NotifyOfPropertyChange(() => CanClear);

                    if (UserSettings.Application.RememberOpenFiles)
                        SystemSettings.ActiveTabIndex = _selectedFile == null ? -1 : TabFileItems.IndexOf(_selectedFile);

                    if (_selectedFile == null)
                        return;

                    _currentService = _selectedFile.TailService;

                    Reload();
                    TabFileItems.Apply(tab => tab.NotifyOfPropertyChange("FollowTailShow"));
                }
                Reset();
            }
        }

        public WindowState SettingsWindowState
        {
            get
            {
                if (UserSettings.Application.RememberWindowState)
                    _windowState = SystemSettings.WindowState;
                return _windowState;
            }
            set
            {
                _windowState = value;
                if (UserSettings.Application.RememberWindowState)
                    SystemSettings.WindowState = value;
            }
        }

        public string Status
        {
            get { return _status; }
            set
            {
                _status = value;
                NotifyOfPropertyChange(() => Status);
            }
        }

        [Import(typeof(ISystemSettings))]
        public ISystemSettings SystemSettings { get; set; }
        public ObservableCollection<TabFileItem> TabFileItems { get; set; }

        [Import(typeof(IUserSettings))]
        public IUserSettings UserSettings { get; set; }

        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    _currentService.Dispose();
                    _configWatcher.Dispose();
                }

                disposedValue = true;
            }
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
        }
    }
}
