using System;

namespace FrameworkSupport
{
    public class SecretKeyBuilder
    {
        public static string GetKey(Guid? guidKey = null)
        {
            guidKey ??= Guid.NewGuid();
            return guidKey.ToString().Replace("-", "");
        }
    }
}