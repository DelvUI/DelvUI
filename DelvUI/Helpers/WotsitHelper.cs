using Dalamud.Plugin.Ipc;
using DelvUI.Config;
using DelvUI.Config.Tree;
using System;
using System.Collections.Generic;

namespace DelvUI.Helpers
{
    internal class WotsitHelper
    {
        private readonly ICallGateSubscriber<string, string, string, uint, string> _registerWithSearch;
        private readonly ICallGateSubscriber<string, bool> _invoke;
        private readonly ICallGateSubscriber<string, bool> _unregisterAll;

        private Dictionary<string, (SectionNode, SubSectionNode?, NestedSubSectionNode?)> _map = new Dictionary<string, (SectionNode, SubSectionNode?, NestedSubSectionNode?)>();

        #region Singleton
        private WotsitHelper()
        {
            _registerWithSearch = Plugin.PluginInterface.GetIpcSubscriber<string, string, string, uint, string>("FA.RegisterWithSearch");
            _unregisterAll = Plugin.PluginInterface.GetIpcSubscriber<string, bool>("FA.UnregisterAll");

            _invoke = Plugin.PluginInterface.GetIpcSubscriber<string, bool>("FA.Invoke");
            _invoke.Subscribe(Invoke);
        }

        public static void Initialize() { Instance = new WotsitHelper(); }

        public static WotsitHelper Instance { get; private set; } = null!;

        ~WotsitHelper()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);

            UnregisterAll();
        }

        protected void Dispose(bool disposing)
        {
            if (!disposing)
            {
                return;
            }

            Instance = null!;
        }
        #endregion

        public void Update()
        {
            _map.Clear();
            UnregisterAll();

            // sections
            foreach (Node node in ConfigurationManager.Instance.ConfigBaseNode.Sections)
            {
                if (node is not SectionNode section) { continue; }

                string guid = _registerWithSearch.InvokeFunc(
                    Plugin.PluginInterface.InternalName,
                    "DelvUI Settings: " + section.Name,
                    "DelvUI " + section.Name,
                    66472
                );

                _map.Add(guid, (section, null, null));

                // sub sections
                foreach (SubSectionNode subSection in section.Children)
                {
                    guid = _registerWithSearch.InvokeFunc(
                        Plugin.PluginInterface.InternalName,
                        "DelvUI Settings: " + section.Name + " > " + subSection.Name,
                        "DelvUI " + subSection.Name,
                        66472
                    );

                    _map.Add(guid, (section, subSection, null));

                    // nested sub sections
                    foreach (SubSectionNode nestedSubSection in subSection.Children)
                    {
                        if (nestedSubSection is not NestedSubSectionNode nestedNode) { continue; }

                        guid = _registerWithSearch.InvokeFunc(
                            Plugin.PluginInterface.InternalName,
                            "DelvUI Settings: " + section.Name + " > " + subSection.Name + " > " + nestedNode.Name,
                            "DelvUI " + nestedNode.Name,
                            66472
                        );

                        _map.Add(guid, (section, subSection, nestedNode));
                    }
                }
            }
        }

        public void Invoke(string guid)
        {
            //_map.TryGetValue()
            if (_map.TryGetValue(guid, out var value) && value.Item1 != null)
            {
                SectionNode section = value.Item1;
                ConfigurationManager.Instance.ConfigBaseNode.SelectedOptionName = section.Name;
                ConfigurationManager.Instance.ConfigBaseNode.RefreshSelectedNode();

                SubSectionNode? subSectionNode = value.Item2;
                if (subSectionNode != null)
                {
                    section.ForceSelectedTabName = subSectionNode.Name;

                    NestedSubSectionNode? nestedSubSectionNode = value.Item3;
                    if (nestedSubSectionNode != null)
                    {
                        subSectionNode.ForceSelectedTabName = nestedSubSectionNode.Name;
                    }
                }

                ConfigurationManager.Instance.OpenConfigWindow();
            }
        }

        public void UnregisterAll()
        {
            _unregisterAll.InvokeFunc(Plugin.PluginInterface.InternalName);
        }
    }
}
