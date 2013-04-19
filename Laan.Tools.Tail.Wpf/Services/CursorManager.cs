using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace Laan.Tools.Tail.Win
{
    public class CursorManager : IDisposable
    {
        private readonly Cursor _stored;
        public CursorManager(Cursor cursor)
        {
            _stored = Application.Current.MainWindow.Cursor;
            Application.Current.MainWindow.Cursor = cursor;
        }

        public CursorManager()
        {
            _stored = Application.Current.MainWindow.Cursor;
            Application.Current.MainWindow.Cursor = Cursors.Wait;
        }

        #region IDisposable Members

        public void Dispose()
        {
            Application.Current.MainWindow.Cursor = _stored;
        }

        #endregion
    }
}
