using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace GpuShare.Frontend.Extensions
{
    public static class EnumExtensions
    {
        public static string GetDisplayName(this Enum value)
        {
            var field = value.GetType().GetField(value.ToString());

            var attr = field?
                .GetCustomAttribute<DisplayAttribute>();

            return attr?.Name ?? value.ToString();
        }
    }
}
