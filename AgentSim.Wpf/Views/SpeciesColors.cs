using System.Collections.Generic;
using System.Windows.Media;

namespace AgentSim.Wpf.Views
{
    // One place that decides which colour each species gets, so the world
    // canvas, the live plot and the Monte Carlo charts always agree.
    //
    // Species are handed colours in the order they are first seen, so two
    // species can never share a colour (a hash would sometimes collide), and
    // the same names get the same colours on every launch because the world
    // is always drawn in the same order.
    internal static class SpeciesColors
    {
        private static readonly Color[] Palette =
        {
            Color.FromRgb(0x4F, 0xC3, 0xF7),   // cyan
            Color.FromRgb(0xFF, 0x6B, 0x6B),   // coral
            Color.FromRgb(0xB3, 0x88, 0xFF),   // violet
            Color.FromRgb(0xFF, 0x9F, 0x43),   // orange
            Color.FromRgb(0xF7, 0x8F, 0xB3),   // pink
            Color.FromRgb(0x7E, 0xE0, 0xC3)    // mint
        };

        private static readonly Dictionary<string, int> Order = new();
        private static readonly Dictionary<int, SolidColorBrush> Brushes = new();
        private static readonly object Gate = new();

        // Agents that have no species set: plain agents and scripted agents.
        public static Brush DefaultBrush => BrushAt(0);
        public static Brush ScriptedBrush => BrushAt(1);

        public static Brush BrushFor(string? species) => BrushAt(IndexFor(species));

        public static Color ColorFor(string? species) => Palette[IndexFor(species) % Palette.Length];

        private static int IndexFor(string? species)
        {
            string key = species ?? "";

            // Agents with no species set share the first colour and don't
            // take a slot from the real species.
            if (key == "Default") return 0;

            lock (Gate)
            {
                if (!Order.TryGetValue(key, out int index))
                {
                    index = Order.Count;
                    Order[key] = index;
                }
                return index;
            }
        }

        private static Brush BrushAt(int index)
        {
            int slot = index % Palette.Length;
            lock (Gate)
            {
                if (!Brushes.TryGetValue(slot, out var brush))
                {
                    brush = new SolidColorBrush(Palette[slot]);
                    brush.Freeze();
                    Brushes[slot] = brush;
                }
                return brush;
            }
        }
    }
}

