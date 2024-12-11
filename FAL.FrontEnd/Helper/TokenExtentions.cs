using System.IdentityModel.Tokens.Jwt;

namespace FAL.FrontEnd.Helper
{
    public class TokenExtentions
    {
        public bool IsTokenValid(string token)
        {
            if (string.IsNullOrEmpty(token)) return false;

            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var jwtToken = tokenHandler.ReadJwtToken(token);

                // Kiểm tra thời hạn Token
                if (jwtToken.ValidTo < DateTime.UtcNow)
                {
                    return false;  // Token hết hạn
                }

                return true;  // Token hợp lệ
            }
            catch
            {
                return false;  // Token không hợp lệ
            }
        }
    }
}
