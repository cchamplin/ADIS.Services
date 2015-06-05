using ADIS.Core.ComponentServices.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ADIS.Services
{
    public class PostedFile : IPostedFile
    {
        string name;
        string content_type;
        Stream stream;

        class ReadSubStream : Stream
        {
            Stream s;
            long offset;
            long end;
            long position;

            public ReadSubStream(Stream s, long offset, long length)
            {
                this.s = s;
                this.offset = offset;
                this.end = offset + length;
                position = offset;
            }

            public override void Flush()
            {
            }

            public override int Read(byte[] buffer, int dest_offset, int count)
            {
                if (buffer == null)
                    throw new ArgumentNullException("buffer");

                if (dest_offset < 0)
                    throw new ArgumentOutOfRangeException("dest_offset", "< 0");

                if (count < 0)
                    throw new ArgumentOutOfRangeException("count", "< 0");

                int len = buffer.Length;
                if (dest_offset > len)
                    throw new ArgumentException("destination offset is beyond array size");
                // reordered to avoid possible integer overflow
                if (dest_offset > len - count)
                    throw new ArgumentException("Reading would overrun buffer");

                if (count > end - position)
                    count = (int)(end - position);

                if (count <= 0)
                    return 0;

                s.Position = position;
                int result = s.Read(buffer, dest_offset, count);
                if (result > 0)
                    position += result;
                else
                    position = end;

                return result;
            }

            public override int ReadByte()
            {
                if (position >= end)
                    return -1;

                s.Position = position;
                int result = s.ReadByte();
                if (result < 0)
                    position = end;
                else
                    position++;

                return result;
            }

            public override long Seek(long d, SeekOrigin origin)
            {
                long real;
                switch (origin)
                {
                    case SeekOrigin.Begin:
                        real = offset + d;
                        break;
                    case SeekOrigin.End:
                        real = end + d;
                        break;
                    case SeekOrigin.Current:
                        real = position + d;
                        break;
                    default:
                        throw new ArgumentException();
                }

                long virt = real - offset;
                if (virt < 0 || virt > Length)
                    throw new ArgumentException();

                position = s.Seek(real, SeekOrigin.Begin);
                return position;
            }

            public override void SetLength(long value)
            {
                throw new NotSupportedException();
            }

            public override void Write(byte[] buffer, int offset, int count)
            {
                throw new NotSupportedException();
            }

            public override bool CanRead
            {
                get { return true; }
            }
            public override bool CanSeek
            {
                get { return true; }
            }
            public override bool CanWrite
            {
                get { return false; }
            }

            public override long Length
            {
                get { return end - offset; }
            }

            public override long Position
            {
                get
                {
                    return position - offset;
                }
                set
                {
                    if (value > Length)
                        throw new ArgumentOutOfRangeException();

                    position = Seek(value, SeekOrigin.Begin);
                }
            }
        }

        internal PostedFile(string name, string content_type, Stream base_stream, long offset, long length)
        {
            this.name = name;
            this.content_type = content_type;
            this.stream = new ReadSubStream(base_stream, offset, length);
        }

        public string ContentType
        {
            get
            {
                return (content_type);
            }
        }

        public int ContentLength
        {
            get
            {
                return (int)stream.Length;
            }
        }

        public string FileName
        {
            get
            {
                return (name);
            }
        }

        public Stream InputStream
        {
            get
            {
                return (stream);
            }
        }

        public void SaveAs(string filename)
        {
            byte[] buffer = new byte[16 * 1024];
            long old_post = stream.Position;

            try
            {
                File.Delete(filename);
                using (FileStream fs = File.Create(filename))
                {
                    stream.Position = 0;
                    int n;

                    while ((n = stream.Read(buffer, 0, 16 * 1024)) != 0)
                    {
                        fs.Write(buffer, 0, n);
                    }
                }
            }
            finally
            {
                stream.Position = old_post;
            }
        }

    }
}
