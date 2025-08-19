#if IMGUI
using System;
using System.Collections.Generic;
using ImGuiNET;

namespace Game.Utils
{
    public static class ImGuiExtensions
    {
        #region STATIC METHODS
        public static bool BeginCollapsingHeader(string name, float indent)
        {
            _indent.Add(indent);

            bool result = ImGui.CollapsingHeader(name);
            if (result)
                ImGui.Indent(indent);
            return result;
        }

        public static void EndCollapsingHeader()
        {
            ImGui.Unindent(_indent[^1]);
            _indent.RemoveAt(_indent.Count - 1);
        }

        public static bool EnumSelector<T>(string name, ref T selection)
        {
            if (selection == null)
                return false;

            bool change = false;
            var enumType = typeof(T);

            if (ImGui.BeginCombo(name, Enum.GetName(enumType, selection)))
            {
                var enumValues = Enum.GetValues(enumType);
                foreach (T item in enumValues)
                {
                    bool selected = item.Equals(selection);
                    if (ImGui.Selectable(Enum.GetName(enumType, item), selected))
                    {
                        selection = item;
                        change = true;
                    }

                    if (selected)
                        ImGui.SetItemDefaultFocus();
                }
                ImGui.EndCombo();
            }

            return change;
        }
        #endregion

        #region STATIC FIELDS
        private static readonly List<float> _indent = [];
        #endregion
    }
}
#endif