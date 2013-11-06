using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using ObjectiveCommons.ConsoleParameters;

namespace Tools.ConsoleParameters
{
    public abstract class ConsoleParametersBase
    {
        public static String KeySymbol { get; set; }
        public static String SeparateSymbol { get; set; }

        static ConsoleParametersBase()
        {
            KeySymbol = "/";
            SeparateSymbol = ":";
        }


        protected void FillUp(string[] args)
        {
            if (args.Length > 0 && !args[0].StartsWith(KeySymbol))
                SetDefaultParameter(args[0]);

            var arguments = args
                .Where(a => a.StartsWith(KeySymbol))
                .Select(a => new ConsoleArgumentShell(a));

            var parameters = this
                .GetType()
                .GetProperties()
                .Select(
                    pi =>
                        new
                        {
                            pi,
                            attr =
                                pi.GetCustomAttributes(typeof (ConsoleParameterNameAttribute), true)
                                    .OfType<ConsoleParameterNameAttribute>()
                                    .FirstOrDefault()
                        })
                .Where(p => p.attr != null)
                .ToDictionary(p => p.attr.ParameterName.ToLower(), p => p.pi);
            
            foreach (var a in arguments)
            {
                string lowerName = a.Name.ToLower();
                if (parameters.ContainsKey(lowerName)) SetPropertyValue(parameters[lowerName], a.Value);
            }
        }

        private void SetDefaultParameter(string parameterValue)
        {
            var defaultProperty = this
                .GetType()
                .GetProperties()
                .Select(
                    pi =>
                        new
                        {
                            pi,
                            attr =
                                pi.GetCustomAttributes(typeof (ConsoleDefaultParameter), true)
                                    .OfType<ConsoleDefaultParameter>()
                                    .FirstOrDefault()
                        })
                .FirstOrDefault(p => p.attr != null);

            if (defaultProperty != null)
            {
                SetPropertyValue(defaultProperty.pi, parameterValue);
            }
        }

        private void SetPropertyValue(PropertyInfo property, string value)
        {
            object val = Convert.ChangeType((object) value, (Type) property.PropertyType);
            property.SetValue(this, val, new object[0]);
        }
    }
}
