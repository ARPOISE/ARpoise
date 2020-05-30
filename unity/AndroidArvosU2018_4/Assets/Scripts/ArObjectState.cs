/*
ArObjectState.cs - ArObject state for Arpoise.

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

using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace com.arpoise.arpoiseapp
{
    public class ArObjectState
    {
        public volatile bool IsDirty = false;

        private readonly List<ArAnimation> _onCreateAnimations = new List<ArAnimation>();
        private readonly List<ArAnimation> _onFollowAnimations = new List<ArAnimation>();
        private readonly List<ArAnimation> _onFocusAnimations = new List<ArAnimation>();
        private readonly List<ArAnimation> _inFocusAnimations = new List<ArAnimation>();
        private readonly List<ArAnimation> _onClickAnimations = new List<ArAnimation>();
        private readonly List<ArAnimation> _billboardAnimations = new List<ArAnimation>();

        private List<ArAnimation> _allAnimations = null;
        private List<ArAnimation> AllAnimations
        {
            get
            {
                if (_allAnimations == null)
                {
                    _allAnimations = _onCreateAnimations
                        .Concat(_onFollowAnimations)
                        .Concat(_onFocusAnimations)
                        .Concat(_inFocusAnimations)
                        .Concat(_onClickAnimations)
                        .ToList();
                }
                return _allAnimations;
            }
            set
            {
                _allAnimations = value;
            }
        }
        private readonly List<ArObject> _arObjects = new List<ArObject>();
        public IEnumerable<ArObject> ArObjects { get { return _arObjects; } }
        public List<ArObject> ArObjectsToDelete { get; private set; }
        public List<ArObject> ArObjectsToPlace { get; private set; }
        public List<Poi> ArPois { get; private set; }

        public ArObjectState()
        {
            ArObjectsToDelete = new List<ArObject>();
            ArObjectsToPlace = null;
            ArPois = new List<Poi>();
        }

        public void SetArObjectsToPlace()
        {
            ArObjectsToPlace = ArObjects.Where(x => !x.IsRelative).ToList();
        }

        public void AddOnCreateAnimation(ArAnimation animation)
        {
            _onCreateAnimations.Add(animation);
            AllAnimations = null;
        }

        public void AddOnFollowAnimation(ArAnimation animation)
        {
            _onFollowAnimations.Add(animation);
            AllAnimations = null;
        }

        public void AddOnFocusAnimation(ArAnimation animation)
        {
            _onFocusAnimations.Add(animation);
            AllAnimations = null;
        }

        public void AddInFocusAnimation(ArAnimation animation)
        {
            _inFocusAnimations.Add(animation);
            AllAnimations = null;
        }

        public void AddOnClickAnimation(ArAnimation animation)
        {
            _onClickAnimations.Add(animation);
            AllAnimations = null;
        }

        public void AddBillboardAnimation(ArAnimation animation)
        {
            _billboardAnimations.Add(animation);
            AllAnimations = null;
        }

        private void RemoveFromAnimations(ArObject arObject)
        {
            _billboardAnimations.RemoveAll(x => arObject.Id == x.PoiId);
            _onCreateAnimations.RemoveAll(x => arObject.Id == x.PoiId);
            _onFollowAnimations.RemoveAll(x => arObject.Id == x.PoiId);
            _onFocusAnimations.RemoveAll(x => arObject.Id == x.PoiId);
            _inFocusAnimations.RemoveAll(x => arObject.Id == x.PoiId);
            _onClickAnimations.RemoveAll(x => arObject.Id == x.PoiId);
            AllAnimations = null;
        }

        public void Add(ArObject arObject)
        {
            _arObjects.Add(arObject);
        }

        private void DestroyArObject(ArObject arObject)
        {
            RemoveFromAnimations(arObject);
            foreach (var child in arObject.ArObjects)
            {
                RemoveFromAnimations(child);
            }
            _arObjects.Remove(arObject);
            Object.Destroy(arObject.WrapperObject);
        }

        public void DestroyArObjects()
        {
            foreach (var arObject in ArObjectsToDelete)
            {
                DestroyArObject(arObject);
            }
            ArObjectsToDelete.Clear();
        }

        public int Count
        {
            get
            {
                return _arObjects.Count;
            }
        }

        public int NumberOfAnimations
        {
            get
            {
                return AllAnimations.Count;
            }
        }

        public bool HandleAnimations(long startTicks, long now)
        {
            var hit = false;

            foreach (var arAnimation in _billboardAnimations)
            {
                arAnimation.Wrapper.transform.LookAt(Camera.main.transform);
            }

            var inFocusAnimationsToStop = _inFocusAnimations.Where(x => x.IsActive).ToList();

            if (_onFocusAnimations.Count > 0 || _inFocusAnimations.Count > 0)
            {
                var ray = Camera.main.ScreenPointToRay(new Vector3(Camera.main.pixelWidth / 2, Camera.main.pixelHeight / 2, 0f));

                RaycastHit hitInfo;
                if (Physics.Raycast(ray, out hitInfo, 1500f))
                {
                    var objectHit = hitInfo.transform.gameObject;
                    if (objectHit != null)
                    {
                        foreach (var arAnimation in _onFocusAnimations.Where(x => objectHit.Equals(x.GameObject)))
                        {
                            if (!arAnimation.IsActive)
                            {
                                arAnimation.Activate(startTicks, now);
                            }
                        }

                        foreach (var arAnimation in _inFocusAnimations.Where(x => objectHit.Equals(x.GameObject)))
                        {
                            if (!arAnimation.IsActive)
                            {
                                arAnimation.Activate(startTicks, now);
                            }
                            inFocusAnimationsToStop.Remove(arAnimation);
                        }
                    }
                }
            }

            if (_onClickAnimations.Count > 0 && Input.GetMouseButtonDown(0))
            {
                var ray = Camera.main.ScreenPointToRay(Input.mousePosition);

                RaycastHit hitInfo;
                if (Physics.Raycast(ray, out hitInfo, 1500f))
                {
                    var objectHit = hitInfo.transform.gameObject;
                    if (objectHit != null)
                    {
                        foreach (var arAnimation in _onClickAnimations.Where(x => objectHit.Equals(x.GameObject)))
                        {
                            hit = true;
                            if (!arAnimation.IsActive)
                            {
                                arAnimation.Activate(startTicks, now);
                            }
                        }
                    }
                }
            }

            var animations = AllAnimations;

            foreach (var arAnimation in animations)
            {
                if (arAnimation.JustActivated && arAnimation.GameObject != null)
                {
                    var audioSource = arAnimation.GameObject.GetComponent<AudioSource>();
                    if (audioSource != null)
                    {
                        audioSource.Play();
                    }
                }

                if (inFocusAnimationsToStop.Contains(arAnimation))
                {
                    arAnimation.Stop(startTicks, now);
                    inFocusAnimationsToStop.Remove(arAnimation);
                }
                else
                {
                    arAnimation.Animate(startTicks, now);
                }

                if (arAnimation.JustStopped && !string.IsNullOrWhiteSpace(arAnimation.FollowedBy))
                {
                    var animationsToFollow = arAnimation.FollowedBy.Split(',');
                    if (animationsToFollow != null)
                    {
                        foreach (var animationToFollow in animationsToFollow)
                        {
                            if (!string.IsNullOrWhiteSpace(animationToFollow))
                            {
                                var animationName = animationToFollow.Trim();
                                foreach (var arAnimationToFollow in animations.Where(x => animationName.Equals(x.Name)))
                                {
                                    if (!arAnimationToFollow.IsActive)
                                    {
                                        arAnimationToFollow.Activate(startTicks, now);
                                        if (arAnimationToFollow.JustActivated && arAnimationToFollow.GameObject != null)
                                        {
                                            var audioSource = arAnimationToFollow.GameObject.GetComponent<AudioSource>();
                                            if (audioSource != null)
                                            {
                                                audioSource.Play();
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            var toBeDestroyed = animations.Where(x => x.IsToBeDestroyed).ToList();
            foreach (var arAnimation in toBeDestroyed)
            {
                var arObject = ArObjects.FirstOrDefault(x => x.Id == arAnimation.PoiId);
                if (arObject != null)
                {
                    DestroyArObject(arObject);
                }
            }

            return hit;
        }

        public List<ArObject> ArObjectsToBeDuplicated()
        {
            var result = new List<ArObject>();
            foreach (var arAnimation in AllAnimations.Where(x => x.IsToBeDuplicated))
            {
                arAnimation.IsToBeDuplicated = false;
                foreach (var arObject in ArObjects.Where(x => x.Id == arAnimation.PoiId))
                {
                    result.Add(arObject);
                }
            }
            return result.Distinct().ToList();
        }
    }
}
