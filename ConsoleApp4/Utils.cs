using System;
using System.IO;
using System.Linq;

namespace ConsoleApp4
{
    
    public static class Utils
    {
        class DummyStream : Stream
        {
            private long _length;
            private long _position = 0;

            static private string Lorem =
                "Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore " +
                "et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut " +
                "aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum " +
                "dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui " +
                "officia deserunt mollit anim id est laborum.";

            private int _loremPosition = 0;
            private byte[] _loremBuffer;

            public DummyStream(long size)
            {
                _length = size;
                _loremBuffer = Lorem.ToCharArray().Select(c => (byte)c).ToArray();
            }
            
            public override void Flush()
            {
                
            }

            public override int Read(byte[] buffer, int offset, int count)
            {
                if (_position >= _length) return 0;
                if (buffer.Length < offset + count) throw new IndexOutOfRangeException();
                var loremSize = _loremBuffer.Length;
                var localPos = offset;
                var toDo = count;
                while (toDo != 0 && _position < _length)
                {
                    var maxLorem = loremSize - _loremPosition;
                    if (maxLorem == 0)
                    {
                        maxLorem = loremSize;
                        _loremPosition = 0;
                    }
                    var maxBuffer = count - (localPos - offset);
                    var max = Math.Min(maxBuffer, maxLorem);
                    var maxTotal = _length - _position;
                    max = Math.Min(max, maxTotal > int.MaxValue ? int.MaxValue : (int)maxTotal);
                    Buffer.BlockCopy(_loremBuffer, _loremPosition, buffer, localPos, max);
                    localPos += max;
                    _loremPosition += max;
                    _position += max;
                    toDo -= max;
                }

                return localPos - offset;
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
                throw new System.NotImplementedException();
            }

            public override bool CanRead => true;
            public override bool CanSeek => false;
            public override bool CanWrite => false;
            public override long Length => _length;
            public override long Position
            {
                get => _position;
                set {} 
            }
        }


        public static Stream GetTextStream(long size)
        {
            return new DummyStream(size);
        }
    }
}