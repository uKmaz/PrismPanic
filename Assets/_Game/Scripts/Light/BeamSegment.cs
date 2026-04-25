using UnityEngine;

namespace PrismPanic.Light
{
    /// <summary>
    /// Pooled beam segment with LineRenderer. Dynamically generates particle systems 
    /// and a secondary LineRenderer to create a 3D spiral effect.
    /// </summary>
    [RequireComponent(typeof(LineRenderer))]
    public class BeamSegment : MonoBehaviour
    {
        private LineRenderer _lineRenderer;
        
        // Dynamic effect components
        private GameObject _spiralObj;
        private LineRenderer _spiralRenderer;
        private ParticleSystem _particleSystem;
        private ParticleSystem.MainModule _psMain;
        private ParticleSystem.EmissionModule _psEmission;
        private ParticleSystem.ShapeModule _psShape;

        private System.Collections.Generic.List<UnityEngine.Light> _beamLights = new System.Collections.Generic.List<UnityEngine.Light>();
        private GameObject _lightsContainer;

        private float _rotationSpeed = 0f;
        private float _spiralRadius = 0.1f;
        private float _timeOffset = 0f;
        
        private float _currentDistance = 0f;
        private float _lastGeneratedDistance = -1f;
        private float _accumulatedSpin = 0f;
        private void Awake()
        {
            _timeOffset = Random.Range(0f, 1000f);
            
            _lineRenderer = GetComponent<LineRenderer>();
            _lineRenderer.useWorldSpace = true;
            _lineRenderer.positionCount = 2;
            
            Material baseMat = _lineRenderer.sharedMaterial; // Grab the material assigned in the Inspector

            // 1. Generate Spiral LineRenderer for level 2 & 3
            _spiralObj = new GameObject("BeamSpiral");
            _spiralObj.transform.SetParent(transform);
            
            _spiralRenderer = _spiralObj.AddComponent<LineRenderer>();
            _spiralRenderer.useWorldSpace = false; // Render in local space
            _spiralRenderer.material = baseMat;
            _spiralObj.SetActive(false);

            // 2. Generate Particles for level 1, 2, 3
            GameObject psObj = new GameObject("BeamParticles");
            psObj.transform.SetParent(transform);
            _particleSystem = psObj.AddComponent<ParticleSystem>();
            
            _psMain = _particleSystem.main;
            _psMain.startSize = PrismPanic.Core.Constants.BEAM_PARTICLE_SIZE; 
            _psMain.startLifetime = 0.5f;
            _psMain.startSpeed = 3.0f; 
            _psMain.simulationSpace = ParticleSystemSimulationSpace.World;
            
            _psShape = _particleSystem.shape;
            _psShape.shapeType = ParticleSystemShapeType.Box;
            _psShape.randomDirectionAmount = 1f; 
            
            _psEmission = _particleSystem.emission;
            _psEmission.rateOverTime = 0;

            var psRenderer = _particleSystem.GetComponent<ParticleSystemRenderer>();
            psRenderer.material = baseMat;

            // 3. Setup Lights Container
            _lightsContainer = new GameObject("BeamLights");
            _lightsContainer.transform.SetParent(transform);
        }

        private void Update()
        {
            if (_spiralObj.activeSelf)
            {
                // Use Perlin noise to create erratic, uneven rotation
                float erraticMod = Mathf.PerlinNoise(Time.time * 8f + _timeOffset, 0f);
                float unevenSpeed = _rotationSpeed * (0.1f + erraticMod * 2.4f);

                // Accumulate the spin. We apply this in SetPositions so LookRotation doesn't erase it!
                _accumulatedSpin += unevenSpeed * Time.deltaTime;
            }
        }

        public void SetPositions(Vector3 start, Vector3 end)
        {
            _lineRenderer.SetPosition(0, start);
            _lineRenderer.SetPosition(1, end);

            Vector3 dir = end - start;
            _currentDistance = dir.magnitude;
            
            if (_currentDistance > 0.001f)
            {
                // Position spiral at the start
                _spiralObj.transform.position = start;
                
                // Point it at the end, but apply our accumulated spin around the Z axis
                Quaternion baseRotation = Quaternion.LookRotation(dir.normalized);
                _spiralObj.transform.rotation = baseRotation * Quaternion.Euler(0, 0, _accumulatedSpin);
                
                if (_spiralObj.activeSelf)
                {
                    GenerateSpiralMesh();
                }
                
                // Update Particle shape box size and position to match the beam
                _particleSystem.transform.position = start + dir * 0.5f;
                _particleSystem.transform.rotation = baseRotation;
                _psShape.scale = new Vector3(0.5f, 0.5f, _currentDistance); 
                
                // Position point lights evenly along the beam (1 light every 3 units)
                int requiredLights = Mathf.Max(1, Mathf.CeilToInt(_currentDistance / 3f));
                
                // Add missing lights
                while (_beamLights.Count < requiredLights)
                {
                    GameObject lightObj = new GameObject("BeamPointLight");
                    lightObj.transform.SetParent(_lightsContainer.transform);
                    UnityEngine.Light l = lightObj.AddComponent<UnityEngine.Light>();
                    l.type = LightType.Point;
                    l.range = PrismPanic.Core.Constants.BEAM_LIGHT_RADIUS;
                    l.intensity = PrismPanic.Core.Constants.BEAM_LIGHT_INTENSITY;
                    l.shadows = LightShadows.None; // Performance: don't cast shadows from every beam segment
                    _beamLights.Add(l);
                }
                
                // Disable excess lights
                for (int i = 0; i < _beamLights.Count; i++)
                {
                    if (i < requiredLights)
                    {
                        _beamLights[i].enabled = true;
                        float t = (float)i / requiredLights + (1f / (requiredLights * 2f)); // Center them in their chunk
                        _beamLights[i].transform.position = Vector3.Lerp(start, end, t);
                    }
                    else
                    {
                        _beamLights[i].enabled = false;
                    }
                }
            }
            else
            {
                // Disable all lights if beam is basically 0 length
                foreach (var l in _beamLights) l.enabled = false;
            }
        }

