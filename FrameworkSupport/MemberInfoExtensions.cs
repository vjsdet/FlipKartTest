using System;
using System.Reflection;

namespace FrameworkSupport
{
    public static class MemberInfoExtensions
    {
        /// <summary>
        /// Checks if a custom attribute exists
        /// </summary>
        /// <param name="attributeType">The attribute type to look for</param>
        /// <param name="inherit">Whether to look for the attribute down the inheritance chain</param>
        /// <returns>boolean indicating the existence of a custom attribute</returns>
        public static bool HasCustomAttribute(this MemberInfo memberInfo, Type attributeType, bool inherit)
        {
            return memberInfo.GetCustomAttributes(attributeType, inherit).Length > 0;
        }
    }
}