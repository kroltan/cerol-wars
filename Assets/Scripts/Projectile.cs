using UnityEngine;

namespace Kroltan.GrapplingHook
{
    public class Projectile : MonoBehaviour
    {
        public float RopeOffset => _ropeOffset;

        public float AttachmentOffset => _attachmentOffset;
        
        [SerializeField]
        private float _ropeOffset;

        [SerializeField]
        private float _attachmentOffset;
    }
}