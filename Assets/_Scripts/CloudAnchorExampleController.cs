using System.Collections.Generic;
using System.Text;
using Photon.Pun;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Example.Photon.CloudAnchor
{
    /// <summary>
    /// connect to photon
    /// check hosted anchor
    ///
    /// 1. not hosted
    /// create local anchor
    /// host local anchor to cloud anchor
    /// get cloud anchor id, set room properties
    ///
    /// 2. already hosted
    /// resolve cloud anchor
    ///
    /// show cloud anchor, shoot andy with photon.instantiate
    /// 
    /// </summary>
    public class CloudAnchorExampleController : MonoBehaviour
    {
        private void Start()
        {
            Initialize();
        }

        private void Update()
        {
            if (_anchorHandler.CloudAnchor != null)
            {
                ShootAndy();
            }
        }

        private void Initialize()
        {
            _photonHandler.OnConnectionStateChanged.AddListener(OnConnectionStateChanged);
            _photonHandler.OnCloudAnchorIdShared.AddListener(OnCloudAnchorId);
            _connectButton.onClick.AddListener(Connect);

            _anchorHandler.OnLocalAnchorCreated.AddListener(OnLocalAnchorCreated);
            _anchorHandler.OnCloudAnchorResolved.AddListener(OnCloudAnchorResolved);
            _hostButton.onClick.AddListener(HostAnchor);
        }


        #region Photon

#pragma warning disable 649
        [HeaderAttribute("Photon")]
        [SerializeField]
        private PhotonHandler _photonHandler;

        [SerializeField]
        private GameObject _photonCanvas;

        [SerializeField]
        private InputField _roomName;

        [SerializeField]
        private Button _connectButton;
#pragma warning restore 649

        private void Connect()
        {
            var roomName = _roomName.text;
            if (string.IsNullOrEmpty(roomName))
            {
                AddLog("Photon:roomName empty!");
                return;
            }

            AddLog($"Photon:connecting to {roomName}...");
            _photonHandler.Connect(roomName);
        }

        private void OnConnectionStateChanged(bool connected)
        {
            AddLog(connected ? "Photon:connected!" : "Photon:disconnected...");
            _photonCanvas?.SetActive(!connected);
            _anchorCanvas?.SetActive(connected);
            if (!connected)
            {
                return;
            }

            var cloudAnchorId = _photonHandler.GetAnchorId();
            if (string.IsNullOrEmpty(cloudAnchorId))
            {
                AddLog("Photon:no anchor hosted");
            }
            else
            {
                AddLog("Photon:already anchor hosted");
                OnCloudAnchorId(cloudAnchorId);
            }
        }

        #endregion


        #region Anchor

#pragma warning disable 649
        [HeaderAttribute("Anchor")]
        [SerializeField]
        private AnchorHandler _anchorHandler;

        [SerializeField]
        private Transform _anchor;

        [SerializeField]
        private GameObject[] _cloudSign;

        [SerializeField]
        private GameObject _anchorCanvas;

        [SerializeField]
        private Button _hostButton;
#pragma warning restore 649

        private void OnLocalAnchorCreated()
        {
            if (_anchorHandler.Anchor == null)
            {
                return;
            }

            var ts = _anchorHandler.Anchor.transform;
            _anchor.SetParent(ts);
            _anchor.transform.SetPositionAndRotation(ts.position, ts.rotation);

            foreach (var o in _cloudSign)
            {
                o.SetActive(false);
            }

            AddLog("Anchor:local anchor created!");
        }

        private void OnCloudAnchorResolved()
        {
            var hosted = _anchorHandler.CloudAnchor != null;
            foreach (var o in _cloudSign)
            {
                o.SetActive(hosted);
            }

            _anchorCanvas.SetActive(!hosted);

            PhotonNetwork.SetInterestGroups(AndyGroup, hosted);

            if (!hosted)
            {
                return;
            }

            var ts = _anchorHandler.CloudAnchor.transform;
            _anchor.SetParent(ts);
            _anchor.transform.SetPositionAndRotation(ts.position, ts.rotation);
        }


        private async void HostAnchor()
        {
            AddLog("Anchor:hosting cloud anchor...");
            await _anchorHandler.HostAnchorAsync();
            if (_anchorHandler.CloudAnchor == null)
            {
                AddLog("Anchor:hosting cloud anchor failed");
                return;
            }

            AddLog("Anchor:cloud anchor hosted!");

            _photonHandler.ShareAnchorId(_anchorHandler.CloudAnchor.CloudId);
        }

        private async void OnCloudAnchorId(string cloudAnchorId)
        {
            AddLog(cloudAnchorId);
            if (string.Equals(_anchorHandler?.CloudAnchor?.CloudId, cloudAnchorId))
            {
                return;
            }

            AddLog("Anchor:resolving cloud anchor...");
            await _anchorHandler.ResolveAnchorAsync(cloudAnchorId);
            if (_anchorHandler.CloudAnchor == null)
            {
                AddLog("Anchor:resolving cloud anchor failed");
                return;
            }

            AddLog("Anchor:cloud anchor resolved!");
        }

        #endregion

        #region Andy

#pragma warning disable 649
        [HeaderAttribute("Andy")]
        [SerializeField]
        private GameObject _andyPrefab;
#pragma warning restore 649

        public const byte AndyGroup = 1;

        private const float FrontDistance = 0.2f;

        private void ShootAndy()
        {
            var touchPosition = Vector3.zero;
#if UNITY_EDITOR
            if (!Input.GetMouseButton(0) || EventSystem.current.IsPointerOverGameObject())
            {
                return;
            }

            touchPosition = Input.mousePosition;
#else
        if (Input.touchCount < 1)
        {
            return;
        }

        var touch = Input.GetTouch(0);

        if (touch.phase != TouchPhase.Began)
        {
            return;
        }

        if (EventSystem.current.IsPointerOverGameObject(touch.fingerId))
        {
            return;
        }

        touchPosition = touch.position;
#endif
            touchPosition.z = FrontDistance;
            var forward = Camera.main.transform.forward;
            var andy = PhotonNetwork.Instantiate(
                _andyPrefab.name,
                Camera.main.ScreenToWorldPoint(touchPosition),
                Quaternion.Euler(0, UnityEngine.Random.Range(0, 360), 0),
                AndyGroup
            );
            andy.GetComponent<Rigidbody>().AddForce(forward * 2 + Vector3.up, ForceMode.Impulse);
        }

        #endregion

        #region Log

#pragma warning disable 649
        [HeaderAttribute("Log")]
        [SerializeField]
        private Text _logText;
#pragma warning restore 649

        private const int LogLinesCount = 5;
        private readonly Queue<string> _logQueue = new Queue<string>(LogLinesCount);
        private readonly StringBuilder _stringBuilder = new StringBuilder(LogLinesCount);

        public void AddLog(string log)
        {
            if (_logQueue.Count >= LogLinesCount)
            {
                _logQueue.Dequeue();
            }

            _logQueue.Enqueue(log);
            _stringBuilder.Clear();

            foreach (var s in _logQueue)
            {
                _stringBuilder.AppendLine(s);
            }

            _logText.text = _stringBuilder.ToString();
        }

        #endregion
    }
}
