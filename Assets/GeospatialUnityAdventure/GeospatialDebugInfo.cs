using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;

using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

using Google.XR.ARCoreExtensions;

using System;

namespace UnityAdventure.Debug.Geospatial
{
    public class GeospatialDebugInfo : MonoBehaviour
    {
        [SerializeField] private AREarthManager earthManager;
        [SerializeField] private Text infoText;

        private bool isGeospatialSupported;

        private GeospatialPose currentGeospatialPose;

        private const string InfoFormat =
            "Latitude/Longitude: {1}°, {2}°{0}" +
            "Horizontal Accuracy: {3}m{0}" +
            "Altitude: {4}m{0}" +
            "Vertical Accuracy: {5}m{0}" +
            "Eun Rotation: {6}{0}" +
            "Orientation Yaw Accuracy: {7}°";

        private const string SupportMessageFormat = "Is Geospatial supported: {0}";

        private readonly string[] locationServiceStatusInfos =
            { "The location service is not running.",
            "The location service is initializing.",
        "The location service is running and the application can query for locations.",
        "Location service initialization failed. The user denied access to the location service."};

        private void OnValidate()
        {
            if (earthManager != null)
            {
                return;
            }

            earthManager = FindAnyObjectByType<AREarthManager>();
        }

        void Start()
        {
            VerifyGeospatialSupport();
        }

        void Update()
        {
            if (ARSession.state != ARSessionState.SessionInitializing && ARSession.state != ARSessionState.SessionTracking)
            {
                return;
            }

            if (!isGeospatialSupported)
            {
                return;
            }

            TrackingState earthTrackingState = earthManager.EarthTrackingState;

            if (earthTrackingState == TrackingState.Tracking)
            {
                currentGeospatialPose = earthManager.CameraGeospatialPose;
                infoText.text = GetGeospatialInfoText(currentGeospatialPose);
            }
            else
            {
                string locationStatusInfo = GenerateLocationServiceInfo();
                infoText.text = string.Format("Geospatial Tracking State: {0} and Location Service Status: {1}", earthTrackingState, locationStatusInfo);
            }
        }

        private void VerifyGeospatialSupport()
        {
            FeatureSupported support = earthManager.IsGeospatialModeSupported(GeospatialMode.Enabled);

            switch (support)
            {
                case FeatureSupported.Supported:
                    isGeospatialSupported = true;
                    infoText.text = string.Format(SupportMessageFormat, isGeospatialSupported);
                    break;
                case FeatureSupported.Unknown:
                    infoText.text = string.Format(SupportMessageFormat, FeatureSupported.Unknown.ToString());
                    Invoke("VerifyGeospatialSupport", 0.5f);
                    break;
                case FeatureSupported.Unsupported:
                    infoText.text = string.Format(SupportMessageFormat, isGeospatialSupported);
                    break;
                default:
                    break;
            }
        }

        private string GetGeospatialInfoText(GeospatialPose pose)
        {
            return string.Format(
            InfoFormat,
            Environment.NewLine,
            pose.Latitude.ToString("F6"),
            pose.Longitude.ToString("F6"),
            pose.HorizontalAccuracy.ToString("F6"),
            pose.Altitude.ToString("F2"),
            pose.VerticalAccuracy.ToString("F2"),
            pose.EunRotation.ToString("F1"),
            pose.OrientationYawAccuracy.ToString("F1"));
        }

        private  string GenerateLocationServiceInfo()
        {
            string locationStatusInfo = string.Empty;
            switch (Input.location.status)
            {
                case LocationServiceStatus.Stopped:
                    locationStatusInfo = locationServiceStatusInfos[0];
                    break;
                case LocationServiceStatus.Initializing:
                    locationStatusInfo = locationServiceStatusInfos[1];
                    break;
                case LocationServiceStatus.Running:
                    locationStatusInfo = locationServiceStatusInfos[2];
                    break;
                case LocationServiceStatus.Failed:
                    locationStatusInfo = locationServiceStatusInfos[3];
                    break;
                default:
                    break;
            }

            return locationStatusInfo;
        }
    }
}
