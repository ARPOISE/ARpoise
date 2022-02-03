/*
MenuButton.cs - Script handling clicks on the ARpoise menu button.

Copyright (C) 2019, Tamiko Thiel and Peter Graf - All Rights Reserved

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

using UnityEngine;
using UnityEngine.UI;

namespace com.arpoise.arpoiseapp
{
    public class MenuButton : MonoBehaviour
    {
        public Button Button;
        private bool _started;

        // Use this for initialization
        protected void Start()
        {
            if (!_started)
            {
                _started = true;
                Button.onClick.AddListener(HandleClick);
                //Debug.Log("MenuButton Start.");
            }
        }

        private ArBehaviourUserInterface _behaviour;
        public void Setup(ArBehaviourUserInterface behaviour)
        {
            Start();
            _behaviour = behaviour;
            //Debug.Log("MenuButton Setup.");
        }

        public void HandleClick()
        {
            if (_behaviour != null)
            {
                //TakeScreenshot();
                _behaviour.HandleMenuButtonClick();
                //Debug.Log("MenuButton HandleClick.");
            }
        }

        // This is unused, it was a try to allow to upload a screenshot
        //
        //public void TakeScreenshot()
        //{
        //    var camera = Camera.main;
        //    if (camera != null)
        //    {
        //        RenderTexture rt = new RenderTexture(camera.pixelWidth, camera.pixelHeight, 24);
        //        Camera.main.targetTexture = rt;
        //        Texture2D screenShot = new Texture2D(camera.pixelWidth, camera.pixelHeight, TextureFormat.RGB24, false);
        //        Camera.main.Render();
        //        RenderTexture.active = rt;
        //        screenShot.ReadPixels(new Rect(0, 0, camera.pixelWidth, camera.pixelHeight), 0, 0);
        //        Camera.main.targetTexture = null;
        //        RenderTexture.active = null; // JC: added to avoid errors
        //        Destroy(rt);
        //        byte[] bytes = screenShot.EncodeToPNG();

        //        _behaviour.RequestUpload("https://www.tamikothiel.com/cgi-bin/LendMeYourFace.cgi", bytes);
        //    }
        //}
    }
}
