using System;
using System.Collections.Generic;
using System.Linq;
using Caliburn.Micro;
using System.Globalization;
using System.Diagnostics;

namespace Laan.Tools.Tail
{
    public class DebugLogger : ILog
    {
        private readonly Type _type;

        public DebugLogger(Type type)
        {
            _type = type;
        }

        private string CreateLogMessage(string format, params object[] args)
        {
            return string.Format(
                CultureInfo.InvariantCulture, 
                "[{0}] {1}",
                DateTime.Now.ToString("o", CultureInfo.InvariantCulture),
                String.Format(CultureInfo.InvariantCulture, format, args)
            );
        }

        public void Error(Exception exception)
        {
            string message = CreateLogMessage(exception.ToString());
#if DEBUG
            Debug.WriteLine(message, "ERROR");
#else
                Console.WriteLine( message );
#endif
        }

        public void Info(string format, params object[] args)
        {
            string message = CreateLogMessage(format, args);
#if DEBUG
            Debug.WriteLine(message, "INFO");
#else
                Console.WriteLine( message );
#endif
        }

        public void Warn(string format, params object[] args)
        {
            string message = CreateLogMessage(format, args);

#if DEBUG
            Debug.WriteLine(message, "WARN");
#else
                Console.WriteLine( message );
#endif
        }
    }
}
