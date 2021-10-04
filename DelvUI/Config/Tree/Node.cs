using System.Collections.Generic;

namespace DelvUI.Config.Tree
{
    public abstract class Node
    {
        protected List<Node> _children = new List<Node>();

        public virtual void Save(string path)
        {
            foreach (Node child in _children)
            {
                child.Save(path);
            }
        }

        public virtual void Load(string path)
        {
            foreach (Node child in _children)
            {
                child.Load(path);
            }
        }

        public virtual string GetBase64String()
        {
            if (_children == null)
            {
                return "";
            }

            string base64String = "";

            foreach (Node child in _children)
            {
                string childString = child.GetBase64String();

                if (childString != "")
                {
                    base64String += "|" + childString;
                }
            }

            return base64String;
        }

        public virtual void LoadBase64String(string[] importStrings)
        {
            if (_children == null)
            {
                return;
            }

            foreach (Node child in _children)
            {
                child.LoadBase64String(importStrings);
            }
        }
    }
}
