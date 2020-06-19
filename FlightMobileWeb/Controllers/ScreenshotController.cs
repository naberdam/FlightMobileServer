using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using FlightServer.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace FlightServer.Controllers
{
    [Route("screenshot")]
    [ApiController]
    public class ScreenshotController : ControllerBase
    {
        private static ITCPClient tcpClient;
        private static DataOfServer dataOfServer;

        private const string ArgumentNullMessage = "The url is null";
        private const string HttpRequestMessage = "No connection could be made because" +
            " the target machine actively refused it";
        private const string TaskCanceledMessage = "The request failed due to timeout";
        private const string TimeoutMessage = "The operation has timed-out. " +
            "Please wait few seconds";
        private const string ServerNotConnected = "Server is not connected";
        private const string RegularException = "Problem in screenshot";
        private const string ImageIsNull = "The image we got is null";

        public ScreenshotController(ITCPClient client, IOptions<DataOfServer> options)
        {
            dataOfServer = options.Value;
            tcpClient = client;
            tcpClient.Connect(dataOfServer.Ip, dataOfServer.Port);
        }
        // GET: Screenshot
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            if (!tcpClient.IsConnect()) { return BadRequest(ServerNotConnected); }
            byte[] image;
            // Open connection with the givven externalUrlServer.
            using HttpClient httpClient = new HttpClient();
            httpClient.Timeout = new TimeSpan(0, 0, 10);
            try
            {
                image = await GetImageFromServer(httpClient);
            }
            catch (ArgumentNullException)
            {
                return NotFound(ScreenshotController.ArgumentNullMessage);
            }
            catch (HttpRequestException e2)
            {
                return MyNotFound(e2.Message, HttpRequestMessage);
            }
            catch (TaskCanceledException e3)
            {
                return MyNotFound(e3.Message, TaskCanceledMessage);
            }
            // This http is not connect.
            catch (Exception)
            {
                return NotFound(RegularException);
            }
            return ReturnImageWithoutExceptions(image);
        }

        private IActionResult ReturnImageWithoutExceptions(byte[] image)
        {
            if (image == null)
            {
                return NotFound(ImageIsNull);
            }
            return File(image, "image/jpg");
        }

        private async Task<byte[]> GetImageFromServer(HttpClient httpClient)
        {
            string requestScreenshot = dataOfServer.HttpAddress + "/screenshot";
            // Get the Json as string.
            HttpResponseMessage resultTest = await httpClient.GetAsync(
                requestScreenshot);
            byte[] image = await resultTest.Content.ReadAsByteArrayAsync();
            return image;
        }

        private string MsgExceptionInNotFound(string msgException, string msgNotTimeOut)
        {
            if (msgException.Contains("timeout"))
            {
                return TimeoutMessage;
            }
            return msgNotTimeOut;
        }

        private ActionResult MyNotFound(string msgException, string msgNotTimeOut)
        {
            string messageOfException = MsgExceptionInNotFound(msgException, 
                msgNotTimeOut);
            return NotFound(messageOfException);
        }

        
    }
}