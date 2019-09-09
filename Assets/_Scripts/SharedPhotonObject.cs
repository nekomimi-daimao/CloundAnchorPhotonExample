using System.Collections;
using Photon.Pun;
using UnityEngine;

namespace Example.Photon.CloudAnchor
{
    [RequireComponent(typeof(PhotonView))]
    public class SharedPhotonObject : MonoBehaviourPunCallbacks, IPunInstantiateMagicCallback
    {
        public void OnPhotonInstantiate(PhotonMessageInfo info)
        {
            var anchor = GameObject.FindWithTag(AnchorHandler.AnchorTag);
            if (anchor == null)
            {
                return;
            }

            this.transform.SetParent(anchor.transform);

            StartCoroutine(Jump());
        }

        private readonly Vector3 JumpForce = new Vector3(0, 4f, 0f);

        private IEnumerator Jump()
        {
            var ts = this.transform;
            var wait = new WaitUntil(() => ts.localPosition.y < 0);

            yield return wait;
            
            if (!this.photonView.IsMine)
            {
                yield break;
            }

            var rigid = this.GetComponent<Rigidbody>();
            rigid.velocity = Vector3.zero;
            rigid.AddForce(ts.forward * 2 + JumpForce, ForceMode.Impulse);
            yield return new WaitForSeconds(0.2f);
            yield return wait;

            if (!this.photonView.IsMine)
            {
                yield break;
            }

            PhotonNetwork.Destroy(this.gameObject);
        }
    }
}
