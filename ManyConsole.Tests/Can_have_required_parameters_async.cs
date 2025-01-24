using System.IO;
using System.Threading.Tasks;
using NUnit.Framework;

namespace ManyConsole.Tests
{
    public class Can_have_required_parameters_async
    {
        [Test]
        public async Task CommandRunWithoutParameters()
        {
            string result = null;
            var noopCommand = new AsyncTestCommand()
                .IsCommand("required", "This command has a required parameter")
                .HasOption("ignored=", "An extra option.", v => { })
                .HasRequiredOption("f|foo=", "This foo to use.", v => result = v)
                .SkipsCommandSummaryBeforeRunning();

            when_the_command_is_run_without_the_parameter_then_the_console_gives_error_output(noopCommand, "foo");
            //            when("that command is ran with the parameter", delegate ()
            StringWriter output = new StringWriter();

            var exitCode = await ConsoleCommandDispatcher.DispatchCommandAsync(noopCommand,
                new[] { "required", "-foo", "bar" }, output);

            Assert.AreEqual(0, exitCode,
                "the exit code does not indicates the call succeeded");

            Assert.AreEqual("bar", result,
                "the option is not received");
        }
        [Test]
        public async Task CommandRunWithIntegerParameters()
        {
            int result = 0;
            var requiresInteger = new AsyncTestCommand()
                .IsCommand("parse-int")
                .HasRequiredOption<int>("value=", "The integer value", v => result = v);

            when_the_command_is_run_without_the_parameter_then_the_console_gives_error_output(requiresInteger, "value");

            //when("the command is passed an integer value", () =>
            StringWriter output = new StringWriter();

            var exitCode = await ConsoleCommandDispatcher.DispatchCommandAsync(requiresInteger,
                new[] { "parse-int", "-value", "42" }, output);

            Assert.AreEqual(0, exitCode,
                "the exit code does not indicates the call succeeded");

            Assert.AreEqual(42, result,
                "the value is not received");
        }

        void when_the_command_is_run_without_the_parameter_then_the_console_gives_error_output(AsyncConsoleCommand command, string parameterName)
        {
            StringWriter output = new StringWriter();

            var exitCode = ConsoleCommandDispatcher.DispatchCommand(command,new[] {command.Command}, output);

            StringAssert.Contains("Missing option: " + parameterName, output.ToString(),
                "the output does not indicates the parameter wasn't specified");

            Assert.AreEqual(-1, exitCode,
                "the exit code does not indicate the call failed");
        }
    }
}
