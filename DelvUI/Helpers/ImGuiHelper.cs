using DelvUI.Config;
using DelvUI.Config.Tree;
using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace DelvUI.Helpers
{
    public static class ImGuiHelper
    {
        public static void DrawSeparator(int topSpacing, int bottomSpacing)
        {
            DrawSpacing(topSpacing);
            ImGui.Separator();
            DrawSpacing(bottomSpacing);
        }

        public static void DrawSpacing(int spacingSize)
        {
            for (int i = 0; i < spacingSize; i++)
            {
                ImGui.NewLine();
            }
        }

        public static void NewLineAndTab()
        {
            ImGui.NewLine();
            Tab();
        }

        public static void Tab()
        {
            ImGui.Text("\u2002");
            ImGui.SameLine();
        }

        public static Node? DrawExportResetContextMenu(Node node, bool canExport, bool canReset)
        {
            Node? nodeToReset = null;

            if (ImGui.BeginPopupContextItem())
            {
                if (canExport && ImGui.Selectable("Export"))
                {
                    var exportString = node.GetBase64String();
                    ImGui.SetClipboardText(exportString ?? "");
                }

                if (canReset && ImGui.Selectable("Reset"))
                {
                    ImGui.CloseCurrentPopup();
                    nodeToReset = node;
                }

                ImGui.EndPopup();
            }

            return nodeToReset;
        }

        public static (bool, bool) DrawConfirmationModal(string title, string message)
        {
            return DrawConfirmationModal(title, new string[] { message });
        }

        public static (bool, bool) DrawConfirmationModal(string title, IEnumerable<string> textLines)
        {
            ConfigurationManager.Instance.ShowingModalWindow = true;

            bool didConfirm = false;
            bool didClose = false;

            ImGui.OpenPopup(title);

            Vector2 center = ImGui.GetMainViewport().GetCenter();
            ImGui.SetNextWindowPos(center, ImGuiCond.Appearing, new Vector2(0.5f, 0.5f));

            bool p_open = true; // i've no idea what this is used for

            if (ImGui.BeginPopupModal(title, ref p_open, ImGuiWindowFlags.AlwaysAutoResize | ImGuiWindowFlags.NoMove))
            {
                float width = 300;
                float height = Math.Min((ImGui.CalcTextSize(" ").Y + 5) * textLines.Count(), 240);

                ImGui.BeginChild("confirmation_modal_message", new Vector2(width, height), false);
                foreach (string text in textLines)
                {
                    ImGui.Text(text);
                }
                ImGui.EndChild();

                ImGui.NewLine();

                if (ImGui.Button("OK", new Vector2(width / 2f - 5, 24)))
                {
                    ImGui.CloseCurrentPopup();
                    didConfirm = true;
                    didClose = true;
                }

                ImGui.SetItemDefaultFocus();
                ImGui.SameLine();
                if (ImGui.Button("Cancel", new Vector2(width / 2f - 5, 24)))
                {
                    ImGui.CloseCurrentPopup();
                    didClose = true;
                }

                ImGui.EndPopup();
            }
            // close button on nav
            else
            {
                didClose = true;
            }

            if (didClose)
            {
                ConfigurationManager.Instance.ShowingModalWindow = false;
            }

            return (didConfirm, didClose);
        }

        public static bool DrawErrorModal(string message)
        {
            ConfigurationManager.Instance.ShowingModalWindow = true;

            bool didClose = false;
            ImGui.OpenPopup("Error");

            Vector2 center = ImGui.GetMainViewport().GetCenter();
            ImGui.SetNextWindowPos(center, ImGuiCond.Appearing, new Vector2(0.5f, 0.5f));

            bool p_open = true; // i've no idea what this is used for
            if (ImGui.BeginPopupModal("Error", ref p_open, ImGuiWindowFlags.AlwaysAutoResize | ImGuiWindowFlags.NoMove))
            {
                ImGui.Text(message);
                ImGui.NewLine();

                var textSize = ImGui.CalcTextSize(message).X;

                if (ImGui.Button("OK", new Vector2(textSize, 24)))
                {
                    ImGui.CloseCurrentPopup();
                    didClose = true;
                }

                ImGui.EndPopup();
            }
            // close button on nav
            else
            {
                didClose = true;
            }

            if (didClose)
            {
                ConfigurationManager.Instance.ShowingModalWindow = false;
            }

            return didClose;
        }

        public static (bool, bool) DrawInputModal(string title, string message, ref string value)
        {
            ConfigurationManager.Instance.ShowingModalWindow = true;

            bool didConfirm = false;
            bool didClose = false;

            ImGui.OpenPopup(title);

            Vector2 center = ImGui.GetMainViewport().GetCenter();
            ImGui.SetNextWindowPos(center, ImGuiCond.Appearing, new Vector2(0.5f, 0.5f));

            bool p_open = true; // i've no idea what this is used for

            if (ImGui.BeginPopupModal(title, ref p_open, ImGuiWindowFlags.AlwaysAutoResize | ImGuiWindowFlags.NoMove))
            {
                var textSize = ImGui.CalcTextSize(message).X;

                ImGui.Text(message);

                ImGui.PushItemWidth(textSize);
                ImGui.InputText("", ref value, 64);

                ImGui.NewLine();
                if (ImGui.Button("OK", new Vector2(textSize / 2f - 5, 24)))
                {
                    ImGui.CloseCurrentPopup();
                    didConfirm = true;
                    didClose = true;
                }

                ImGui.SetItemDefaultFocus();
                ImGui.SameLine();
                if (ImGui.Button("Cancel", new Vector2(textSize / 2f - 5, 24)))
                {
                    ImGui.CloseCurrentPopup();
                    didClose = true;
                }

                ImGui.EndPopup();
            }
            // close button on nav
            else
            {
                didClose = true;
            }

            if (didClose)
            {
                ConfigurationManager.Instance.ShowingModalWindow = false;
            }

            return (didConfirm, didClose);
        }

        public static string? DrawTextTagsList(string name, ref string searchText)
        {
            string? selectedTag = null;

            ImGui.SetNextWindowSize(new(200, 300));

            if (ImGui.BeginPopup(name))
            {
                // search
                ImGui.InputText("", ref searchText, 64);

                if (!ImGui.IsAnyItemActive() && !ImGui.IsMouseClicked(ImGuiMouseButton.Left))
                {
                    ImGui.SetKeyboardFocusHere(0);
                }

                foreach (string key in TextTagsHelper.TextTags.Keys)
                {
                    if (searchText.Length > 0 && !key.Contains(searchText))
                    {
                        continue;
                    }

                    // tag
                    if (ImGui.Selectable(key))
                    {
                        selectedTag = key;
                        searchText = "";
                    }

                    // help tooltip
                    if (ImGui.IsItemHovered())
                    {
                        TextTag textTag = TextTagsHelper.TextTags[key];
                        string message = textTag.ExplicitHelpText != null ? textTag.ExplicitHelpText + "\n" : "";

                        if (textTag.ExampleValues != null && textTag.ExampleValues.Length > 0)
                        {
                            message += "(";

                            int count = textTag.ExampleValues.Length;
                            for (int i = 0; i < count; i++)
                            {
                                object value = textTag.ExampleValues[i];

                                message += i == count - 1 ? value.ToString() : value.ToString() + ", ";
                            }

                            message += ") => " + textTag.Execute(null, textTag.ExampleValues);

                        }

                        ImGui.SetTooltip(message);
                    }
                }

                ImGui.EndPopup();
            }

            return selectedTag;
        }
    }
}
