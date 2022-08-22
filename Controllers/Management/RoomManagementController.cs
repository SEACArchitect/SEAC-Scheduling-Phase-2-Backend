using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Nri_Webapplication_Backend.Helpers;
using Nri_Webapplication_Backend.Managers;
using Nri_Webapplication_Backend.Models;

namespace Nri_Webapplication_Backend.Controllers.Management
{


    [Route("room_management")]
    [ApiController]
    public class RoomManagementController : ControllerBase
    {
        public readonly IRespondHelper respondHelper;
        public readonly IRoomManager roomManager;
        public readonly ICommonHelper commonHelper;

        public RoomManagementController(IRespondHelper respondHelper, IRoomManager roomManager, ICommonHelper commonHelper)
        {
            this.respondHelper = respondHelper;
            this.roomManager = roomManager;
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

                if (result.roleId == (int)Roles.viewer)
                {
                    return NotFound(this.respondHelper.Respond(null, null, "Unauthorized", null, null));
                }

                //Get all data
                var resultFindAll = await this.roomManager.FindAsync();

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

        [HttpGet("{id}")]
        public async Task<IActionResult> GetByID(int id)
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


                if (id == 0)
                {
                    return Ok(this.respondHelper.Respond(0, 0, "User id is empty", null, null));
                }

                //Get all data from table users
                var resultFindAll = await this.roomManager.FindAsync();

                //Get by id data from table users
                var resultFindByID = await this.roomManager.FindIdAsync(id);

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

        //[HttpGet("room_name/{roomName}")]
        //public async Task<IActionResult> GetByName(string roomName)
        //{
        //    try
        //    {

        //        var jwtToken = Request.Headers["Authorization"];


        //        if (string.IsNullOrWhiteSpace(jwtToken))
        //        {
        //            return Unauthorized(this.respondHelper.Respond(null, null, "Unauthorized", null, null));
        //        }

        //        //Check authen by jwt
        //        var result = this.commonHelper.DecodeJwt(jwtToken);

        //        if (result == null)
        //        {
        //            return Unauthorized(this.respondHelper.Respond(null, null, "Unauthorized", null, null));
        //        }


        //        if (roomName == null)
        //        {
        //            return Ok(this.respondHelper.Respond(0, 0, "Name is empty", null, null));
        //        }

        //        //Get all data from table users
        //        var resultFindAll = await this.roomManager.FindAsync();

        //        //Get by id data from table users
        //        var resultFindByID = await this.roomManager.FindRoomNameAsync(roomName);

        //        if (resultFindByID == null)
        //        {
        //            return Ok(this.respondHelper.Respond(resultFindAll.Count(), 0, "Not found", null, null));
        //        }

        //        var data = new
        //        {
        //            total = resultFindAll.Count(),
        //            count = 1,
        //            message = "Get by room name success",
        //            data = resultFindByID
        //        };

        //        return Ok(this.respondHelper.Respond(data.total, data.count, data.message, null, data.data));

        //    }
        //    catch (Exception ex)
        //    {
        //        return BadRequest(this.respondHelper.Respond(0, 0, $"Error : {ex}", null, null));
        //    }
        //}

        [HttpPost("new")]
        public async Task<IActionResult> Create(RoomModel entity)
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

                //Get all data from table content
                var resultFindAll = await this.roomManager.FindAsync();

                //Check content code duplicate
                var chkContentCode = resultFindAll.Where(o => o.Name == entity.Name).Count();

                if (chkContentCode > 0)
                {
                    return BadRequest(this.respondHelper.Respond(0, 0, "This room name already exists", null, null));
                }

                entity.createdBy = Convert.ToInt32(result.userId); //Result is userID
                var responseCreate = await this.roomManager.InsertAsync(entity);


                return Created("", this.respondHelper.Respond(resultFindAll.Count(), 1, "Insert success", null, null));
            }
            catch (Exception ex)
            {
                return BadRequest(this.respondHelper.Respond(0, 0, $"Error : {ex}", null, null));
            }
        }

        [HttpPut("edit/{id}")]
        public async Task<IActionResult> Update(RoomModel entity, int id)
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

                if (id == 0)
                {
                    return Ok(this.respondHelper.Respond(0, 0, "id  is empty", null, null));
                }

                //Get all data from table users
                var resultFindAll = await this.roomManager.FindAsync();

                //Check duplicate room name
                var checkResult = resultFindAll.Where(obj => obj.Name == entity.Name).FirstOrDefault();

                if(checkResult != null && id != checkResult.Id)
                {
                    return BadRequest(this.respondHelper.Respond(resultFindAll.Count(), 0, $"This room name already exists", null, null));
                }

                //Get user by id for check
                var resultFindByID = await this.roomManager.FindIdAsync(id);

                if (resultFindByID == null)
                {
                    return Ok(this.respondHelper.Respond(resultFindAll.Count(), 0, $"Cannot found id on database", null, null));
                }

                entity.updatedBy = Convert.ToInt32(result.userId); //Result is userID
                //Update user 
                var responseUser = await this.roomManager.UpdateAsync(entity, id);

                return Ok(this.respondHelper.Respond(resultFindAll.Count(), 1, $"Update id {id} success", null, null));
            }
            catch (Exception ex)
            {
                return BadRequest(this.respondHelper.Respond(0, 0, $"Error : {ex}", null, null));
            }
        }

        [HttpDelete("room/{id}")]
        public async Task<IActionResult> Delete(RoomModel entity, int id)
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

                if (id == 0)
                {
                    return Ok(this.respondHelper.Respond(0, 0, "UserID  is empty", null, null));
                }

                //Get all data from table users
                var resultFindAll = await this.roomManager.FindAsync();

                //Get user by id for check
                var resultFindByID = await this.roomManager.FindIdAsync(id);

                if (resultFindByID == null)
                {
                    return Ok(this.respondHelper.Respond(resultFindAll.Count(), 0, $"Cannot found userID on database", null, null));
                }

                entity.updatedBy = Convert.ToInt32(result);
                //Update user 
                var responseUser = await this.roomManager.DeleteAsync(entity, id);

                return Ok(this.respondHelper.Respond(resultFindAll.Count(), 1, entity.IsActive == 0 ? $"InActvie room id {id} success" : $"Active room id {id} success", null, null));
            }
            catch (Exception ex)
            {
                return BadRequest(this.respondHelper.Respond(0, 0, $"Error : {ex}", null, null));
            }
        }

        [HttpGet("type")]
        public async Task<IActionResult> GetAllRoomType()
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

                if (result.roleId == (int)Roles.viewer)
                {
                    return NotFound(this.respondHelper.Respond(null, null, "Unauthorized", null, null));
                }

                //Get all data
                var resultFindAll = await this.roomManager.FindRoomTypeAsync();

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

        [HttpGet("available_for_content_format")]
        public async Task<IActionResult> GetAllContentFormat()
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
                var resultFindAll = await this.roomManager.FindContentFormat();

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


    }
}
