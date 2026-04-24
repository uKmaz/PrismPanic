using System.Collections.Generic;
using UnityEngine;
using PrismPanic.Enemies;

namespace PrismPanic.Light
{
    /// <summary>
    /// Static registry of illuminated angels. Populated by BeamCaster each frame,
    /// queried by AngelController to decide freeze behavior.
    /// </summary>
    public static class AngelIlluminationRegistry
    {
        private static readonly HashSet<AngelController> _illuminated = new HashSet<AngelController>();

        public static void Clear()
        {
            _illuminated.Clear();
        }

        public static void Register(AngelController angel)
        {
            _illuminated.Add(angel);
        }

        public static bool IsIlluminated(AngelController angel)
        {
            return _illuminated.Contains(angel);
        }

        public static int Count => _illuminated.Count;
    }
}
