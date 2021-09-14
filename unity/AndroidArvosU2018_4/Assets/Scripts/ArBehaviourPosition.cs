/*
ArBehaviourPosition.cs - MonoBehaviour for ARpoise position handling.

Copyright (C) 2018, Tamiko Thiel and Peter Graf - All Rights Reserved

ARpoise - Augmented Reality point of interest service environment 

This file is part of ARpoise.

    ARpoise is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    ARpoise is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with ARpoise.  If not, see <https://www.gnu.org/licenses/>.

For more information on 

Tamiko Thiel, see www.TamikoThiel.com/
Peter Graf, see www.mission-base.com/peter/
ARpoise, see www.ARpoise.com/

*/

using System;
using System.Collections;
using System.Linq;
using UnityEngine;
#if PLATFORM_ANDROID
using UnityEngine.Android;
#endif

namespace com.arpoise.arpoiseapp
{
    public class ArBehaviourPosition : ArBehaviourMultiUser
    {
        #region Globals

        public GameObject ArCamera = null;

        public ArObjectState ArObjectState { get; protected set; }
        public string ErrorMessage { get; set; }
        #endregion

        #region Protecteds

#if HAS_AR_CORE
        protected const string AppName = "AR-vos";
#else
#if HAS_AR_KIT
        protected const string AppName = "AR-vos";
#else
        protected const string AppName = "ARpoise";
#endif
#endif
        protected const float PositionTolerance = 1.25f;
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

        public virtual bool InfoPanelIsActive()
        {
            return false;
        }

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

        private float _handledLatitude = 0;
        private float _handledLongitude = 0;

