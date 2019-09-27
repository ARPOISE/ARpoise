using com.arpoise.arpoiseapp;
using UnityEngine;
using UnityEngine.XR.iOS;

public class GenerateImageAnchor : MonoBehaviour
{
    private GameObject _gameObject;
    private bool _gameObjectCreated = false;

    public TriggerObject TriggerObject { get; set; }
    public ArBehaviourImage ArBehaviour { get; set; }

    // Use this for initialization
    private void Start()
    {
        UnityARSessionNativeInterface.ARImageAnchorAddedEvent += AddImageAnchor;
        UnityARSessionNativeInterface.ARImageAnchorUpdatedEvent += UpdateImageAnchor;
        UnityARSessionNativeInterface.ARImageAnchorRemovedEvent += RemoveImageAnchor;
    }

    private void AddImageAnchor(ARImageAnchor arImageAnchor)
    {
        Debug.LogFormat("image anchor added[{0}] : tracked => {1}", arImageAnchor.Identifier, arImageAnchor.IsTracked);
        if (arImageAnchor.ReferenceImageName == "dynamicImage")
        {
            Vector3 position = UnityARMatrixOps.GetPosition(arImageAnchor.Transform);
            Quaternion rotation = UnityARMatrixOps.GetRotation(arImageAnchor.Transform);

            var arObjectState = ArBehaviour.ArObjectState;
            if (arObjectState != null && TriggerObject != null && !_gameObjectCreated)
            {
                _gameObjectCreated = true;

                lock (arObjectState)
                {
                    transform.position = position;
                    transform.rotation = rotation;

                    var result = ArBehaviour.CreateArObject(
                        arObjectState,
                        TriggerObject.gameObject,
                        null,
                        transform,
                        TriggerObject.poi,
                        TriggerObject.poi.id,
                        out _gameObject
                        );
                    if (!ArBehaviourPosition.IsEmpty(result))
                    {
                        ArBehaviour.ErrorMessage = result;
                        return;
                    }
                }
            }
        }
    }

    private void UpdateImageAnchor(ARImageAnchor arImageAnchor)
    {
        Debug.LogFormat("image anchor updated[{0}] : tracked => {1}", arImageAnchor.Identifier, arImageAnchor.IsTracked);
        if (arImageAnchor.ReferenceImageName == "dynamicImage")
        {
            if (_gameObject != null)
            {
                if (arImageAnchor.IsTracked)
                {
                    if (!_gameObject.activeSelf)
                    {
                        _gameObject.SetActive(true);
                    }
                    _gameObject.transform.position = UnityARMatrixOps.GetPosition(arImageAnchor.Transform);
                    _gameObject.transform.rotation = UnityARMatrixOps.GetRotation(arImageAnchor.Transform);
                }
                else if (_gameObject.activeSelf)
                {
                    //_imageAnchorGO.SetActive(false);
                }
            }
        }
    }

    private void RemoveImageAnchor(ARImageAnchor arImageAnchor)
    {
        Debug.LogFormat("image anchor removed[{0}] : tracked => {1}", arImageAnchor.Identifier, arImageAnchor.IsTracked);
        if (arImageAnchor.ReferenceImageName == "dynamicImage")
        {
            if (_gameObject != null)
            {
                GameObject.Destroy(_gameObject);
                _gameObject = null;
            }
        }
    }

    private void OnDestroy()
    {
        UnityARSessionNativeInterface.ARImageAnchorAddedEvent -= AddImageAnchor;
        UnityARSessionNativeInterface.ARImageAnchorUpdatedEvent -= UpdateImageAnchor;
        UnityARSessionNativeInterface.ARImageAnchorRemovedEvent -= RemoveImageAnchor;
    }

    // Update is called once per frame
    private void Update()
    {
    }
}
