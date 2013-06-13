using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace ObjectiveCommons.Collections
{
    public class RangeObservableCollection<T> : ObservableCollection<T>
    {
        internal bool SupressNotifications = false;

        protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            if (!SupressNotifications)
                base.OnCollectionChanged(e);
        }

        public void AddRange(IEnumerable<T> list)
        {
            if (list == null)
                throw new ArgumentNullException("list");

            SupressNotifications = true;

            foreach (T item in list)
            {
                Add(item);
            }
            SupressNotifications = false;
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }
    }

    public class CollectionRefreshLocker
    {
    }
}
