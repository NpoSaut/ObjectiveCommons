using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace ObjectiveCommons
{
    /// <summary>
    /// Повторяет указанное действие при возниконвении указанных исключений
    /// </summary>
    public class Repeater
    {
        private abstract class ExceptionCapture
        {
            public abstract Type ExceptionType { get; }
            public abstract void OnCaptured(Exception exc);
        }

        private class ExceptionCapture<TException> : ExceptionCapture
            where TException : Exception
        {
            public override Type ExceptionType { get { return typeof (TException); } }
            public Action<TException> CatchAction { get; private set; }

            public override void OnCaptured(Exception exc) { if (CatchAction != null) CatchAction((TException) exc); }

            public ExceptionCapture(Action<TException> CatchAction)
            {
                this.CatchAction = CatchAction;
            }
        }

        public TimeSpan Timeout { get; private set; }
        public int MaximumAttempts { get; set; }
        private IList<ExceptionCapture> CatchingExceptions { get; set; }

        public Action Job { get; private set; }

        /// <summary>
        /// Возвращает строителя нового повторителя
        /// </summary>
        /// <param name="Job">Повторяемое действие</param>
        /// <returns>Возвращает строителя для создания повторителя</returns>
        public static RepeaterBuilder Repeat(Action Job)
        {
            return new RepeaterBuilder(Job);
        }

        private Repeater(Action Job) { this.Job = Job; }

        /// <summary>
        /// Запускает процесс
        /// </summary>
        public void Run()
        {
            int counter = 0;
            var sw = new Stopwatch();
            sw.Start();

            while (sw.Elapsed < Timeout || counter < MaximumAttempts)
            {
                try
                {
                    Job();
                    break;
                }
                catch (Exception e)
                {
                    var t = e.GetType();
                    var capture = CatchingExceptions.FirstOrDefault(c => t == c.ExceptionType || t.IsSubclassOf(c.ExceptionType));
                    if (capture == null) throw;
                    else capture.OnCaptured(e);
                }
                counter++;
            }
        }

        public class RepeaterBuilder
        {
            public Action Job { get; private set; }
            public TimeSpan Timeout { get; private set; }
            public int MaximumAttempts { get; set; }
            private IList<ExceptionCapture> CatchingExceptions { get; set; }

            internal RepeaterBuilder(Action Job)
            {
                this.Job = Job;
                Timeout = TimeSpan.MaxValue;
                MaximumAttempts = int.MaxValue;
                CatchingExceptions = new List<ExceptionCapture>();
            }

            /// <summary>
            /// Создаёт повторителя с заданными параметрами
            /// </summary>
            public Repeater Build()
            {
                if (Job == null) throw new ArgumentException("Не выбрана работа");
                return new Repeater(Job)
                       {
                           Timeout = Timeout,
                           MaximumAttempts = MaximumAttempts,
                           CatchingExceptions = CatchingExceptions
                       };
            }
            /// <summary>
            /// Указывает на необходимость прервать повторения по таймауту
            /// </summary>
            /// <param name="Timeout">Таймаут операции</param>
            public RepeaterBuilder WithTimeout(TimeSpan Timeout)
            {
                this.Timeout = Timeout;
                return this;
            }
            /// <summary>
            /// Указывает на необходимость прервать повторения при достижении заданного количества неудачных попыток
            /// </summary>
            /// <param name="Count">Максимальное количество неудачных повторений</param>
            public RepeaterBuilder WithMaximumAttempts(int Count)
            {
                this.MaximumAttempts = Count;
                return this;
            }
            /// <summary>
            /// Добавляет отлавливаемое исключение
            /// </summary>
            /// <typeparam name="TException">Тип отлавливаемого исключения</typeparam>
            /// <param name="CatchAction">Действие, которое необходимо выполнить при возникновении этого исключения</param>
            public RepeaterBuilder Catch<TException>(Action<TException> CatchAction = null) where TException : Exception
            {
                CatchingExceptions.Add(new ExceptionCapture<TException>(CatchAction));
                return this;
            }
        }
    }
}