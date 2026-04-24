# Adrenaline Pump Feature Implementation

This document provides the exact technical steps to implement the "Adrenaline Pump" effect when an Angel gets dangerously close to the player. It includes a dark screen, red pumping edges, and a slight camera shake.

Per project rules, **no magic numbers** are used. All values are centralized in `Constants.cs`.

---

## 1. Constants Update (`Core/Constants.cs`)

Add these constants to your `Constants.cs` file. This centralizes all tuning for the effect.

```csharp
public static class Constants
{
    // ... your existing constants ...

    [Header("Adrenaline Effect")]
    public const float ADRENALINE_TRIGGER_RADIUS = 2.5f;
    public const float ADRENALINE_FADE_IN_SPEED = 5.0f;
    public const float ADRENALINE_FADE_OUT_SPEED = 3.0f;
    
    // Pulse (Heartbeat) Math
    public const float ADRENALINE_PUMP_SPEED = 8.0f;       // How fast the heartbeat pulses
    public const float ADRENALINE_PUMP_MAGNITUDE = 0.2f;   // How much the volume weight fluctuates

    [Header("Camera Shake")]
    public const float SHAKE_BASE_INTENSITY = 0.05f;       // Subtle constant shake
    public const float SHAKE_PUMP_INTENSITY = 0.15f;       // Stronger shake on the "heartbeat"
}
```

---

## 2. EventBus Addition (`Core/EventBus.cs`)

Add a new event to broadcast when the player enters or exits the danger zone.

```csharp
// --- Polish / Juice ---
// Passes 'true' when an active, pursuing Angel is close, 'false' when safe
public static event Action<bool> OnAdrenalineStateChanged; 
```

---

## 3. Danger Detection (`Player/DangerDetector.cs`)

Attach this script to the **Player GameObject**.
1. Add a `SphereCollider` to the Player.
2. Set `Is Trigger` to **true**.
3. Set the `Radius` to `Constants.ADRENALINE_TRIGGER_RADIUS`.

```csharp
using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(SphereCollider))]
public class DangerDetector : MonoBehaviour
{
    private HashSet<AngelController> _nearbyAngels = new HashSet<AngelController>();
    private bool _isAdrenalineActive = false;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy"))
        {
            AngelController angel = other.GetComponent<AngelController>();
            if (angel != null)
            {
                _nearbyAngels.Add(angel);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Enemy"))
        {
            AngelController angel = other.GetComponent<AngelController>();
            if (angel != null)
            {
                _nearbyAngels.Remove(angel);
            }
        }
    }

    private void Update()
    {
        bool hasThreat = false;
        
        // Only trigger if an angel is close AND is actively pursuing (not stunned/dead)
        foreach (var angel in _nearbyAngels)
        {
            if (angel.CurrentState == AngelState.Pursuing)
            {
                hasThreat = true;
                break;
            }
        }

        if (hasThreat != _isAdrenalineActive)
        {
            _isAdrenalineActive = hasThreat;
            EventBus.OnAdrenalineStateChanged?.Invoke(_isAdrenalineActive);
        }
    }
}
```

---

## 4. URP Volume Setup (The Visuals)

1. Create an empty GameObject in your scene named `Adrenaline_Volume`.
2. Add a **Volume** component. Set `Mode` to **Global**.
3. Set `Weight` to **0**.
4. Create a new Volume Profile and add these overrides:
   - **Vignette:** 
     - Color: Dark Red (`#5A0000` or similar)
     - Intensity: `0.5`
     - Smoothness: `0.8`
   - **Color Adjustments:** 
     - Post Exposure: `-1.5` (This makes the screen get darker)
   - **Chromatic Aberration:**
     - Intensity: `1.0` (Distorts the edges of the screen)

---

## 5. Adrenaline Controller (`UI/AdrenalineController.cs`)

Attach this script to the `Adrenaline_Volume` GameObject. It handles the pulsating visuals and the camera shake.

```csharp
using UnityEngine;
using UnityEngine.Rendering;

[RequireComponent(typeof(Volume))]
public class AdrenalineController : MonoBehaviour
{
    private Volume _adrenalineVolume;
    private bool _isActive = false;
    private float _targetWeight = 0f;
    private float _currentWeight = 0f;
    
    private Vector3 _originalCameraPos;
    private Transform _mainCameraTransform;

    private void Awake()
    {
        _adrenalineVolume = GetComponent<Volume>();
        _adrenalineVolume.weight = 0f;
        
        _mainCameraTransform = Camera.main.transform;
        _originalCameraPos = _mainCameraTransform.localPosition;
    }

    private void OnEnable()
    {
        EventBus.OnAdrenalineStateChanged += HandleAdrenalineState;
    }

    private void OnDisable()
    {
        EventBus.OnAdrenalineStateChanged -= HandleAdrenalineState;
    }

    private void HandleAdrenalineState(bool isActive)
    {
        _isActive = isActive;
        if (!_isActive)
        {
            // Reset camera position when safe
            _mainCameraTransform.localPosition = _originalCameraPos;
        }
    }

    private void Update()
    {
        // Calculate the base target weight based on active state
        float baseTarget = _isActive ? 1f : 0f;
        
        // Lerp towards the base target
        float speed = _isActive ? Constants.ADRENALINE_FADE_IN_SPEED : Constants.ADRENALINE_FADE_OUT_SPEED;
        _targetWeight = Mathf.Lerp(_targetWeight, baseTarget, Time.deltaTime * speed);

        // Apply a sine wave "pump" effect if active
        float pumpOffset = 0f;
        if (_isActive)
        {
            // Creates a pulsating heartbeat math wave (0 to 1)
            float sineWave = (Mathf.Sin(Time.time * Constants.ADRENALINE_PUMP_SPEED) + 1f) / 2f;
            pumpOffset = sineWave * Constants.ADRENALINE_PUMP_MAGNITUDE;
            
            ApplyCameraShake(sineWave);
        }

        // Apply final weight (clamped between 0 and 1)
        _currentWeight = Mathf.Clamp01(_targetWeight + pumpOffset);
        _adrenalineVolume.weight = _currentWeight;
    }

    private void ApplyCameraShake(float pumpSineWave)
    {
        // Base low-frequency shake
        Vector2 randomCircle = Random.insideUnitCircle * Constants.SHAKE_BASE_INTENSITY;
        
        // High-frequency shake when the heartbeat "pumps" (when sine wave is near 1)
        if (pumpSineWave > 0.8f) 
        {
            randomCircle += Random.insideUnitCircle * Constants.SHAKE_PUMP_INTENSITY;
        }

        Vector3 newPos = _originalCameraPos + new Vector3(randomCircle.x, randomCircle.y, 0f);
        
        // Smooth out the shake
        _mainCameraTransform.localPosition = Vector3.Lerp(_mainCameraTransform.localPosition, newPos, Time.deltaTime * 30f);
    }
}
```
