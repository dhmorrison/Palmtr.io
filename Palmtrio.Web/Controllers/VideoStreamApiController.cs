using Palmtrio.Web.Framework.Utilities;
using Palmtrio.Domain.Interfaces;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace Palmtrio.Web.Controllers
{
    //http://studentguru.gr/b/eloy/archive/2015/07/13/video-streaming-for-mobile-clients-via-asp-net-web-api-2-tutorial/
    //https://blogs.msdn.microsoft.com/webdev/2012/11/23/asp-net-web-api-and-http-byte-range-support/
    //https://greenbytes.de/tech/webdav/draft-ietf-httpbis-p5-range-latest.html#range.requests
    //https://larryjordan.com/articles/basics-of-http-live-streaming/

    public class VideoStreamApiController : ApiController
    {
        private IApiResourceManager _apiResourceManager;
        public VideoStreamApiController(IApiResourceManager InjVideoStreamService)
        {
            _apiResourceManager = InjVideoStreamService;
        }

        private const string FOLDER_PATH = "E:\\EXTERNAL\\RESX\\";
        private const string MIME_MP4 = "video/mp4";
        private const string MIME_OGG = "video/ogg";
        private const string MIME_WEBM = "video/webm";

        public HttpResponseMessage GetVideoStream([FromUri] string filename, string extension, string speed, string vsid)
        {
            Guid videoStreamId;
            if(Guid.TryParse(vsid, out videoStreamId))
            {
                if (!_apiResourceManager.Exists(videoStreamId))
                    return new HttpResponseMessage(HttpStatusCode.NotFound);
            }
            else
            {
                return new HttpResponseMessage(HttpStatusCode.NotFound);
            }

            VideoStreamFactory factory;
            string mimeType = "";
            string videoFilePath = FOLDER_PATH + filename + "." + extension;
            switch (extension)
            {
                case "webm":
                    switch (speed)
                    {
                        case "slow":
                            factory = new VideoStreamFactory(8448, 60);
                            break;
                        case "fast":
                        default:
                            factory = new VideoStreamFactory(65536, 60);
                            break;
                    }
                    mimeType = MIME_WEBM;
                    break;
                case "ogg":
                    factory = new VideoStreamFactory(31360, 60);
                    mimeType = MIME_OGG;
                    break;
                case "mp4":
                default:
                    switch (speed)
                    {
                        case "slow":
                            factory = new VideoStreamFactory(7680, 60);
                            break;
                        case "fast":
                        default:
                            factory = new VideoStreamFactory(65536, 60);
                            break;
                    }
                    mimeType = MIME_MP4;
                    break;
            }

            byte[] videoBytes = File.ReadAllBytes(videoFilePath);
            HttpResponseMessage response;
            if(Request.Headers.Range != null && Request.Headers.Range.Ranges.Any())
            {
                try
                {
                    var from = Request.Headers.Range.Ranges.First().From;
                    var to = Request.Headers.Range.Ranges.First().To;
                    response = Request.CreateResponse(HttpStatusCode.PartialContent);
                    response.Content = factory.CreateHttpContent(videoBytes, mimeType, Request.Headers.Range);

                    _apiResourceManager.AssignApiResource(videoStreamId, (IApiResourceHandle)factory);
                }
                catch(InvalidByteRangeException ex)
                {
                    var errorResponse = Request.CreateErrorResponse(HttpStatusCode.RequestedRangeNotSatisfiable, ex.Message);
                    errorResponse.Headers.Add("Accept-Ranges", "bytes");
                    errorResponse.Content.Headers.ContentRange = ex.ContentRange;
                    return errorResponse;
                }
            }
            else
            {
                if (_apiResourceManager.HasPending(videoStreamId))
                {
                    response = Request.CreateResponse(HttpStatusCode.OK);
                    response.Content = factory.CreateHttpContent(videoBytes, mimeType);

                    _apiResourceManager.AssignApiResource(videoStreamId, (IApiResourceHandle)factory);
                }
                else
                {
                    response = Request.CreateResponse(HttpStatusCode.Forbidden);
                }
            }

            response.Headers.Add("Accept-Ranges", "bytes");
            return response;
        }


    }

}
