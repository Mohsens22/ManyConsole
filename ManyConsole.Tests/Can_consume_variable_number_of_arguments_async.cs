using NUnit.Framework;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace ManyConsole.Tests
{
    public class Can_consume_variable_number_of_arguments_async
    {
        [Test]
        public async Task Expecting2CalledWith0()
        {
            var output = await run_command_with_parameters(new[] { "command1" });
            StringAssert.Contains("Invalid number of arguments-- expected 2 more", output,
                "the output does not have an errorstring asking for 2 more parameters");
        }
        [Test]
        public async Task Expecting2CalledWith1()
        {
            var output = await run_command_with_parameters(new[] { "command1", "1" });
            StringAssert.Contains("Invalid number of arguments-- expected 1 more", output,
                "the output does not have an errorstring asking for 1 more parameter");
        }
        [Test]
        public async Task Expecting2CalledWith2()
        {
            var output = await run_command_with_parameters(new[] { "command1", "1", "2" });
            StringAssert.AreEqualIgnoringCase("Executing command1:", output.Trim(),
                "unexpected output for valid command");
        }
        [Test]
        public async Task Expecting2CalledWith5()
        {
            var output = await run_command_with_parameters(new[] { "command1", "1", "2", "3", "4", "5" });
            StringAssert.AreEqualIgnoringCase("Executing command1:", output.Trim(),
                "unexpected output for valid command");
        }
        [Test]
        public async Task Expecting2To5CalledWith0()
        {
            var output = await run_command_with_parameters(new[] { "command2" });
            StringAssert.Contains("Invalid number of arguments-- expected 2 more", output,
                "the output does not have an errorstring asking for 2 more parameters");
        }
        [Test]
        public async Task Expecting2To5CalledWith1()
        {
            var output = await run_command_with_parameters(new[] { "command2", "1" });
            StringAssert.Contains("Invalid number of arguments-- expected 1 more", output,
                "the output does not have an errorstring asking for 1 more parameter");
        }
        [Test]
        public async Task Expecting2To5CalledWith2()
        {
            var output = await run_command_with_parameters(new[] { "command2", "1", "2" });
            StringAssert.AreEqualIgnoringCase("Executing command2:", output.Trim(),
                "unexpected output for valid command");
        }
        [Test]
        public async Task Expecting2To5CalledWith4()
        {
            var output = await run_command_with_parameters(new[] { "command2", "1", "2", "3", "4" });
            StringAssert.AreEqualIgnoringCase("Executing command2:", output.Trim(),
                "unexpected output for valid command");
        }
        [Test]
        public async Task Expecting2To5CalledWith5()
        {
            var output = await run_command_with_parameters(new[] { "command2", "1", "2", "3", "4", "5" });
            StringAssert.AreEqualIgnoringCase("Executing command2:", output.Trim(),
                "unexpected output for valid command");
        }
        [Test]
        public async Task Expecting2To5CalledWith6()
        {
            var output = await run_command_with_parameters(new[] { "command2", "1", "2", "3", "4", "5", "6" });
            StringAssert.Contains("Extra parameters specified: 6", output,
                "the output does not have an errorstring iundicating superfluous parameter(s)");
        }

        public class AsyncCommandWithAtLeast2Parameters : AsyncConsoleCommand
        {
            public AsyncCommandWithAtLeast2Parameters()
            {
                this.IsCommand("command1");
                HasAdditionalArgumentsBetween(2, null, "");
            }

            public override Task<int> RunAsync(string[] remainingArguments)
            {
                return Task.FromResult(0);
            }
        }

        public class AsyncCommandWith2To5Parameters : AsyncConsoleCommand
        {
            public AsyncCommandWith2To5Parameters()
            {
                this.IsCommand("command2");
                HasAdditionalArgumentsBetween(2, 5, "");
            }

            public override Task<int> RunAsync(string[] remainingArguments)
            {
                return Task.FromResult(0);
            }
        }

        // Async commands executed with async dispatcher
        private async Task<string> run_command_with_parameters(string[] parameters)
        {
            StringBuilder sb = new StringBuilder();
            var sw = new StringWriter(sb);

            await ConsoleCommandDispatcher.DispatchCommandAsync(
                new BaseConsoleCommand[]
                {
                    new AsyncCommandWithAtLeast2Parameters(),
                    new AsyncCommandWith2To5Parameters()
                },
                parameters,
                sw);

            return sb.ToString();
        }
    }
}
