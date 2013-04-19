using System;
using System.Collections.Generic;
using System.Linq;
using Caliburn.Micro;
using System.Windows.Media;
using System.Text.RegularExpressions;
using System.Windows;

namespace Laan.Tools.Tail.Win
{
    public class BufferItem : PropertyChangedBase
    {
        private string _line;
        private bool _isSelected;

        private bool _hasHighlighter;
        private Brush _foregroundColor;
        private Brush _backgroundColor;
        private IUserSettings _config;
        
        public BufferItem(IUserSettings config)
        {
            ApplyConfig(config);
        }

        public string Line
        {
            get { return _line; }
            set
            {
                _line = value;
                ApplyConfig(_config);
                NotifyOfPropertyChange(() => Line);
            }
        }

        public TextWrapping TextWrapping
        {
            get { return _config.TextWrapping; }
        }

        public void ApplyConfig(IUserSettings config)
        {
            _config = config;
            NotifyOfPropertyChange(() => TextWrapping);
            
            if (_line == null)
                return;

            Highlighter highlighter = _config.Highlighters.FirstOrDefault(h => h.Regex.IsMatch(_line));
            HasHighlighter = highlighter != null;
            if (highlighter != null)
            {
                ForegroundColor = new SolidColorBrush(highlighter.Foreground);
                BackgroundColor = new SolidColorBrush(highlighter.Background);
            }
                
        }

        public bool IsSelected
        {
            get { return _isSelected; }
            set
            {
                _isSelected = value;
                NotifyOfPropertyChange(() => IsSelected);
            }
        }

        public Brush ForegroundColor
        {
            get { return _foregroundColor; }
            set
            {
                _foregroundColor = value;
                NotifyOfPropertyChange(() => ForegroundColor);
            }
        }

        public Brush BackgroundColor
        {
            get { return _backgroundColor; }
            set
            {
                _backgroundColor = value;
                NotifyOfPropertyChange(() => BackgroundColor);
            }
        }

        public bool HasHighlighter
        {
            get { return _hasHighlighter; }
            set
            {
                _hasHighlighter = value;
                NotifyOfPropertyChange(() => HasHighlighter);
            }
        }


    }
}
