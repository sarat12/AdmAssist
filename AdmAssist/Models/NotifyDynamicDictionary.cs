using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Dynamic;

namespace AdmAssist.Models
{
    /// <summary>
    /// Bindable dynamic dictionary.
    /// </summary>
    public sealed class NotifyDynamicDictionary : DynamicObject, INotifyPropertyChanged, IDictionary<string, object>
    {
        /// <summary>
        /// The internal dictionary.
        /// </summary>
        private readonly Dictionary<string, object> _dictionary;

        public ICollection<string> Keys => _dictionary.Keys;

        public ICollection<object> Values => _dictionary.Values;

        public int Count => _dictionary.Count;

        public bool IsReadOnly => false;

        /// <summary>
        /// Creates a new BindableDynamicDictionary with an empty internal dictionary.
        /// </summary>
        public NotifyDynamicDictionary()
        {
            _dictionary = new Dictionary<string, object>();
        }

        /// <summary>
        /// Copies the contents of the given dictionary to initilize the internal dictionary.
        /// </summary>
        /// <param name="source"></param>
        public NotifyDynamicDictionary(IDictionary<string, object> source)
        {
            _dictionary = new Dictionary<string, object>(source);
        }
        /// <summary>
        /// You can still use this as a dictionary.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public object this[string key]
        {
            get
            {
                return _dictionary.TryGetValue(key, out object value) ? value : null;
            }
            set
            {
                _dictionary[key] = value;
                RaisePropertyChanged(key);
            }
        }

        /// <summary>
        /// This allows you to get properties dynamically.
        /// </summary>
        /// <param name="binder"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            return _dictionary.TryGetValue(binder.Name, out result);
        }

        /// <summary>
        /// This allows you to set properties dynamically.
        /// </summary>
        /// <param name="binder"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public override bool TrySetMember(SetMemberBinder binder, object value)
        {
            _dictionary[binder.Name] = value;
            RaisePropertyChanged(binder.Name);
            return true;
        }

        /// <summary>
        /// This is used to list the current dynamic members.
        /// </summary>
        /// <returns></returns>
        public override IEnumerable<string> GetDynamicMemberNames()
        {
            return _dictionary.Keys;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void RaisePropertyChanged(string propertyName)
        {
            var propChange = PropertyChanged;
            propChange?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public bool ContainsKey(string key)
        {
            return _dictionary.ContainsKey(key);
        }

        public void Add(string key, object value)
        {
            _dictionary.Add(key, value);
        }

        public bool Remove(string key)
        {
            return _dictionary.Remove(key);
        }

        public bool TryGetValue(string key, out object value)
        {
            return _dictionary.TryGetValue(key, out value);
        }

        public void Add(KeyValuePair<string, object> item)
        {
            _dictionary.Add(item.Key, item.Value);
        }

        public void Clear()
        {
            _dictionary.Clear();
        }

        public bool Contains(KeyValuePair<string, object> item)
        {
            return _dictionary.ContainsKey(item.Key) && _dictionary.ContainsValue(item.Value);
        }

        public void CopyTo(KeyValuePair<string, object>[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        public bool Remove(KeyValuePair<string, object> item)
        {
            return _dictionary.Remove(item.Key);
        }

        public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
        {
            return _dictionary.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _dictionary.GetEnumerator();
        }
    }
}
