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
        private int NotificationsSupressCounter = 0;

        private bool NotificationsAwaiting = false;

        protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            if (NotificationsSupressCounter <= 0)
                base.OnCollectionChanged(e);
            else
            {
                NotificationsAwaiting = true;
#if ACTIONS_WITH_RANGE
                if (e.Action == NotifyCollectionChangedAction.Add)
                    for (int i = 0; i < e.NewItems.Count; i++)
                        RegisterNewItem(e.NewStartingIndex + i, e.NewItems[i]);
                if (e.Action == NotifyCollectionChangedAction.Remove)
                    for (int i = 0; i < e.OldItems.Count; i++)
                        RegisterOldItem(e.OldStartingIndex + i, e.OldItems[i]);
#endif
            }
        }

        /// <summary>
        /// Добавляет последовательность элементов в коллекцию, вызывая только одно событие CollectionChanged
        /// </summary>
        /// <param name="elements">Элементы для добавления</param>
        public void AddRange(IEnumerable<T> elements)
        {
            if (elements == null)
                throw new ArgumentNullException("elements");

            using (var l = Locker())
            {
                foreach (T item in elements)
                {
                    Add(item);
                }
            }
        }

        private System.Collections.ArrayList NewItems { get; set; }
        private int? NewItemsIndex { get; set; }
        private System.Collections.ArrayList OldItems { get; set; }
        private int? OldItemsIndex { get; set; }
        private NotifyCollectionChangedAction? lockedaction { get; set; }

        /// <summary>
        /// Заставляет коллекцию подавить нотификации
        /// </summary>
        protected void SupressNotifications()
        {
            if (NotificationsSupressCounter == 0)
            {
                NewItems = new System.Collections.ArrayList(); NewItemsIndex = null;
                OldItems = new System.Collections.ArrayList(); OldItemsIndex = null;
            }
            NotificationsSupressCounter++;
        }
        /// <summary>
        /// Разрешает коллекции излучать нотификации
        /// </summary>
        protected void ReleaseNotifications()
        {
            NotificationsSupressCounter--;

            if (NotificationsSupressCounter <= 0)
            {
                if (NotificationsAwaiting)
                {
#if ACTIONS_WITH_RANGE
                    NotifyCollectionChangedEventArgs e;
                    switch (lockedaction ?? NotifyCollectionChangedAction.Reset)
                    {
                        case NotifyCollectionChangedAction.Add:
                            e = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, NewItems, NewItemsIndex.Value);
                            break;
                        case NotifyCollectionChangedAction.Remove:
                            e = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, OldItems, OldItemsIndex.Value);
                            break;
                        default:
                            e = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset);
                            break;
                    }
                    OnCollectionChanged(e);
#else
                    OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
#endif
                }
                NotificationsAwaiting = false;
            }
        }

        public bool RegisterNewItem(int index, object item)
        {
            bool isSequent = false;
            if (NewItemsIndex == null)
            {
                NewItemsIndex = index;
                isSequent = true;
            }
            else if (index >= NewItemsIndex && index <= NewItemsIndex + NewItems.Count + 1)
            {
                isSequent = true;
            }
            NewItems.Add(item);
            if (lockedaction == null) lockedaction = NotifyCollectionChangedAction.Add;
            bool allGood = isSequent && lockedaction == NotifyCollectionChangedAction.Add;
            if (!allGood) lockedaction = NotifyCollectionChangedAction.Reset;
            return allGood;
        }
        public bool RegisterOldItem(int index, object item)
        {
            bool isSequent = false;
            if (OldItemsIndex == null || index == OldItemsIndex - 1)
            {
                OldItemsIndex = index;
                isSequent = true;
            }
            else if (index == OldItemsIndex)
            {
                isSequent = true;
            }
            OldItems.Add(item);
            if (lockedaction == null) lockedaction = NotifyCollectionChangedAction.Remove;
            bool allGood = isSequent && lockedaction == NotifyCollectionChangedAction.Remove;
            if (!allGood) lockedaction = NotifyCollectionChangedAction.Reset;
            return allGood;
        }

        //protected override void InsertItem(int index, T item)
        //{
        //    base.InsertItem(index, item);
        //    RegisterNewItem(index, item);
        //}
        //protected override void RemoveItem(int index)
        //{
        //    RegisterOldItem(index, Items[index]);
        //    base.RemoveItem(index);
        //}

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
