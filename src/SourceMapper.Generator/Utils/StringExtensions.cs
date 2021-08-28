using System;
using System.Collections.Generic;
using System.Text;

namespace SourceMapper.Generator.Utils
{
    public static class StringExtensions
    {
        public static string ToMd5(this string s)
        {
            using var provider = System.Security.Cryptography.MD5.Create();
            var builder = new StringBuilder();

            foreach (var b in provider.ComputeHash(Encoding.UTF8.GetBytes(s)))
            {
                builder.Append(b.ToString("x2").ToLower());
            }

            return builder.ToString();
        }
    }
}
