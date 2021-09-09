using System;

namespace DelvUI.Config.Attributes
{
    [AttributeUsage(AttributeTargets.Field)]
    public class CheckboxAttribute : Attribute
    {
        public string friendlyName;

        public CheckboxAttribute(string friendlyName) { this.friendlyName = friendlyName; }
    }

    [AttributeUsage(AttributeTargets.Field)]
    public class DragFloatAttribute : Attribute
    {
        public string friendlyName;
        public float min;
        public float max;
        public float velocity;

        public DragFloatAttribute(string friendlyName)
        {
            this.friendlyName = friendlyName;
            min = 1f;
            max = 1000f;
            velocity = 1f;
        }
    }

    [AttributeUsage(AttributeTargets.Field)]
    public class DragIntAttribute : Attribute
    {
        public string friendlyName;
        public int min;
        public int max;
        public int velocity;

        public DragIntAttribute(string friendlyName)
        {
            this.friendlyName = friendlyName;
            min = 1;
            max = 1000;
            velocity = 1;
        }
    }

    [AttributeUsage(AttributeTargets.Field)]
    public class DragFloat2Attribute : Attribute
    {
        public string friendlyName;
        public float min;
        public float max;
        public float velocity;

        public DragFloat2Attribute(string friendlyName)
        {
            this.friendlyName = friendlyName;
            min = 1f;
            max = 1000f;
            velocity = 1f;
        }
    }

    [AttributeUsage(AttributeTargets.Field)]
    public class DragInt2Attribute : Attribute
    {
        public string friendlyName;
        public int min;
        public int max;
        public int velocity;

        public DragInt2Attribute(string friendlyName)
        {
            this.friendlyName = friendlyName;
            min = 1;
            max = 1000;
            velocity = 1;
        }
    }

    [AttributeUsage(AttributeTargets.Field)]
    public class InputTextAttribute : Attribute
    {
        public string friendlyName;
        public uint maxLength;

        public InputTextAttribute(string friendlyName)
        {
            this.friendlyName = friendlyName;
            maxLength = 999;
        }
    }

    [AttributeUsage(AttributeTargets.Field)]
    public class ColorEdit4Attribute : Attribute
    {
        public string friendlyName;

        public ColorEdit4Attribute(string friendlyName) { this.friendlyName = friendlyName; }
    }

    [AttributeUsage(AttributeTargets.Field)]
    public class OrderAttribute : Attribute
    {
        public int pos;

        public OrderAttribute(int pos)
        {
            this.pos = pos;
        }
    }

    [AttributeUsage(AttributeTargets.Field)]
    public class CollapseControlAttribute : Attribute
    {
        public int pos;
        public int id;

        public CollapseControlAttribute(int pos, int id)
        {
            this.pos = pos;
            this.id = id;
        }
    }

    [AttributeUsage(AttributeTargets.Field)]
    public class CollapseWithAttribute : Attribute
    {
        public int pos;
        public int id;

        public CollapseWithAttribute(int pos, int id)
        {
            this.pos = pos;
            this.id = id;
        }
    }

    [AttributeUsage(AttributeTargets.Class)]
    public class PortableAttribute : Attribute
    {
        public bool portable;

        public PortableAttribute(bool portable)
        {
            this.portable = portable;
        }
    }

    [AttributeUsage(AttributeTargets.Method)]
    public class ManualDrawAttribute : Attribute
    { }
}
