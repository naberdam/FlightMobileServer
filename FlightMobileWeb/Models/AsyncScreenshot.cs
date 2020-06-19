using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FlightServer.Models
{
    public class AsyncScreenshot
    {
        public byte[] Command { get; private set; }
        public Task<byte[]> Task { get => Completion.Task; }
        public TaskCompletionSource<byte[]> Completion { get; private set; }
        public AsyncScreenshot()
        {
            // Watch out! Run Continuations Async is important!
            Completion = new TaskCompletionSource<byte[]>(
            TaskCreationOptions.RunContinuationsAsynchronously);
        }
    }
}
