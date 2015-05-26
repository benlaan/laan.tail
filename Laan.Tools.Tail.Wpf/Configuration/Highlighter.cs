using System;
using System.Collections.Generic;
using System.Windows.Media;
using System.Text.RegularExpressions;
using System.Xml.Serialization;
using System.Xml;

namespace Laan.Tools.Tail
{
    [Serializable]
    public class Highlighter
    {
        private string _expression;

        [XmlIgnore]
        public string Expression
        {
            get
            {
                return _expression;
            }
            set
            {
                if (_expression == value)
                    return;
                _expression = value;
                Compile();
            }
        }
        
        [XmlIgnore]
        public Regex Regex { get; set; }

        [XmlElement("Expression")]
        public XmlNode RegExExpression
        {
            get { return new XmlDocument().CreateCDataSection(Expression); }
            set { Expression = value != null ? value.Value : null; }
        }

        [XmlIgnore]
        public Color Background { get; set; }

        public string BackgroundColor
        {
            get { return Background.ToString(); }
            set { Background = (Color)ColorConverter.ConvertFromString(value); }
        }
        
        [XmlIgnore]
        public Color Foreground { get; set; }

        public string ForegroundColor
        {
            get { return Foreground.ToString(); }
            set { Foreground = (Color)ColorConverter.ConvertFromString(value); }
        }

        public bool CaseSensitive { get; set; }

        public void Compile()
        {
            var options = RegexOptions.Compiled;
            if (!CaseSensitive)
                options |= RegexOptions.IgnoreCase;

            Regex = new Regex(Expression, options);
        }

    }
}