        // Calculate positions for all ar objects
        private void PlaceArObjects(ArObjectState arObjectState)
        {
            var arObjectsToPlace = arObjectState.ArObjectsToPlace;
            if (arObjectsToPlace != null)
            {
                var filteredLatitude = UsedLatitude;
                var filteredLongitude = UsedLongitude;

                if (!arObjectsToPlace.Any(x => x.IsDirty) && _handledLatitude == filteredLatitude && _handledLongitude == filteredLongitude)
                {
                    return;
                }
                _handledLatitude = filteredLatitude;
                _handledLongitude = filteredLongitude;

                foreach (var arObject in arObjectsToPlace)
                {
                    arObject.IsDirty = false;
                    if (arObject.Poi.visibilityRange > 0)
                    {
                        var distance = CalculateDistance(arObject.Latitude, arObject.Longitude, filteredLatitude, filteredLongitude);
                        var isVisible = Math.Abs(distance) <= PositionTolerance * arObject.Poi.visibilityRange;
                        if (isVisible != arObject.WrapperObject.activeSelf)
                        {
                            arObject.WrapperObject.SetActive(isVisible);
                        }
                    }
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
#if QUEST_ARPOISE
            // If in quest mode, set a fixed initial location and forget about the location service
            //
            {
                // EOF
                //FilteredLatitude = OriginalLatitude = 49.020586f;
                //FilteredLongitude = OriginalLongitude = 12.09294f;
                // Ay Corona!
                //FilteredLatitude = OriginalLatitude = 48.158601475435f;
                //FilteredLongitude = OriginalLongitude = 11.580199727856f;

                // Quest Default
                FilteredLatitude = OriginalLatitude = 48.158f;
                FilteredLongitude = OriginalLongitude = -11.58f;

                Debug.Log("QUEST_ARPOISE fixed location, lat " + OriginalLatitude + ", lon " + OriginalLongitude);

                var second = DateTime.Now.Ticks / 10000000L;
                var random = new System.Random((int)second);
                var nextMove = second + 5 + random.Next(0, 5);

                while (second > 0)
                {
                    second = DateTime.Now.Ticks / 10000000L;
                    if (second >= nextMove)
                    {
                        nextMove = second + 5 + random.Next(0, 5);

                        FilteredLatitude = OriginalLatitude + 0.000001f * random.Next(-15, 15);
                        FilteredLongitude = OriginalLongitude + 0.000001f * random.Next(-12, 12);
                        Debug.Log("QUEST_ARPOISE new location, lat " + FilteredLatitude + ", lon " + FilteredLongitude);
                    }
                    var arObjectState = ArObjectState;
                    if (arObjectState != null)
                    {
                        PlaceArObjects(arObjectState);
                    }
                    yield return new WaitForSeconds(.1f);
                }
            }
            // End of quest mode
#endif
#if UNITY_EDITOR
            // If in editor mode, set a fixed initial location and forget about the location service
            //
            {
                // MUC-HDK
                //FilteredLatitude = OriginalLatitude = 48.144f;
                //FilteredLongitude = OriginalLongitude = 11.586f;

                // MUC-AINMILLER
                FilteredLatitude = OriginalLatitude = 48.158526f;
                FilteredLongitude = OriginalLongitude = 11.578670f;

                // R-Bahnhof
                //FilteredLatitude = OriginalLatitude = 49.012142f;
                //FilteredLongitude = OriginalLongitude = 12.098089f;


                Debug.Log("UNITY_EDITOR fixed location, lat " + OriginalLatitude + ", lon " + OriginalLongitude);

                var second = DateTime.Now.Ticks / 10000000L;
                var random = new System.Random((int)second);
                var nextMove = second + 9 + random.Next(0, 6);

                while (second > 0 || second <= 0)
                {
                    second = DateTime.Now.Ticks / 10000000L;
                    if (second >= nextMove)
                    {
                        nextMove = second + 6 + random.Next(0, 6);

                        FilteredLatitude = OriginalLatitude + 0.00001f * random.Next(-5, 5);
                        FilteredLongitude = OriginalLongitude + 0.00001f * random.Next(-4, 4);
                        Debug.Log("UNITY_EDITOR new location, lat " + FilteredLatitude + ", lon " + FilteredLongitude);
                    }
                    var arObjectState = ArObjectState;
                    if (arObjectState != null)
                    {
                        PlaceArObjects(arObjectState);
                    }
                    yield return new WaitForSeconds(.1f);
                }
            }
            // End of editor mode
#endif
            int nFails = 0;
            bool doInitialize = true;
            while (string.IsNullOrWhiteSpace(ErrorMessage))
            {
                while (InfoPanelIsActive())
                {
                    yield return new WaitForSeconds(.01f);
                }

                if (doInitialize)
                {
                    doInitialize = false;
                    Input.compass.enabled = true;

                    int maxWait = 4500;
#if UNITY_ANDROID
                    bool first = true;
                    while (!Permission.HasUserAuthorizedPermission(Permission.FineLocation) && maxWait > 0)
                    {
                        if (first)
                        {
                            Permission.RequestUserPermission(Permission.FineLocation);
                            first = false;
                        }
                        yield return new WaitForSeconds(.01f);
                        maxWait--;
                    }
#endif
                    if (_locationService == null)
                    {
                        _locationService = new LocationService();
                        maxWait = 1000;
                        while (!_locationService.isEnabledByUser && maxWait > 0)
                        {
                            yield return new WaitForSeconds(.01f);
                            maxWait--;
                        }
                        if (!_locationService.isEnabledByUser)
                        {
                            ErrorMessage = $"Please enable the location service for the {AppName} app!";
                            yield break;
                        }
                    }

                    Input.location.Start(.1f, .1f);
                    yield return new WaitForSeconds(.2f);

                    maxWait = 3000;
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
                            ErrorMessage = $"Please enable the location service for {AppName} app.";
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

                var setLocation = true;
                if (PositionUpdateInterval > 0)
                {
                    var now = DateTime.Now;
                    if (_nextPositionUpdate < now)
                    {
                        _nextPositionUpdate = now.AddMilliseconds(PositionUpdateInterval * 1000f);
                    }
                    else
                    {
                        setLocation = false;
                    }
                }

                if (setLocation)
                {
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

        private DateTime _nextPositionUpdate = DateTime.MinValue;
        protected float PositionUpdateInterval = 0;

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
        #endregion
    }
}
