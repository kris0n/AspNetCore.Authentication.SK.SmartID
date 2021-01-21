namespace AspNetCore.Authentication.SK.SmartID.SmartID
{
    public class Session
    {
        public Session(string id, string verificationCode, string authenticationHashInBase64)
        {
            Id = id;
            VerificationCode = verificationCode;
            AuthenticationHashInBase64 = authenticationHashInBase64;
        }

        public string Id { get; }

        public string VerificationCode { get; }

        public string AuthenticationHashInBase64 { get; }

    }
}