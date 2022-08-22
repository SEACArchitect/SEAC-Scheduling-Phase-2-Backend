using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nri_Webapplication_Backend.Managers;
using Nri_Webapplication_Backend.Helpers;
using Nri_Webapplication_Backend.Models;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Authorization;

namespace Nri_Webapplication_Backend.Controllers.Management
{
    [Route("planner")]
    [ApiController]
    public class PlannerController : ControllerBase
    {
        public readonly IRespondHelper respondHelper;
        public readonly ICommonHelper commonHelper;
        public readonly IContentManager contentManager;
        public readonly IPlannerManager plannerManager;
        public readonly IRoomManager roomManager;
        public readonly ITrainerManager trainerManager;
        public PlannerController(IRespondHelper respondHelper, ICommonHelper commonHelper, IContentManager contentManager, IPlannerManager plannerManager, IRoomManager roomManager, ITrainerManager trainerManager)
        {
            this.respondHelper = respondHelper;
            this.commonHelper = commonHelper;
            this.contentManager = contentManager;
            this.plannerManager = plannerManager;
            this.roomManager = roomManager;
            this.trainerManager = trainerManager;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll(DateTime startDateTime, DateTime endDateTime)
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

                if (result.roleId == (int)Roles.viewer )
                {
                    return NotFound(this.respondHelper.Respond(null, null, "Unauthorized", null, null));
                }

                var resultPlanner = this.plannerManager.FindPlannerByDate(startDateTime, endDateTime);



                return Ok(this.respondHelper.Respond(resultPlanner.Count(), resultPlanner.Count(), "Get success", null, resultPlanner));
            }
            catch (Exception ex)
            {
                throw ex;
            }
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

        [HttpGet("learning_type")]
        public async Task<IActionResult> GetAllLearningType()
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
                var resultFindAll = await this.contentManager.FindLearningTypeAsync();

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

        [HttpGet("content_format")]
        public async Task<IActionResult> GetContentFormat()
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
                var resultFindAll = await this.contentManager.FindContentFormatAsync();

                var data = new
                {
                    total = resultFindAll.Count(),
                    count = resultFindAll.Count(),
                    message = "Get all success",
                    data = resultFindAll
                };

