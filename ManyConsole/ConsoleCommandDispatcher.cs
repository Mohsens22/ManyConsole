using ManyConsole.Internal;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace ManyConsole
{
    public class ConsoleCommandDispatcher
    {
        public static int DispatchCommand(BaseConsoleCommand command, string[] arguments, TextWriter consoleOut)
        {
            return DispatchCommand(new[] { command }, arguments, consoleOut);
        }

        public static Task<int> DispatchCommandAsync(IEnumerable<BaseConsoleCommand> commands, string[] arguments, TextWriter consoleOut, bool skipExeInExpectedUsage = false)
        {
            var commandStatus = GetCommandOrStatus(commands, arguments, consoleOut, skipExeInExpectedUsage);
            if (commandStatus.Status.HasValue)
            {
                return Task.FromResult(commandStatus.Status.Value);
            }
            else if (commandStatus.Command != null)
            {
                if (typeof(IConsoleCommand<AsyncConsoleCommand>).IsAssignableFrom(commandStatus.Command.GetType()))
                {
                    // is an async command
                    return ((AsyncConsoleCommand)commandStatus.Command).RunAsync(commandStatus.RemainingArgs.ToArray());
                }
                else if (typeof(IConsoleCommand<ConsoleCommand>).IsAssignableFrom(commandStatus.Command.GetType()))
                {
                    // it's a normal command
                    return Task.FromResult(((ConsoleCommand)commandStatus.Command).Run(commandStatus.RemainingArgs.ToArray()));
                }
                else
                {
                    // an invalid/custom implementation
                    consoleOut.WriteLine("Invalid implementation. You need to use either ConsoleCommand or AsyncConsoleCommand.");
                    return Task.FromResult(-1);
                }
            }
            return Task.FromResult(-1);
        }

        public static int DispatchCommand(IEnumerable<BaseConsoleCommand> commands, string[] arguments, TextWriter consoleOut, bool skipExeInExpectedUsage = false)
        {
            var commandStatus = GetCommandOrStatus(commands, arguments, consoleOut, skipExeInExpectedUsage);
            if (commandStatus.Status.HasValue)
            {
                return commandStatus.Status.Value;
            }
            else if (commandStatus.Command != null)
            {
                if (typeof(IConsoleCommand<AsyncConsoleCommand>).IsAssignableFrom(commandStatus.Command.GetType()))
                {
                    // is an async command
                    return ((AsyncConsoleCommand)commandStatus.Command).RunAsync(commandStatus.RemainingArgs.ToArray()).GetAwaiter().GetResult();
                }
                else if (typeof(IConsoleCommand<ConsoleCommand>).IsAssignableFrom(commandStatus.Command.GetType()))
                {
                    // it's a normal command
                    return ((ConsoleCommand)commandStatus.Command).Run(commandStatus.RemainingArgs.ToArray());
                }
                else
                {
                    // an invalid/custom implementation
                    consoleOut.WriteLine("Invalid implementation. You need to use either ConsoleCommand or AsyncConsoleCommand.");
                    return -1;
                }
            }
            return -1;
        }

        private static (int? Status, BaseConsoleCommand Command, List<string> RemainingArgs) GetCommandOrStatus(
            IEnumerable<BaseConsoleCommand> commands,
            string[] arguments,
            TextWriter consoleOut,
            bool skipExeInExpectedUsage)
        {
            BaseConsoleCommand selectedCommand = null;

            TextWriter console = consoleOut;

            foreach (var command in commands)
            {
                ValidateConsoleCommand(command);
            }

            try
            {
                List<string> remainingArguments;

                if (commands.Count() == 1)
                {
                    selectedCommand = commands.First();

                    if (arguments.Count() > 0 && CommandMatchesArgument(selectedCommand, arguments.First()))
                    {
                        remainingArguments = selectedCommand.GetActualOptions().Parse(arguments.Skip(1));
                    }
                    else
                    {
                        remainingArguments = selectedCommand.GetActualOptions().Parse(arguments);
                    }
                }
                else
                {
                    if (arguments.Count() < 1)
                        throw new ConsoleHelpAsException("No arguments specified.");

                    if (arguments[0].Equals("help", StringComparison.InvariantCultureIgnoreCase))
                    {
                        selectedCommand = GetMatchingCommand(commands, arguments.Skip(1).FirstOrDefault());

                        if (selectedCommand == null)
                            ConsoleHelp.ShowSummaryOfCommands(commands, console);
                        else
                            ConsoleHelp.ShowCommandHelp(selectedCommand, console, skipExeInExpectedUsage);

                        return (-1, null, null);
                    }

                    selectedCommand = GetMatchingCommand(commands, arguments.First());

                    if (selectedCommand == null)
                        throw new ConsoleHelpAsException("Command name not recognized.");

                    remainingArguments = selectedCommand.GetActualOptions().Parse(arguments.Skip(1));
                }

                selectedCommand.CheckRequiredArguments();

                CheckRemainingArguments(remainingArguments, selectedCommand.RemainingArgumentsCountMin, selectedCommand.RemainingArgumentsCountMax);

                var preResult = selectedCommand.OverrideAfterHandlingArgumentsBeforeRun(remainingArguments.ToArray());

                if (preResult.HasValue)
                    return (preResult.Value, null, null);

                ConsoleHelp.ShowParsedCommand(selectedCommand, console);

                return (null, selectedCommand, remainingArguments);
            }
            catch (ConsoleHelpAsException e)
            {
                return (DealWithException(e, console, skipExeInExpectedUsage, selectedCommand, commands), null, null);
            }
            catch (Mono.Options.OptionException e)
            {
                return (DealWithException(e, console, skipExeInExpectedUsage, selectedCommand, commands), null, null);
            }
        }

        private static int DealWithException(Exception e, TextWriter console, bool skipExeInExpectedUsage, BaseConsoleCommand selectedCommand, IEnumerable<BaseConsoleCommand> commands)
        {
            if (selectedCommand != null)
            {
                console.WriteLine();
                console.WriteLine(e.Message);
                ConsoleHelp.ShowCommandHelp(selectedCommand, console, skipExeInExpectedUsage);
            }
            else
            {
                ConsoleHelp.ShowSummaryOfCommands(commands, console);
            }

            return -1;
        }

        private static BaseConsoleCommand GetMatchingCommand(IEnumerable<BaseConsoleCommand> command, string name)
        {
            return command.FirstOrDefault(c => CommandMatchesArgument(c, name));
        }

        private static bool CommandMatchesArgument(BaseConsoleCommand command, string arg)
        {
            if (String.IsNullOrEmpty(arg))
            {
                return false;
            }
            if (arg.Equals(command.Command, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
            else if (command.Aliases != null && command.Aliases.Count > 0)
            {
                foreach (string alias in command.Aliases)
                {
                    if (arg.Equals(alias, StringComparison.OrdinalIgnoreCase))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        private static void ValidateConsoleCommand(BaseConsoleCommand command)
        {
            if (string.IsNullOrEmpty(command.Command))
            {
                throw new InvalidOperationException(String.Format(
                    "Command {0} did not call IsCommand in its constructor to indicate its name and description.",
                    command.GetType().Name));
            }
        }

        private static void CheckRemainingArguments(List<string> remainingArguments, int? parametersRequiredAfterOptionsMin, int? parametersRequiredAfterOptionsMax)
        {
            ConsoleUtil.VerifyNumberOfArguments(remainingArguments.ToArray(),
                    parametersRequiredAfterOptionsMin, parametersRequiredAfterOptionsMax);
        }

        public static IEnumerable<BaseConsoleCommand> FindCommandsInSameAssemblyAs(Type typeInSameAssembly)
        {
            if (typeInSameAssembly == null)
                throw new ArgumentNullException("typeInSameAssembly");

            return FindCommandsInAssembly(typeInSameAssembly.Assembly);
        }

        public static IEnumerable<BaseConsoleCommand> FindCommandsInAllLoadedAssemblies()
        {
            return AppDomain.CurrentDomain.GetAssemblies().SelectMany(FindCommandsInAssembly);
        }

        public static IEnumerable<BaseConsoleCommand> FindCommandsInAssembly(Assembly assembly)
        {
            if (assembly == null)
                throw new ArgumentNullException("assembly");

            var commandTypes = assembly.GetTypes()
                .Where(t => t.IsSubclassOf(typeof(BaseConsoleCommand)))
                .Where(t => !t.IsAbstract)
                .OrderBy(t => t.FullName);

            List<BaseConsoleCommand> result = new List<BaseConsoleCommand>();

            foreach (var commandType in commandTypes)
            {
                var constructor = commandType.GetConstructor(new Type[] { });

                if (constructor == null)
                    continue;

                result.Add((BaseConsoleCommand)constructor.Invoke(new object[] { }));
            }

            return result;
        }
    }
}
