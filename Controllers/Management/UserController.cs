using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using MySqlConnector;
using Nri_Webapplication_Backend.Managers;
using Nri_Webapplication_Backend.Helpers;
using Nri_Webapplication_Backend.Models;

namespace Nri_Webapplication_Backend.Controllers.Management
{
    [Route("user_management")]
    [ApiController]
    public class UserController : ControllerBase
    {
        public readonly IRespondHelper respondHelper;
        public readonly IUserManager userBal;
        public readonly IUserRoleManager userRole;
        public readonly ICommonHelper commonHelper;

        public UserController(IRespondHelper respondHelper, IUserManager userBal, IUserRoleManager userRole, ICommonHelper commonHelper)
        {
            this.respondHelper = respondHelper;
            this.userBal = userBal;
            this.userRole = userRole;
            this.commonHelper = commonHelper;
        }


        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var jwtToken = Request.Headers["Authorization"];


                if (string.IsNullOrWhiteSpace(jwtToken))
                {
                    return Unauthorized(this.respondHelper.Respond(null, null, "Unauthorized", null, null));
                }

                //Check authen by jwt
                var result = this.commonHelper.DecodeJwtV2(jwtToken);

                if (result == null)
                {
                    return Unauthorized(this.respondHelper.Respond(null, null, "Unauthorized", null, null));
                }

                if (result.roleId == (int)Roles.adminReadOnly || result.roleId == (int)Roles.viewer)
                {
                    return NotFound(this.respondHelper.Respond(null, null, "Unauthorized", null, null));
                }

                //Get all data
                var resultFindAll = await this.userBal.FindAsync();

                var data = new
                {
                    total = resultFindAll.Count(),
                    count = resultFindAll.Count(),
                    message = "Get all success",
                    data = resultFindAll
                };

