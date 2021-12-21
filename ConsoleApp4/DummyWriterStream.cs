using System;
using System.IO;
using System.Threading.Tasks;

namespace ConsoleApp4
{
    public class DummyWriterStream : Stream
    {
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _file?.Dispose();
            }

            base.Dispose(disposing);
        }

        private string _name;
        private long _written = 0;
        private string _fileName;
        private Stream _file;

        public DummyWriterStream(string name, string fileName)
        {
            _name = name;
            _fileName = fileName;
            if(_fileName!= null)
                _file = File.OpenWrite(fileName);
        }
        
        public override void Flush()
        {
            if(_fileName!=null) _file.Flush();
            Console.WriteLine($"[{_name}] Flush.");
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            throw new System.NotImplementedException();
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new System.NotImplementedException();
        }

        public override void SetLength(long value)
        {
            throw new System.NotImplementedException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            Console.WriteLine($"[{_name}] Write {count} bytes.");
            if (_fileName != null)
            {
                _file.Write(buffer, offset, count);
            }
            _written += count;
        }

        public override bool CanRead => false;
        public override bool CanSeek => false;
        public override bool CanWrite => true;
        public override long Length => _written;
        public override long Position
        {
            get => _written;
            set { }
        }

        public override async ValueTask DisposeAsync()
        {
            if (_fileName != null)
            {
                await _file.DisposeAsync();
            }
            await base.DisposeAsync();
        }

    }
}