        private void GenerateSpiralMesh()
        {
            // Optimization: Only rebuild the mesh points if the beam length changes
            if (Mathf.Abs(_lastGeneratedDistance - _currentDistance) < 0.01f) return;
            _lastGeneratedDistance = _currentDistance;

            // Calculate how many points we need for a smooth curve
            int points = Mathf.Max(10, (int)(_currentDistance * 20f));
            _spiralRenderer.positionCount = points;
            
            // Loops per unit of distance
            float frequency = PrismPanic.Core.Constants.SPIRAL_FREQUENCY; 

            for (int i = 0; i < points; i++)
            {
                float t = (float)i / (points - 1);
                float z = t * _currentDistance; // Travel down local Z axis
                
                // Calculate X and Y to make a circle
                float angle = z * Mathf.PI * 2f * frequency;
                float x = Mathf.Cos(angle) * _spiralRadius;
                float y = Mathf.Sin(angle) * _spiralRadius;

                _spiralRenderer.SetPosition(i, new Vector3(x, y, z));
            }
        }

        public void ApplyStyle(int bounceCount, float width)
        {
            _lineRenderer.startWidth = width;
            _lineRenderer.endWidth = width;

            Color color = Color.white;
            float emissionRate = 0;
            bool useSpiral = false;
            
            switch (bounceCount)
            {
                case 0: // White, normal
                    color = Color.white;
                    emissionRate = 0;
                    useSpiral = false;
                    break;
                case 1: // Blue, little particles
                    color = Color.blue; 
                    emissionRate = 50;
                    useSpiral = true;
                    _spiralRadius = width * PrismPanic.Core.Constants.SPIRAL_RADIUS_RED_MULTIPLIER;
                    _rotationSpeed = PrismPanic.Core.Constants.ROTATION_SPEED_RED; 
                    break;
                case 2: // Red, particles + rotating spiral
                    color = Color.red;
                    emissionRate = 100;
                    useSpiral = true;
                    _spiralRadius = width * PrismPanic.Core.Constants.SPIRAL_RADIUS_RED_MULTIPLIER;
                    _rotationSpeed = PrismPanic.Core.Constants.ROTATION_SPEED_RED; 
                    break;
                case 3: // Purple, excessive particles + fast spiral
                    color = new Color(0.8f, 0f, 1f); // Bright Purple
                    emissionRate = 250;
                    useSpiral = true;
                    _spiralRadius = width * PrismPanic.Core.Constants.SPIRAL_RADIUS_PURPLE_MULTIPLIER;
                    _rotationSpeed = PrismPanic.Core.Constants.ROTATION_SPEED_PURPLE; 
                    break;
            }

            // Make colors HDR to trigger Bloom/Glow
            float hdrIntensity = 4.0f;
            Color hdrColor = new Color(color.r * hdrIntensity, color.g * hdrIntensity, color.b * hdrIntensity, 1f);

            // 1. Core LineRenderer Color
            _lineRenderer.startColor = color; // vertex color alpha
            _lineRenderer.endColor = color;
            MaterialPropertyBlock block = new MaterialPropertyBlock();
            _lineRenderer.GetPropertyBlock(block);
            block.SetColor("_BaseColor", hdrColor);
            block.SetColor("_Color", hdrColor);
            _lineRenderer.SetPropertyBlock(block);

            // 2. Spiral Appearance
            _spiralObj.SetActive(useSpiral);
            if (useSpiral)
            {
                _spiralRenderer.startWidth = width * PrismPanic.Core.Constants.SPIRAL_WIDTH_MULTIPLIER;
                _spiralRenderer.endWidth = width * PrismPanic.Core.Constants.SPIRAL_WIDTH_MULTIPLIER;
                
                Color spiralColor = color;
                spiralColor.a = 0.8f;
                _spiralRenderer.startColor = spiralColor;
                _spiralRenderer.endColor = spiralColor;
                
                Color hdrSpiralColor = hdrColor;
                hdrSpiralColor.a = 0.8f;

                MaterialPropertyBlock spiralBlock = new MaterialPropertyBlock();
                _spiralRenderer.GetPropertyBlock(spiralBlock);
                spiralBlock.SetColor("_BaseColor", hdrSpiralColor);
                spiralBlock.SetColor("_Color", hdrSpiralColor);
                _spiralRenderer.SetPropertyBlock(spiralBlock);

                // Force mesh generation if active
                if (_currentDistance > 0.001f)
                {
                    // Reset last generated to force rebuild just in case width changed
                    _lastGeneratedDistance = -1f; 
                    GenerateSpiralMesh();
                }
            }

            // 3. Particle Appearance
            _psMain.startColor = hdrColor;
            _psEmission.rateOverTime = emissionRate;
            if (emissionRate > 0 && !_particleSystem.isPlaying)
            {
                _particleSystem.Play();
            }
            else if (emissionRate == 0)
            {
                _particleSystem.Stop();
                _particleSystem.Clear();
            }

            // 4. Physical Lights Appearance
            foreach (var l in _beamLights)
            {
                l.color = color; // Real Unity lights don't need HDR intensity multiplication
            }
        }
    }
}
