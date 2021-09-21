using System;

namespace DelvUI.Config.Attributes
{
    #region class attributes
    [AttributeUsage(AttributeTargets.Class)]
    public class PortableAttribute : Attribute
    {
        public bool portable;

        public PortableAttribute(bool portable)
        {
            this.portable = portable;
        }
    }

    [AttributeUsage(AttributeTargets.Class)]
    public class DisableableAttribute : Attribute
    {
        public bool disableable;

        public DisableableAttribute(bool disableable)
        {
            this.disableable = disableable;
        }
    }
    #endregion

    #region method attributes
    [AttributeUsage(AttributeTargets.Method)]
    public class ManualDrawAttribute : Attribute
    {
    }
    #endregion

    #region field attributes
    [AttributeUsage(AttributeTargets.Field)]
    public class ConfigAttribute : Attribute
    {
        public bool isMonitored = false;
        public bool separator = false;
        public bool spacing = false;
    }

    [AttributeUsage(AttributeTargets.Field)]
    public class CheckboxAttribute : ConfigAttribute
    {
        public string friendlyName;

        public CheckboxAttribute(string friendlyName)
        {
            this.friendlyName = friendlyName;
        }
    }

    [AttributeUsage(AttributeTargets.Field)]
    public class DragFloatAttribute : ConfigAttribute
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
    public class DragIntAttribute : ConfigAttribute
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
    public class DragFloat2Attribute : ConfigAttribute
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
    public class DragInt2Attribute : ConfigAttribute
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
    public class InputTextAttribute : ConfigAttribute
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
    public class ColorEdit4Attribute : ConfigAttribute
    {
        public string friendlyName;

        public ColorEdit4Attribute(string friendlyName)
        {
            this.friendlyName = friendlyName;

        }
    }

    [AttributeUsage(AttributeTargets.Field)]
    public class ComboAttribute : ConfigAttribute
    {
        public string friendlyName;
        public string[] options;


        public ComboAttribute(string friendlyName, params string[] options)
        {
            this.friendlyName = friendlyName;
            this.options = options;

        }
    }

    [AttributeUsage(AttributeTargets.Field)]
    public class AnchorAttribute : ComboAttribute
    {
        public AnchorAttribute(string friendlyName)
            : base(friendlyName, new string[] { "Center", "Left", "Right", "Top", "TopLeft", "TopRight", "Bottom", "BottomLeft", "BottomRight" })
        {
        }
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

    [AttributeUsage(AttributeTargets.Field)]
    public class DragDropHorizontalAttribute : ConfigAttribute
    {
        public string friendlyName;
        public string[] names;


        public DragDropHorizontalAttribute(string friendlyName, params string[] names)
        {
            this.friendlyName = friendlyName;
            this.names = names;

        }
    }

    [AttributeUsage(AttributeTargets.Field)]
    public class NestedConfigAttribute : Attribute
    {
        public string friendlyName;
        public int pos;

        public NestedConfigAttribute(string friendlyName, int pos)
        {
            this.friendlyName = friendlyName;
            this.pos = pos;

        }
    }

    [AttributeUsage(AttributeTargets.Field)]
    public class DynamicList : ConfigAttribute
    {
        public string friendlyName;
        public string[] options;


        public DynamicList(string friendlyName, params string[] options)
        {
            this.friendlyName = friendlyName;
            this.options = options;

        }
    }
    #endregion
}
