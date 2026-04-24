using UnityEngine;
using UnityEngine.Rendering;
using PrismPanic.Core;

[RequireComponent(typeof(Volume))]
public class AdrenalineController : MonoBehaviour
{
    private Volume _adrenalineVolume;
    private bool _isActive = false;
    private float _targetWeight = 0f;
    private float _currentWeight = 0f;
    
    private Transform _mainCameraTransform;
    private Vector3 _currentShakeOffset = Vector3.zero;
    private Vector3 _targetShakeOffset = Vector3.zero;

    private void Awake()
    {
        _adrenalineVolume = GetComponent<Volume>();
        _adrenalineVolume.weight = 0f;
        
        if (Camera.main != null)
        {
            _mainCameraTransform = Camera.main.transform;
        }
    }

    private void OnEnable()
    {
        EventBus.OnAdrenalineStateChanged += HandleAdrenalineState;
        
        // Subscribe to URP rendering callbacks for non-destructive camera shake
        RenderPipelineManager.beginCameraRendering += OnBeginCameraRendering;
        RenderPipelineManager.endCameraRendering += OnEndCameraRendering;
    }

    private void OnDisable()
    {
        EventBus.OnAdrenalineStateChanged -= HandleAdrenalineState;
        
        RenderPipelineManager.beginCameraRendering -= OnBeginCameraRendering;
        RenderPipelineManager.endCameraRendering -= OnEndCameraRendering;
    }

    private void HandleAdrenalineState(bool isActive)
    {
        _isActive = isActive;
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
            float sineWave = (Mathf.Sin(Time.time * Constants.ADRENALINE_PUMP_SPEED) + 1f) / 2f;
            pumpOffset = sineWave * Constants.ADRENALINE_PUMP_MAGNITUDE;
            
            // Calculate shake target
            Vector2 randomCircle = Random.insideUnitCircle * Constants.SHAKE_BASE_INTENSITY;
            if (sineWave > 0.8f) 
            {
                randomCircle += Random.insideUnitCircle * Constants.SHAKE_PUMP_INTENSITY;
            }
            _targetShakeOffset = new Vector3(randomCircle.x, randomCircle.y, 0f);
        }
        else
        {
            _targetShakeOffset = Vector3.zero;
        }

        // Smooth the shake offset
        _currentShakeOffset = Vector3.Lerp(_currentShakeOffset, _targetShakeOffset, Time.deltaTime * 30f);

        // Apply final weight (clamped between 0 and 1)
        _currentWeight = Mathf.Clamp01(_targetWeight + pumpOffset);
        _adrenalineVolume.weight = _currentWeight;
    }

    // --- URP RENDER HOOKS ---
    // By shaking the camera strictly during the render phase, we do not interfere 
    // with any CameraController scripts, parenting, or physics logic!

    private void OnBeginCameraRendering(ScriptableRenderContext context, Camera camera)
    {
        if (camera == Camera.main && _mainCameraTransform != null)
        {
            _mainCameraTransform.position += _currentShakeOffset;
        }
    }

    private void OnEndCameraRendering(ScriptableRenderContext context, Camera camera)
    {
        if (camera == Camera.main && _mainCameraTransform != null)
        {
            _mainCameraTransform.position -= _currentShakeOffset;
        }
    }
}
