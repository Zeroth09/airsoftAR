using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using System.Collections.Generic;
using System.Collections;

namespace AirsoftARBattle.Core
{
    /// <summary>
    /// Manager untuk AR plane detection dan ground tracking
    /// Mengelola spawn positioning dan world anchoring
    /// </summary>
    public class ARPlaneManager : MonoBehaviour
    {
        [Header("AR Components")]
        [SerializeField] private ARPlaneManager arPlaneManager;
        [SerializeField] private ARRaycastManager raycastManager;
        [SerializeField] private ARSessionOrigin arSessionOrigin;
        
        [Header("Plane Settings")]
        [SerializeField] private bool detectHorizontalPlanes = true;
        [SerializeField] private bool detectVerticalPlanes = false;
        [SerializeField] private float minPlaneSize = 2f; // Minimum plane size untuk spawn
        [SerializeField] private int maxDetectedPlanes = 10;
        
        [Header("Spawn Points")]
        [SerializeField] private GameObject spawnPointPrefab;
        [SerializeField] private Material validSpawnMaterial;
        [SerializeField] private Material invalidSpawnMaterial;
        
        [Header("Visual")]
        [SerializeField] private bool showPlaneVisualization = true;
        [SerializeField] private Material planeMaterial;
        
        // Plane tracking
        private List<ARPlane> detectedPlanes = new List<ARPlane>();
        private List<Vector3> validSpawnPoints = new List<Vector3>();
        private Dictionary<ARPlane, GameObject> planeVisuals = new Dictionary<ARPlane, GameObject>();
        
        // Raycast
        private List<ARRaycastHit> raycastHits = new List<ARRaycastHit>();
        
        // Events
        public System.Action<ARPlane> OnPlaneDetected;
        public System.Action<ARPlane> OnPlaneRemoved;
        public System.Action<List<Vector3>> OnSpawnPointsUpdated;
        public System.Action<Vector3> OnGroundPositionFound;
        
        private void Start()
        {
            InitializeARPlaneManager();
        }
        
        /// <summary>
        /// Initialize AR plane manager
        /// </summary>
        private void InitializeARPlaneManager()
        {
            if (arPlaneManager == null)
            {
                arPlaneManager = FindObjectOfType<ARPlaneManager>();
            }
            
            if (raycastManager == null)
            {
                raycastManager = FindObjectOfType<ARRaycastManager>();
            }
            
            if (arSessionOrigin == null)
            {
                arSessionOrigin = FindObjectOfType<ARSessionOrigin>();
            }
            
            // Setup plane manager
            if (arPlaneManager != null)
            {
                arPlaneManager.planesChanged += OnPlanesChanged;
                
                // Set detection mode
                var planeManager = arPlaneManager;
                if (detectHorizontalPlanes && detectVerticalPlanes)
                {
                    planeManager.requestedDetectionMode = PlaneDetectionMode.Horizontal | PlaneDetectionMode.Vertical;
                }
                else if (detectHorizontalPlanes)
                {
                    planeManager.requestedDetectionMode = PlaneDetectionMode.Horizontal;
                }
                else if (detectVerticalPlanes)
                {
                    planeManager.requestedDetectionMode = PlaneDetectionMode.Vertical;
                }
                else
                {
                    planeManager.requestedDetectionMode = PlaneDetectionMode.None;
                }
                
                Debug.Log($"AR Plane detection initialized: {planeManager.requestedDetectionMode}");
            }
        }
        
        /// <summary>
        /// Called when planes change
        /// </summary>
        private void OnPlanesChanged(ARPlanesChangedEventArgs eventArgs)
        {
            // Added planes
            foreach (var plane in eventArgs.added)
            {
                OnPlaneAdded(plane);
            }
            
            // Updated planes
            foreach (var plane in eventArgs.updated)
            {
                OnPlaneUpdated(plane);
            }
            
            // Removed planes
            foreach (var plane in eventArgs.removed)
            {
                OnPlaneRemoved_Internal(plane);
            }
            
            UpdateSpawnPoints();
        }
        
