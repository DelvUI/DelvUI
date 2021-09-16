using System;

namespace DelvUI.Config
{

    public enum ChangeType
    {
        None = 0,
        ListAdd = 1,
        ListRemove = 2
    }

    public class OnChangeBaseArgs : EventArgs
    {
        public string KeyName { get; }
        public ChangeType ChangeType { get; private set; }

        public OnChangeBaseArgs(string keyName, ChangeType type = ChangeType.None)
        {
            KeyName = keyName;
            ChangeType = type;
        }
    }

    public class OnChangeEventArgs<T> : OnChangeBaseArgs
    {
        public T Value { get; }

        public OnChangeEventArgs(string keyName, T value, ChangeType type = ChangeType.None) : base(keyName, type)
        {
            Value = value;
        }
    }

    public interface IOnChangeEventArgs
    {
        public abstract event EventHandler<OnChangeBaseArgs> onValueChanged;

        public abstract void onValueChangedRegisterEvent(OnChangeBaseArgs e);
    }

}
