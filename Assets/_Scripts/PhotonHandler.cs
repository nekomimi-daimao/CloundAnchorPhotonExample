using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.Events;

namespace Example.Photon.CloudAnchor
{
    public class PhotonHandler : MonoBehaviourPunCallbacks
    {
        #region Anchor

        public const string AnchorKey = "AnchorKey";

        public string GetAnchorId()
        {
            if (!PhotonNetwork.InRoom)
            {
                return null;
            }

            if (!PhotonNetwork.CurrentRoom.CustomProperties.ContainsKey(AnchorKey))
            {
                return null;
            }

            return PhotonNetwork.CurrentRoom.CustomProperties[AnchorKey] as string;
        }

        public void ShareAnchorId(string anchorId)
        {
            if (!PhotonNetwork.InRoom)
            {
                return;
            }

            if (!string.IsNullOrEmpty(GetAnchorId()))
            {
                return;
            }

            var propsToSet = new ExitGames.Client.Photon.Hashtable
            {
                [AnchorKey] = anchorId
            };

            PhotonNetwork.CurrentRoom.SetCustomProperties(propsToSet);
        }

        #endregion


        #region PUN2 Callbacks

        [SerializeField]
        public OnConnectionStateChangedEvent OnConnectionStateChanged = new OnConnectionStateChangedEvent();

        [System.Serializable]
        public class OnConnectionStateChangedEvent : UnityEvent<bool>
        {
        }

        [SerializeField]
        public OnCloudAnchorIdSharedEvent OnCloudAnchorIdShared = new OnCloudAnchorIdSharedEvent();

        [System.Serializable]
        public class OnCloudAnchorIdSharedEvent : UnityEvent<string>
        {
        }

        private bool _connected = false;

        public bool Connected
        {
            get => _connected;
            private set
            {
                if (_connected == value)
                {
                    return;
                }

                _connected = value;
                OnConnectionStateChanged?.Invoke(value);
            }
        }

        private string _roomName;

        public void Connect(string inputRoomName)
        {
            _roomName = inputRoomName;
            PhotonNetwork.ConnectUsingSettings();
        }

        public override void OnConnectedToMaster()
        {
            base.OnConnectedToMaster();
            PhotonNetwork.JoinOrCreateRoom(_roomName, new RoomOptions(), TypedLobby.Default);
        }

        public override void OnJoinedRoom()
        {
            base.OnJoinedRoom();
            Connected = true;
        }

        public override void OnJoinRandomFailed(short returnCode, string message)
        {
            base.OnJoinRandomFailed(returnCode, message);
            Connected = false;
        }

        public override void OnDisconnected(DisconnectCause cause)
        {
            base.OnDisconnected(cause);
            Connected = false;
        }

        public override void OnRoomPropertiesUpdate(Hashtable propertiesThatChanged)
        {
            base.OnRoomPropertiesUpdate(propertiesThatChanged);
            if (!propertiesThatChanged.ContainsKey(AnchorKey))
            {
                return;
            }

            var key = propertiesThatChanged[AnchorKey] as string;
            if (string.IsNullOrEmpty(key))
            {
                return;
            }

            OnCloudAnchorIdShared?.Invoke(key);
        }

        #endregion
    }
}
