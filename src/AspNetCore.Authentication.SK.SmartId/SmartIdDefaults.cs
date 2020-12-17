namespace AspNetCore.Authentication.SK.SmartId
{
    public static class SmartIdDefaults
    {
        public const string AuthenticationScheme = "Smart-ID";
        
        public const string DisplayName = "Smart-ID";

        public const string DemoCertificatePublicKey =
            "3082010a02820101008f09708fe030a4c40b359434db47aa94f91c4ac6ff71ce7a2b5f93d3551ef98e1dd4380665a53244ca8e021a1d4757dc12a28fe45d4f1609bbbb8d20a00534918378fb529df381e46f3fcd1dd44fe3b64a207eecc2bed5d3b3258d8a7b15a7a13a633bbd8c79661eb1f0198dd7253ba785459775891374501f44e866e019b699aa29ddc380a1a969ab58e9083e39f9a790b7a96d0ebce83e04251a0770b45c809a9196109b176fba39d6d4b93b057828a1b8411b7e823bcc7464366f27bcd7cab896a259f3975ff3909182da4539fa571b7db0c0f0d5fbadd87b9f9c5f09055cb3969a5fa015d5ea6d69ae9793a5df46aa997b775360627c8d4a755f3b69b8470203010001";

        internal const string DemoRelyingPartyUuid = "00000000-0000-0000-0000-000000000000";
        internal const string DemoRelyingPartyName = "DEMO";
        internal const string DemoHostUrl = "https://sid.demo.sk.ee/smart-id-rp/v1/";
        internal const string LiveHostUrl = "https://rp-api.smart-id.com/v1/";
    }
}