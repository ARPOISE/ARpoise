// Derived from - Live Training: Shop UI with Runtime Scroll Lists - https://unity3d.com/learn/tutorials/topics/user-interface-ui/intro-and-setup

using UnityEngine;
using UnityEngine.UI;

namespace com.arpoise.arpoiseapp
{
    public class LayerButton : MonoBehaviour
    {
        public Button button;
        public Text nameLabel;
        public Text line2Label;
        public Text line3Label;
        public Image iconImage;

        private ArItem _item;
        private ArBehaviourUserInterface _behaviour;

        protected void Start()
        {
            button.onClick.AddListener(HandleClick);
        }

        public void Setup(ArItem currentItem, ArBehaviourUserInterface behaviour)
        {
            _item = currentItem;
            nameLabel.text = _item.itemName;
            line2Label.text = _item.line2;
            line3Label.text = _item.distance + "m" + (string.IsNullOrEmpty(_item.line3) ? string.Empty : ", " + _item.line3);
            iconImage.sprite = _item.icon;
            _behaviour = behaviour;
        }

        public void HandleClick()
        {
            _behaviour.HandleLayerButtonClick(_item);
        }
    }
}
