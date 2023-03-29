using System;

namespace TalkToAPI.Helpers
{
    public class TokenDTO
    {
        public string Token { get; set; }
        public DateTime Expiration { get; set; }
        public string RefreshToken { get; set; }
        public DateTime ExpirationRefreshToken { get; set; }
    }
}