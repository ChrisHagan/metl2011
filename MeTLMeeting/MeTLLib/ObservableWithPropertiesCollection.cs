using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Collections.Specialized;
using System.Collections.Generic;

namespace MeTLLib
{
    /// <summary>
    /// ObservableCollection that also updates when a property on one of its items has changed 
    /// </summary>
    /// <typeparam name="T">A type that implements the INotifyPropertyChanged interface</typeparam>
    public class ObservableWithPropertiesCollection<T> : ObservableCollection<T> where T : INotifyPropertyChanged
    {
        public ObservableWithPropertiesCollection() : base()
        {
            RegisterHandlers();
        }

        public ObservableWithPropertiesCollection(List<T> initialiseList) : base(initialiseList)
        {
            RegisterHandlers();
        }

        void RegisterHandlers()
        {
            this.CollectionChanged += ObservableWithPropertiesCollection_CollectionChanged;
        }

        void ObservableWithPropertiesCollection_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.OldItems != null)
            {
                foreach (var item in e.OldItems)
                {
                    (item as INotifyPropertyChanged).PropertyChanged -= Item_PropertyChanged;
                }
            }
            else if (e.NewItems != null)
            {
                foreach (var item in e.NewItems)
                {
                    (item as INotifyPropertyChanged).PropertyChanged += Item_PropertyChanged;
                }
            }
        }

        void Item_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            NotifyCollectionChangedEventArgs args = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset);
            OnCollectionChanged(args);
        }
    }
}
