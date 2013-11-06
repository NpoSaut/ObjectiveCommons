using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tools.ConsoleParameters
{
    /// <summary>
    /// Имя аргумента для параметра
    /// </summary>
    public class ConsoleParameterNameAttribute : Attribute
    {
        public String ParameterName { get; set; }
        public String Description { get; set; }

        public ConsoleParameterNameAttribute(string ParameterName, string Description = null)
        {
            this.ParameterName = ParameterName;
            this.Description = Description;
        }
    }
}
