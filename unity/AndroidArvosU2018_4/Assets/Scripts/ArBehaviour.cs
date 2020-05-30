/*
ArBehaviour.cs - MonoBehaviour for Arpoise.

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

using UnityEngine;

#if HAS_AR_CORE
#else
#if HAS_AR_KIT
#else
using Vuforia;
#endif
#endif

namespace com.arpoise.arpoiseapp
{
    public class ArBehaviour : ArBehaviourUserInterface
    {
        #region Start
        protected override void Start()
        {
            base.Start();

#if UNITY_EDITOR
            Debug.Log("UNITY_EDITOR Start");
#endif

#if HAS_AR_CORE
#else
#if HAS_AR_KIT
#else
            ArCamera.GetComponent<VuforiaBehaviour>().enabled = true;
            VuforiaRuntime.Instance.InitVuforia();
#endif
#endif

#if UNITY_IOS_unused

            if (dontDestroyOnLoad)
                DontDestroyOnLoad(this.gameObject);
            DeepLinkReceiverIsAlive(); // Let the App Controller know it's ok to call URLOpened now.
#endif
            // Start GetPosition() coroutine 
            StartCoroutine("GetPosition");
            // Start GetData() coroutine 
            StartCoroutine("GetData");
        }
        #endregion

        #region Update
        protected override void Update()
        {
            base.Update();
        }
        #endregion

        #region iOS deep link

#if UNITY_IOS_unused
        public string LinkUrl;

        [DllImport("__Internal")]
        private static extern void DeepLinkReceiverIsAlive();
        [System.Serializable]
        public class StringEvent : UnityEvent { }
        public StringEvent urlOpenedEvent;
        public bool dontDestroyOnLoad = true;

        public void URLOpened(string url)
        {
            LinkUrl = url;
            Debug.Log("Link url" + LinkUrl);
        }
#endif
        #endregion
    }
}
