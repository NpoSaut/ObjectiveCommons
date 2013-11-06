using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tools.ConsoleParameters;

namespace ObjectiveCommons.ConsoleParameters
{
    class ConsoleArgumentShell
    {
        public string Name { get; set; }
        public string Value { get; set; }

        public ConsoleArgumentShell(String argument)
        {
            argument = argument.Substring(ConsoleParametersBase.KeySymbol.Length);
            int separateIndex = argument.IndexOf(ConsoleParametersBase.SeparateSymbol, System.StringComparison.Ordinal);
            if (separateIndex == -1)
            {
                Name = argument;
                Value = "true";
            }
            else
            {
                Name = argument.Substring(0, separateIndex);
                Value = argument.Substring(separateIndex + ConsoleParametersBase.SeparateSymbol.Length);
            }
        }
    }
}
