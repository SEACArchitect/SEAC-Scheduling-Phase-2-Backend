using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Nri_Webapplication_Backend.Managers;
using Nri_Webapplication_Backend.Helpers;
using Nri_Webapplication_Backend.Models;

namespace Nri_Webapplication_Backend.Controllers.Management
{
    [Route("trainer_management")]
    [ApiController]
    public class TrainerController : ControllerBase
    {
        public readonly IRespondHelper respondHelper;
        public readonly IContentManager contentManager;
        public readonly ITrainerManager trainerManager;
        public readonly ICommonHelper commonHelper;

        public TrainerController(IRespondHelper respondHelper, IContentManager contentManager, ICommonHelper commonHelper, ITrainerManager trainerManater)
        {
            this.respondHelper = respondHelper;
            this.contentManager = contentManager;
            this.commonHelper = commonHelper;
            this.trainerManager = trainerManater;
        }

        [HttpGet("business_type")]
        public async Task<IActionResult> GetAllBusinessType()
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
                var resultFindAll = await this.contentManager.FindContentBusinessTypeAsync();

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


        [HttpGet("room_type")]
        public async Task<IActionResult> GetEventRoomType()
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
                var resultFindAll = await this.trainerManager.FindEventRoomTypeAsync();

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

        [HttpGet("skill")]
        public async Task<IActionResult> GetSkill()
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
                var resultFindAll = await this.trainerManager.FindSkillAsync();

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

        [HttpGet("trainer_type")]
        public async Task<IActionResult> GetTrainer()
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
                var resultFindAll = await this.trainerManager.FindTrainerAsync();

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

        [HttpGet("contract_type")]
        public async Task<IActionResult> GetContactType()
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
                var resultFindAll = await this.trainerManager.FindContractTypeAsync();

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

        [HttpPost("new")]
        public async Task<IActionResult> Create(TrainerModel entity)
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
                var resultFindAll = await this.trainerManager.FindAllAsync();

                var chkEmail = resultFindAll.Where(o => o.email == entity.email).Count();

                if (chkEmail > 0)
                {
                    return BadRequest(this.respondHelper.Respond(0, 0, "This email already exists", null, null));
                }

                entity.createdBy = Convert.ToInt32(result.userId); //Result is userID

                var responseCreate = await this.trainerManager.InsertAsync(entity);


                return Created("", this.respondHelper.Respond(resultFindAll.Count(), 1, "Insert success", null, null));
            }
            catch (Exception ex)
            {
                return BadRequest(this.respondHelper.Respond(0, 0, $"Error : {ex}", null, null));
            }
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
                var resultFindAll = await this.trainerManager.FindAllAsync();

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

        [HttpGet("{trainerID}")]
        public async Task<IActionResult> GetByID(int trainerID)
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

                if (trainerID == 0)
                {
                    return Ok(this.respondHelper.Respond(0, 0, "trainerId id is empty", null, null));
                }

                //Get all data
                var resultFindAll = await this.trainerManager.FindAllAsync();

                var data = new
                {
                    total = resultFindAll.Count(),
                    count = 1,
                    message = $"Get by trainer {trainerID}  success",
                    data = resultFindAll.Where(o => o.id == trainerID).FirstOrDefault()
                };

                return Ok(this.respondHelper.Respond(data.total, data.count, data.message, null, data.data));
            }
            catch (Exception ex)
            {
                return BadRequest(this.respondHelper.Respond(0, 0, $"Error : {ex}", null, null));
            }
        }

        [HttpPut("edit/{id}")]
        public async Task<IActionResult> Update(TrainerModel entity, int id)
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
                    return Ok(this.respondHelper.Respond(0, 0, "trainer  is empty", null, null));
                }

                //Get all data from table users
                var resultFindAll = await this.trainerManager.FindAllAsync();

                //Get user by id for check
                var resultFindByID = resultFindAll.Where(o => o.id == id);

                if (resultFindByID == null)
                {
                    return Ok(this.respondHelper.Respond(resultFindAll.Count(), 0, $"Cannot found contentID on database", null, null));
                }

                //Update user 
                var responseUser = await this.trainerManager.UpdateAsync(entity, id);

                return Ok(this.respondHelper.Respond(resultFindAll.Count(), 1, $"Update trainerId {id} success", null, null));
            }
            catch (Exception ex)
            {
                return BadRequest(this.respondHelper.Respond(0, 0, $"Error : {ex}", null, null));
            }
        }

        [HttpGet("contact_type/{contactTypeID}")]
        public async Task<IActionResult> GetContactTypeID(int contactTypeID)
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
                var resultFindAll = await this.trainerManager.FindContacType(contactTypeID);

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
