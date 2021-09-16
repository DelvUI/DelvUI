using System;

namespace DelvUI.Config.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    public class SectionAttribute : Attribute
    {
        public string SectionName;

        public SectionAttribute(string name) { SectionName = name; }
    }

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class SubSectionAttribute : Attribute
    {
        public string SubSectionName;
        public int Depth;

        public SubSectionAttribute(string subSectionName, int depth)
        {
            SubSectionName = subSectionName;
            Depth = depth;
        }
    }
}
