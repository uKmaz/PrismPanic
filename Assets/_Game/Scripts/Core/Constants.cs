using UnityEngine;

namespace PrismPanic.Core
{
    public enum FlashlightMode
    {
        Closed,
        Wide,
        Laser
    }

    /// <summary>
    /// Global constants. No magic numbers in scripts — reference these instead.
    /// </summary>
    public static class Constants
    {
        // --- Physics Layers ---
        public static readonly int LayerPlayer = 6;
        public static readonly int LayerEnemy = 7;
        public static readonly int LayerMirror = 8;
        public static readonly int LayerWall = 9;
        public static readonly int LayerPillar = 10;
        public static readonly int LayerBeam = 11;
        public static readonly int LayerDoor = 12;

        // --- Layer Masks (for raycasting) ---
        public static readonly int BeamRaycastMask =
            (1 << LayerMirror) | (1 << LayerWall) | (1 << LayerPillar) | (1 << LayerEnemy);

        public static readonly int EnemyMovementMask =
            (1 << LayerWall) | (1 << LayerPillar);

        public static readonly int GroundRaycastMask =
            (1 << LayerWall) | (1 << LayerPillar) | (1 << 0); // Default layer for floor

        // --- Beam ---
        public const int MAX_BOUNCES = 3;
        public const float DEFAULT_BEAM_RANGE = 20f;
        public const float BEAM_WIDTH = 0.06f;

        // --- Player Defaults ---
        public const float BASE_MOVE_SPEED = 5f;
        public const float BASE_STUN_DURATION = 3f;
        public const float BASE_WIDE_ANGLE = 75f;
        public const float BASE_WIDE_INTENSITY = 20f;
        public const float BASE_LASER_INTENSITY = 50f;
        public const float BASE_LASER_ANGLE = 10f;
        public const float BASE_BEAM_RANGE = 40f; // Increased to 40 so it can bounce further
        public const float BEAM_GROWTH_SPEED = 8f; // Units per second
        public const float BEAM_START_LENGTH = 0f;
        public const float BASE_MAX_ENERGY = 100f;
        public const float ENERGY_REGEN_RATE = 20f; // per sec
        public const float ENERGY_DRAIN_WIDE = 10f; // per sec
        public const float ENERGY_DRAIN_LASER = 30f; // per sec
        public const float ENERGY_OVERHEAT_THRESHOLD = 0.70f; // 70% before usable again

        // --- Angel Defaults ---
        public const float ANGEL_BASE_SPEED = 1.5f;
        public const int ANGEL_BASE_HP = 2;

        // --- Pool Sizes ---
        public const int POOL_ANGELS = 8;
        public const int POOL_BEAM_SEGMENTS = 6;
        public const int POOL_MIRRORS = 12;
        public const int POOL_PILLARS = 8;
        public const int POOL_DOORS = 3;
        public const int POOL_WALLS = 60;
        public const int POOL_FLOORS = 60;

        // --- Gameplay ---
        public const int DOORS_PER_LEVEL = 3;
        public const float ROOM_TRANSITION_DELAY = 0.5f;

        // --- Adrenaline Effect ---
        public const float ADRENALINE_TRIGGER_RADIUS = 2.5f;
        public const float ADRENALINE_FADE_IN_SPEED = 5.0f;
        public const float ADRENALINE_FADE_OUT_SPEED = 3.0f;
        public const float ADRENALINE_PUMP_SPEED = 8.0f;
        public const float ADRENALINE_PUMP_MAGNITUDE = 0.2f;
        public const float SHAKE_BASE_INTENSITY = 0.05f;
        public const float SHAKE_PUMP_INTENSITY = 0.15f;

        // --- Beam Visuals ---
        public const float SPIRAL_RADIUS_RED_MULTIPLIER = 1.0f;
        public const float SPIRAL_RADIUS_PURPLE_MULTIPLIER = 1.8f;
        public const float SPIRAL_WIDTH_MULTIPLIER = 0.4f;
        public const float SPIRAL_FREQUENCY = 1.5f;
        public const float ROTATION_SPEED_RED = 600f;
        public const float ROTATION_SPEED_PURPLE = 1500f;
        public const float BEAM_PARTICLE_SIZE = 0.05f;
        public const float BEAM_LIGHT_RADIUS = 8f;
        public const float BEAM_LIGHT_INTENSITY = 0.05f;
    }
}
