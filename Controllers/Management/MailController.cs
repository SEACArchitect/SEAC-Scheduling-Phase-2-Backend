using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

using Nri_Webapplication_Backend.Helpers;
using Nri_Webapplication_Backend.Managers;
using Nri_Webapplication_Backend.Models;

namespace Nri_Webapplication_Backend.Controllers.Management
{
    [Route("mail")]
    [ApiController]
    public class MailController : ControllerBase
    {
        public readonly IRespondHelper respondHelper;
        public readonly IMailManager mailManager;
        public readonly ICommonHelper commonHelper;
        public readonly IIcsSentMailManager icsSentMailManager;
        public readonly IAutoSentMailHelper autoSentMailHelper;
        private readonly IHttpContextAccessor _httpContextAccessor;
        public readonly IConfiguration config;
        public readonly IUserRoleManager userRoleManager;

        public MailController(IRespondHelper respondHelper, IMailManager mailManager, ICommonHelper commonHelper, IIcsSentMailManager icsSentMail, IAutoSentMailHelper autoSentMailHelper, IHttpContextAccessor httpContextAccessor, IConfiguration config,IUserRoleManager userRoleManager)
        {
            this.respondHelper = respondHelper;
            this.mailManager = mailManager;
            this.commonHelper = commonHelper;
            this.icsSentMailManager = icsSentMail;
            this.autoSentMailHelper = autoSentMailHelper;
            this._httpContextAccessor = httpContextAccessor;
            this.config = config;
            this.userRoleManager = userRoleManager;
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
                var resultFindAll = await this.mailManager.FindAllAsync();

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


        [HttpPost]
        public async Task<IActionResult> SendEmail(List<int> entity)
        {
            try
            {
                string baseUrl = this.config["ICSSetting:url"];
                string resultStr = "";
                List<AutoMailAppoinmentModel> events = await this.icsSentMailManager.FindAllEventScheduleAsync(entity, 0);
                foreach (var e in events)
                {
                    var icsUUID = Guid.NewGuid().ToString().ToUpper();
                    var emailLogList = await this.icsSentMailManager.GetEmailLog(e.EventSchedule.EventScheduleId, e.TrainerMaster.TrainerId);
                    var sendResult = this.autoSentMailHelper.SendEmail(e, emailLogList, baseUrl, icsUUID);
                    if (emailLogList.Where(s => s.IsCancel == 0).ToList().Count > 0)
                    {
                        icsUUID = emailLogList.FirstOrDefault().UUID;
                    }
                    var logResult = this.icsSentMailManager.InsertEmailLogAsync(e.EventSchedule.EventScheduleId.ToString(), e.TrainerMaster.TrainerId, icsUUID, 1);
                    if (await sendResult && await logResult)
                    {
                        resultStr = "Sent Mail success";
                    }
                }
                var data = new
                {
                    total = events.Count(),
                    count = events.Count(),
                    message = resultStr,
                    data = events
                };
                return Ok(this.respondHelper.Respond(data.total, data.count, data.message, null, null));
            }
            catch (Exception ex)
            {
                return BadRequest(this.respondHelper.Respond(0, 0, $"Error : {ex}", null, null));
            }
        }
        [HttpPost("request_moderator")]
        public async Task<IActionResult> SendMailRequestMod(RequesModerator e)
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

                var modManager = await userRoleManager.FindModManager();
                List<string> email = new List<string>();
                if (modManager.Count == 0)
                {
                    return BadRequest(this.respondHelper.Respond(null, null, "Moderator manager not found.Please contact administrator.", null, null));
                }
                foreach (var item in modManager)
                {
                    if (item != null)
                    {
                        if (item.Email != null)
                        {
                            email.Add(item.Email);
                        }
                    }
                }

                var sendResult = this.autoSentMailHelper.SendReqMod(e, email);

                if (await sendResult != true)
                {
                    return BadRequest(this.respondHelper.Respond(null, null, "Sent Email Failed", null, null));
                }

                return Ok(this.respondHelper.Respond(null, null, "Sent Email Success.", null, null));
            }
            catch (Exception ex)
            {
                return BadRequest(this.respondHelper.Respond(null, null, $"Error : {ex}", null, null));
                throw ex;
            }
        }
    }
}
