using System;

namespace AspNetCore.Authentication.SK.SmartID.SmartID
{
    public class SmartIdTroubleException : Exception
    {
        public Trouble Trouble { get; }

        public SmartIdTroubleException(Trouble trouble) : base(trouble.ToString())
        {
            Trouble = trouble;
        }
    }
}