using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ObjectiveCommons.Interfaces
{
    /// <summary>
    /// Объект, содержащий информацию о дате начала и конца актуальности
    /// </summary>
    public interface IContinuous
    {
        DateTime StartDate { get; set; }
        DateTime? EndDate { get; set; }
    }

    public static class IContinuousHelper
    {
        public static bool IsContainsDate(this IContinuous ContinuousObject, DateTime Date)
        {
            return ContinuousObject.StartDate <= Date && (ContinuousObject.EndDate == null || ContinuousObject.EndDate > Date);
        }
    }
}
