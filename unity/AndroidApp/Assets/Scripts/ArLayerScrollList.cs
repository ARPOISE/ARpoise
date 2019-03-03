// Derived from - Live Training: Shop UI with Runtime Scroll Lists - https://unity3d.com/learn/tutorials/topics/user-interface-ui/intro-and-setup

using UnityEngine;
using System.Collections.Generic;

namespace com.arpoise.arpoiseapp
{
    [System.Serializable]
    public class Item
    {
        public string itemName;
        public string layerName;
        public int distance;
        public string url;
        public string line2;
        public string line3;
        public Sprite icon;
    }

    public class ArLayerScrollList 
    {
        private Transform _contentPanel;
        private SimpleObjectPool _buttonObjectPool;

        public ArLayerScrollList(Transform contentPanel, SimpleObjectPool buttonObjectPool)
        {
            _contentPanel = contentPanel;
            _buttonObjectPool = buttonObjectPool;
        }

        public void AddButtons(List<Item> itemList, ArBehaviour behaviour)
        {
            if (itemList != null)
            {
                foreach (var item in itemList)
                {
                    var newButton = _buttonObjectPool.GetObject();
                    newButton.transform.SetParent(_contentPanel);

                    var button = newButton.GetComponent<LayerButton>();
                    button.Setup(item, behaviour);
                }
            }
        }

        public void RemoveButtons()
        {
            while (_contentPanel.childCount > 0)
            {
                var toRemove = _contentPanel.GetChild(0).gameObject;
                _buttonObjectPool.ReturnObject(toRemove);
            }
        }
    }
}
