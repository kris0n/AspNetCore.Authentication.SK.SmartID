namespace AspNetCore.Authentication.SK.SmartId
{
    public enum Trouble
    {
        NoPermissionToIssueRequest,
        UserDoesNotHaveAccountInSmartIdSystem,
        SessionExpired,
        UserRefused,
        Timeout,
        DocumentUnusable,
        WrongVc,
        ResponseSignatureValidationFailed,
        ResponseSignersCertificateHasExpired,
        ResponseSignersCertificateIsNotTrusted,
        Unknown
    }
}