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
        public const int MAX_BOUNCES = 10;
        public const float DEFAULT_BEAM_RANGE = 20f;
        public const float BEAM_WIDTH = 0.06f;
        public const float BASE_BEAM_RADIUS = 0.05f; // SphereCast radius — grows with BeamRadiusWidener upgrade

        // --- Player Defaults ---
        public const float BASE_MOVE_SPEED = 5f;
        public const float BASE_STUN_DURATION = 3f;
        public const float BASE_WIDE_ANGLE = 75f;
        public const float BASE_WIDE_INTENSITY = 20f;
        public const float BASE_LASER_INTENSITY = 50f;
        public const float BASE_LASER_ANGLE = 10f;
        public const float BASE_BEAM_RANGE = 999f; // Infinite range
        public const float BEAM_GROWTH_SPEED = 8f; // Units per second
        public const float BEAM_START_LENGTH = 0f;
        public const float BASE_MAX_ENERGY = 100f;
        public const float ENERGY_REGEN_RATE = 20f; // per sec
        public const float ENERGY_DRAIN_WIDE = 10f; // per sec
        public const float ENERGY_DRAIN_LASER = 30f; // per sec
        public const float ENERGY_OVERHEAT_THRESHOLD = 0.70f; // 70% before usable again

        // --- Angel Defaults ---
        public const float ANGEL_BASE_SPEED = 1.5f;
        public const int ANGEL_BASE_HP = 100;

        // --- Pool Sizes ---
        public const int POOL_ANGELS = 8;
        public const int POOL_SHADOW_ANGELS = 4;
        public const int POOL_BEAM_SEGMENTS = 33;
        public const int POOL_MIRRORS = 12;
        public const int POOL_PILLARS = 8;
        public const int POOL_DOORS = 3;
        public const int POOL_WALLS = 100;
        public const int POOL_FLOORS = 550;

        // --- Gameplay ---
        public const int DOORS_PER_LEVEL = 3;
        public const float ROOM_TRANSITION_DELAY = 0.5f;
        public const float DOOR_INTERACT_RADIUS = 1.5f;   // Distance to show prompt
        public const float DOOR_CONFIRM_RADIUS = 1.0f;    // Distance to accept F key press

        // --- Door Icon Transform ---
        public const float DOOR_ICON_Z_OFFSET = -0.1f;  // How far in front of the door sprite the icon renders

        // --- Angel Darkness Visibility ---
        public const float ANGEL_HIDDEN_ALPHA    = 0f;    // Alpha when not illuminated
        public const float ANGEL_VISIBLE_ALPHA   = 1f;    // Alpha when illuminated by flashlight
        public const float ANGEL_FADE_IN_SPEED   = 8f;    // How fast angel appears when lit
        public const float ANGEL_FADE_OUT_SPEED  = 3f;    // How fast angel disappears in darkness

        // --- Adrenaline Effect ---
        public const float ADRENALINE_TRIGGER_RADIUS = 2.5f;
        public const float ADRENALINE_FADE_IN_SPEED = 5.0f;
        public const float ADRENALINE_FADE_OUT_SPEED = 3.0f;
        public const float ADRENALINE_PUMP_SPEED = 8.0f;
        public const float ADRENALINE_PUMP_MAGNITUDE = 0.2f;
        public const float SHAKE_BASE_INTENSITY = 0.05f;
        public const float SHAKE_PUMP_INTENSITY = 0.15f;
        public const float IMPACT_SHAKE_MAGNITUDE = 2.0f;
        public const float IMPACT_SHAKE_DECAY = 5.0f;
        public const float MAX_IMPACT_TRAUMA = 2.0f;

        // --- Beam Visuals ---
        public const float SPIRAL_RADIUS_BLUE_MULTIPLIER = 0.7f;
        public const float SPIRAL_RADIUS_RED_MULTIPLIER = 1.0f;
        public const float SPIRAL_RADIUS_PURPLE_MULTIPLIER = 1.8f;
        public const float SPIRAL_WIDTH_MULTIPLIER = 0.4f;
        public const float SPIRAL_FREQUENCY = 1.5f;
        public const float ROTATION_SPEED_BLUE = 300f;
        public const float ROTATION_SPEED_RED = 600f;
        public const float ROTATION_SPEED_PURPLE = 1500f;
        public const float BEAM_PARTICLE_SIZE = 0.05f;
        public const float BEAM_LIGHT_RADIUS = 8f;
        public const float BEAM_LIGHT_INTENSITY = 0.05f;

        // --- Boss Fight ---
        public const float BOSS_ROTATION_SPEED = 30f;            // Degrees per second
        public const float BOSS_LASER_RANGE = 50f;
        public const int BOSS_LASER_DAMAGE = 1;
        public const int BOSS_MAX_HP = 500;
        public const float BOSS_LASER_WIDTH = 0.1f;
        public const float BOSS_SHUFFLE_INTERVAL = 10f;          // Seconds between pillar/mirror reshuffle
        public const float BOSS_SHUFFLE_MOVE_SPEED = 3f;         // How fast objects slide to new positions
        public const float BOSS_LASER_STRIKE_DURATION = 15f;      // How long the laser fires (seconds)
        public const float BOSS_LASER_COOLDOWN = 3f;             // Pause between laser strikes
        public const float BOSS_RAY_ESCALATION_INTERVAL = 2f;   // Every 30 sec, boss gets +1 ray
        public const int BOSS_MAX_RAY_COUNT = 4;                 // Maximum number of simultaneous rays
        public const float BOSS_MIN_DISTANCE_FROM_BOSS = 3f;    // Objects can't be closer than this to boss
        public const float BOSS_MIN_DISTANCE_BETWEEN_OBJECTS = 0.5f; // Objects can't be closer than this to each other
    }
}