        /// <summary>
        /// Handle plane added
        /// </summary>
        private void OnPlaneAdded(ARPlane plane)
        {
            if (detectedPlanes.Count >= maxDetectedPlanes)
            {
                return;
            }
            
            detectedPlanes.Add(plane);
            
            if (showPlaneVisualization)
            {
                CreatePlaneVisualization(plane);
            }
            
            OnPlaneDetected?.Invoke(plane);
            
            Debug.Log($"AR Plane detected: {plane.trackableId}, Size: {plane.size}");
        }
        
        /// <summary>
        /// Handle plane updated
        /// </summary>
        private void OnPlaneUpdated(ARPlane plane)
        {
            if (showPlaneVisualization && planeVisuals.ContainsKey(plane))
            {
                UpdatePlaneVisualization(plane);
            }
        }
        
        /// <summary>
        /// Handle plane removed
        /// </summary>
        private void OnPlaneRemoved_Internal(ARPlane plane)
        {
            detectedPlanes.Remove(plane);
            
            if (planeVisuals.ContainsKey(plane))
            {
                Destroy(planeVisuals[plane]);
                planeVisuals.Remove(plane);
            }
            
            OnPlaneRemoved?.Invoke(plane);
            
            Debug.Log($"AR Plane removed: {plane.trackableId}");
        }
        
        /// <summary>
        /// Create plane visualization
        /// </summary>
        private void CreatePlaneVisualization(ARPlane plane)
        {
            GameObject visualization = new GameObject($"PlaneVisualization_{plane.trackableId}");
            visualization.transform.SetParent(plane.transform);
            visualization.transform.localPosition = Vector3.zero;
            visualization.transform.localRotation = Quaternion.identity;
            
            // Add mesh components
            MeshRenderer renderer = visualization.AddComponent<MeshRenderer>();
            MeshFilter filter = visualization.AddComponent<MeshFilter>();
            
            renderer.material = planeMaterial;
            
            planeVisuals[plane] = visualization;
            UpdatePlaneVisualization(plane);
        }
        
        /// <summary>
        /// Update plane visualization
        /// </summary>
        private void UpdatePlaneVisualization(ARPlane plane)
        {
            if (!planeVisuals.ContainsKey(plane)) return;
            
            GameObject visualization = planeVisuals[plane];
            MeshFilter filter = visualization.GetComponent<MeshFilter>();
            
            if (filter != null)
            {
                // Create simple quad mesh for plane
                Mesh mesh = new Mesh();
                Vector2 size = plane.size;
                
                Vector3[] vertices = new Vector3[4]
                {
                    new Vector3(-size.x/2, 0, -size.y/2),
                    new Vector3(size.x/2, 0, -size.y/2),
                    new Vector3(size.x/2, 0, size.y/2),
                    new Vector3(-size.x/2, 0, size.y/2)
                };
                
                int[] triangles = new int[6] { 0, 1, 2, 0, 2, 3 };
                Vector2[] uvs = new Vector2[4]
                {
                    new Vector2(0, 0),
                    new Vector2(1, 0),
                    new Vector2(1, 1),
                    new Vector2(0, 1)
                };
                
                mesh.vertices = vertices;
                mesh.triangles = triangles;
                mesh.uv = uvs;
                mesh.RecalculateNormals();
                
                filter.mesh = mesh;
            }
        }
        
        /// <summary>
        /// Update spawn points berdasarkan detected planes
        /// </summary>
        private void UpdateSpawnPoints()
        {
            validSpawnPoints.Clear();
            
            foreach (var plane in detectedPlanes)
            {
                if (IsValidSpawnPlane(plane))
                {
                    // Generate spawn points pada plane
                    Vector3[] spawnPositions = GenerateSpawnPositionsOnPlane(plane);
                    validSpawnPoints.AddRange(spawnPositions);
                }
            }
            
            OnSpawnPointsUpdated?.Invoke(validSpawnPoints);
        }
        
