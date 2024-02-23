#if !NETCOREAPP
using System;
using System.IO.Hashing;
using System.Runtime.InteropServices;
using System.Security.Cryptography;

namespace Pitchfork.SemanticVersioning
{
    // helps prevent collisions when running on netfx
    internal static class StringHasher
    {
        private static readonly long _seed = GenerateSeed();

        private static long GenerateSeed()
        {
            using var rng = RandomNumberGenerator.Create();

            byte[] bytes = new byte[sizeof(long)];
            rng.GetBytes(bytes);
            return BitConverter.ToInt64(bytes, 0);
        }

        public static int ComputeHash(string? value) => ComputeHash(value.AsSpan());

        public static int ComputeHash(ReadOnlySpan<char> value)
        {
            ulong hash;

            if (value.Length < int.MaxValue / sizeof(char))
            {
                hash = XxHash3.HashToUInt64(MemoryMarshal.AsBytes(value), _seed);
            }
            else
            {
                // append in 512 megachar (1GB) chunks
                const int ChunkSizeInChars = 512 * 1024 * 1024;

                XxHash3 hasher = new XxHash3(_seed);

                while (!value.IsEmpty)
                {
                    ReadOnlySpan<char> thisSegment = value[..Math.Min(value.Length, ChunkSizeInChars)];
                    hasher.Append(MemoryMarshal.AsBytes(thisSegment));
                    value = value[thisSegment.Length..];
                }

                hash = hasher.GetCurrentHashAsUInt64();
            }

            uint hashHi = (uint)(hash >> 32);
            uint hashLo = (uint)hash;
            return (int)(hashHi + (hashLo * 1566083941u)); // from string.GetNonRandomizedHashCode
        }
    }
}
#endif
