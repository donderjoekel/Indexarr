using System;
using System.Text;

namespace NzbDrone.Common.Extensions
{
    public static class Base64Extensions
    {
        public static string ToBase64(this byte[] bytes)
        {
            return Convert.ToBase64String(bytes);
        }

        public static string ToBase64(this string input)
        {
            return Encoding.UTF8.GetBytes(input).ToBase64();
        }

        public static string ToBase64(this long input)
        {
            return BitConverter.GetBytes(input).ToBase64();
        }

        public static string FromBase64(this string str) =>
            Encoding.UTF8.GetString(Convert.FromBase64String(str));
    }
}
