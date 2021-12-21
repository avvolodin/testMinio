using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices.ComTypes;
using System.Threading.Tasks;
using Amazon;
using Amazon.S3;
using Amazon.S3.Model;

namespace ConsoleApp4
{
    public class AwsWriterStream : Stream
    {
        private string accessKey = "5TRF1SJ3IUTH8GTJPUFE";
        private string secretKey = "WKu19asqJznkqxGxaA24sx0CIEjrvVE5Vh4AlfCz";
        private string s3url = "http://localhost:9000";
        private AmazonS3Client _s3Client;
        private byte[] _buffer = new byte[6*1024*1024];
        private int _bufferPosition = 0;
        private int _awsPart = 0;
        private string _key;
        private string _bucketName;
        private string _uploadId = null;
        private List<UploadPartResponse> _responses = new List<UploadPartResponse>();
        private bool _disposed = false;

        public AwsWriterStream(string bucketName, string key)
        {
            _bucketName = bucketName;
            _key = key;
            AWSConfigsS3.UseSignatureVersion4 = true;
            var config = new AmazonS3Config
            {
                ServiceURL = s3url,
                ForcePathStyle = true
            };

            _s3Client = new AmazonS3Client(
                accessKey,
                secretKey,
                config
            );
        }
        
        protected override void Dispose(bool disposing)
        {

            if (disposing && !_disposed)
            {
                FlushBuffer();
                FinishUpload();
                _disposed = true;
            }

            base.Dispose(disposing);
        }

        private long _length = 0;
        
        
        public override void Flush()
        {
            //throw new System.NotImplementedException();
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
            
            while (count > 0)
            {
                var chunk = GetChunk(count);
                Buffer.BlockCopy(buffer, offset, _buffer, _bufferPosition, chunk);
                _bufferPosition += chunk;
                count -= chunk;
                if (_bufferPosition == _buffer.Length) FlushBuffer();
            }
        }

        private void FinishUpload()
        {
            var req = new CompleteMultipartUploadRequest
            {
                BucketName = _bucketName,
                Key = _key,
                UploadId = _uploadId
            };
            req.AddPartETags(_responses);
            var resp = _s3Client.CompleteMultipartUploadAsync(req).GetAwaiter().GetResult();


        }
        private async Task StartMultipart()
        {
            var req = new InitiateMultipartUploadRequest
            {
                Key = _key,
                BucketName = _bucketName
            };
            var resp = await _s3Client.InitiateMultipartUploadAsync(req);
            _uploadId = resp.UploadId;

        }

        private async Task UploadBuffer()
        {
            _awsPart++;
            if (_uploadId == null) await StartMultipart();
            var s = new MemoryStream(_buffer, 0, _bufferPosition, false);
            var uploadRequest = new UploadPartRequest
            {
                BucketName = _bucketName,
                Key = _key,
                UploadId = _uploadId,
                PartNumber = _awsPart,
                PartSize = _bufferPosition,
                InputStream = s
            };
            _responses.Add(await _s3Client.UploadPartAsync(uploadRequest));
        }

        private void FlushBuffer()
        {
            //sync -> switch
            UploadBuffer().GetAwaiter().GetResult();
            _bufferPosition = 0;
        }

        private int GetChunk(int count)
        {
            var bufRest = _buffer.Length - _bufferPosition;
            return Math.Min(bufRest, count);
        }

        public override bool CanRead => false;
        public override bool CanSeek => false;
        public override bool CanWrite => true;
        public override long Length => _length;
        public override long Position
        {
            get => _length;
            set { }
        }
    }
}