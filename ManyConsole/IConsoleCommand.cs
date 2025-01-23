using Mono.Options;
using System;

namespace ManyConsole
{
    internal interface IConsoleCommand<T>
    {
        T IsCommand(string command, string oneLineDescription = "");

        T HasAlias(string alias);

        T HasLongDescription(string longDescription);

        T HasAdditionalArguments(int? count = 0, string helpText = "");

        T HasAdditionalArgumentsBetween(int? min, int? max, string helpText = "");

        T AllowsAnyAdditionalArguments(string helpText = "");

        T SkipsCommandSummaryBeforeRunning();

        T SkipsPropertyInCommandSummary(string propertyName);

        T HasOption(string prototype, string description, Action<string> action);

        T HasRequiredOption(string prototype, string description, Action<string> action);

        T HasOption<TAction>(string prototype, string description, Action<TAction> action);

        T HasRequiredOption<TAction>(string prototype, string description, Action<TAction> action);

        T HasOption(string prototype, string description, OptionAction<string, string> action);

        T HasOption<TKey, TValue>(string prototype, string description, OptionAction<TKey, TValue> action);

        void CheckRequiredArguments();

        int? OverrideAfterHandlingArgumentsBeforeRun(string[] remainingArguments);

        OptionSet GetActualOptions();
    }
}
