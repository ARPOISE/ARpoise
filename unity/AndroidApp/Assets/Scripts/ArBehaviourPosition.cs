/*
ArBehaviourPosition.cs - MonoBehaviour for Arpoise, position handling.

Copyright (C) 2018, Tamiko Thiel and Peter Graf - All Rights Reserved

ARPOISE - Augmented Reality Point Of Interest Service 

This file is part of Arpoise.

    Arpoise is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    Arpoise is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with Arpoise.  If not, see <https://www.gnu.org/licenses/>.

For more information on 

Tamiko Thiel, see www.TamikoThiel.com/
Peter Graf, see www.mission-base.com/peter/
Arpoise, see www.Arpoise.com/

*/

using System;
using System.Collections;
using System.Linq;
using UnityEngine;

namespace com.arpoise.arpoiseapp
{
    public class ArBehaviourPosition : MonoBehaviour
    {
        #region Globals

        public GameObject ArCamera = null;
        public ArObjectState ArObjectState { get; protected set; }
        public string ErrorMessage { get; set; }
        #endregion

        #region Protecteds

        protected int AreaSize = 0;
        protected int AreaWidth = 0;

        protected float FilteredLongitude = 0;
        protected float FilteredLatitude = 0;
        protected bool ApplyKalmanFilter = true;

        protected float? FixedDeviceLatitude = null;
        protected float? FixedDeviceLongitude = null;
        protected float InitialHeading = 0;
        protected float HeadingShown = 0;
        protected float CurrentHeading = 0;
        protected double LocationTimestamp = 0;
        protected float LocationHorizontalAccuracy = 0;
        protected float LocationLongitude = 0;
        protected float LocationLatitude = 0;
        protected float OriginalLatitude = 0;
        protected float OriginalLongitude = 0;
        protected DeviceOrientation InitialDeviceOrientation = DeviceOrientation.LandscapeLeft;

        protected bool CameraIsInitializing = true;
        protected long StartTicks = 0;

        protected float UsedLatitude
        {
            get
            {
                var latitude = FixedDeviceLatitude;
                if (latitude.HasValue)
                {
                    return latitude.Value;
                }
                return FilteredLatitude;
            }
        }

        protected float UsedLongitude
        {
            get
            {
                var longitude = FixedDeviceLongitude;
                if (longitude.HasValue)
                {
                    return longitude.Value;
                }
                return FilteredLongitude;
            }
        }
        #endregion

        #region PlaceArObjects

        private float _latitudeHandled = 0;
        private float _longitudeHandled = 0;

        // Calculate positions for all ar objects
        private void PlaceArObjects(ArObjectState arObjectState)
        {
            var arObjectsToPlace = arObjectState.ArObjectsToPlace;
            if (arObjectsToPlace != null)
            {
                var filteredLatitude = UsedLatitude;
                var filteredLongitude = UsedLongitude;

                if (!arObjectsToPlace.Any(x => x.IsDirty) && _latitudeHandled == filteredLatitude && _longitudeHandled == filteredLongitude)
                {
                    return;
                }
                _latitudeHandled = filteredLatitude;
                _longitudeHandled = filteredLongitude;

                foreach (var arObject in arObjectsToPlace)
                {
                    arObject.IsDirty = false;
                    var latDistance = CalculateDistance(arObject.Latitude, arObject.Longitude, filteredLatitude, arObject.Longitude);
                    var lonDistance = CalculateDistance(arObject.Latitude, arObject.Longitude, arObject.Latitude, filteredLongitude);

                    if (arObject.Latitude < UsedLatitude)
                    {
                        if (latDistance > 0)
                        {
                            latDistance *= -1;
                        }
                    }
                    else
                    {
                        if (latDistance < 0)
                        {
                            latDistance *= -1;
                        }
                    }
                    if (arObject.Longitude < UsedLongitude)
                    {
                        if (lonDistance > 0)
                        {
                            lonDistance *= -1;
                        }
                    }
                    else
                    {
                        if (lonDistance < 0)
                        {
                            lonDistance *= -1;
                        }
                    }

                    if (AreaSize <= 0 && AreaWidth > 0)
                    {
                        AreaSize = AreaWidth;
                    }
                    if (AreaSize > 0)
                    {
                        if (AreaWidth <= 0)
                        {
                            AreaWidth = AreaSize;
                        }

                        var halfWidth = AreaWidth / 2f;
                        while (lonDistance > 0 && lonDistance > halfWidth)
                        {
                            lonDistance -= AreaWidth;
                        }
                        while (lonDistance < 0 && lonDistance < -halfWidth)
                        {
                            lonDistance += AreaWidth;
                        }

                        var halfSize = AreaSize / 2f;
                        while (latDistance > 0 && latDistance > halfSize)
                        {
                            latDistance -= AreaSize;
                        }
                        while (latDistance < 0 && latDistance < -halfSize)
                        {
                            latDistance += AreaSize;
                        }
                        var distanceToAreaBorder = Mathf.Min(Mathf.Abs(Mathf.Abs(latDistance) - halfSize), Mathf.Abs(Mathf.Abs(lonDistance) - halfWidth));
                        if (distanceToAreaBorder < 1)
                        {
                            // The object is less than 1 meter from the border, scale it down with the distance it has
                            arObject.Scale = distanceToAreaBorder;
                        }
                        else
                        {
                            arObject.Scale = 1;
                        }
                    }
                    else
                    {
                        arObject.Scale = 1;
                    }
                    arObject.TargetPosition = new Vector3(lonDistance, arObject.RelativeAltitude, latDistance);
                }
            }
        }
        #endregion
        
