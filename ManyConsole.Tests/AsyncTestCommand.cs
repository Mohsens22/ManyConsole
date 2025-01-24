using System;
using System.Threading.Tasks;

namespace ManyConsole.Tests
{
    public class AsyncTestCommand : AsyncConsoleCommand
    {
        public Func<int> Action = delegate { return 0; };

        public override Task<int> RunAsync(string[] remainingArguments)
        {
            var result = Action();
            return Task.FromResult(result);
        }
    }
}
