using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using Palmtrio.Domain.Interfaces;

namespace Palmtrio.Web.Framework.Utilities
{
    internal struct VideoStreamContentByteRange
    {
        public readonly long From;
        public readonly long To;

        public VideoStreamContentByteRange(long From, long To)
        {
            this.From = From;
            this.To = To;
        }
    }

    internal static class VideoStreamFactoryHelper
    {
        public static VideoStreamContentByteRange ExtractByteRange(long? From, long? To, long ContentExtent)
        {
            long fromCooked;
            long toCooked;

            if (From.HasValue && To.HasValue)
            {
                fromCooked = From.Value;
                toCooked = To.Value;
            }
            else
            {
                if (From.HasValue)
                {
                    fromCooked = From.Value;
                    toCooked = ContentExtent;
                }
                else
                {
                    if (To.HasValue)
                    {
                        fromCooked = ContentExtent - To.Value + 1;
                        toCooked = ContentExtent;
                    }
                    else
                    {
                        fromCooked = 0;
                        toCooked = ContentExtent;
                    }
                }
            }
            return new VideoStreamContentByteRange(fromCooked, toCooked);
        }

        public static VideoStreamContentByteRange[] ExtractByteRanges(RangeHeaderValue SourceRangeHeader, long ContentExtent)
        {
            List<VideoStreamContentByteRange> result = new List<VideoStreamContentByteRange>();

            foreach(var range in SourceRangeHeader.Ranges)
            {
                var byteRangeBeingCreated = ExtractByteRange(range.From, range.To, ContentExtent);

                if(range.From < 0 || range.From > ContentExtent || range.To > ContentExtent)
                {
                    ;//this is temporary 
                }
                else
                {
                    result.Add(byteRangeBeingCreated);
                }
            }

            return result.ToArray();
        }

        public static long GetByteRangeHeaderLength(VideoStreamContentByteRange[] ByteRanges, Guid ByteRangeBoundary, long SourceBytesLength, string MimeType)
        {
            if (ByteRanges.Length <= 1)
                return 0;

            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < ByteRanges.Length; i++)
            {
                if(i > 0)
                {
                    sb.AppendLine();
                }
                sb.AppendLine("--" + ByteRangeBoundary.ToString());
                sb.AppendLine("Content-Type: " + MimeType);
                sb.AppendLine("Content-Range: bytes " + ByteRanges[i].From.ToString() + "-" + (ByteRanges[i].To - 1).ToString() + "/" + SourceBytesLength);
                sb.AppendLine();
            }
            sb.AppendLine();
            sb.AppendLine("--" + ByteRangeBoundary.ToString() + "--");

