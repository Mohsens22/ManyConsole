using ManyConsole.Internal;
using Mono.Options;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ManyConsole
{
    public abstract class BaseConsoleCommand : ConsoleUtil
    {
        private static readonly string[] DefaultTraceCommandSkipProperties =
        {
            nameof(Command),
            nameof(Aliases),
            nameof(OneLineDescription),
            nameof(LongDescription),
            nameof(Options),
            nameof(TraceCommandAfterParse),
            nameof(TraceCommandSkipProperties),
            nameof(RemainingArgumentsCountMin),
            nameof(RemainingArgumentsCountMax),
            nameof(RemainingArgumentsHelpText),
            nameof(RequiredOptions)
        };

        public string Command { get; protected internal set; }
        public List<string> Aliases { get; protected internal set; }
        public string OneLineDescription { get; protected internal set; }
        public string LongDescription { get; protected internal set; }
        public OptionSet Options { get; protected set; }
        public bool TraceCommandAfterParse { get; protected internal set; }
        public ISet<string> TraceCommandSkipProperties { get; protected internal set; }
        public int? RemainingArgumentsCountMin { get; protected internal set; }
        public int? RemainingArgumentsCountMax { get; protected internal set; }
        public string RemainingArgumentsHelpText { get; protected internal set; }
        protected internal OptionSet OptionsHasd { get; set; }
        protected internal List<RequiredOptionRecord> RequiredOptions { get; set; }

        public BaseConsoleCommand()
        {
            OneLineDescription = "";
            Options = new OptionSet();
            TraceCommandAfterParse = true;
            TraceCommandSkipProperties = new SortedSet<string>(DefaultTraceCommandSkipProperties, StringComparer.Ordinal);
            RemainingArgumentsCountMax = 0;
            RemainingArgumentsHelpText = "";
            OptionsHasd = new OptionSet();
            RequiredOptions = new List<RequiredOptionRecord>();
        }

        public virtual void CheckRequiredArguments()
        {
            var missingOptions = RequiredOptions
                .Where(o => !o.WasIncluded).Select(o => o.Name).OrderBy(n => n).ToArray();

            if (missingOptions.Any())
            {
                throw new ConsoleHelpAsException("Missing option: " + string.Join(", ", missingOptions));
            }
        }

        public virtual int? OverrideAfterHandlingArgumentsBeforeRun(string[] remainingArguments)
        {
            return null;
        }

        public OptionSet GetActualOptions()
        {
            var result = new OptionSet();

            foreach (var option in Options)
                result.Add(option);

            foreach (var option in OptionsHasd)
                result.Add(option);

            return result;
        }

        protected internal void DefineCommand(string command, string oneLineDescription = "")
        {
            Command = command;
            OneLineDescription = oneLineDescription;
        }

        protected internal void SetAlias(string alias)
        {
            if (!String.IsNullOrEmpty(alias))
            {
                if (Aliases == null)
                {
                    Aliases = new List<string>();
                }
                Aliases.Add(alias);
            }
        }

        protected internal void SetAdditionalArgumentsBetween(int? min, int? max, string helpText)
        {
            RemainingArgumentsCountMin = min;
            RemainingArgumentsCountMax = max;
            RemainingArgumentsHelpText = helpText;
        }

        protected internal void SetRequiredOption<T>(string prototype, string description, Action<T> action)
        {
            var requiredRecord = new RequiredOptionRecord();

            var previousOptions = OptionsHasd.ToArray();

            OptionsHasd.Add<T>(prototype, description, s =>
            {
                requiredRecord.WasIncluded = true;
                action(s);
            });

            var newOption = OptionsHasd.Single(o => !previousOptions.Contains(o));

            requiredRecord.Name = newOption.GetNames().OrderByDescending(n => n.Length).First();

            RequiredOptions.Add(requiredRecord);
        }
    }
}
