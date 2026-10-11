using LoanApp.Models;
using LoanApplication.Application.DTO;
using LoanApplication.Application.Interfcae;
using LoanApplication.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Google.Apis.Auth;

namespace LoanApplication.Infrastructure.Repositories
{
    public class AuthService : IAuthService
    {
        private readonly AppDBContext db;
        private readonly IConfiguration config;
        public AuthService(AppDBContext db, IConfiguration config)
        {
            this.db = db;
            this.config = config;
        }

        // REGISTER
        public async Task<bool> RegisterUserAsync(RegisterDto dto)
        {
            var existingUser = await db.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);
            if (existingUser != null)
                return false;
            string hashedPassword = HashPassword(dto.Password);
            var customer = new Customer
            {
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                Age = dto.Age,
                Email = dto.Email,
                Password = hashedPassword,
                MobileNo = dto.MobileNo,
                Pan = dto.Pan,
                AadhaarNo = dto.AadhaarNo,
                EmploymentType = dto.EmploymentType,
                MonthlyIncome = dto.MonthlyIncome,
                MonthlyInvestment = dto.MonthlyInvestment,
                IsEmailVerified = false
            };
            db.Customers.Add(customer);
            await db.SaveChangesAsync();
            var user = new User
            {
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                Email = dto.Email,
                Mobile = dto.MobileNo,
                Password = hashedPassword,

                // Customer role
                RoleId = 1,
                CustomerId = customer.CustomerId,
                IsTwoFactorEnabled = false
            };

            db.Users.Add(user);
            await db.SaveChangesAsync();
            return true;
        }

        // LOGIN
        public async Task<AuthResponseDto> LoginAsync(LoginDto dto)
        {
            var user = await db.Users.Include(u => u.Role).FirstOrDefaultAsync(u => u.Email == dto.Email);
            if (user == null)
            {
                return new AuthResponseDto
                {
                    Message = "Invalid email or password."
                };
            }

            bool passwordValid = VerifyPassword(
                dto.Password,
                user.Password
            );

            if (!passwordValid)
            {
                return new AuthResponseDto
                {
                    Message = "Invalid email or password."
                };
            }
            return ProcessUserLoginFlow(user);
        }

        // GOOGLE LOGIN
        public async Task<AuthResponseDto> GoogleLoginAsync(GoogleAuthDto dto)
        {
            string email = await ExtractEmailFromGoogleToken(dto.IdToken);

            if (string.IsNullOrEmpty(email))
            {
                return new AuthResponseDto
                {
                    Message = "Invalid Google token."
                };
            }

            var user = await db.Users.Include(u => u.Role).FirstOrDefaultAsync(u => u.Email == email);

            // User does not exist
            if (user == null)
            {
                var customer = new Customer
                {
                    FirstName = "GoogleUser",
                    LastName = "OAuth",
                    Age = 25,
                    Email = email,
                    Password = HashPassword("GoogleUser"),
                    MobileNo = "9999999999",
                    Pan = "ABCDE1234F",
                    AadhaarNo = "100020003000",
                    EmploymentType = "Salaried",
                    MonthlyIncome = 30000,
                    MonthlyInvestment = 5000,
                    IsEmailVerified = true
                };
                db.Customers.Add(customer);
                await db.SaveChangesAsync();
                user = new User
                {
                    FirstName = "GoogleUser",
                    LastName = "OAuth",
                    Email = email,
                    Mobile = "9999999999",
                    Password = HashPassword("GoogleUser"),
                    RoleId = 2,
                    CustomerId = customer.CustomerId,
                    IsTwoFactorEnabled = false
                };
                db.Users.Add(user);
                await db.SaveChangesAsync();
                // Role navigation was not loaded for newly created user
                user.Role = await db.Roles.FirstOrDefaultAsync(r => r.RoleId == 1);
            }
            return ProcessUserLoginFlow(user);
        }

        // VERIFY 2FA
        public async Task<AuthResponseDto> VerifyTwoFactorAsync(
            Verify2FaDto dto)
        {
            var userId = GetUserIdFromPreAuthToken(dto.PreAuthToken);
            if (userId == null)
            {
                return new AuthResponseDto
                {
                    Message = "Invalid or expired 2FA session."
                };
            }
            var user = await db.Users.Include(u => u.Role).FirstOrDefaultAsync(u => u.UserId == userId);
            if (user == null ||
                string.IsNullOrEmpty(user.TwoFactorSecret))
            {
                return new AuthResponseDto
                {
                    Message = "2FA setup not found."
                };
            }

            bool validCode = ValidateTotpCode(user.TwoFactorSecret,dto.Code);
            if (!validCode)
            {
                return new AuthResponseDto
                {
                    Message = "Invalid 2FA code."
                };
            }
            string token = GenerateJwtToken(user,false);

            return new AuthResponseDto
            {
                RequiresTwoFactor = false,
                Token = token,
                Message = "2FA verification successful."
            };
        }

        // ENABLE 2FA
        public async Task<Setup2FaDto> EnableTwoFactorAsync(int userId)
        {
            var user = await db.Users.FirstOrDefaultAsync(u => u.UserId == userId);

            if (user == null)
                return null;
            string secret = GenerateBase32Secret();
            user.TwoFactorSecret = secret;
            user.IsTwoFactorEnabled = true;
            await db.SaveChangesAsync();

            return new Setup2FaDto
            {
                ManualEntryKey = secret,
                QrCodeUri =$"otpauth://totp/LoanApp:{user.Email}" +$"?secret={secret}&issuer=LoanApp"
            };
        }