        /// <summary>
        /// Check apakah plane valid untuk spawn
        /// </summary>
        private bool IsValidSpawnPlane(ARPlane plane)
        {
            // Check size
            Vector2 size = plane.size;
            float area = size.x * size.y;
            
            if (area < minPlaneSize * minPlaneSize)
                return false;
            
            // Check type (horizontal untuk spawning)
            if (plane.alignment != PlaneAlignment.HorizontalUp)
                return false;
            
            return true;
        }
        
        /// <summary>
        /// Generate spawn positions pada plane
        /// </summary>
        private Vector3[] GenerateSpawnPositionsOnPlane(ARPlane plane)
        {
            List<Vector3> positions = new List<Vector3>();
            Vector2 size = plane.size;
            
            // Generate grid of spawn points
            int pointsX = Mathf.Max(1, Mathf.FloorToInt(size.x / 2f));
            int pointsZ = Mathf.Max(1, Mathf.FloorToInt(size.y / 2f));
            
            for (int x = 0; x < pointsX; x++)
            {
                for (int z = 0; z < pointsZ; z++)
                {
                    float posX = (x - pointsX / 2f) * 2f;
                    float posZ = (z - pointsZ / 2f) * 2f;
                    
                    Vector3 localPos = new Vector3(posX, 0, posZ);
                    Vector3 worldPos = plane.transform.TransformPoint(localPos);
                    
                    positions.Add(worldPos);
                }
            }
            
            return positions.ToArray();
        }
        
        /// <summary>
        /// Raycast untuk ground position
        /// </summary>
        public bool TryGetGroundPosition(Vector2 screenPosition, out Vector3 groundPosition)
        {
            groundPosition = Vector3.zero;
            
            if (raycastManager == null) return false;
            
            raycastHits.Clear();
            if (raycastManager.Raycast(screenPosition, raycastHits, TrackableType.PlaneWithinPolygon))
            {
                if (raycastHits.Count > 0)
                {
                    groundPosition = raycastHits[0].pose.position;
                    OnGroundPositionFound?.Invoke(groundPosition);
                    return true;
                }
            }
            
            return false;
        }
        
        /// <summary>
        /// Get center screen ground position
        /// </summary>
        public bool TryGetCenterGroundPosition(out Vector3 groundPosition)
        {
            Vector2 centerScreen = new Vector2(Screen.width / 2f, Screen.height / 2f);
            return TryGetGroundPosition(centerScreen, out groundPosition);
        }
        
        /// <summary>
        /// Get random valid spawn point
        /// </summary>
        public Vector3 GetRandomSpawnPoint()
        {
            if (validSpawnPoints.Count == 0)
            {
                return Vector3.zero;
            }
            
            int randomIndex = Random.Range(0, validSpawnPoints.Count);
            return validSpawnPoints[randomIndex];
        }
        
        /// <summary>
        /// Get all valid spawn points
        /// </summary>
        public List<Vector3> GetValidSpawnPoints()
        {
            return new List<Vector3>(validSpawnPoints);
        }
        
        /// <summary>
        /// Get detected planes
        /// </summary>
        public List<ARPlane> GetDetectedPlanes()
        {
            return new List<ARPlane>(detectedPlanes);
        }
        
        /// <summary>
        /// Toggle plane visualization
        /// </summary>
        public void SetPlaneVisualization(bool show)
        {
            showPlaneVisualization = show;
            
            foreach (var visual in planeVisuals.Values)
            {
                visual.SetActive(show);
            }
        }
        
        /// <summary>
        /// Clear all detected planes
        /// </summary>
        public void ClearDetectedPlanes()
        {
            foreach (var visual in planeVisuals.Values)
            {
                Destroy(visual);
            }
            
            planeVisuals.Clear();
            detectedPlanes.Clear();
            validSpawnPoints.Clear();
        }
        
        /// <summary>
        /// Cleanup
        /// </summary>
        private void OnDestroy()
        {
            if (arPlaneManager != null)
            {
                arPlaneManager.planesChanged -= OnPlanesChanged;
            }
            
            ClearDetectedPlanes();
        }
    }
}