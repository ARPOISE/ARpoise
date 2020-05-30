// Derived from - Live Training: Shop UI with Runtime Scroll Lists - https://unity3d.com/learn/tutorials/topics/user-interface-ui/intro-and-setup

using UnityEngine;
using System.Collections.Generic;

namespace com.arpoise.arpoiseapp
{
    public class ArLayerScrollList 
    {
        private readonly Transform _contentPanel;
        private readonly SimpleObjectPool _buttonObjectPool;

        public ArLayerScrollList(Transform contentPanel, SimpleObjectPool buttonObjectPool)
        {
            _contentPanel = contentPanel;
            _buttonObjectPool = buttonObjectPool;
        }

        public void AddButtons(List<ArItem> itemList, ArBehaviourUserInterface behaviour)
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
