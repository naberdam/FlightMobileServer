using System;
using System.Collections.Generic;
using System.Linq;
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
        public CommandController(MySimulatorModel flightGear1)
        {
            flightGear = flightGear1;
        }

        // POST: api/Command
        [HttpPost]
        public async Task<ActionResult<string>> Post([FromBody]Command value)
        {
            string myResult = await flightGear.Execute(value);
            if (myResult == MySimulatorModel.EverythingIsGood)
            {
                return Ok(MySimulatorModel.EverythingIsGood);
            }
            return NotFound(myResult/*AppropriateError(myResult)*/);
        }

        private string AppropriateError(Result result)
        {
            string exceptionMsg;
            switch (result)
            {
                case Result.WriteObjectDisposedException:
                    exceptionMsg = MySimulatorModel.WriteObjectDisposedException;
                    break;
                case Result.WriteInvalidOperationException:
                    exceptionMsg = MySimulatorModel.WriteInvalidOperationException;
                    break;
                case Result.WriteIOException:
                    exceptionMsg = MySimulatorModel.WriteIOException;
                    break;
                case Result.ReadObjectDisposedException:
                    exceptionMsg = MySimulatorModel.ReadObjectDisposedException;
                    break;
                case Result.ReadInvalidOperationException:
                    exceptionMsg = MySimulatorModel.ReadInvalidOperationException;
                    break;
                case Result.ReadTimeoutException:
                    exceptionMsg = MySimulatorModel.ReadTimeoutException;
                    break;
                case Result.ReadIOException:
                    exceptionMsg = MySimulatorModel.ReadIOException;
                    break;
                case Result.RegularException:
                    exceptionMsg = MySimulatorModel.RegularException;
                    break;
                default:
                    exceptionMsg = "";
                    break;
            }
            return exceptionMsg;
        }
    }
}