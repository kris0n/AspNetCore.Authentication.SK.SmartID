using System;
using System.Linq;
using System.Security.Cryptography;

namespace AspNetCore.Authentication.SK.SmartID.SmartID
{
    public static class VerificationCodeCalculator
    {
        public static string Calculate(byte[] documentHash)
        {
            using var shaM = new SHA256Managed();

            var hash = shaM.ComputeHash(documentHash);
            var code = BitConverter.ToUInt16(hash.TakeLast(2).Reverse().ToArray()) % 10000;

            return $"{code:0000}";
        }
    }
}
