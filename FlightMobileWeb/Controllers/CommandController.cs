using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using FlightMobileAppServer.Models;
using FlightServer.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FlightServer.Controllers
{
    [Route("api/command")]
    [ApiController]
    public class CommandController : ControllerBase
    {
        private MySimulatorModel flightGear;
        // Const string that represents when url is not correct and send it to client.
        private const string NoSuchUrl = "There is no such url";
        public CommandController(MySimulatorModel flightGear1)
        {
            flightGear = flightGear1;
        }

        // POST: api/command
        [HttpPost]
        public async Task<ActionResult<string>> Post([FromBody]Command value)
        {
            // If the request url is not correct, send appropirate message.
            if (!CheckUrlRequest()) { return BadRequest(NoSuchUrl); }
            // The url is correct, so we can execute the program with the given value.
            string myResult = await flightGear.Execute(value);
            // Check if everything is good.
            if (myResult == MySimulatorModel.EverythingIsGood)
            {
                return Ok(MySimulatorModel.EverythingIsGood);
            }
            // Something went wrong, then send the message in myResult.
            return NotFound(myResult);
        }
        // Function that checks if the request url is correct. 
        private bool CheckUrlRequest()
        {
            string urlRequest = Request.Path;
            // The pattern we asked for.
            string pattern = @"^/api/command$";
            // Check if the givven url is correct.
            if (!Regex.IsMatch(urlRequest, pattern))
            {
                return false;
            }
            return true;
        }
    }
}