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
        private readonly List<ArObject> _arObjects = new List<ArObject>();
        private readonly List<ArObject> _arObjectsToDelete = new List<ArObject>();
        private readonly List<Poi> _arPois = new List<Poi>();
        private readonly List<ArAnimation> _onCreateAnimations = new List<ArAnimation>();
        private readonly Dictionary<GameObject, ArAnimation> _onFocusAnimations = new Dictionary<GameObject, ArAnimation>();
        private readonly List<ArAnimation> _billboardAnimations = new List<ArAnimation>();

        public volatile bool IsDirty = false;
        public ArAnimation OnFocusAnimation = null;
        public List<ArObject> ArObjectsToPlace = null;

        public List<ArObject> ArObjects { get { return _arObjects; } }
        public List<ArObject> ArObjectsToDelete { get { return _arObjectsToDelete; } }
        public List<Poi> ArPois { get { return _arPois; } }
        public List<ArAnimation> OnCreateAnimations { get { return _onCreateAnimations; } }
        public Dictionary<GameObject, ArAnimation> OnFocusAnimations { get { return _onFocusAnimations; } }
        public List<ArAnimation> BillboardAnimations { get { return _billboardAnimations; } }

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
            foreach (var pair in OnFocusAnimations.Where(x => arObject.Id == x.Value.PoiId).ToList())
            {
                if (OnFocusAnimation == pair.Value)
                {
                    OnFocusAnimation = null;
                }
                OnFocusAnimations.Remove(pair.Key);
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
