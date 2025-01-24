using NUnit.Framework;
using System.Linq;
using System.Threading.Tasks;

namespace ManyConsole.Tests
{
    public class abstract_commands_arent_loaded_async
    {
        public abstract class AsyncAbstractCommand : AsyncConsoleCommand
        {
        }

        public abstract class AnotherAsyncAbstractCommand : AsyncAbstractCommand
        {
            public AnotherAsyncAbstractCommand() {}
        }

        public class NonabstractAsyncCommand : AnotherAsyncAbstractCommand
        {
            public NonabstractAsyncCommand()
            {
                this.IsCommand("NonabstractCommand");
            }

            public override Task<int> RunAsync(string[] remainingArguments)
            {
                return Task.FromResult(0);
            }
        }

        [Test]
        public void AbstractCommandArentLoaded()
        {
            var commands = ConsoleCommandDispatcher.FindCommandsInSameAssemblyAs(this.GetType());

            Assert.IsTrue(commands.Any(c => c.GetType() == typeof(NonabstractAsyncCommand)), "No non-abstract commands are found.");
            Assert.IsFalse(commands.Any(c => c.GetType() == typeof(AsyncAbstractCommand)), "AbstractCommands present - should be ignored.");
            Assert.IsFalse(commands.Any(c => c.GetType() == typeof(AnotherAsyncAbstractCommand)), "AnotherAbstractCommand present - should be ignored.");
        }

    }
}
