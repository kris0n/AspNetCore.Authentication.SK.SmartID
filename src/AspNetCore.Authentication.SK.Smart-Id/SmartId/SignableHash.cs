using System;

namespace AspNetCore.Authentication.SK.SmartId.SmartId
{
    public class SignableHash
    {
        private readonly byte[] _hash;

        protected SignableHash(byte[] hash)
        {
            _hash = hash;
        }

        public string HashInBase64()
        {
            return Convert.ToBase64String(_hash);
        }

        public string CalculateVerificationCode()
        {
            return VerificationCodeCalculator.Calculate(_hash);
        }
    }
}