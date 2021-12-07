using UnityEngine;

namespace Kroltan.GrapplingHook
{
    public abstract class BaseSurfaceAim : MonoBehaviour
    {
        public abstract RaycastHit? Target { get; }
    }
}