            return (long)Encoding.ASCII.GetByteCount(sb.ToString());
        }
    }

    public class VideoStreamFactory : IApiResourceHandle
    {
        private int _bufferSize;
        private int _throttleRate;
        private Guid _byteRangeBoundary = Guid.NewGuid();

        private bool _abort = false;

        public VideoStreamFactory(int BufferSize, int ThrottleRate)
        {
            _bufferSize = BufferSize;
            _throttleRate = ThrottleRate;
        }

        public HttpContent CreateHttpContent(byte[] ContentBytes, string MimeType)
        {
            return CreateHttpContent(ContentBytes, MimeType, null, null);
        }

        public HttpContent CreateHttpContent(byte[] ContentBytes, string MimeType, long? From, long? To)
        {
            long contentExtent = (long)ContentBytes.Length - 1;
            VideoStreamContentByteRange byteRange = VideoStreamFactoryHelper.ExtractByteRange(From, To, contentExtent);

            if (byteRange.From < 0 || byteRange.From > contentExtent || byteRange.To > contentExtent)
            {
                string errorMessage = "The requested range (bytes="
                    + ((From.HasValue) ? From.Value.ToString() : "")
                    + "-"
                    + ((To.HasValue) ? To.Value.ToString() : "")
                    + ") does not overlap with the current extent of the selected resource.";
                throw new InvalidByteRangeException(new ContentRangeHeaderValue(0, ContentBytes.Length), errorMessage);
            }

            VideoStreamContentByteRange[] singleByteRange = new VideoStreamContentByteRange[1];
            singleByteRange[0] = byteRange;

            var result = new VideoStreamContent(this, ContentBytes, MimeType, _bufferSize, _throttleRate, singleByteRange);
            result.Headers.ContentRange = new ContentRangeHeaderValue(byteRange.From, byteRange.To, ContentBytes.Length);
            result.Headers.ContentType = new MediaTypeHeaderValue(MimeType);

            return result;
        }

        public HttpContent CreateHttpContent(byte[] ContentBytes, string MimeType, RangeHeaderValue RequestRangeHeader)
        {
            long contentExtent = ContentBytes.Length - 1;
            var byteRanges = VideoStreamFactoryHelper.ExtractByteRanges(RequestRangeHeader, contentExtent);

            if (byteRanges.Length > 1)
            {
                var result = new VideoStreamContent(this, ContentBytes, MimeType, _bufferSize, _throttleRate, byteRanges);
                string mediaType = "multipart/byteranges";
                result.Headers.ContentType = new MediaTypeHeaderValue(mediaType);
                result.Headers.ContentType.Parameters.Add(new NameValueHeaderValue("boundary", _byteRangeBoundary.ToString()));

                return result;
            }
            else
            {
                var from = RequestRangeHeader.Ranges.First().From;
                var to = RequestRangeHeader.Ranges.First().To;

                return CreateHttpContent(ContentBytes, MimeType, from, to);
            }
        }

        public void Abort()
        {
            _abort = true;
        }

        private async Task ThrottledWrite(byte[] SourceBytes, string MimeType, Stream OutStream, VideoStreamContentByteRange[] ByteRanges, int BufferSize, int ThrottleRate)
        {
            await Task.Factory.StartNew(() =>
            {
                try
                {
                    for(int i = 0; i < ByteRanges.Length; i++)
                    {
                        VideoStreamContentByteRange currentRange = ByteRanges[i];

                        int indexFrom = (int)currentRange.From;
                        int countTo = (int)(currentRange.To - currentRange.From);

                        if (ByteRanges.Length > 1)
                        {
                            using (StreamWriter streamWriter = new StreamWriter(OutStream, System.Text.Encoding.ASCII, 4096, leaveOpen: true))
                            {
                                if(i > 0)
                                {
                                    streamWriter.WriteLine();
                                }
                                streamWriter.WriteLine("--" + _byteRangeBoundary.ToString());
                                streamWriter.WriteLine("Content-Type: " + MimeType);
                                streamWriter.WriteLine("Content-Range: bytes " + currentRange.From.ToString() + "-" + (currentRange.To - 1).ToString() + "/" + SourceBytes.Length);
                                streamWriter.WriteLine();
                                OutStream.Flush();
                            }
                        }

                        byte[] buffer = new byte[BufferSize];
                        int bytesRead = 1;
                        using (MemoryStream streamBytes = new MemoryStream(buffer: SourceBytes, index: indexFrom, count: countTo, writable: false))
                        {
                            while (bytesRead > 0)
                            {
                                bytesRead = streamBytes.Read(buffer, 0, BufferSize);
                                OutStream.Write(buffer, 0, bytesRead);
                                OutStream.Flush();

                                if (_abort)
                                {
                                    break;
                                }

                                Thread.Sleep(ThrottleRate);
                            }
                        }
                    }

                    if (ByteRanges.Length > 1)
                    {
                        using (StreamWriter streamWriter = new StreamWriter(OutStream, System.Text.Encoding.ASCII, 4096, leaveOpen: true))
                        {
                            streamWriter.WriteLine();
                            streamWriter.WriteLine("--" + _byteRangeBoundary.ToString() + "--");
                            OutStream.Flush();
                        }
                    }
                }
                catch (HttpException ex)
                {
                    var snoop = ex;
                }
                finally
                {
                    OutStream.Dispose();
                }
            }, TaskCreationOptions.LongRunning);
        }

        private class VideoStreamContent : HttpContent
        {
            private byte[] _source;
            private int _bufferSize;
            private int _throttleRate;
            private string _mimeType;

            private VideoStreamContentByteRange[] _byteRanges;
            private VideoStreamFactory _factory;

            public VideoStreamContent(VideoStreamFactory Factory, byte[] ContentBytes, string MimeType, int BufferSize, int ThrottleRate, VideoStreamContentByteRange[] ByteRanges)
            {
                _source = ContentBytes;
                _bufferSize = BufferSize;
                _throttleRate = ThrottleRate;
                _mimeType = MimeType;

                _byteRanges = new VideoStreamContentByteRange[ByteRanges.Length];
                for(int i = 0; i < ByteRanges.Length; i++)
                {
                    long from = ByteRanges[i].From;
                    long to = ByteRanges[i].To + 1;

                    _byteRanges[i] = new VideoStreamContentByteRange(from, to);
                }

                _factory = Factory;
            }

            protected override async Task SerializeToStreamAsync(Stream stream, System.Net.TransportContext context)
            {
                await _factory.ThrottledWrite(_source, _mimeType, stream, _byteRanges, _bufferSize, _throttleRate);
            }

            protected override bool TryComputeLength(out long length)
            {
                long totalLength = 0;
                for (int i = 0; i < _byteRanges.Length; i++)
                {
                    totalLength += _byteRanges[i].To - _byteRanges[i].From;
                }

                totalLength += VideoStreamFactoryHelper.GetByteRangeHeaderLength(_byteRanges, _factory._byteRangeBoundary, _source.Length, _mimeType);

                length = totalLength;

                return true;
            }

            protected override void Dispose(bool disposing)
            {
                if(_factory != null)
                {
                    _factory.Abort();
                }

                base.Dispose(disposing);
            }
        }

    }


}