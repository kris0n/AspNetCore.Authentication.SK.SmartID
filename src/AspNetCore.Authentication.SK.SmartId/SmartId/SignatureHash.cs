using System.Security.Cryptography;
using System.Text;

namespace AspNetCore.Authentication.SK.SmartID.SmartID
{
    public class SignatureHash : SignableHash
    {
        private SignatureHash(byte[] hash) : base(hash)
        {
        }

        public static SignatureHash GetHash(byte[] document)
        {
            using var shaM = new SHA512Managed();
            var hash = shaM.ComputeHash(document);

            var signatureHash = new SignatureHash(hash);
            return signatureHash;
        }
    }
}