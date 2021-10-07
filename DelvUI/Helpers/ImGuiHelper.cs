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

        public static Node? DrawExportResetContextMenu(Node node, bool canExport)
        {
            Node? nodeToReset = null;

            if (ImGui.BeginPopupContextItem())
            {
                if (canExport && ImGui.Selectable("Export"))
                {
                    var exportString = node.GetBase64String();
                    ImGui.SetClipboardText(exportString ?? "");
                }

                if (ImGui.Selectable("Reset"))
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
            bool didConfirm = false;
            bool didClose = false;

            ImGui.OpenPopup(title);

            Vector2 center = ImGui.GetMainViewport().GetCenter();
            ImGui.SetNextWindowPos(center, ImGuiCond.Appearing, new Vector2(0.5f, 0.5f));

            bool p_open = true; // i've no idea what this is used for

            if (ImGui.BeginPopupModal(title, ref p_open, ImGuiWindowFlags.AlwaysAutoResize | ImGuiWindowFlags.NoMove))
            {
                float maxWidth = 0;

                foreach (string text in textLines)
                {
                    ImGui.Text(text);

                    float textWidth = ImGui.CalcTextSize(text).X;
                    maxWidth = Math.Max(maxWidth, textWidth);
                }

                ImGui.NewLine();

                if (ImGui.Button("OK", new Vector2(maxWidth / 2f - 5, 24)))
                {
                    ImGui.CloseCurrentPopup();
                    didConfirm = true;
                    didClose = true;
                }

                ImGui.SetItemDefaultFocus();
                ImGui.SameLine();
                if (ImGui.Button("Cancel", new Vector2(maxWidth / 2f - 5, 24)))
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

            return (didConfirm, didClose);
        }

        public static bool DrawErrorModal(string message)
        {
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

            return didClose;
        }
    }
}