                return Ok(this.respondHelper.Respond(data.total, data.count, data.message, null, data.data));
            }
            catch (Exception ex)
            {
                return BadRequest(this.respondHelper.Respond(0, 0, $"Error : {ex}", null, null));
            }
        }

        [HttpGet("{userID}")]
        public async Task<IActionResult> GetByID(int userID)
        {
            try
            {

                var jwtToken = Request.Headers["Authorization"];


                if (string.IsNullOrWhiteSpace(jwtToken))
                {
                    return Unauthorized(this.respondHelper.Respond(null, null, "Unauthorized", null, null));
                }

                //Check authen by jwt
                var result = this.commonHelper.DecodeJwtV2(jwtToken);

                if (result == null)
                {
                    return Unauthorized(this.respondHelper.Respond(null, null, "Unauthorized", null, null));
                }

                if (result.roleId == (int)Roles.adminReadOnly || result.roleId == (int)Roles.viewer)
                {
                    return NotFound(this.respondHelper.Respond(null, null, "Unauthorized", null, null));
                }


                if (userID == 0)
                {
                    return Ok(this.respondHelper.Respond(0, 0, "User id is empty", null, null));
                }

                //Get all data from table users
                var resultFindAll = await this.userBal.FindAsync();

                //Get by id data from table users
                var resultFindByID = await this.userBal.FindByIDAsync(userID);

                if (resultFindByID == null)
                {
                    return Ok(this.respondHelper.Respond(resultFindAll.Count(), 0, "Not found", null, null));
                }

                var data = new
                {
                    total = resultFindAll.Count(),
                    count = 1,
                    message = "Get by id success",
                    data = resultFindByID
                };

                return Ok(this.respondHelper.Respond(data.total, data.count, data.message, null, data.data));

            }
            catch (Exception ex)
            {
                return BadRequest(this.respondHelper.Respond(0, 0, $"Error : {ex}", null, null));
            }
        }

        [HttpPost("new")]
        public async Task<IActionResult> Create(UserManagementModel entity)
        {
            try
            {

                var jwtToken = Request.Headers["Authorization"];


                if (string.IsNullOrWhiteSpace(jwtToken))
                {
                    return Unauthorized(this.respondHelper.Respond(null, null, "Unauthorized", null, null));
                }

                //Check authen by jwt
                var result = this.commonHelper.DecodeJwtV2(jwtToken);

                if (result == null)
                {
                    return Unauthorized(this.respondHelper.Respond(null, null, "Unauthorized", null, null));
                }

                if (result.roleId == (int)Roles.adminReadOnly || result.roleId == (int)Roles.viewer)
                {
                    return NotFound(this.respondHelper.Respond(null, null, "Unauthorized", null, null));
                }


                if (entity == null)
                {
                    return Ok(this.respondHelper.Respond(0, 0, "Body  is empty", null, null));
                }

                

                //Get all data from table users
                var resultFindAll = await this.userBal.FindAsync();

                var checkUser = resultFindAll.Where(o => o.username == entity.username).Count();

                if (checkUser > 0)
                {
                    return BadRequest(this.respondHelper.Respond(0, 0, "This username already exists", null, null));
                }

                entity.createdBy = Convert.ToInt32(result.roleId); //Result is userID

                string generatePassword = await this.commonHelper.GeneratePassword();

                entity.password =  this.commonHelper.encrypt(generatePassword);

                var responseCreate = await this.userBal.InsertAsync(entity);

                await this.commonHelper.SendEmail(entity.username, generatePassword);

                return Created("", this.respondHelper.Respond(resultFindAll.Count(), 1, "Insert success", null, null));
            }
            catch (Exception ex)
            {
                return BadRequest(this.respondHelper.Respond(0, 0, $"Error : {ex}", null, null));
            }
        }

        [HttpPut("edit/{userID}")]
        public async Task<IActionResult> Update(UserManagementModel entity, int userID)
        {
            try
            {

                var jwtToken = Request.Headers["Authorization"];


                if (string.IsNullOrWhiteSpace(jwtToken))
                {
                    return Unauthorized(this.respondHelper.Respond(null, null, "Unauthorized", null, null));
                }

                //Check authen by jwt
                var result = this.commonHelper.DecodeJwtV2(jwtToken);

                if (result == null)
                {
                    return Unauthorized(this.respondHelper.Respond(null, null, "Unauthorized", null, null));
                }

                if (result.roleId == (int)Roles.adminReadOnly || result.roleId == (int)Roles.viewer)
                {
                    return NotFound(this.respondHelper.Respond(null, null, "Unauthorized", null, null));
                }

                if (entity == null)
                {
                    return Ok(this.respondHelper.Respond(0, 0, "Body  is empty", null, null));
                }

                if (userID == 0)
                {
                    return Ok(this.respondHelper.Respond(0, 0, "UserID  is empty", null, null));
                }

                //Get all data from table users
                var resultFindAll = await this.userBal.FindAsync();

                //Get user by id for check
                var resultFindByID = await this.userBal.FindByIDAsync(userID);

                if (resultFindByID == null)
                {
                    return Ok(this.respondHelper.Respond(resultFindAll.Count(), 0, $"Cannot found userID on database", null, null));
                }

                entity.updatedBy = Convert.ToInt32(result.userId); //Result is userID
                //Update user 
                var responseUser = await this.userBal.UpdateAsync(entity, userID);

                return Ok(this.respondHelper.Respond(resultFindAll.Count(), 1, $"Update userID {userID} success", null, responseUser));
            }
            catch (Exception ex)
            {
                return BadRequest(this.respondHelper.Respond(0, 0, $"Error : {ex}", null, null));
            }
        }

        [HttpDelete("role/{userID}")]
        public async Task<IActionResult> Delete(UserManagementModel entity, int userID)
        {
            try
            {

                var jwtToken = Request.Headers["Authorization"];


                if (string.IsNullOrWhiteSpace(jwtToken))
                {
                    return Unauthorized(this.respondHelper.Respond(null, null, "Unauthorized", null, null));
                }

                //Check authen by jwt
                var result = this.commonHelper.DecodeJwtV2(jwtToken);

                if (result == null)
                {
                    return Unauthorized(this.respondHelper.Respond(null, null, "Unauthorized", null, null));
                }

                if (result.roleId == (int)Roles.adminReadOnly || result.roleId == (int)Roles.viewer)
                {
                    return NotFound(this.respondHelper.Respond(null, null, "Unauthorized", null, null));
                }

                if (entity == null)
                {
                    return Ok(this.respondHelper.Respond(0, 0, "Body  is empty", null, null));
                }

                if (userID == 0)
                {
                    return Ok(this.respondHelper.Respond(0, 0, "UserID  is empty", null, null));
                }

                //Get all data from table users
                var resultFindAll = await this.userBal.FindAsync();

                //Get user by id for check
                var resultFindByID = await this.userBal.FindByIDAsync(userID);

                if (resultFindByID == null)
                {
                    return Ok(this.respondHelper.Respond(resultFindAll.Count(), 0, $"Cannot found userID on database", null, null));
                }

                entity.updatedBy = Convert.ToInt32(result);
                //Update user 
                var responseUser = await this.userBal.DeleteAsync(entity, userID);

                return Ok(this.respondHelper.Respond(resultFindAll.Count(), 1, entity.isActive == 0 ? $"InActvie userID {userID} success" : $"Active userID {userID} success", null, null));
            }
            catch (Exception ex)
            {
                return BadRequest(this.respondHelper.Respond(0, 0, $"Error : {ex}", null, null));
            }
        }

        [HttpGet("role")]
        public async Task<IActionResult> GetAllRole()
        {
            try
            {

                var jwtToken = Request.Headers["Authorization"];


                if (string.IsNullOrWhiteSpace(jwtToken))
                {
                    return Unauthorized(this.respondHelper.Respond(null, null, "Unauthorized", null, null));
                }

                //Check authen by jwt
                var result = this.commonHelper.DecodeJwtV2(jwtToken);

                if (result == null)
                {
                    return Unauthorized(this.respondHelper.Respond(null, null, "Unauthorized", null, null));
                }

                if (result.roleId == (int)Roles.adminReadOnly || result.roleId == (int)Roles.viewer)
                {
                    return NotFound(this.respondHelper.Respond(null, null, "Unauthorized", null, null));
                }

                //Get all data
                var resultFindAll = await this.userRole.FindAsync();

                var data = new
                {
                    total = resultFindAll.Count(),
                    count = resultFindAll.Count(),
                    message = "Get all success",
                    data = resultFindAll
                };

                return Ok(this.respondHelper.Respond(data.total, data.count, data.message, null, data.data));
            }
            catch (Exception ex)
            {
                return BadRequest(this.respondHelper.Respond(0, 0, $"Error : {ex}", null, null));
            }
        }

        [HttpPut("password/{userID}")]
        public async Task<IActionResult> UpdatePassword(int userID)
        {
            try
            {
                UserManagementModel entity = new UserManagementModel();
                var jwtToken = Request.Headers["Authorization"];


                if (string.IsNullOrWhiteSpace(jwtToken))
                {
                    return Unauthorized(this.respondHelper.Respond(null, null, "Unauthorized", null, null));
                }

                //Check authen by jwt
                var result = this.commonHelper.DecodeJwtV2(jwtToken);

                if (result == null)
                {
                    return Unauthorized(this.respondHelper.Respond(null, null, "Unauthorized", null, null));
                }

                if (result.roleId == (int)Roles.adminReadOnly || result.roleId == (int)Roles.viewer)
                {
                    return NotFound(this.respondHelper.Respond(null, null, "Unauthorized", null, null));
                }

                //if (entity == null)
                //{
                //    return Ok(this.respondHelper.Respond(0, 0, "Body  is empty", null, null));
                //}

                if (userID == 0)
                {
                    return Ok(this.respondHelper.Respond(0, 0, "UserID  is empty", null, null));
                }

                //Get all data from table users
                var resultFindAll = await this.userBal.FindAsync();

                //Get user by id for check
                var resultFindByID = await this.userBal.FindByIDAsync(userID);

                if (resultFindByID == null)
                {
                    return Ok(this.respondHelper.Respond(resultFindAll.Count(), 0, $"Cannot update password on database", null, null));
                }


                string generatePassword = await this.commonHelper.GeneratePassword();

                entity.password = this.commonHelper.encrypt(generatePassword);
                //Update user 
                var responseUser = await this.userBal.UpdatePasswordAsync(entity, userID);

                await this.commonHelper.SendEmail(resultFindByID.username, generatePassword);

                return Ok(this.respondHelper.Respond(resultFindAll.Count(), 1, "Update password success", null, null));
            }
            catch (Exception ex)
            {
                return BadRequest(this.respondHelper.Respond(0, 0, $"Error : {ex}", null, null));
            }
        }
        
        [HttpGet("moderator_manager")]
        public async Task<IActionResult> GetModManager()
        {
            try
            {
                var jwtToken = Request.Headers["Authorization"];

                if (string.IsNullOrWhiteSpace(jwtToken))
                {
                    return Unauthorized(this.respondHelper.Respond(null, null, "Unauthorized", null, null));
                }

                var result = this.commonHelper.DecodeJwtV2(jwtToken);

                if (result == null)
                {
                    return Unauthorized(this.respondHelper.Respond(null, null, "Unauthorized", null, null));
                }

                if (result.roleId == (int)Roles.adminReadOnly || result.roleId == (int)Roles.viewer)
                {
                    return NotFound(this.respondHelper.Respond(null, null, "Unauthorized", null, null));
                }

                var resultFindAll = await this.userRole.FindModManager();
                var data = new
                {
                    total = resultFindAll.Count(),
                    count = resultFindAll.Count(),
                    message = "Get all success",
                    data = resultFindAll
                };

                return Ok(this.respondHelper.Respond(resultFindAll.Count(), 1, data.message, null, data.data));
            }
            catch (Exception ex)
            {
                return BadRequest(this.respondHelper.Respond(0, 0, $"Error : {ex}", null, null));
                throw ex;
            }
        }
    }
}
