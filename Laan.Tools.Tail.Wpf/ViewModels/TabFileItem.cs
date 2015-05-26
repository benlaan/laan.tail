using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;

using Caliburn.Micro;
using System.Timers;
using Laan.Tools.Tail.Services;

namespace Laan.Tools.Tail
{
    public class TabFileItem : PropertyChangedBase
    {
        private int _lastChangeCount;
        private int _changeCount;
        private string _fileName;
        private string _title;
        private bool _followTail;
        private int _currentRow;
                
        private readonly ShellViewModel _viewModel;

        /// <summary>
        /// Initializes a new instance of the TabFileItem class.
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="title"></param>
        public TabFileItem(ShellViewModel viewModel, string fileName, string title, TailService tailService)
        {
            _viewModel = viewModel;
            _fileName = fileName;
            _title = title;

            TailService = tailService;
        }

        public TailService TailService { get; set; }

        public bool IsDirty { get; set; }

        public string Title
        {
            get { return _title; }
            set
            {
                _title = value;
                NotifyOfPropertyChange(() => Title);
            }
        }

        public string FileName
        {
            get { return _fileName; }
            set
            {
                _fileName = value;
                NotifyOfPropertyChange(() => FileName);
                NotifyOfPropertyChange(() => FileExists);
            }
        }
        
        public bool FileExists
        {
            get { return File.Exists(FileName); }
        }

        public int ChangeCount
        {
            get { return _lastChangeCount; }
            set
            {
                _lastChangeCount = value;
                NotifyOfPropertyChange(() => ChangeCount);
            }
        }
        
        public void Close()
        {
            _viewModel.Close(this);
        }

        public void CloseAll()
        {
            _viewModel.CloseAll();
        }

        public void CloseOthers()
        {
            _viewModel.CloseOthers(this);
        }

        public void CopyFilePath()
        {
            Clipboard.SetText(FileName);
        }

        public void OpenInExplorerFolder()
        {
            Process.Start(new ProcessStartInfo("explorer.exe", Path.GetDirectoryName(FileName)));
        }

        public void OpenInTextEditor()
        {
            var app = RegistryReader.GetRegisteredApplication(FileName, "txt");

            Process.Start(new ProcessStartInfo() { FileName = app, Arguments = FileName });
        }

        public void ApplyChange()
        {
            _changeCount++;
            IsDirty = true;
        }

        public void ResetChange()
        {
            ChangeCount = _changeCount;
            _changeCount = 0;
        }

        public int CurrentRow
        {
            get { return _currentRow; }
            set
            {
                if (_currentRow == value)
                    return;
                
                if (value < 0 || value >= _viewModel.Buffer.Count)
                    return;

                _currentRow = value;
                if (_viewModel.Buffer.Count > 0)
                    FollowTail = _currentRow == _viewModel.Buffer.Count - 1;

                NotifyOfPropertyChange(() => CurrentRow);
            }
        }

        public bool FollowTailShow 
        { 
            get { return (this == _viewModel.SelectedTab) && FollowTail; } 
        }
        
        public bool FollowTail
        {
            get { return _followTail; }
            set
            {
                if (_followTail == value)
                    return;
                
                _followTail = value;
                if (_followTail && _viewModel.Buffer.Count > 0 && IsDirty)
                {
                    CurrentRow = _viewModel.Buffer.Count - 1;
                    _viewModel.Reload();
                    IsDirty = false;
                }
                
                NotifyOfPropertyChange(() => FollowTail);
                NotifyOfPropertyChange(() => FollowTailShow);
            }
        }

        public override string ToString()
        {
            return FileName;
        }
    }
}
