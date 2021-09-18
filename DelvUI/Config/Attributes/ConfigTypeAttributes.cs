using System;

namespace DelvUI.Config.Attributes
{
    [AttributeUsage(AttributeTargets.Field)]
    public class ConfigAttribute : Attribute
    {
        public bool isMonitored = false;
    }

    [AttributeUsage(AttributeTargets.Field)]
    public class CheckboxAttribute : ConfigAttribute
    {
        public string friendlyName;
        public bool separator;

        public CheckboxAttribute(string friendlyName)
        {
            this.friendlyName = friendlyName;
            separator = false;
        }
    }

    [AttributeUsage(AttributeTargets.Field)]
    public class DragFloatAttribute : ConfigAttribute
    {
        public string friendlyName;
        public float min;
        public float max;
        public float velocity;
        public bool separator;


        public DragFloatAttribute(string friendlyName)
        {
            this.friendlyName = friendlyName;
            min = 1f;
            max = 1000f;
            velocity = 1f;
            separator = false;

        }
    }

    [AttributeUsage(AttributeTargets.Field)]
    public class DragIntAttribute : ConfigAttribute
    {
        public string friendlyName;
        public int min;
        public int max;
        public int velocity;
        public bool separator;

        public DragIntAttribute(string friendlyName)
        {
            this.friendlyName = friendlyName;
            min = 1;
            max = 1000;
            velocity = 1;
            separator = false;
        }
    }

    [AttributeUsage(AttributeTargets.Field)]
    public class DragFloat2Attribute : ConfigAttribute
    {
        public string friendlyName;
        public float min;
        public float max;
        public float velocity;
        public bool separator;

        public DragFloat2Attribute(string friendlyName)
        {
            this.friendlyName = friendlyName;
            min = 1f;
            max = 1000f;
            velocity = 1f;
            separator = false;
        }
    }

    [AttributeUsage(AttributeTargets.Field)]
    public class DragInt2Attribute : ConfigAttribute
    {
        public string friendlyName;
        public int min;
        public int max;
        public int velocity;
        public bool separator;

        public DragInt2Attribute(string friendlyName)
        {
            this.friendlyName = friendlyName;
            min = 1;
            max = 1000;
            velocity = 1;
            separator = false;
        }
    }

    [AttributeUsage(AttributeTargets.Field)]
    public class InputTextAttribute : ConfigAttribute
    {
        public string friendlyName;
        public uint maxLength;
        public bool separator;


        public InputTextAttribute(string friendlyName)
        {
            this.friendlyName = friendlyName;
            maxLength = 999;
            separator = false;

        }
    }

    [AttributeUsage(AttributeTargets.Field)]
    public class ColorEdit4Attribute : ConfigAttribute
    {
        public string friendlyName;
        public bool separator;


        public ColorEdit4Attribute(string friendlyName)
        {
            this.friendlyName = friendlyName;
            separator = false;

        }
    }

    [AttributeUsage(AttributeTargets.Field)]
    public class ComboAttribute : ConfigAttribute
    {
        public string friendlyName;
        public string[] options;
        public bool separator;


        public ComboAttribute(string friendlyName, params string[] options)
        {
            this.friendlyName = friendlyName;
            this.options = options;
            separator = false;

        }
    }

    [AttributeUsage(AttributeTargets.Field)]
    public class OrderAttribute : Attribute
    {
        public int pos;
        public bool separator;

        public OrderAttribute(int pos)
        {
            this.pos = pos;
            separator = false;

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
    public class DragDropHorizontalAttribute : Attribute
    {
        public string friendlyName;
        public string[] names;
        public bool separator;


        public DragDropHorizontalAttribute(string friendlyName, params string[] names)
        {
            this.friendlyName = friendlyName;
            this.names = names;
            separator = false;

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
    {
    }

    [AttributeUsage(AttributeTargets.Field)]
    public class NestedConfigAttribute : Attribute
    {
        public string friendlyName;
        public int pos;
        public bool separator;

        public NestedConfigAttribute(string friendlyName, int pos)
        {
            this.friendlyName = friendlyName;
            this.pos = pos;
            separator = false;

        }
    }

    [AttributeUsage(AttributeTargets.Field)]
    public class DynamicList : ConfigAttribute
    {
        public string friendlyName;
        public string[] options;
        public bool separator;


        public DynamicList(string friendlyName, params string[] options)
        {
            this.friendlyName = friendlyName;
            this.options = options;
            separator = false;

        }
    }    

}