                return Ok(this.respondHelper.Respond(data.total, data.count, data.message, null, resultFindAll));
            }
            catch (Exception ex)
            {
                return BadRequest(this.respondHelper.Respond(0, 0, $"Error : {ex}", null, null));
            }
        }

        [HttpGet("category")]
        public async Task<IActionResult> GetCategory()
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
                var resultFindAll = await this.plannerManager.FindCategoryAsync();

                var data = new
                {
                    total = resultFindAll.Count(),
                    count = resultFindAll.Count(),
                    message = "Get all success",
                    data = resultFindAll
                };

                return Ok(this.respondHelper.Respond(data.total, data.count, data.message, null, resultFindAll));
            }
            catch (Exception ex)
            {
                return BadRequest(this.respondHelper.Respond(0, 0, $"Error : {ex}", null, null));
            }
        }

        [HttpGet("room")]
        public async Task<IActionResult> GetRoom(DateTime createDate, string startTime, string endTime, int contentId, int businessTypeId)
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
                var resultFindAll = await this.plannerManager.FindRoomAllAsync(createDate, startTime, endTime, contentId, businessTypeId);

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

        [HttpGet("getroombyroomtypeandformat")]
        public IActionResult GetRoom(int roomTypeId, int contentFormatId)
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
                var resultFindAll = this.plannerManager.FindRoomByBusinessTypeAndContentFormat(roomTypeId, contentFormatId);

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

        [HttpGet("language")]
        public async Task<IActionResult> GetLanguage()
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
                var resultFindAll = await this.plannerManager.FindLanguageAsync();

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

        [HttpGet("content")]
        public async Task<IActionResult> GetContentMaster()
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
                var resultFindAll = await this.plannerManager.FindAllContentMasterAsync();

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

        [HttpGet("trainer")]
        public async Task<IActionResult> GetTrainerContractType(DateTime createDate, string startTime, string endTime, int contentId, string mode)
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
                var resultFindAll = await this.plannerManager.FindAllTrainerContractTypeAsync(createDate, startTime, endTime, contentId, mode);

                var data = new
                {
                    total = resultFindAll != null ? resultFindAll.Count() : 0,
                    count = resultFindAll != null ? resultFindAll.Count() : 0,
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

        [HttpGet("modelator")]
        public async Task<IActionResult> GetModerator(DateTime createDate, string startTime, string endTime, int businessTypeId, int roomId)
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

                var resultFindAll = await this.plannerManager.FindAllModeratorMasterAsync(createDate, startTime, endTime, businessTypeId, roomId);
                //Get all data
                //var resultFindAll = await this.plannerManager.FindAllModeratorOld();

                var data = new
                {
                    total = resultFindAll != null ? resultFindAll.Count() : 0,
                    count = resultFindAll != null ? resultFindAll.Count() : 0,
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

        [HttpGet("contactType")]
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
                var resultFindAll = await this.plannerManager.FindAllContactTypeAsyc();

                var data = new
                {
                    total = resultFindAll != null ? resultFindAll.Count() : 0,
                    count = resultFindAll != null ? resultFindAll.Count() : 0,
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

        [HttpPost("create_schedule")]
        public async Task<IActionResult> Insert(PlannerModel_Request entity)
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

                entity.CreatedBy = Convert.ToInt32(result.userId); //Result is userID

                var responseCreate = await this.plannerManager.InsertAsync(entity);

                if (responseCreate == -1)
                {
                    return BadRequest(this.respondHelper.Respond(0, 0, "Already exist information", null, null));
                }

                var resultFindAll = await this.plannerManager.FindAllAsync();

                var response = new
                {
                    scheduleId = responseCreate
                };

                return Created("", this.respondHelper.Respond(resultFindAll.Count(), 1, "Insert success", null, response));
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [HttpPut("edit/{id}")]
        public async Task<IActionResult> Update(PlannerModel_Request entity, int id)
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

                entity.CreatedBy = Convert.ToInt32(result.userId); //Result is userID

                string baseUrl = $"{this.Request.Scheme}://{this.Request.Host.Value.ToString()}{this.Request.PathBase.Value.ToString()}";


                var responseCreate = await this.plannerManager.UpdateAsync(entity, id, baseUrl);

                if (!responseCreate)
                {
                    return BadRequest(this.respondHelper.Respond(0, 0, "Cannot edit the information because the event has been cancelled.", null, null));
                }

                var resultFindAll = await this.plannerManager.FindAllAsync();

                return Ok(this.respondHelper.Respond(resultFindAll.Count(), 1, "Update success", null, null));
            }
            catch (Exception ex)
            {
                return BadRequest(this.respondHelper.Respond(0, 0, $"Error : {ex}", null, null));
            }
        }

        [HttpGet("filter_schedule")]
        public async Task<IActionResult> FilterSchedule(int businessTypeId, int learningTypeId, int contentFormatId)
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

                if ( result.roleId == (int)Roles.viewer)
                {
                    return NotFound(this.respondHelper.Respond(null, null, "Unauthorized", null, null));
                }


                var resultFindAll = await this.plannerManager.FilterContentName(businessTypeId, learningTypeId, contentFormatId);

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
                throw ex;
            }
        }

        [HttpGet("{scheduleId}")]
        public async Task<IActionResult> GetByScheduleId(int scheduleId)
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

                if ( result.roleId == (int)Roles.viewer)
                {
                    return NotFound(this.respondHelper.Respond(null, null, "Unauthorized", null, null));
                }

                //Get all data
                var resultFindById =  await this.plannerManager.FindByIdPlannerAsync(scheduleId);

                if (resultFindById != null)
                {

                    if (resultFindById.Trainer != null)
                    {
                        //Find count 
                        var resultGetCount = await this.plannerManager.FindEventCountTrainer(resultFindById.Trainer.Id, resultFindById.StartTime);

                        resultFindById.EventCount = resultGetCount;
                    }
                    else
                    {
                        resultFindById.EventCount = 0;
                    }

                    var data = new
                    {
                        message = "Get success",
                        data = resultFindById
                    };

                    if (data.data != null)
                    {
                        return Ok(this.respondHelper.Respond(1, 1, data.message, null, data.data));
                    }

                    
                }

                return Ok(this.respondHelper.Respond(0, 0, "Planner is empty", null, null));
            }
            catch (Exception ex)
            {
                return BadRequest(this.respondHelper.Respond(0, 0, $"Error : {ex}", null, null));
            }
        }

        [HttpGet("status")]
        public async Task<IActionResult> GetStatus()
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
                var resultFindAll = await this.plannerManager.FindAllStatusAsync();

                var data = new
                {
                    total = resultFindAll != null ? resultFindAll.Count() : 0,
                    count = resultFindAll != null ? resultFindAll.Count() : 0,
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

        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> Delete(int id)
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

                int CreatedBy = Convert.ToInt32(result.userId); //Result is userID

                var responseCreate = await this.plannerManager.DeleteAsync(id, CreatedBy);

                if (!responseCreate)
                {
                    return BadRequest();
                }

                var resultFindAll = await this.plannerManager.FindAllAsync();

                return Ok(this.respondHelper.Respond(resultFindAll.Count(), 1, "Update success", null, null));
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        //Button cancele event
        [HttpPut("cancel/{id}")]
        public async Task<IActionResult> Cancel(int id)
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

                int CreatedBy = Convert.ToInt32(result.userId); //Result is userID

                string baseUrl = $"{this.Request.Scheme}://{this.Request.Host.Value.ToString()}{this.Request.PathBase.Value.ToString()}";

                var responseCreate = await this.plannerManager.CancelAsync(id, CreatedBy, baseUrl);

                if (!responseCreate)
                {
                    return BadRequest();
                }

                var resultFindAll = await this.plannerManager.FindAllAsync();

                return Ok(this.respondHelper.Respond(resultFindAll.Count(), 1, "Delete success", null, null));
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [HttpGet("schedule")]
        public async Task<IActionResult> GetSchedule(int trainerId, DateTime startTime, DateTime endTime)
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

                if ( result.roleId == (int)Roles.viewer)
                {
                    return NotFound(this.respondHelper.Respond(null, null, "Unauthorized", null, null));
                }

                //Get all data
                var resultFindAll = await this.plannerManager.FindScheduleAsync(trainerId, startTime, endTime);

                var data = new
                {
                    total = resultFindAll != null ? resultFindAll.Count() : 0,
                    count = resultFindAll != null ? resultFindAll.Count() : 0,
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

        [HttpPut("delete_trainer")]
        public async Task<IActionResult> DeleteTrainer(int scheduleId, int trainerId)
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

                //Delete trainer
                var resultFindAll = await this.plannerManager.DeleteTrainer(scheduleId, trainerId);

                if (!resultFindAll)
                {
                    return BadRequest();
                }


                return Ok(this.respondHelper.Respond(0, 0, null, "Delete trainer success", 0));
            }
            catch (Exception ex)
            {
                return BadRequest(this.respondHelper.Respond(0, 0, $"Error : {ex}", null, null));
            }
        }

        [HttpGet("current_moderator")]
        public async Task<IActionResult> GetCurrentModerator(int eventScheduleId)
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

                if (eventScheduleId == 0)
                {
                    return NotFound(this.respondHelper.Respond(null, null, "Please enter schedule id.", null, null));
                }

                //Get all data
                var resultFindAll = await this.plannerManager.GetCurMod(eventScheduleId);

                var data = new
                {
                    total = resultFindAll != null ? resultFindAll.Count() : 0,
                    count = resultFindAll != null ? resultFindAll.Count() : 0,
                    message = "Get all success",
                    data = resultFindAll
                };

                return Ok(this.respondHelper.Respond(data.total, data.count, data.message, null, data.data));
            }
            catch (Exception ex)
            {
                return BadRequest(this.respondHelper.Respond(0, 0, $"Error : {ex}", null, null));
                throw ex;
            }
        }

    }
}
