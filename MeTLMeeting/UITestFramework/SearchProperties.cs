using System;
using System.ComponentModel;
using System.Windows.Automation;

namespace UITestFramework
{
    public enum PropertyExpressionOperator
    {
        EqualTo,
        Contains
    }

    public class PropertyExpression : INotifyPropertyChanged
    {
        private AutomationProperty propertyName;
        private string propertyValue;
        private PropertyExpressionOperator propertyOperator = PropertyExpressionOperator.EqualTo;

        private static object notifyLock = new Object();

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(string whichProperty)
        {
            lock (notifyLock)
            {
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs(whichProperty));
                }
            }
        }

        public PropertyExpression()
        {
        }

        public PropertyExpression(AutomationProperty propertyName, string propertyValue)
        {
            PropertyName = propertyName;
            PropertyValue = propertyValue;
            PropertyOperator = PropertyExpressionOperator.EqualTo;
        }

        public PropertyExpression(AutomationProperty propertyName, string propertyValue, PropertyExpressionOperator propertyOperator)
        {
            PropertyName = propertyName;
            PropertyValue = propertyValue;
            PropertyOperator = propertyOperator;
        }

        #region Properties
        public AutomationProperty PropertyName
        {
            get
            {
                return this.propertyName;
            }
            set
            {
                if (value != this.propertyName)
                {
                    this.propertyName = value;
                    NotifyPropertyChanged("PropertyName");
                }
            }
        }

        public string PropertyValue
        {
            get
            {
                return this.propertyValue;
            }
            set
            {
                if (value != this.propertyValue)
                {
                    this.propertyValue = value;
                    NotifyPropertyChanged("PropertyValue");
                }
            }
        }

        public PropertyExpressionOperator PropertyOperator
        {
            get
            {
                return this.propertyOperator;
            }
            set
            {
                if (value != this.propertyOperator)
                {
                    this.propertyOperator = value;
                    NotifyPropertyChanged("PropertyOperator");
                }
            }
        }
        #endregion
    }

    /*public class PropertyExpressionCollection : ICollection<PropertyExpression>, INotifyCollectionChanged
    {
        private static object notifyLock = new Object();

        public event NotifyCollectionChangedEventHandler CollectionChanged;

        private void NotifyCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            lock (notifyLock)
            {
                if (CollectionChanged != null)
                {
                    CollectionChanged(this, e);
                }
            }
        }
        public PropertyExpressionCollection()
        {
        }

        public void Add(string propertyName, string propertyValue)
        {
            Add(new PropertyExpression(propertyName, propertyValue));
            NotifyCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add));
        }
        public void Add(string propertyName, string propertyValue, PropertyExpressionOperator propertyOperator)
        {
            List.Add(new PropertyExpression(propertyName, propertyValue, propertyOperator));
            NotifyCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add));
        }
        public void Add(PropertyExpression propertyExpression)
        {
            List.Add(propertyExpression);
            NotifyCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add));
        }

        public void Remove(PropertyExpression propertyExpression)
        {
            List.Remove(propertyExpression);
            NotifyCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove));
        }

        public static PropertyExpressionCollection ReadOnly(PropertyExpressionCollection collection)
        {
            
        }
        protected override void OnValidate(object value)
        {
            base.OnValidate(value);
            if (!(value is PropertyExpression))
            {
                throw new ArgumentException("Collection only supports PropertyExpression objects");
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return (IEnumerator)GetEnumerator();
        }
    }*/
}
