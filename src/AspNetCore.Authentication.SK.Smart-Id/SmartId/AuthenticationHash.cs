using System.Security.Cryptography;

namespace AspNetCore.Authentication.SK.SmartId.SmartId
{
    public class AuthenticationHash : SignableHash
    {
        private AuthenticationHash(byte[] hash) : base(hash)
        {
        }

        public static AuthenticationHash GenerateRandomHash()
        {
            using var shaM = new SHA512Managed();
            var randomBytes = GetRandomBytes();
            var hash = shaM.ComputeHash(randomBytes);

            var authenticationHash = new AuthenticationHash(hash);
            return authenticationHash;
        }

        private static byte[] GetRandomBytes()
        {
            var data = new byte[64];

            using var crypto = new RNGCryptoServiceProvider();
            crypto.GetBytes(data);

            return data;
        }
    }
}