using System;

namespace AspNetCore.Authentication.SK.SmartId.SmartId
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