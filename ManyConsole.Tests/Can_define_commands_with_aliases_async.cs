using NUnit.Framework;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace ManyConsole.Tests
{
    public class Can_define_commands_with_aliases_async
    {
        [Test]
        public void CommandWithTwoAliasesDirect()
        {
            var output = run_command_with_parameters(new[] { "command" });
            StringAssert.AreEqualIgnoringCase("Executing command:", output.Trim(),
                "unexpected output for valid command");
        }
        [Test]
        public void CommandWithTwoAliasesFirst()
        {
            var output = run_command_with_parameters(new[] { "--command" });
            StringAssert.AreEqualIgnoringCase("Executing command:", output.Trim(),
                "unexpected output for valid command");
        }
        [Test]
        public void CommandWithTwoAliasesSecond()
        {
            var output = run_command_with_parameters(new[] { "-c" });
            StringAssert.AreEqualIgnoringCase("Executing command:", output.Trim(),
                "unexpected output for valid command");
        }

        public class AsyncCommandWith2Aliases: AsyncConsoleCommand
        {
            public AsyncCommandWith2Aliases()
            {
                this.IsCommand("command");
                this.HasAlias("--command");
                this.HasAlias("-c");
            }

            public override Task<int> RunAsync(string[] remainingArguments)
            {
                return Task.FromResult(0);
            }
        }
        
        // Async command executed via a normal dispatcher
        private string run_command_with_parameters(string[] parameters)
        {
            StringBuilder sb = new StringBuilder();
            var sw = new StringWriter(sb);

            ConsoleCommandDispatcher.DispatchCommand(
                new BaseConsoleCommand[]
                {
                    new AsyncCommandWith2Aliases()
                },
                parameters,
                sw);

            return sb.ToString();
        }
    }
}
