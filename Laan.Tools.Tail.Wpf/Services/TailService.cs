using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;

namespace Laan.Tools.Tail
{
    public class TailService : IDisposable
    {
        private readonly FileSystemWatcher _watcher;
        private readonly string _fileName;
        private readonly IUserSettings _config;

        private List<string> _buffer;
        private int _lineWidth;
        private int _tailLength;

        /// <summary>
        /// Initializes a new instance of the TailService class.
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="tailLength"></param>
        public TailService(IUserSettings config, string fileName)
        {
            _config = config;
            _fileName = Path.GetFullPath(fileName);
            if (!File.Exists(fileName))
                return;

            ApplyConfiguration(config);

            _watcher = new FileSystemWatcher(Path.GetDirectoryName(_fileName));
            _watcher.EnableRaisingEvents = true;

            var changes = Observable
                .FromEventPattern<FileSystemEventHandler, FileSystemEventArgs>(
                    h => {
                        _watcher.Changed += h;
                        _watcher.Created += h;
                        _watcher.Deleted += h;
                    },
                    h => {
                        _watcher.Changed -= h;
                        _watcher.Created -= h;
                        _watcher.Deleted -= h;
                    }
                )
                .Where(e => e.EventArgs.FullPath == _fileName)
                .Sample(TimeSpan.FromMilliseconds(config.Tail.BufferDelay))
                .Subscribe(e =>
                {
                    if (Changed == null)
                        return;

                    Changed(this, new TailChangedEventArgs(_fileName));
                });
        }

        public void ApplyConfiguration(IUserSettings config)
        {
            _tailLength = config.Tail.Length;
            _lineWidth = config.Tail.Width;
        }
        
        private async Task<bool> TryRead()
        {
            var errors = 3;
            do
            {
            	try
                {
                    await ReadFile();
                    return true;
                }
                catch (IOException ex)
                {
                    Console.WriteLine(ex.ToString());
                    errors--;
                    _buffer = new List<string>();
                }
            } 
            while (errors > 0);

            return false;
        }
        
        private async Task ReadFile()
        {
            using (FileStream fs = new FileStream(_fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                int size = _tailLength * (_lineWidth + 1) * 2;

                // go to head of the tail
                fs.Seek(-1 * Math.Min(fs.Length, size), SeekOrigin.End);

                var lines = new List<string>(_config.Tail.Length * 2);

                using (var reader = new StreamReader(fs))
                {
                    while (!reader.EndOfStream)
                        lines.Add(await reader.ReadLineAsync());
                }

                _buffer = lines
                    .Skip(1)
                    .Take(_tailLength)
                    .ToList();
            }
        }

        public event TailChangedEventHandler Changed = delegate { };

        public async Task<IList<string>> Buffer()
        {
            await TryRead();
            return _buffer;
        }

        public void Dispose()
        {
            if (_watcher == null)
                return;

            _watcher.EnableRaisingEvents = false;
            _watcher.Dispose();
        }
    }
}
