using DelvUI.Helpers;
using System.Collections.Generic;

namespace DelvUI.Config.Tree
{
    public abstract class Node
    {
        protected List<Node> _children = new List<Node>();

        #region reset
        protected Node? _nodeToReset = null;
        protected string? _nodeToResetName = null;

        protected void DrawExportResetContextMenu(Node node, string name)
        {
            if (_nodeToReset != null)
            {
                return;
            }

            _nodeToReset = ImGuiHelper.DrawExportResetContextMenu(node, node.AllowExport());
            _nodeToResetName = name;
        }

        protected virtual bool AllowExport()
        {
            foreach (Node child in _children)
            {
                if (child.AllowExport())
                {
                    return true;
                }
            }

            return false;
        }

        protected bool DrawResetModal()
        {
            if (_nodeToReset == null || _nodeToResetName == null)
            {
                return false;
            }

            string[] lines = new string[] { "Are you sure you want to reset \"" + _nodeToResetName + "\"?" };
            var (didReset, didClose) = ImGuiHelper.DrawConfirmationModal("Reset?", lines);

            if (didReset)
            {
                _nodeToReset.Reset();
                _nodeToReset = null;
            }
            else if (didClose)
            {
                _nodeToReset = null;
            }

            return didReset;
        }


        public virtual void Reset()
        {
            foreach (Node child in _children)
            {
                child.Reset();
            }
        }
        #endregion

        #region save and load
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
        #endregion

        #region export
        public virtual string? GetBase64String()
        {
            if (_children == null)
            {
                return "";
            }

            string base64String = "";

            foreach (Node child in _children)
            {
                string? childString = child.GetBase64String();

                if (childString != null && childString.Length > 0)
                {
                    base64String += "|" + childString;
                }
            }

            return base64String;
        }
        #endregion
    }
}