        #region GetPosition

        private LocationService _locationService = null;

        // A Coroutine for retrieving the current location
        //
        protected IEnumerator GetPosition()
        {
#if UNITY_EDITOR
            // If in editor mode, set a fixed initial location and forget about the location service
            //
            FilteredLatitude = OriginalLatitude = 48.158464f;
            FilteredLongitude = OriginalLongitude = 11.578708f;

            Debug.Log("UNITY_EDITOR fixed location, lat " + FilteredLatitude + ", lon " + FilteredLongitude);

            while (FilteredLatitude <= 90)
            {
                yield return new WaitForSeconds(3600f);
            }
            // End of editor mode
#endif
#if HAS_AR_CORE
            // If it is the Android ARCore app, set a fixed initial location and forget about the location service
            //
            FilteredLatitude = OriginalLatitude = 5f;
            FilteredLongitude = OriginalLongitude = 5f;

            Debug.Log("HAS_AR_CORE fixed location, lat " + FilteredLatitude + ", lon " + FilteredLongitude);

            while (FilteredLatitude <= 90)
            {
                yield return new WaitForSeconds(3600f);
            }
            // End of Android ARCore app
#endif
            int nFails = 0;
            bool doInitialize = true;
            while (IsEmpty(ErrorMessage))
            {
                if (doInitialize)
                {
                    doInitialize = false;
                    Input.compass.enabled = true;

                    if (_locationService == null)
                    {
                        _locationService = new LocationService();
                        if (!_locationService.isEnabledByUser)
                        {
                            ErrorMessage = "Please enable the location service for the ARpoise application.";
                            yield break;
                        }
                    }

                    Input.location.Start(.1f, .1f);
                    yield return new WaitForSeconds(.2f);

                    int maxWait = 3000;
                    while (Input.location.status == LocationServiceStatus.Initializing && maxWait > 0)
                    {
                        yield return new WaitForSeconds(.01f);
                        maxWait--;
                    }

                    if (maxWait < 1)
                    {
                        ErrorMessage = "Location service didn't initialize in 30 seconds.";
                        yield break;
                    }

                    if (Input.location.status == LocationServiceStatus.Failed)
                    {
                        if (++nFails > 10)
                        {
                            ErrorMessage = "Please enable the location service for the ARpoise app.";
                            yield break;
                        }
                        Input.location.Stop();
                        doInitialize = true;
                        continue;
                    }

                    FilteredLatitude = OriginalLatitude = Input.location.lastData.latitude;
                    FilteredLongitude = OriginalLongitude = Input.location.lastData.longitude;

                    InitialDeviceOrientation = Input.deviceOrientation;
                }

                // For the first .2 seconds we remember the initial camera heading
                if (CameraIsInitializing && StartTicks > 0 && DateTime.Now.Ticks > StartTicks + 2000000)
                {
                    CameraIsInitializing = false;
                }
                CurrentHeading = Input.compass.trueHeading;

                if (LocationLatitude != Input.location.lastData.latitude
                    || LocationLongitude != Input.location.lastData.longitude
                    || LocationTimestamp != Input.location.lastData.timestamp
                    || LocationHorizontalAccuracy != Input.location.lastData.horizontalAccuracy
                )
                {
                    LocationLatitude = Input.location.lastData.latitude;
                    LocationLongitude = Input.location.lastData.longitude;
                    LocationTimestamp = Input.location.lastData.timestamp;
                    LocationHorizontalAccuracy = Input.location.lastData.horizontalAccuracy;

                    KalmanFilter(LocationLatitude, LocationLongitude, LocationHorizontalAccuracy, (long)(1000L * LocationTimestamp));
                }

                var arObjectState = ArObjectState;
                if (arObjectState != null)
                {
                    PlaceArObjects(arObjectState);
                }
                yield return new WaitForSeconds(.01f);
            }
            yield break;
        }
        #endregion

