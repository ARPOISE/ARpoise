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

using System;
using UnityEngine;

#if HAS_AR_CORE
#else
#if HAS_AR_KIT
#else
#if QUEST_ARPOISE
#else
using Vuforia;
#endif
#endif
#endif

namespace com.arpoise.arpoiseapp
{
    public class ArBehaviour : ArBehaviourUserInterface
    {
        public GameObject Cube = null;

        #region Start
        protected override void Start()
        {
            base.Start();

#if QUEST_ARPOISE
            Debug.Log("QUEST_ARPOISE Start");
#endif
#if UNITY_EDITOR
            Debug.Log("UNITY_EDITOR Start");
#endif

#if HAS_AR_CORE
#else
#if HAS_AR_KIT
#else
#if QUEST_ARPOISE
#else
            ArCamera.GetComponent<VuforiaBehaviour>().enabled = true;
            VuforiaRuntime.Instance.InitVuforia();
#endif
#endif
#endif

#if UNITY_IOS_unused

            if (dontDestroyOnLoad)
                DontDestroyOnLoad(this.gameObject);
            DeepLinkReceiverIsAlive(); // Let the App Controller know it's ok to call URLOpened now.
#endif
            // Start GetPosition() coroutine 
            StartCoroutine(nameof(GetPosition));
            // Start GetData() coroutine 
            StartCoroutine(nameof(GetData));
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
