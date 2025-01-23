using Mono.Options;
using System;
using System.Threading.Tasks;

namespace ManyConsole
{
    public abstract class AsyncConsoleCommand : BaseConsoleCommand, IConsoleCommand<AsyncConsoleCommand>
    {
        public AsyncConsoleCommand IsCommand(string command, string oneLineDescription = "")
        {
            DefineCommand(command, oneLineDescription);
            return this;
        }

        public AsyncConsoleCommand HasAlias(string alias)
        {
            SetAlias(alias);
            return this;
        }

        public AsyncConsoleCommand HasLongDescription(string longDescription)
        {
            LongDescription = longDescription;
            return this;
        }

        public AsyncConsoleCommand HasAdditionalArguments(int? count = 0, string helpText = "")
        {
            return HasAdditionalArgumentsBetween(count, count, helpText);
        }

        public AsyncConsoleCommand HasAdditionalArgumentsBetween(int? min, int? max, string helpText = "")
        {
            SetAdditionalArgumentsBetween(min, max, helpText);
            return this;
        }

        public AsyncConsoleCommand AllowsAnyAdditionalArguments(string helpText = "")
        {
            return HasAdditionalArgumentsBetween(null, null, helpText);
        }

        public AsyncConsoleCommand SkipsCommandSummaryBeforeRunning()
        {
            TraceCommandAfterParse = false;
            return this;
        }

        public AsyncConsoleCommand SkipsPropertyInCommandSummary(string propertyName)
        {
            TraceCommandSkipProperties.Add(propertyName);
            return this;
        }

        public AsyncConsoleCommand HasOption(string prototype, string description, Action<string> action)
        {
            OptionsHasd.Add(prototype, description, action);
            return this;
        }

        public AsyncConsoleCommand HasRequiredOption(string prototype, string description, Action<string> action)
        {
            HasRequiredOption<string>(prototype, description, action);
            return this;
        }

        public AsyncConsoleCommand HasOption<T>(string prototype, string description, Action<T> action)
        {
            OptionsHasd.Add(prototype, description, action);
            return this;
        }

        public AsyncConsoleCommand HasRequiredOption<T>(string prototype, string description, Action<T> action)
        {
            SetRequiredOption(prototype, description, action);
            return this;
        }

        public AsyncConsoleCommand HasOption(string prototype, string description, OptionAction<string, string> action)
        {
            OptionsHasd.Add(prototype, description, action);
            return this;
        }

        public AsyncConsoleCommand HasOption<TKey, TValue>(string prototype, string description, OptionAction<TKey, TValue> action)
        {
            OptionsHasd.Add(prototype, description, action);
            return this;
        }

        public abstract Task<int> RunAsync(string[] remainingArguments);
    }
}
