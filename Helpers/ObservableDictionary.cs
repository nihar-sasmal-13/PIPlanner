using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;

namespace PIPlanner.Helpers
{
    public class ObservableDictionary<K, V> : Dictionary<K, V>, IDictionary<K, V>, INotifyCollectionChanged, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public event NotifyCollectionChangedEventHandler CollectionChanged;

        public new V this[K key]
        {
            get => base[key];
            set
            {
                base[key] = value;
                PropertyChangedEventHandler temp = PropertyChanged;
                temp?.Invoke(this, null);
            }
        }

        public new void Add(K key, V value)
        {
            base.Add(key, value);
            NotifyCollectionChangedEventHandler temp = CollectionChanged;
            temp?.Invoke(this, null);
        }

        public new void Clear()
        {
            base.Clear();
            NotifyCollectionChangedEventHandler temp = CollectionChanged;
            temp?.Invoke(this, null);
        }

        public new bool Remove(K key)
        {
            if (base.Remove(key))
            {
                NotifyCollectionChangedEventHandler temp = CollectionChanged;
                temp?.Invoke(this, null);
                return true;
            }
            return false;
        }
    }
}
