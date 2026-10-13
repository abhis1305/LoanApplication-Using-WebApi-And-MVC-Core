using LoanApplication.Domain.Entities;
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
using Google.Apis.Auth;
using Microsoft.Extensions.Configuration;

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
        // LOGIN
        public async Task<AuthResponseDto> LoginAsync(LoginDto dto)
        {
            // 1. Validate email and password input
            if (string.IsNullOrWhiteSpace(dto.Email) ||
                string.IsNullOrWhiteSpace(dto.Password))
            {
                return new AuthResponseDto
                {
                    Message = "Invalid email or password."
                };
            }

            // 2. Find user with Role details
            var user = await db.Users.Include(u => u.Role).FirstOrDefaultAsync(u => u.Email == dto.Email);

            // 3. Check whether user exists
            if (user == null)
            {
                return new AuthResponseDto
                {
                    Message = "Invalid email or password."
                };
            }

            // 4. Google-only users may not have a password
            if (string.IsNullOrWhiteSpace(user.Password))
            {
                return new AuthResponseDto
                {
                    Message = "Invalid email or password."
                };
            }

            // 5. Verify password
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

            // 6. Continue login or 2FA flow
            return ProcessUserLoginFlow(user);
        }
        // GOOGLE LOGIN
        public async Task<AuthResponseDto> GoogleLoginAsync(GoogleAuthDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.IdToken))
            {
                return new AuthResponseDto
                {
                    Message = "Google ID token is required."
                };
            }

            var clientId = config["Google:ClientId"];

            if (string.IsNullOrWhiteSpace(clientId))
            {
                return new AuthResponseDto
                {
                    Message = "Google Client ID is not configured."
                };
            }

            GoogleJsonWebSignature.Payload googleUser;

            try
            {
                googleUser = await GoogleJsonWebSignature.ValidateAsync(
                    dto.IdToken,
                    new GoogleJsonWebSignature.ValidationSettings
                    {
                        Audience = new[] { clientId }
                    });
            }
            catch
            {
                return new AuthResponseDto
                {
                    Message = "Invalid or expired Google ID token."
                };
            }

            if (string.IsNullOrWhiteSpace(googleUser.Email) ||
                googleUser.EmailVerified != true)
            {
                return new AuthResponseDto
                {
                    Message = "Google email is missing or not verified."
                };
            }

            var email = googleUser.Email.Trim().ToLowerInvariant();

            var firstName = !string.IsNullOrWhiteSpace(googleUser.GivenName)
                ? googleUser.GivenName
                : googleUser.Name ?? email.Split('@')[0];

            var lastName = googleUser.FamilyName ?? string.Empty;

            var user = await db.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Email == email);

            // Existing user: continue the normal login/2FA flow.
            if (user != null)
            {
                user.Role ??= await db.Roles
                    .FirstOrDefaultAsync(r => r.RoleId == user.RoleId);

                return ProcessUserLoginFlow(user);
            }

            // New Google user: find the Customer role.
            var customerRole = await db.Roles
                .FirstOrDefaultAsync(r => r.RoleId == 1);

            if (customerRole == null)
            {
                return new AuthResponseDto
                {
                    Message = "Customer role is not configured."
                };
            }

            await using var transaction =
                await db.Database.BeginTransactionAsync();

            var customer = new Customer
            {
                FirstName = firstName,
                LastName = lastName,
                Email = email,
                IsEmailVerified = true,

                // Collect these details later from the customer.
                Password = null,
                Age = null,
                MobileNo = null,
                Pan = null,
                AadhaarNo = null,
                EmploymentType = null,
                MonthlyIncome = null,
                MonthlyInvestment = null
            };

            db.Customers.Add(customer);
            await db.SaveChangesAsync();

            user = new User
            {
                FirstName = firstName,
                LastName = lastName,
                Email = email,
                Password = null,
                Mobile = null,
                RoleId = customerRole.RoleId,
                CustomerId = customer.CustomerId,
                IsTwoFactorEnabled = false,
                Role = customerRole
            };

            db.Users.Add(user);
            await db.SaveChangesAsync();

            await transaction.CommitAsync();

            return ProcessUserLoginFlow(user);
        }

        // COMPLETE PROFILE
        public async Task<bool> CompleteProfileAsync(
            int userId,
            CompleteProfileDto dto)
        {
            // 1. Find the logged-in user
            var user = await db.Users.FirstOrDefaultAsync(u => u.UserId == userId);
            if (user == null || user.CustomerId == null)
            {
                return false;
            }
            // 2. Find that user's existing Customer record
            var customer = await db.Customers.FirstOrDefaultAsync(c => c.CustomerId == user.CustomerId.Value);
            if (customer == null)
            {
                return false;
            }

            // 3. Fill the profile fields that are currently missing.
            customer.Age ??= dto.Age;
            customer.MobileNo ??= dto.MobileNo;
            customer.Pan ??= dto.Pan;
            customer.AadhaarNo ??= dto.AadhaarNo;
            customer.EmploymentType ??= dto.EmploymentType;
            customer.MonthlyIncome ??= dto.MonthlyIncome;
            customer.MonthlyInvestment ??= dto.MonthlyInvestment;
            // Keep User.Mobile in sync when it has no value.
            if (string.IsNullOrWhiteSpace(user.Mobile))
            {
                user.Mobile = dto.MobileNo;
            }
           
            await db.SaveChangesAsync();
            return true;
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
                Role = user.Role?.RoleName,
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


        private AuthResponseDto ProcessUserLoginFlow(User user)
        {
            if (user.IsTwoFactorEnabled)
            {
                string preAuthToken = GenerateJwtToken(user, true);

                return new AuthResponseDto
                {
                    RequiresTwoFactor = true,
                    PreAuthToken = preAuthToken,
                    Message = "2FA required. Verify code to continue."
                };
            }

            string token = GenerateJwtToken(user, false);

            return new AuthResponseDto
            {
                RequiresTwoFactor = false,
                Token = token,
                Role = user.Role?.RoleName,
                Message = "Login successful."
            };
        }

        // JWT TOKEN
        private string GenerateJwtToken(User user,bool isPreAuth)
        {
            string keyValue = config["Jwt:Key"]?? throw new InvalidOperationException("Jwt:Key is not configured.");
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(keyValue));

            var credentials =new SigningCredentials(key,SecurityAlgorithms.HmacSha256);
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier,user.UserId.ToString()),
                new Claim(ClaimTypes.Email,user.Email),
                new Claim("Purpose",isPreAuth? "2FA_Verification": "Access")
            };

            if (!isPreAuth)
            {
                string? roleName = user.Role?.RoleName;
                if (string.IsNullOrWhiteSpace(roleName))
                {
                    throw new InvalidOperationException("User role is not configured.");
                }
                claims.Add(new Claim(ClaimTypes.Role, roleName));
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
                string keyValue = config["Jwt:Key"]?? throw new InvalidOperationException("Jwt:Key is not configured.");
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