        #region Misc
        protected virtual IEnumerator GetData()
        {
            ErrorMessage = "ArBehaviourPosition.GetData needs to be overridden";
            yield break;
        }

        // Calculates the distance between two sets of coordinates, taking into account the curvature of the earth
        protected float CalculateDistance(float lat1, float lon1, float lat2, float lon2)
        {
            var R = 6371.0; // Mean radius of earth in KM
            var dLat = lat2 * Mathf.PI / 180 - lat1 * Mathf.PI / 180;
            var dLon = lon2 * Mathf.PI / 180 - lon1 * Mathf.PI / 180;
            float a = Mathf.Sin(dLat / 2) * Mathf.Sin(dLat / 2) +
              Mathf.Cos(lat1 * Mathf.PI / 180) * Mathf.Cos(lat2 * Mathf.PI / 180) *
              Mathf.Sin(dLon / 2) * Mathf.Sin(dLon / 2);
            var c = 2 * Mathf.Atan2(Mathf.Sqrt(a), Mathf.Sqrt(1 - a));
            var distance = R * c;
            return (float)(distance * 1000f); // meters
        }

        private long _timeStampInMilliseconds;
        private readonly double _qMetersPerSecond = 3;
        private double _variance = -1;

        // Kalman filter processing for latitude and longitude
        private void KalmanFilter(double currentLatitude, double currentLongitude, double accuracy, long timeStampInMilliseconds)
        {
            if (!ApplyKalmanFilter)
            {
                FilteredLatitude = (float)currentLatitude;
                FilteredLongitude = (float)currentLongitude;
                return;
            }

            if (accuracy < 1)
            {
                accuracy = 1;
            }
            if (_variance < 0)
            {
                // if variance < 0, the object is unitialised, so initialise it with current values
                _timeStampInMilliseconds = timeStampInMilliseconds;
                FilteredLatitude = (float)currentLatitude;
                FilteredLongitude = (float)currentLongitude;
                _variance = accuracy * accuracy;
            }
            else
            {
                // apply Kalman filter
                long timeIncreaseInMilliseconds = timeStampInMilliseconds - _timeStampInMilliseconds;
                if (timeIncreaseInMilliseconds > 0)
                {
                    // time has moved on, so the uncertainty in the current position increases
                    _variance += timeIncreaseInMilliseconds * _qMetersPerSecond * _qMetersPerSecond / 1000;
                    _timeStampInMilliseconds = timeStampInMilliseconds;
                    // TO DO: USE VELOCITY INFORMATION HERE TO GET A BETTER ESTIMATE OF CURRENT POSITION
                }

                // Kalman gain matrix K = Covariance * Inverse(Covariance + MeasurementVariance)
                // NB: because K is dimensionless, it doesn't matter that variance has different units to lat and lon
                double k = _variance / (_variance + accuracy * accuracy);
                // apply K
                FilteredLatitude += (float)(k * (currentLatitude - FilteredLatitude));
                FilteredLongitude += (float)(k * (currentLongitude - FilteredLongitude));
                // new Covarariance  matrix is (IdentityMatrix - K) * Covariance 
                _variance = (1 - k) * _variance;
            }
        }

        // Not string is null or white space
        public static bool IsEmpty(string s)
        {
            return s == null || string.IsNullOrEmpty(s.Trim());
        }
        #endregion
    }
}
