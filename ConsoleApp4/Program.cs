using System;
using System.IO;
using System.IO.Compression;
using System.Text;

namespace ConsoleApp4
{
    class Program
    {
        static void Def()
        {
            var d = new DeflateStream(new DummyWriterStream("Deflate", null), CompressionMode.Compress);
            var t = Utils.GetTextStream(2048);
            t.CopyTo(d);
            d.Flush();
            
        }
        
        static void Main(string[] args)
        {
          
            //using var sink = new DummyWriterStream("Archive", "D:\\archive.zip");
            var fname = $"{Guid.NewGuid().ToString()}.zip";
            using var sink = new AwsWriterStream("test", fname);
            
            using (var zf = new ZipArchive(sink, ZipArchiveMode.Create))
            {

                for (int i = 1; i <= 100; i++)
                {
                    var entry = zf.CreateEntry($"Test{i}.txt", CompressionLevel.NoCompression);
                    using var stream = entry.Open();
                    Utils.GetTextStream(1024*1024+777).CopyTo(stream);
                }

            }
            Console.WriteLine("End.");
            
        }
    }
}