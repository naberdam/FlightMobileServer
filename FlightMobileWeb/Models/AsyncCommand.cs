using FlightMobileAppServer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FlightServer.Models
{
    public enum Result { Ok, WriteObjectDisposedException, 
        WriteInvalidOperationException, WriteIOException, 
        ReadObjectDisposedException, ReadInvalidOperationException,
        ReadTimeoutException, ReadIOException, RegularException
    }
    public class AsyncCommand
    {
        public Command Command { get; private set; }
        public Task<string> Task { get => Completion.Task; }
        public TaskCompletionSource<string> Completion { get; private set; }
        public AsyncCommand(Command input)
        {
            Command = input;
            // Watch out! Run Continuations Async is important!
            Completion = new TaskCompletionSource<string>(
            TaskCreationOptions.RunContinuationsAsynchronously);
        }
    }
}
