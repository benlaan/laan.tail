
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Laan.Tools.Tail.Win;
using System.Diagnostics;
using System.Timers;

namespace Laan.Tools.Tail
{
    public class TailService : IDisposable
    {
        private readonly FileSystemWatcher _watcher;
        private readonly string _fileName;

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
            _fileName = Path.GetFullPath(fileName);
            if (!File.Exists(fileName))
                return;

            ApplyConfig(config);

            _watcher = new FileSystemWatcher(Path.GetDirectoryName(_fileName));
            _watcher.EnableRaisingEvents = true;
            _watcher.Changed += WatcherChanged;
            _watcher.Created += WatcherChanged;
            _watcher.Deleted += WatcherChanged;
        }

        private void WatcherChanged(object sender, FileSystemEventArgs e)
        {
            if (e.FullPath != _fileName)
                return;

            Changed(this, new TailChangedEventArgs(_fileName));
        }

        public void ApplyConfig(IUserSettings config)
        {
            _tailLength = config.Tail.Length;
            _lineWidth = config.Tail.Width;
        }
        
        private bool TryRead()
        {
            var errors = 3;
            do
            {
            	try
                {
                    ReadFile();
                    return true;
                }
                catch (IOException ex)
                {
                    Console.WriteLine(ex.ToString());
                    errors--;
                }
            } 
            while (errors > 0);
            return false;
        }

        private int CheckLineEnding(byte character, BinaryReader reader, int end)
        {
            int pair = 0;
            switch (character)
            {
                case 10:
                    pair = 13;
                    break;
                case 13:
                    pair = 10;
                    break;
                default:
                    return end;
            }

            if (reader.PeekChar() == pair)
            {
                reader.ReadByte();
                return end - 1;
            }
            return end - 1;
        }
        
        private List<string> GetBufferStream(FileStream fileStream)
        {
            int index = 0;
            var lines = new List<string>(2 * _tailLength);

            using (var sr = new BinaryReader(fileStream))
            {
                while (fileStream.Position < fileStream.Length)
                {
                    byte b;
                    index = 0;
                    List<byte> buffer = new List<byte>(512);
                    do
                    {
                        b = sr.ReadByte();
                        buffer.Add(b);
                        index++;
                    }
                    while (b != 13 && b != 10 && fileStream.Position < fileStream.Length);
					int end = CheckLineEnding(b, sr, index);

                    string line = UTF8Encoding.UTF8.GetString(buffer.ToArray(), 0, end);
                    lines.Add(line);
                }
            }
            return lines.Tail(_tailLength).ToList();
        }


        private void ReadFile()
        {
            using (FileStream fs = new FileStream(_fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                int size = _tailLength * (_lineWidth + 1) * 2;

                // go to head of the tail
                fs.Seek(-1 * Math.Min(fs.Length, size), SeekOrigin.End);
                _buffer = GetBufferStream(fs);
            }
        }

        public event TailChangedEventHandler Changed = delegate { };

        public IList<string> Buffer 
        {
            get { return TryRead() ? _buffer : new List<string>(); } 
        }
        
        #region IDisposable Members

        public void Dispose()
        {
            if (_watcher == null)
                return;
            _watcher.EnableRaisingEvents = false;
            _watcher.Changed -= WatcherChanged;
            _watcher.Created -= WatcherChanged;
            _watcher.Deleted -= WatcherChanged;
            _watcher.Dispose();
        }

        #endregion
    }
}
