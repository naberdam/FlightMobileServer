using FlightMobileAppServer.Models;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;

namespace FlightServer.Models
{
    public class MySimulatorModel
    {
        ITCPClient client;
        private readonly BlockingCollection<AsyncCommand> _queueCommand;
        private bool readSucceed;
        // Const string that represents all exceptions that we want to send to client.
        public const string WriteObjectDisposedException = "The server has been " +
            "closed. Please check your connection";
        public const string WriteInvalidOperationException = "The server is not " +
            "connected to a remote host. Please check your connection";
        public const string WriteIOException = "An error occurred when accessing" +
            " the socket. Please check your connection";
        public const string ReadObjectDisposedException = "The NetworkStream is " +
            "closed. Please check your connection";
        public const string ReadInvalidOperationException = "The NetworkStream does" +
            " not support reading. Please check your connection";
        public const string ReadTimeoutException = "The operation has timed-out. " +
            "Please wait few seconds";
        public const string ReadIOException = "An error occurred when accessing the" +
            " socket. Please check your connection";
        public const string RegularException = "Something got wrong. Please check " +
            "your connection";
        public const string EverythingIsGood = "Ok";

        // Const string for locations of our Command's properties
        public const string ThrottleLocation = "/controls/engines/current-engine/" +
            "throttle";
        public const string ElevatorLocation = "/controls/flight/elevator";
        public const string AileronLocation = "/controls/flight/aileron";
        public const string RudderLocation = "/controls/flight/rudder";

        public MySimulatorModel(ITCPClient tcpClient)
        {
            _queueCommand = new BlockingCollection<AsyncCommand>();
            this.client = tcpClient;
            Start();
        }

        // Called by the WebApi Controller, it will await on the returned Task<>
        // This is not an async method, since it does not await anything.
        public Task<string> Execute(Command cmd)
        {
            var asyncCommand = new AsyncCommand(cmd);
            _queueCommand.Add(asyncCommand);
            return asyncCommand.Task;
        }

        // ShouldStop the thread and log out.
        public void Disconnect()
        {
            this.client.Disconnect();
        }
        // Function that starts the Task.
        public void Start()
        {
            Task.Factory.StartNew(ProcessCommands);
        }
        // Function that call to read function of tcpClient and catches all 
        // The exceptions that can happen.
        private string ReadFromServer()
        {
            try
            {
                string strFromServer = client.Read();
                readSucceed = true;
                return strFromServer;
            }
            catch (ObjectDisposedException)
            {
                return HandleOtherExceptions(ReadObjectDisposedException);
            }
            catch (InvalidOperationException)
            {
                return HandleOtherExceptions(ReadInvalidOperationException);
            }
            catch (TimeoutException)
            {
                return HandleOtherExceptions(ReadTimeoutException);
            }
            catch (IOException e)
            {
                return HandleIOException(e);
            }
            catch (Exception)
            {
                return HandleOtherExceptions(RegularException);
            }
        }

        private string HandleOtherExceptions(string msgOfExcption)
        {
            readSucceed = false;
            return msgOfExcption;
        }
        // Function that handle in IOException when we read from server.
        private string HandleIOException(IOException e)
        {
            readSucceed = false;
            string msg;
            // Sometimes there is timeout but this exception belongs to IOException.
            if (e.Message.Contains("Unable to read data from the transport " +
                "connection: A connection attempt failed because the connected" +
                " party did not properly respond after a period of time, or" +
                " established connection failed because connected host has " +
                "failed to respond."))
            {
                msg = ReadTimeoutException;
            }
            else
            {
                // Regular IOException.
                msg = ReadIOException;
            }
            return msg;
        }

        // Function that call to write function of tcpClient and catches all 
        // The exceptions that can happen.
        private string WriteToServer(string variable)
        {
            try
            {
                client.Write(variable);
                return EverythingIsGood;
            }
            catch (ObjectDisposedException)
            {
                return WriteObjectDisposedException;
            }
            catch (InvalidOperationException)
            {
                return WriteInvalidOperationException;
            }
            catch (IOException)
            {
                return WriteIOException;
            }
            catch (Exception)
            {
                return RegularException;
            }
        }
        // Function that send information to server while the server is connected.
        public void ProcessCommands()
        {
            while (client.IsConnect())
            {
                foreach (AsyncCommand command in _queueCommand.GetConsumingEnumerable())
                {
                    OneIterationOfProcessCommands(command);
                }
            }            
        }
        // Function that does one iteration of the loop in ProcessCommands and set the 
        // Result if there is an exception or not.
        private void OneIterationOfProcessCommands(AsyncCommand command)
        {
            string aileronAction = OneActionOfWriteAndRead(AileronLocation,
                command.Command.Aileron);
            if (!CheckReturnOfAction(aileronAction, command)) { return; }
            string elevatorAction = OneActionOfWriteAndRead(ElevatorLocation,
                command.Command.Elevator);
            if (!CheckReturnOfAction(elevatorAction, command)) { return; }
            string rudderAction = OneActionOfWriteAndRead(RudderLocation,
                command.Command.Rudder);
            if (!CheckReturnOfAction(rudderAction, command)) { return; }
            string throttleAction = OneActionOfWriteAndRead(ThrottleLocation,
                command.Command.Throttle);
            if (!CheckReturnOfAction(throttleAction, command)) { return; }
            command.Completion.SetResult(EverythingIsGood);
        }
        // Function that checks if everything was good in writing and reading from server
        // And if something went wrong it returns false.
        private bool CheckReturnOfAction(string oneAction, AsyncCommand command)
        {
            if (oneAction != EverythingIsGood) 
            {
                command.Completion.SetResult(oneAction);
                return false; 
            }
            return true;
        }
        // Function thats write and read from server
        private string OneActionOfWriteAndRead(string locationOfVariable, 
            double valueOfVariable)
        {
            string messageToServerWithSet = RequestFromServer(true, locationOfVariable, 
                valueOfVariable);
            string statusOfWriteToServer = WriteToServer(messageToServerWithSet);
            if (statusOfWriteToServer != EverythingIsGood) 
            {
                return statusOfWriteToServer;
            }
            string messageToServerInGet = RequestFromServer(false, locationOfVariable, 
                valueOfVariable);
            client.Write(messageToServerInGet);
            string statusOfReadFromServer = ReadFromServer();
            if (!IsValidInput(statusOfReadFromServer, valueOfVariable)) 
            {
                return statusOfReadFromServer;
            }
            return EverythingIsGood;
        }
        // Function that checks if the responseFromRead is good.
        private bool IsValidInput(string responseFromRead, double valueFromJSON)
        {
            if (!readSucceed) { return false; }
            double numberOfRead;
            try
            {
                numberOfRead = Double.Parse(responseFromRead);
            }
            // Cant prase the responseFromRead then return false.
            catch (Exception)
            {
                return false;
            }
            if (valueFromJSON != numberOfRead)
            {
                return false;
            }
            return true;
        }
        // Function that returns string request of get/set according to the given isSet.
        private string RequestFromServer(bool isSet, string locationInServer, double val)
        {
            string messageToServer;
            if (isSet)
            {
                messageToServer = "set ";
                messageToServer += locationInServer + " " + val;
            }
            else
            {
                messageToServer = "get ";
                messageToServer += locationInServer;
            }
            messageToServer += "\r\n";
            return messageToServer;
        }
    }
}