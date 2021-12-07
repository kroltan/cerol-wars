using UnityEngine;

namespace Kroltan.GrapplingHook
{
    public record Color4(float R, float G, float B, float A)
    {
        public static implicit operator Color(Color4 self)
        {
            return new Color(self.R, self.G, self.B, self.A);
        }

        public static implicit operator Color4(Color self)
        {
            return new Color4(self.r, self.g, self.b, self.a);
        }
    }
}