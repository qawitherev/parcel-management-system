using ParcelManagement.Core.Entities;

namespace ParcelManagement.Api.Utility
{
    public class EnumUtils
    {
        public static T? ToEnumOrNull<T>(string stringEnum) where T : struct, Enum
        {
            if (Enum.TryParse<T>(stringEnum, out var status))
            {
                return status;
            }
            return null;
        }

        public static T? ToEnumOrThrow<T>(string stringEnum) where T : struct, Enum
        {
            if (Enum.TryParse<T>(stringEnum, out var enumRes))
            {
                return enumRes;
            }
            else
            {
                throw new InvalidCastException($"Invalid type: {stringEnum}");
            }
        }
    }
}