        // LOGIN FLOW
        private AuthResponseDto ProcessUserLoginFlow(User user)
        {
            if (user.IsTwoFactorEnabled)
            {
                string preAuthToken =GenerateJwtToken(user, true);
                return new AuthResponseDto
                {
                    RequiresTwoFactor = true,
                    PreAuthToken = preAuthToken,
                    Message = "2FA required. Verify code to continue."
                };
            }
            string token =GenerateJwtToken(user, false);
            return new AuthResponseDto
            {
                RequiresTwoFactor = false,
                Token = token,
                Message = "Login successful."
            };
        }

        // JWT TOKEN
        private string GenerateJwtToken(User user,bool isPreAuth)
        {
            string keyValue =config["Jwt:Key"]?? "SUPER_SECRET_KEY_1234567890_JWT_LOAN_APP";
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(keyValue));

            var credentials =new SigningCredentials(key,SecurityAlgorithms.HmacSha256);
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier,user.UserId.ToString()),
                new Claim(ClaimTypes.Email,user.Email),
                new Claim("Purpose",isPreAuth? "2FA_Verification": "Access")
            };

            if (!isPreAuth && user.Role != null)
            {
                claims.Add(new Claim(ClaimTypes.Role,user.Role.RoleName));
            }

            DateTime expiry = isPreAuth? DateTime.UtcNow.AddMinutes(5): DateTime.UtcNow.AddHours(2);
            var token = new JwtSecurityToken(issuer: config["Jwt:Issuer"] ?? "LoanAppAPI",
                audience: config["Jwt:Audience"] ?? "LoanAppClients",
                claims: claims,
                expires: expiry,
                signingCredentials: credentials
            );
            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        // GET USER FROM PRE-AUTH TOKEN
        private int? GetUserIdFromPreAuthToken(
            string token)
        {
            try
            {
                var handler = new JwtSecurityTokenHandler();
                string keyValue =config["Jwt:Key"]?? "SUPER_SECRET_KEY_1234567890_JWT_LOAN_APP";
                var key = Encoding.UTF8.GetBytes(keyValue);
                var parameters =new TokenValidationParameters
                    {
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey =new SymmetricSecurityKey(key),
                        ValidateIssuer = false,
                        ValidateAudience = false,
                        ValidateLifetime = true,
                        ClockSkew = TimeSpan.Zero
                    };

                var principal =handler.ValidateToken(token,parameters,out _);

                if (principal.FindFirst("Purpose")?.Value!= "2FA_Verification")
                {
                    return null;
                }

                string id =principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (int.TryParse(id, out int userId))
                    return userId;
                return null;
            }
            catch
            {
                return null;
            }
        }

        // GOOGLE TOKEN
        private async Task<string> ExtractEmailFromGoogleToken(string idToken)
        {
            try
            {
                string clientId =config["Google:ClientId"];
                var settings =new GoogleJsonWebSignature.ValidationSettings
                    {
                        Audience = new[] { clientId }
                    };
                var payload =await GoogleJsonWebSignature.ValidateAsync(idToken,settings);
                return payload.Email ?? "";
            }
            catch
            {
                return "";
            }
        }

        // PASSWORD HASH
        private string HashPassword(string password)
        {
            using var sha = SHA256.Create();
            byte[] bytes =sha.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(bytes);
        }

        private bool VerifyPassword(string inputPassword,string storedHash)
        {
            return HashPassword(inputPassword)== storedHash;
        }

        // BASE32 SECRET
        private string GenerateBase32Secret()
        {
            const string chars ="ABCDEFGHIJKLMNOPQRSTUVWXYZ234567";
            byte[] buffer = new byte[20];
            RandomNumberGenerator.Fill(buffer);
            StringBuilder result =new StringBuilder();
            foreach (byte b in buffer)
            {
                result.Append(
                    chars[b % chars.Length]
                );
            }
            return result.ToString();
        }

        // TOTP
        private bool ValidateTotpCode(string secret,string code)
        {
            long timeStep =DateTimeOffset.UtcNow.ToUnixTimeSeconds()/ 30;

            for (int i = -1; i <= 1; i++)
            {
                if (GenerateCodeForTimeStep(secret,timeStep + i)== code.Trim())
                {
                    return true;
                }
            }
            return false;
        }

        private string GenerateCodeForTimeStep(string secret,long timeStep)
        {
            byte[] key = Base32Decode(secret);
            byte[] timeBytes =BitConverter.GetBytes(timeStep);
            if (BitConverter.IsLittleEndian)
                Array.Reverse(timeBytes);
            using var hmac =new HMACSHA1(key);
            byte[] hash =hmac.ComputeHash(timeBytes);
            int offset =hash[hash.Length - 1] & 0x0F;
            int binaryCode =((hash[offset] & 0x7F) << 24)|((hash[offset + 1] & 0xFF) << 16)|((hash[offset + 2] & 0xFF) << 8)|(hash[offset + 3] & 0xFF);

            return (binaryCode % 1000000).ToString("D6");
        }

        private byte[] Base32Decode(string input)
        {
            const string chars ="ABCDEFGHIJKLMNOPQRSTUVWXYZ234567";
            input = input.Trim().Replace(" ", "").ToUpper();
            List<byte> bytes = new List<byte>();
            int buffer = 0;
            int bitsLeft = 0;

            foreach (char c in input)
            {
                int value = chars.IndexOf(c);
                if (value < 0)
                    continue;
                buffer =(buffer << 5) | value;
                bitsLeft += 5;
                if (bitsLeft >= 8)
                {
                    bitsLeft -= 8;
                    bytes.Add((byte)(buffer >> bitsLeft));
                    buffer &=(1 << bitsLeft) - 1;
                }
            }
            return bytes.ToArray();
        }
    }
}