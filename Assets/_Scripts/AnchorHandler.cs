using System.Collections;
using System.Threading.Tasks;
using GoogleARCore;
using GoogleARCore.CrossPlatform;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace Example.Photon.CloudAnchor
{
    public class AnchorHandler : MonoBehaviour
    {
        public const string AnchorTag = "Anchor";


        #region Local

        [SerializeField]
        public UnityEvent OnLocalAnchorCreated = new UnityEvent();

        private Anchor _anchor;

        public Anchor Anchor
        {
            get => _anchor;
            private set
            {
                _anchor = value;
                if (value != null)
                {
                    OnLocalAnchorCreated?.Invoke();
                }
            }
        }

        void Update()
        {
            if (CloudAnchor != null)
            {
                return;
            }

            var touchX = 0f;
            var touchY = 0f;

#if UNITY_EDITOR
            if (!Input.GetMouseButton(0) || EventSystem.current.IsPointerOverGameObject())
            {
                return;
            }

            var p = Input.mousePosition;
            touchX = p.x;
            touchY = p.y;
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

        touchX = touch.position.x;
        touchY = touch.position.y;
#endif

            if (!Frame.Raycast(touchX, touchY, TrackableHitFlags.PlaneWithinPolygon, out var hit))
            {
                return;
            }

            if (hit.Trackable is DetectedPlane plane && plane.PlaneType == DetectedPlaneType.HorizontalUpwardFacing)
            {
                if (Anchor != null)
                {
                    Anchor.transform.DetachChildren();
                    GameObject.Destroy(Anchor.gameObject);
                    Anchor = null;
                }

                Anchor = plane.CreateAnchor(hit.Pose);
            }
        }

        #endregion

        #region Cloud

        [SerializeField]
        public UnityEvent OnCloudAnchorResolved = new UnityEvent();

        private XPAnchor _cloudAnchor;

        public XPAnchor CloudAnchor
        {
            get => _cloudAnchor;
            private set
            {
                _cloudAnchor = value;
                OnCloudAnchorResolved?.Invoke();
            }
        }

        public void HostAnchor()
        {
            if (Anchor == null)
            {
                return;
            }

            XPSession.CreateCloudAnchor(Anchor).ThenAction(result =>
            {
                if (result.Response != CloudServiceResponse.Success)
                {
                    return;
                }

                CloudAnchor = result.Anchor;
            });
        }

        public IEnumerator HostAnchorCoroutine()
        {
            if (Anchor == null)
            {
                yield break;
            }

            var create = XPSession.CreateCloudAnchor(Anchor);
            yield return create.WaitForCompletion();

            var anchorResult = create.Result;

            if (anchorResult.Response != CloudServiceResponse.Success)
            {
                yield break;
            }

            CloudAnchor = anchorResult.Anchor;
        }

        public Task<XPAnchor> HostAnchorAsync()
        {
            if (Anchor == null)
            {
                return Task.FromResult<XPAnchor>(null);
            }

            var tcs = new TaskCompletionSource<XPAnchor>();

            XPSession.CreateCloudAnchor(Anchor).ThenAction(result =>
            {
                if (result.Response != CloudServiceResponse.Success)
                {
                    tcs.SetResult(null);
                    return;
                }

                CloudAnchor = result.Anchor;
                tcs.SetResult(CloudAnchor);
            });

            return tcs.Task;
        }


        public void ResolveAnchor(string id)
        {
            XPSession.ResolveCloudAnchor(id).ThenAction((result =>
            {
                if (result.Response != CloudServiceResponse.Success)
                {
                    return;
                }

                CloudAnchor = result.Anchor;
            }));
        }

        public IEnumerator ResolveAnchorCoroutine(string id)
        {
            var resolve = XPSession.ResolveCloudAnchor(id);
            yield return resolve.WaitForCompletion();

            var anchorResult = resolve.Result;
            if (anchorResult.Response != CloudServiceResponse.Success)
            {
                yield break;
            }

            CloudAnchor = anchorResult.Anchor;
        }

        public Task<XPAnchor> ResolveAnchorAsync(string id)
        {
            var tcs = new TaskCompletionSource<XPAnchor>();

            XPSession.ResolveCloudAnchor(id).ThenAction((result =>
            {
                if (result.Response != CloudServiceResponse.Success)
                {
                    Debug.LogError("RESOLVE ERROR! "+result.Response.ToString());
                    tcs.SetResult(null);
                    return;
                }

                CloudAnchor = result.Anchor;
                tcs.SetResult(CloudAnchor);
            }));
            return tcs.Task;
        }

        #endregion
    }
}
