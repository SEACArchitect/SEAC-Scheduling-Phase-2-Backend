using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nri_Webapplication_Backend.Helpers;
using Nri_Webapplication_Backend.Managers;
using Nri_Webapplication_Backend.Models;

namespace Nri_Webapplication_Backend.Controllers.Management
{
    [Route("event")]
    [ApiController]
    public class EventController : ControllerBase
    {
        public readonly IRespondHelper respondHelper;
        public readonly IEventManager eventManager;
        public readonly ICommonHelper commonHelper;
        public EventController(IRespondHelper respondHelper, IEventManager eventManager, ICommonHelper commonHelper)
        {
            this.respondHelper = respondHelper;
            this.eventManager = eventManager;
            this.commonHelper = commonHelper;
        }

        [HttpGet("logs")]
        public async Task<IActionResult> EventLogs(DateTime startDateTime, DateTime endDateTime, string fields = "", string actions = "")
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

                // Check permission

                if ( result.roleId == (int)Roles.viewer)
                {
                    return NotFound(this.respondHelper.Respond(null, null, "Unauthorized", null, null));
                }

                // Logic for event log
                List<string> listFields = fields.ToLower().Split('_', StringSplitOptions.RemoveEmptyEntries).ToList();
                List<string> listActions = actions.ToLower().Split('_', StringSplitOptions.RemoveEmptyEntries).ToList();
                var resultFindAll = await this.eventManager.EventLogsAsync(startDateTime, endDateTime, listFields, listActions);
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
        
        [HttpGet("log_fields")]
        public async Task<IActionResult> GetUpdateFields()
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

                // Check permission

                if (result.roleId == (int)Roles.viewer)
                {
                    return NotFound(this.respondHelper.Respond(null, null, "Unauthorized", null, null));
                }
                // Logic for event log               
                var resultFindAll = await this.eventManager.GetUpdateFields();
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
        [HttpGet("export")]
        public async Task<IActionResult> EventScheduleExport(string businessType, DateTime startDateTime, DateTime endDateTime)
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

                // Check permission

                if (result.roleId == (int)Roles.viewer)
                {
                    return NotFound(this.respondHelper.Respond(null, null, "Unauthorized", null, null));
                }
                // Logic for event log   
                dynamic resultFindAll = null;
                if (businessType == "1")
                {
                    resultFindAll = await this.eventManager.EventExportYNU(startDateTime, endDateTime);

                }
                else if (businessType == "2")
                {
                    resultFindAll = await this.eventManager.EventExportYNUY(startDateTime, endDateTime);

                }
                else if (businessType == "0" || businessType == "3" || businessType == "4")
                {
                    List<string> bus = new List<string>();  
                    if (businessType == "0")
                    {
                        bus.Add("1");
                        bus.Add("2");
                        bus.Add("3");
                        bus.Add("4");
                    }
                    else
                    {
                        bus.Add(businessType);
                    }                    
                    resultFindAll = await this.eventManager.EventExportAll(bus,startDateTime, endDateTime);
                }
                var data = new
                {
                    total = resultFindAll.Count,
                    count = resultFindAll.Count,
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
