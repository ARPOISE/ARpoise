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
        public List<ArObject> ArObjectsToPlace = null;

        public List<ArObject> ArObjects { get; private set; }
        public List<ArObject> ArObjectsToDelete { get; private set; }
        public List<Poi> ArPois { get; private set; }
        public List<ArAnimation> OnCreateAnimations { get; private set; }
        public List<ArAnimation> OnFollowAnimations { get; private set; }
        public List<ArAnimation> OnFocusAnimations { get; private set; }
        public List<ArAnimation> InFocusAnimations { get; private set; }
        public List<ArAnimation> OnClickAnimations { get; private set; }
        public List<ArAnimation> BillboardAnimations { get; private set; }

        public ArObjectState()
        {
            ArObjects = new List<ArObject>();
            ArObjectsToDelete = new List<ArObject>();
            ArPois = new List<Poi>();
            OnCreateAnimations = new List<ArAnimation>();
            OnFollowAnimations = new List<ArAnimation>();
            OnFocusAnimations = new List<ArAnimation>();
            InFocusAnimations = new List<ArAnimation>();
            OnClickAnimations = new List<ArAnimation>();
            BillboardAnimations = new List<ArAnimation>();
        }

        private void RemoveFromAnimations(ArObject arObject)
        {
            foreach (var animation in BillboardAnimations.Where(x => arObject.Id == x.PoiId).ToList())
            {
                BillboardAnimations.Remove(animation);
            }
            foreach (var animation in OnCreateAnimations.Where(x => arObject.Id == x.PoiId).ToList())
            {
                OnCreateAnimations.Remove(animation);
            }
            foreach (var animation in OnFollowAnimations.Where(x => arObject.Id == x.PoiId).ToList())
            {
                OnFollowAnimations.Remove(animation);
            }
            foreach (var animation in OnFocusAnimations.Where(x => arObject.Id == x.PoiId).ToList())
            {
                OnFocusAnimations.Remove(animation);
            }
            foreach (var animation in InFocusAnimations.Where(x => arObject.Id == x.PoiId).ToList())
            {
                InFocusAnimations.Remove(animation);
            }
            foreach (var animation in OnClickAnimations.Where(x => arObject.Id == x.PoiId).ToList())
            {
                OnClickAnimations.Remove(animation);
            }
        }

        public void DestroyArObjects()
        {
            foreach (var arObject in ArObjectsToDelete)
            {
                RemoveFromAnimations(arObject);
                foreach (var child in arObject.ArObjects)
                {
                    RemoveFromAnimations(child);
                }
                ArObjects.Remove(arObject);
                Object.Destroy(arObject.WrapperObject);
            }
            ArObjectsToDelete.Clear();
        }
    }
}
