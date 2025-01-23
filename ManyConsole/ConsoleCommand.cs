using Mono.Options;
using System;

namespace ManyConsole
{
    public abstract class ConsoleCommand : BaseConsoleCommand, IConsoleCommand<ConsoleCommand>
    {
        public ConsoleCommand IsCommand(string command, string oneLineDescription = "")
        {
            DefineCommand(command, oneLineDescription);
            return this;
        }

        public ConsoleCommand HasAlias(string alias)
        {
            SetAlias(alias);
            return this;
        }

        public ConsoleCommand HasLongDescription(string longDescription)
        {
            LongDescription = longDescription;
            return this;
        }

        public ConsoleCommand HasAdditionalArguments(int? count = 0, string helpText = "")
        {
            return HasAdditionalArgumentsBetween(count, count, helpText);
        }

        public ConsoleCommand HasAdditionalArgumentsBetween(int? min, int? max, string helpText = "")
        {
            SetAdditionalArgumentsBetween(min, max, helpText);
            return this;
        }

        public ConsoleCommand AllowsAnyAdditionalArguments(string helpText = "")
        {
            return HasAdditionalArgumentsBetween(null, null, helpText);
        }

        public ConsoleCommand SkipsCommandSummaryBeforeRunning()
        {
            TraceCommandAfterParse = false;
            return this;
        }

        public ConsoleCommand SkipsPropertyInCommandSummary(string propertyName)
        {
            TraceCommandSkipProperties.Add(propertyName);
            return this;
        }

        public ConsoleCommand HasOption(string prototype, string description, Action<string> action)
        {
            OptionsHasd.Add(prototype, description, action);
            return this;
        }

        public ConsoleCommand HasRequiredOption(string prototype, string description, Action<string> action)
        {
            HasRequiredOption<string>(prototype, description, action);
            return this;
        }

        public ConsoleCommand HasOption<T>(string prototype, string description, Action<T> action)
        {
            OptionsHasd.Add(prototype, description, action);
            return this;
        }

        public ConsoleCommand HasRequiredOption<T>(string prototype, string description, Action<T> action)
        {
            SetRequiredOption(prototype, description, action);
            return this;
        }

        public ConsoleCommand HasOption(string prototype, string description, OptionAction<string, string> action)
        {
            OptionsHasd.Add(prototype, description, action);
            return this;
        }

        public ConsoleCommand HasOption<TKey, TValue>(string prototype, string description, OptionAction<TKey, TValue> action)
        {
            OptionsHasd.Add(prototype, description, action);
            return this;
        }

        public abstract int Run(string[] remainingArguments);
    }
}