using System;

namespace DelvUI.Config.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    public class SectionAttribute : Attribute
    {
        public string SectionName;
        public bool ForceAllowExport;

        public SectionAttribute(string name, bool forceAllowExport = false)
        {
            SectionName = name;
            ForceAllowExport = forceAllowExport;
        }
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
