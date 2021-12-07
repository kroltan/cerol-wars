using Cinemachine;
using Kroltan.GrapplingHook.UI;
using Mirror;
using UnityEngine;

namespace Kroltan.GrapplingHook.Networking
{
    public class NetworkPlayer : NetworkBehaviour
    {
        
        [SerializeField]
        private LayerMask _damageLayer;

        private readonly RaycastHit[] _damageHits = new RaycastHit[8];
        private GrappleGun _gun;
        private Rope _rope;

        private void Start()
        {
            _gun = GetComponentInChildren<GrappleGun>(true);
            _gun.Attached += (point, normal) =>
            {
                if (isLocalPlayer)
                {
                    CmdSetAttachment(point, normal);
                }
            };
            _gun.Detached += () =>
            {
                if (isLocalPlayer)
                {
                    CmdClearAttachment();
                }
            };
            
            GetComponentInChildren<PlayerMovement>().enabled = isLocalPlayer;
            GetComponentInChildren<BaseSurfaceAim>().enabled = isLocalPlayer;
            GetComponentInChildren<CinemachineVirtualCameraBase>().enabled = isLocalPlayer;

            _rope = GetComponentInChildren<Rope>(true);
        }

        private void Update()
        {
            if (!isServer)
            {
                return;
            }

            if (transform.position.y < -50)
            {
                Respawn();
                return;
            }

            if (!_rope.isActiveAndEnabled || _rope.CurrentSegment == null)
            {
                return;
            }

            var delta = _rope.CurrentSegment.End - _rope.CurrentSegment.Start;
            var hitCount = Physics.RaycastNonAlloc(
                _rope.CurrentSegment.Start,
                delta,
                _damageHits,
                delta.magnitude,
                _damageLayer
            );

            for (var i = 0; i < hitCount; i++)
            {
                var hit = _damageHits[i];
                if (hit.transform == transform || !hit.transform.TryGetComponent(out NetworkPlayer victim))
                {
                    continue;
                }

                Scoreboard.Instance.AddScore(
                    GetComponent<Nameplate>().Name,
                    victim.transform.position,
                    victim.GetComponent<Rigidbody>().velocity
                );

                victim.Respawn();
            }
        }
        
        [Server]
        private void Respawn()
        {
            var connection = connectionToClient;

            NetworkServer.DestroyPlayerForConnection(connection);

            var position = NetworkManager.singleton.GetStartPosition();
            var newPlayer = Instantiate(
                NetworkManager.singleton.playerPrefab,
                position.position,
                position.rotation
            );

            NetworkServer.AddPlayerForConnection(connection, newPlayer);
        }

        [Command]
        private void CmdSetAttachment(Vector3 point, Vector3 normal)
        {
            _gun.Attach(point, normal);
            SetAttachmentRpc(point, normal);
        }

        [Command]
        private void CmdClearAttachment()
        {
            _gun.Detach();
            ClearAttachmentRpc();
        }

        [ClientRpc(includeOwner = false)]
        private void SetAttachmentRpc(Vector3 point, Vector3 normal)
        {
            _gun.Attach(point, normal);
        }

        [ClientRpc(includeOwner = false)]
        private void ClearAttachmentRpc()
        {
            _gun.Detach();
        }
    }
}