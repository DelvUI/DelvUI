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
}
