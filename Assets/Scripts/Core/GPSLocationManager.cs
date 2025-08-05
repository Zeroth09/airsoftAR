using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace AirsoftARBattle.Core
{
    /// <summary>
    /// Manager untuk GPS location dan positioning system
    /// Mengelola lokasi real-world players untuk battle area
    /// </summary>
    public class GPSLocationManager : MonoBehaviour
    {
        [Header("GPS Settings")]
        [SerializeField] private float updateRate = 1f; // Update setiap detik
        [SerializeField] private float accuracyThreshold = 10f; // Meter
        [SerializeField] private float maxBattleRadius = 500f; // Radius battle area dalam meter
        
        [Header("Battle Area")]
        [SerializeField] private double centerLatitude;
        [SerializeField] private double centerLongitude;
        [SerializeField] private bool useDynamicCenter = true; // Auto set center dari player pertama
        
        // Current location data
        private LocationInfo currentLocation;
        private bool isLocationServiceStarted = false;
        private bool hasValidLocation = false;
        private float lastLocationUpdate;
        
        // Battle area data
        private Vector2 battleCenter;
        private Dictionary<int, Vector2> playerPositions = new Dictionary<int, Vector2>();
        
        // Events
        public System.Action<LocationInfo> OnLocationUpdated;
        public System.Action<Vector2> OnPlayerPositionUpdated;
        public System.Action<bool> OnBattleAreaStatusChanged;
        
        private void Start()
        {
            StartLocationService();
        }
        
        private void Update()
        {
            if (isLocationServiceStarted && Time.time - lastLocationUpdate > updateRate)
            {
                UpdateLocation();
            }
        }
        
        /// <summary>
        /// Start GPS location service
        /// </summary>
        private void StartLocationService()
        {
            if (!Input.location.isEnabledByUser)
            {
                Debug.LogError("Location services not enabled by user");
                return;
            }
            
            Input.location.Start(accuracyThreshold, accuracyThreshold);
            isLocationServiceStarted = true;
            
            StartCoroutine(WaitForLocationService());
        }
        
        /// <summary>
        /// Wait untuk location service ready
        /// </summary>
        private IEnumerator WaitForLocationService()
        {
            int maxWait = 20; // 20 detik timeout
            
            while (Input.location.status == LocationServiceStatus.Initializing && maxWait > 0)
            {
                yield return new WaitForSeconds(1);
                maxWait--;
            }
            
            if (maxWait <= 0)
            {
                Debug.LogError("GPS location service timed out");
                yield break;
            }
            
            if (Input.location.status == LocationServiceStatus.Failed)
            {
                Debug.LogError("Unable to determine device location");
                yield break;
            }
            
            hasValidLocation = true;
            UpdateLocation();
            
            // Set battle center jika dynamic
            if (useDynamicCenter && battleCenter == Vector2.zero)
            {
                SetBattleCenter(currentLocation.latitude, currentLocation.longitude);
            }
        }
        
        /// <summary>
        /// Update current location
        /// </summary>
        private void UpdateLocation()
        {
            if (!hasValidLocation) return;
            
            currentLocation = Input.location.lastData;
            lastLocationUpdate = Time.time;
            
            // Convert GPS ke local coordinates
            Vector2 localPosition = GPSToLocalPosition(currentLocation.latitude, currentLocation.longitude);
            
            // Update player position
            OnLocationUpdated?.Invoke(currentLocation);
            OnPlayerPositionUpdated?.Invoke(localPosition);
            
            // Check battle area
            CheckBattleArea(localPosition);
        }
        
        /// <summary>
        /// Set battle center coordinates
        /// </summary>
        public void SetBattleCenter(double latitude, double longitude)
        {
            centerLatitude = latitude;
            centerLongitude = longitude;
            battleCenter = Vector2.zero; // Center sebagai origin
            
            Debug.Log($"Battle center set to: {latitude}, {longitude}");
        }
        
        /// <summary>
        /// Convert GPS coordinates ke local position
        /// </summary>
        public Vector2 GPSToLocalPosition(double latitude, double longitude)
        {
            if (centerLatitude == 0 && centerLongitude == 0)
            {
                return Vector2.zero;
            }
            
            // Konversi GPS ke meter menggunakan Haversine formula
            double latDiff = latitude - centerLatitude;
            double lonDiff = longitude - centerLongitude;
            
            // Approximate conversion (1 degree ≈ 111km)
            float x = (float)(lonDiff * 111000 * Mathf.Cos((float)(centerLatitude * Mathf.Deg2Rad)));
            float z = (float)(latDiff * 111000);
            
            return new Vector2(x, z);
        }
        
        /// <summary>
        /// Check apakah player masih dalam battle area
        /// </summary>
        private void CheckBattleArea(Vector2 position)
        {
            float distanceFromCenter = Vector2.Distance(position, battleCenter);
            bool isInBattleArea = distanceFromCenter <= maxBattleRadius;
            
            OnBattleAreaStatusChanged?.Invoke(isInBattleArea);
            
            if (!isInBattleArea)
            {
                Debug.LogWarning($"Player outside battle area! Distance: {distanceFromCenter}m");
            }
        }
        
        /// <summary>
        /// Get current GPS location
        /// </summary>
        public LocationInfo GetCurrentLocation()
        {
            return currentLocation;
        }
        
        /// <summary>
        /// Get local position dari GPS
        /// </summary>
        public Vector2 GetLocalPosition()
        {
            if (!hasValidLocation) return Vector2.zero;
            return GPSToLocalPosition(currentLocation.latitude, currentLocation.longitude);
        }
        
        /// <summary>
        /// Check apakah location service ready
        /// </summary>
        public bool IsLocationReady()
        {
            return hasValidLocation && isLocationServiceStarted;
        }
        
        /// <summary>
        /// Get distance antara dua GPS coordinates
        /// </summary>
        public float GetDistance(double lat1, double lon1, double lat2, double lon2)
        {
            // Haversine formula
            double R = 6371000; // Earth radius dalam meter
            double dLat = (lat2 - lat1) * Mathf.Deg2Rad;
            double dLon = (lon2 - lon1) * Mathf.Deg2Rad;
            
            double a = Mathf.Sin((float)dLat / 2) * Mathf.Sin((float)dLat / 2) +
                      Mathf.Cos((float)lat1 * Mathf.Deg2Rad) * Mathf.Cos((float)lat2 * Mathf.Deg2Rad) *
                      Mathf.Sin((float)dLon / 2) * Mathf.Sin((float)dLon / 2);
            
            double c = 2 * Mathf.Atan2(Mathf.Sqrt(a), Mathf.Sqrt(1 - a));
            double distance = R * c;
            
            return (float)distance;
        }
        
        /// <summary>
        /// Stop location service
        /// </summary>
        private void OnDestroy()
        {
            if (isLocationServiceStarted)
            {
                Input.location.Stop();
            }
        }
        
        /// <summary>
        /// Force location update
        /// </summary>
        public void ForceLocationUpdate()
        {
            if (hasValidLocation)
            {
                UpdateLocation();
            }
        }
        
        /// <summary>
        /// Set max battle radius
        /// </summary>
        public void SetMaxBattleRadius(float radius)
        {
            maxBattleRadius = radius;
        }
        
        /// <summary>
        /// Get max battle radius
        /// </summary>
        public float GetMaxBattleRadius()
        {
            return maxBattleRadius;
        }
    }
}