using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
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

        // Const string that represents all exceptions that we want to send to client.
        private const string ArgumentNullMessage = "The url is null";
        private const string HttpRequestMessage = "No connection could be made because" +
            " the target machine actively refused it";
        private const string TaskCanceledMessage = "The request failed due to timeout";
        private const string TimeoutMessage = "The operation has timed-out. " +
            "Please wait few seconds";
        private const string ServerNotConnected = "Server is not connected";
        private const string RegularException = "Problem in screenshot";
        private const string ImageIsNull = "The image we got is null";
        private const string NoSuchUrl = "There is no such url";

        // We get in the ctor the dataOfServer from appsettings.
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
            // If the request url is not correct, send appropirate message.
            if (!CheckUrlRequest()) { return BadRequest(NoSuchUrl); }
            // The url is correct but we need to check if the server is connected.
            if (!tcpClient.IsConnect()) { return BadRequest(ServerNotConnected); }
            // The server is connected, so open connection with the givven externalUrlServer.
            using HttpClient httpClient = new HttpClient();
            httpClient.Timeout = new TimeSpan(0, 0, 10);
            return await GetImage(httpClient);
        }
        // Function that checks if the request url is correct. 
        private bool CheckUrlRequest()
        {
            string urlRequest = Request.Path;
            // The pattern we asked for.
            string pattern = @"^/screenshot$";
            // Check if the givven url is correct.
            if (!Regex.IsMatch(urlRequest, pattern))
            {
                return false;
            }
            return true;
        }
        // Get image using the given httpClient.
        private async Task<IActionResult> GetImage(HttpClient httpClient)
        {
            byte[] image;
            try
            {
                image = await GetImageFromServer(httpClient);
            }
            catch (ArgumentNullException)
            {
                return NotFound(ArgumentNullMessage);
            }
            catch (HttpRequestException e2)
            {
                // Check if it is TimeOutException or not.
                return MyNotFound(e2.Message, HttpRequestMessage);
            }
            catch (TaskCanceledException e3)
            {
                // Check if it is TimeOutException or not.
                return MyNotFound(e3.Message, TaskCanceledMessage);
            }
            // Something else went wrong.
            catch (Exception)
            {
                return NotFound(RegularException);
            }
            // Everything is good, but we need to check if the image is null or not.
            return ReturnImageWithoutExceptions(image);
        }
        // Function that checks if the given image is null or not, and according to 
        // This check we return appropriate IActionResult.
        private IActionResult ReturnImageWithoutExceptions(byte[] image)
        {
            if (image == null)
            {
                return NotFound(ImageIsNull);
            }
            return File(image, "image/jpg");
        }
        // Function that gets the image from server.
        private async Task<byte[]> GetImageFromServer(HttpClient httpClient)
        {
            string requestScreenshot = dataOfServer.HttpAddress + "/screenshot";
            // Get the Json as string.
            HttpResponseMessage resultTest = await httpClient.GetAsync(
                requestScreenshot);
            byte[] image = await resultTest.Content.ReadAsByteArrayAsync();
            return image;
        }
        // Function that checks if it is TimeOutException or not.
        private string MsgExceptionInNotFound(string msgException, string msgNotTimeOut)
        {
            if (msgException.Contains("timeout"))
            {
                return TimeoutMessage;
            }
            return msgNotTimeOut;
        }
        // Function that defines my NotFound. We use this function when we think that
        // Maybe we have timeout.
        private ActionResult MyNotFound(string msgException, string msgNotTimeOut)
        {
            string messageOfException = MsgExceptionInNotFound(msgException, 
                msgNotTimeOut);
            return NotFound(messageOfException);
        }

        
    }
}