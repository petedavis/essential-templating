using System;
using System.Diagnostics.Contracts;
using System.IO;

namespace Essential.Templating.Razor.Host.Storage
{
    public class TextSource : IDisposable
    {
        private readonly string _id;

        private readonly string _fileName;

        private readonly TextReader _reader;

        private bool _disposed;

        public TextSource(TextReader reader, string id = null, string fileName = null)
        {
            Contract.Requires<ArgumentNullException>(reader != null);
            Contract.Requires<ArgumentException>(id != string.Empty, "Empty id is not allowed.");

            _reader = reader;
            _id = id ?? (string.IsNullOrEmpty(fileName) ? Guid.NewGuid().ToString() : fileName);
            _fileName = fileName;
        }

        public string Id
        {
            get { return _id; }
        }

        public string FileName
        {
            get { return _fileName; }
        }

        public TextReader Reader
        {
            get { return _reader; }
        }

        public override string ToString()
        {
            return _id;
        }

        public void Dispose()
        {
            Dispose(true);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }
            if (disposing && _reader != null)
            {
                _reader.Dispose();
            }
            _disposed = true;
        }
    }
}
