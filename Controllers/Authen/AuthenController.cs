using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Nri_Webapplication_Backend.Managers;
using Nri_Webapplication_Backend.Helpers;
using Nri_Webapplication_Backend.Models;

namespace Nri_Webapplication_Backend.Controllers.Authen
{
    [Route("authen")]
    [ApiController]
    public class AuthenController : ControllerBase
    {
        public readonly IRespondHelper respondHelper;
        public readonly IUserManager userBal;
        public readonly IUserRoleManager userRole;
        public readonly IConfiguration config;
        public readonly ICommonHelper commonHelper;
        public readonly IUserManager userManager;

        public AuthenController(IRespondHelper respondHelper, IUserManager userBal, IUserRoleManager userRole, IConfiguration config, ICommonHelper commonHelper, IUserManager userManager)
        {
            this.respondHelper = respondHelper;
            this.userBal = userBal;
            this.userRole = userRole;
            this.config = config;
            this.commonHelper = commonHelper;
            this.userManager = userManager;
        }

        [HttpPost("signIn")]
        public async Task<IActionResult> singIn(AuthenModel entity)
        {
            try
            {
                if (entity == null)
                {
                    return Ok(this.respondHelper.Respond(0, 0, "Body  is empty", null, null));
                }


                //Check username and password in daatabase
                var checkUsername = await this.userManager.SignInAsync(entity.username);

                if (checkUsername == null)
                {
                    return Unauthorized(this.respondHelper.Respond(null, null, $"Login {entity.username} failed", null, null));

                }

                var decrypt = this.commonHelper.Decrypt(checkUsername.password);

                if (decrypt != entity.password)
                {
                    return Unauthorized(this.respondHelper.Respond(null, null, $"Login {entity.username} failed", null, null));
                }

                //Generate jwt
                string tokenJwt = await this.commonHelper.GenerateJwt(checkUsername);

                if (checkUsername == null)
                {
                    return Unauthorized(this.respondHelper.Respond(null, null, $"login user {entity.username} failed", null, null));
                }

                //Insert data into LoginHistory
                if (await this.userManager.InsertLoginHistoryAsync(checkUsername.userID) == false)
                {
                    return Unauthorized(this.respondHelper.Respond(null, null, $"Cannot login {entity.username} failed", null, null));
                }

                var resultData = new
                {
                    token = tokenJwt,
                    username = checkUsername.username,
                    userRoleId = checkUsername.userRoleID,
                };

                return Ok(this.respondHelper.Respond(null, null, $"Login user {entity.username} success", null, resultData));
            }
            catch (Exception ex)
            {
                return BadRequest(this.respondHelper.Respond(0, 0, $"Error : {ex}", null, null));
            }
        }


    }

}
