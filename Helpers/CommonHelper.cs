using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Logging;
using Microsoft.IdentityModel.Tokens;
using Nri_Webapplication_Backend.Models;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using static Nri_Webapplication_Backend.Helpers.CommonHelper;

namespace Nri_Webapplication_Backend.Helpers
{
    public interface ICommonHelper
    {
        Task<string> GenerateJwt(UserManagementModel entity);
        Task<string> GeneratePassword();
        Task<Boolean> SendEmail(string username, string password);
        string DecodeJwt(string bearerToken);
        DecodeModel DecodeJwtV2(string bearerToken);
        string Base64Encode(string base64EncodedData);
        string Base64Decode(string base64EncodedData);
        string encrypt(string encryptString);
        string Decrypt(string cipherText);
    }
    public class CommonHelper : ICommonHelper
    {
        public readonly IConfiguration config;
        public CommonHelper(IConfiguration config)
        {
            this.config = config;
        }

        /// <summary>
        /// Create json web tokne
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public async Task<string> GenerateJwt(UserManagementModel entity)
        {
            try
            {
                var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(this.config["JsonWebTokenSetting:Key"]));
                var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

                var claims = new List<Claim>()
            {
                // Not disclosing internal userId
                new Claim("userId", entity.userID.ToString()),
                new Claim("username", entity.username),
                new Claim("userRoleId",entity.userRoleID.ToString())
            };

                var token = new JwtSecurityToken(
                  this.config["JsonWebTokenSetting:Issuer"],
                  null,
                  claims,
                  expires: DateTime.Now.AddDays(365),
                  signingCredentials: credentials);

                return new JwtSecurityTokenHandler().WriteToken(token);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// Create password
        /// </summary>
        /// <returns></returns>
        public async Task<string> GeneratePassword()
        {
            try
            {
                const string uppercase = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";

                var random = new Random();
                var randomString = new string(Enumerable.Repeat(uppercase, 1)
                                                        .Select(s => s[random.Next(s.Length)]).ToArray());

                const string symbols = "_@-";
                var randomSymbol = new string(Enumerable.Repeat(symbols, 1)
                                                       .Select(s => s[random.Next(s.Length)]).ToArray());

                const string lowcase = "abcdefghijklmnopqrstuvwxyz";
                var randomLowcase = new string(Enumerable.Repeat(lowcase, 1)
                                                        .Select(s => s[random.Next(s.Length)]).ToArray());

                const string number = "1234567890";
                var radomNumber = new string(Enumerable.Repeat(number, 1)
                                                        .Select(s => s[random.Next(s.Length)]).ToArray());

                return randomString + randomSymbol + randomLowcase + radomNumber + Guid.NewGuid().ToString("d").Substring(1, 4);

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<Boolean> SendEmail(string username, string password)
        {
            try
            {
                using (SmtpClient smtp = new SmtpClient())
                {
                    //smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
                    //smtp.UseDefaultCredentials = true;
                    smtp.EnableSsl = true;
                    smtp.Host = this.config["EmailSetting:Host"];
                    smtp.Port = Convert.ToInt32(this.config["EmailSetting:Port"]);

                    smtp.Credentials = new NetworkCredential(this.config["EmailSetting:Email"], this.config["EmailSetting:Password"]);

                    smtp.Send(CreateEmailMessage(username, password));
                }

                return true;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private MailMessage CreateEmailMessage(string username, string password)
        {
            // string from = "scheduling@seasiacenter.com";
            // string to = username;
            // string subject = "SEAC - Username and Password";
            // string body = $"SEAC Scheduling \n\nusername: {username}\npassword: {password.Trim()}\nURL: https://scheduling.seasiacenter.com";
            // var emailMessage = new MailMessage(from, to, subject, body);
            var emailMessage = new MailMessage(); 
            emailMessage.From = new MailAddress(this.config["EmailSetting:Email"]);
            emailMessage.To.Add(new MailAddress(username));
            emailMessage.IsBodyHtml = true;
            emailMessage.Subject = "SEAC - Username and Password";
            emailMessage.Body = GetFormattedMessageHTML(username, password);
            return emailMessage;
        }

        private String GetFormattedMessageHTML(string userName, string password)
        {
            string mail_body = @"    <!doctype html>
<html>

<head>
    <title>
    </title>
    <meta http-equiv='Content-Language' content='th' />
    <meta http-equiv='Content-Type' content='text/html; charset=UTF-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1'>
    <style type='text/css'>
    </style>
</head>

<body
    style='
        background-color: #fff;
        font-family: Segoe UI, Tahoma, Geneva, Verdana, sans-serif;
    '>
    <table width='100%'
        style='
            max-width: 480px;
        '>
        <tbody>
            <tr>
                <td colspan='2'>
                    <h3 
                        style='
                            margin: 0px 0px 0px 0px;
                            max-width: 480px;
                            text-align: center;
                        '>
                        SEAC Scheduling Registration
                    </h3>
                </td>
            </tr>
            <tr>
                <td width='25%'>USERNAME</td>
                <td width='75%'>{userName}</td>
            </tr>
            <tr>
                <td>PASSWORD</td>
                <td>{password}</td>
            </tr>
            <tr>
                <td>SITE URL</td>
                <td>https://scheduling.seasiacenter.com</td>
            </tr>
        </tbody>
    </table>
</body>

</html>
            ";
            return mail_body.Replace("{userName}", userName).Replace("{password}", password);
        }

        public class DecodeModel
        {
            public string userId { get; set; }
            public int roleId { get; set; }
        }
        public string DecodeJwt(string bearerToken)
        {
            try
            {
                var subJwt = bearerToken.Split(" ");

                if (subJwt.Count() < 1)
                {
                    return null;
                }

                string jwtToken = subJwt[1];

                if (string.IsNullOrWhiteSpace(jwtToken))
                {
                    return null;
                }

                var handler = new JwtSecurityTokenHandler();

                var jsonToken = handler.ReadToken(subJwt[1]);
                var tokenS = handler.ReadToken(subJwt[1]) as JwtSecurityToken;

                var claim = tokenS.Claims;

                if (claim == null)
                {
                    return null;
                }

                // ValidateToken
                SecurityToken validatedToken;
                TokenValidationParameters TVp = new TokenValidationParameters()
                {
                    ValidateIssuer = true,   // Because there is no issuer in the generated token
                    ValidateAudience = false, // Because there is no audiance in the generated token
                    ValidateLifetime = true, // Because there is no expiration in the generated token
                    ValidIssuer = this.config["JsonWebTokenSetting:Issuer"],

                };
                IdentityModelEventSource.ShowPII = true;

                var tokenHandler = new JwtSecurityTokenHandler();


                IPrincipal principal = tokenHandler.ValidateToken(jwtToken, TVp, out validatedToken);

                var expClaim = tokenS.Claims.First(x => x.Type == "exp").Value;

                DateTime expirationTime = DateTimeOffset.FromUnixTimeSeconds(long.Parse(expClaim)).DateTime;

                if (expirationTime < DateTime.Now)
                {
                    return null;
                }


                var userId = tokenS.Claims.First(claim => claim.Type == "userId").Value;


                return userId;
            }
            catch (Exception ex)
            {
                return null;
            }
        }
        public DecodeModel DecodeJwtV2(string bearerToken)
        {
            try
            {
                var subJwt = bearerToken.Split(" ");

                if (subJwt.Count() < 1)
                {
                    return null;
                }

                string jwtToken = subJwt[1];

                if (string.IsNullOrWhiteSpace(jwtToken))
                {
                    return null;
                }

                var handler = new JwtSecurityTokenHandler();

                var jsonToken = handler.ReadToken(subJwt[1]);
                var tokenS = handler.ReadToken(subJwt[1]) as JwtSecurityToken;

                var claim = tokenS.Claims;

                if (claim == null)
                {
                    return null;
                }

                // ValidateToken
                SecurityToken validatedToken;
                TokenValidationParameters TVp = new TokenValidationParameters()
                { 
                    ValidateIssuer = true,   // Because there is no issuer in the generated token
                    ValidateAudience = false, // Because there is no audiance in the generated token
                    ValidateLifetime = true, // Because there is no expiration in the generated token
                    ValidIssuer = this.config["JsonWebTokenSetting:Issuer"],
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(this.config["JsonWebTokenSetting:Key"])) // The same key as the one that generate the token

                };
                IdentityModelEventSource.ShowPII = true;

                var tokenHandler = new JwtSecurityTokenHandler();


                IPrincipal principal = tokenHandler.ValidateToken(jwtToken, TVp, out validatedToken);


                var expClaim = tokenS.Claims.First(x => x.Type == "exp").Value;

                DateTime expirationTime = DateTimeOffset.FromUnixTimeSeconds(long.Parse(expClaim)).DateTime;

                if (expirationTime < DateTime.Now)
                {
                    return null;
                }


                var userId = tokenS.Claims.First(claim => claim.Type == "userId").Value;
                var roleId = tokenS.Claims.First(claim => claim.Type == "userRoleId").Value;

                var DecodeModel = new DecodeModel
                {
                    userId = userId,
                    roleId = Convert.ToInt32(roleId)
                };

                return DecodeModel;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public string Base64Encode(string plainText)
        {
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
            return System.Convert.ToBase64String(plainTextBytes);
        }

        public string Base64Decode(string base64EncodedData)
        {
            var base64EncodedBytes = System.Convert.FromBase64String(base64EncodedData);

            return System.Text.Encoding.UTF8.GetString(base64EncodedBytes);
        }

        public string encrypt(string encryptString)
        {
            string EncryptionKey = this.config["EncryptKey"];
            byte[] clearBytes = Encoding.Unicode.GetBytes(encryptString);
            using (Aes encryptor = Aes.Create())
            {
                Rfc2898DeriveBytes pdb = new Rfc2898DeriveBytes(EncryptionKey, new byte[] {
            0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76
        });
                encryptor.Key = pdb.GetBytes(32);
                encryptor.IV = pdb.GetBytes(16);
                using (MemoryStream ms = new MemoryStream())
                {
                    using (CryptoStream cs = new CryptoStream(ms, encryptor.CreateEncryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(clearBytes, 0, clearBytes.Length);
                        cs.Close();
                    }
                    encryptString = Convert.ToBase64String(ms.ToArray());
                }
            }
            return encryptString;
        }

        public string Decrypt(string cipherText)
        {
            string EncryptionKey = this.config["EncryptKey"];
            cipherText = cipherText.Replace(" ", "+");
            byte[] cipherBytes = Convert.FromBase64String(cipherText);
            using (Aes encryptor = Aes.Create())
            {
                Rfc2898DeriveBytes pdb = new Rfc2898DeriveBytes(EncryptionKey, new byte[] {
            0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76
        });
                encryptor.Key = pdb.GetBytes(32);
                encryptor.IV = pdb.GetBytes(16);
                using (MemoryStream ms = new MemoryStream())
                {
                    using (CryptoStream cs = new CryptoStream(ms, encryptor.CreateDecryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(cipherBytes, 0, cipherBytes.Length);
                        cs.Close();
                    }
                    cipherText = Encoding.Unicode.GetString(ms.ToArray());
                }
            }
            return cipherText;
        }
    }
}
