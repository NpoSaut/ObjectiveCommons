using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace ObjectiveCommons.Collections
{
    /// <summary>
    /// ObservableCollection с возможностью временного подавления уведомлений об изменении элементов
    /// </summary>
    /// <typeparam name="T">Тип элемента коллекции</typeparam>
    public class LockableObservableCollection<T> : ObservableCollection<T>
    {
        /// <summary>
        /// Подавляет уведомления об изменении коллекции
        /// </summary>
        private bool NotificationsSupressed = false;

        private bool NotificationsAwaiting = false;

        protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            if (!NotificationsSupressed)
                base.OnCollectionChanged(e);
            else
                NotificationsAwaiting = true;
        }

        /// <summary>
        /// Добавляет последовательность элементов в коллекцию, вызывая только одно событие CollectionChanged
        /// </summary>
        /// <param name="elements">Элементы для добавления</param>
        public void AddRange(IEnumerable<T> elements)
        {
            if (elements == null)
                throw new ArgumentNullException("elements");

            using (Locker())
            {
                foreach (T item in elements)
                {
                    Add(item);
                }
            }
        }

        /// <summary>
        /// Заставляет коллекцию подавить нотификации
        /// </summary>
        protected void SupressNotifications()
        {
            NotificationsSupressed = true;
        }
        /// <summary>
        /// Разрешает коллекции излучать нотификации
        /// </summary>
        protected void ReleaseNotifications()
        {
            NotificationsSupressed = false;
            
            if (NotificationsAwaiting)
                OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));

            NotificationsAwaiting = false;
        }

        /// <summary>
        /// Объект подавления нотификаций в коллекции.
        /// </summary>
        public class NotificationLocker : IDisposable
        {
            public LockableObservableCollection<T> OnCollection { get; private set; }

            public NotificationLocker(LockableObservableCollection<T> OnCollection)
            {
                this.OnCollection = OnCollection;
                OnCollection.SupressNotifications();
            }

            public void Dispose()
            {
                OnCollection.ReleaseNotifications();
            }
        }
        /// <summary>
        /// Заставляет коллекцию временно подавить нотификации об изменении
        /// </summary>
        /// <returns>IDisposable-объект NotificationLocker, подавляющий нотификации в коллекции</returns>
        public NotificationLocker Locker()
        {
            return new NotificationLocker(this);
        }
    }
}
