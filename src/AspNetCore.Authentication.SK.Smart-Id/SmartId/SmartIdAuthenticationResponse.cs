using System;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using AspNetCore.Authentication.SK.SmartId.SmartId.Rest.Dao;
using Rfc2253;

namespace AspNetCore.Authentication.SK.SmartId.SmartId
{
    public class SmartIdAuthenticationResponse
    {
        private readonly string _signedHashInBase64;
        private readonly string _signatureValueInBase64;
        private readonly X509Certificate2 _certificate;
        
        public string EndResult { get; }

        public SmartIdAuthenticationResponse(SessionStatus sessionStatus, string signedHashInBase64)
        {
            if (sessionStatus.Result == null)
                throw new ArgumentException("Result required", nameof(sessionStatus));

            EndResult = sessionStatus.Result.EndResult;
            _signedHashInBase64 = signedHashInBase64;
            _signatureValueInBase64 = sessionStatus.Signature?.Value;

            if (sessionStatus.Cert?.Value != null)
                _certificate = new X509Certificate2(Convert.FromBase64String(sessionStatus.Cert.Value));
        }

        public void EnsureValid()
        {
            if (!VerifyResponseEndResult())
                throw new SmartIdTroubleException(TroubleFromEndResult(EndResult));

            if (!VerifySignature())
                throw new SmartIdTroubleException(Trouble.ResponseSignatureValidationFailed);

            if (!VerifyCertificateExpiry())
                throw new SmartIdTroubleException(Trouble.ResponseSignersCertificateHasExpired);

            if (!IsCertificateTrusted())
                throw new SmartIdTroubleException(Trouble.ResponseSignersCertificateIsNotTrusted);
        }

        public ClaimsIdentity GetIdentity()
        {
            if (_certificate == null)
            {
                return null;
            }

            var name = DistinguishedName.Create(_certificate.Subject);

            var identity = new ClaimsIdentity(new Claim[]{}, SmartIdDefaults.AuthenticationScheme);

            FindAndAddClaim(identity, name, ClaimTypes.NameIdentifier, "SERIALNUMBER");
            FindAndAddClaim(identity, name, ClaimTypes.GivenName, "G");
            FindAndAddClaim(identity, name, ClaimTypes.Surname, "SN");
            FindAndAddClaim(identity, name, ClaimTypes.SerialNumber, "SERIALNUMBER", true);
            FindAndAddClaim(identity, name, ClaimTypes.Country, "C");

            return identity;
        }

        private static void FindAndAddClaim(ClaimsIdentity identity, DistinguishedName name, string claimType,
            string nameType, bool splitSerial = false)
        {
            var distinguishedName = name.Rdns.FirstOrDefault(dn => dn.Type.Value == nameType);

            if (distinguishedName == null)
                return;

            var value = distinguishedName.Value.Value;

            if (splitSerial)
                value = value.Split('-', 2)[1];

            identity.AddClaim(new Claim(claimType, value));
        }

        private bool IsCertificateTrusted()
        {
            return _certificate!.Verify();
        }

        private bool VerifyCertificateExpiry()
        {
            return _certificate!.NotAfter >= DateTime.UtcNow;
        }

        private bool VerifySignature()
        {
            var rsaPublicKey = _certificate.GetRSAPublicKey();
            var signedHash = Convert.FromBase64String(_signedHashInBase64);
            var signature = Convert.FromBase64String(_signatureValueInBase64!);

            return rsaPublicKey.VerifyHash(signedHash,
                signature, HashAlgorithmName.SHA512,
                RSASignaturePadding.Pkcs1);
        }

        private bool VerifyResponseEndResult()
        {
            return "OK".Equals(EndResult, StringComparison.InvariantCultureIgnoreCase);
        }

        private static Trouble TroubleFromEndResult(string endResult)
        {
            return endResult switch
            {
                "USER_REFUSED" => Trouble.UserRefused,
                "TIMEOUT" => Trouble.Timeout,
                "DOCUMENT_UNUSABLE" => Trouble.DocumentUnusable,
                "WRONG_VC" => Trouble.WrongVc,
                _ => Trouble.Unknown
            };
        }
    }
}