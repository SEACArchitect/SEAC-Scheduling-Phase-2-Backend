using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;

using Nri_Webapplication_Backend.Helpers;
using Nri_Webapplication_Backend.Managers;
using Nri_Webapplication_Backend.Models;
using Microsoft.Extensions.Configuration;

namespace Nri_Webapplication_Backend.Controllers
{
    [ApiController]
    [Route("[controller]")]

    public class ICSMailController : ControllerBase
    {
        public readonly IRespondHelper respondHelper;
        public readonly IIcsSentMailManager icsSentMailManager;
        public readonly IAutoSentMailHelper autoSentMailHelper;
        public readonly ICommonHelper commonHelper;
        private readonly IHttpContextAccessor _httpContextAccessor;
        public readonly IConfiguration config;
        public ICSMailController(IRespondHelper respondHelper, ICommonHelper commonHelper, IIcsSentMailManager icsSentMailManager, IAutoSentMailHelper autoSentMailHelper, IHttpContextAccessor httpContextAccessor, IConfiguration config)
        {
            this.respondHelper = respondHelper;
            this.icsSentMailManager = icsSentMailManager;
            this.autoSentMailHelper = autoSentMailHelper;
            this.commonHelper = commonHelper;
            this._httpContextAccessor = httpContextAccessor;
            this.config = config;
        }

        [HttpGet("")]
        public async Task<IActionResult> SentAutoMail()
        {
            try
            {
                string baseUrl = this.config["ICSSetting:url"];
               
                string respondStr = "";
              
                var autoMailAppoinmentModel = await this.icsSentMailManager.FindAllEventScheduleAsync();
                foreach (var eventSchedule in autoMailAppoinmentModel)
                {
                    var icsUUID = Guid.NewGuid().ToString().ToUpper();
                    var emailLogList = await this.icsSentMailManager.GetEmailLog(eventSchedule.EventSchedule.EventScheduleId, eventSchedule.TrainerMaster.TrainerId);
                    var sendResult = this.autoSentMailHelper.SendEmail(eventSchedule, emailLogList, baseUrl, icsUUID);
                    var eventScheduleId = eventSchedule.EventSchedule.EventScheduleId.ToString();
                    if (emailLogList.Where(s => s.IsCancel == 0).ToList().Count > 0)
                    {
                        icsUUID = emailLogList.FirstOrDefault().UUID;
                    }
                    var logResult = this.icsSentMailManager.InsertEmailLogAsync(eventScheduleId, eventSchedule.TrainerMaster.TrainerId, icsUUID, 1);
                    if (await sendResult && await logResult)
                    {
                        respondStr = "Sent Mail success";
                    }
                }
                var data = new
                {
                    total = autoMailAppoinmentModel.Count(),
                    count = autoMailAppoinmentModel.Count(),
                    message = respondStr,
                    data = autoMailAppoinmentModel
                };
                return Ok(this.respondHelper.Respond(data.total, data.count, data.message, null, null));
            }
            catch (Exception ex)
            {
                return BadRequest(this.respondHelper.Respond(0, 0, $"Error : {ex}", null, null));
            }
        }

        [HttpGet("CancleEmail")]
        public async Task<IActionResult> CancleEmail(int eventScheduleId, int trainerId)
        {
            try
            {
                string baseUrl = this.config["ICSSetting:url"];

                string respondStr = "";
                List<int> EntityList = new List<int>();
                EntityList.Add(eventScheduleId);
                List<EmailLogModel> emailLogsList = new List<EmailLogModel>();
                 emailLogsList = await this.icsSentMailManager.GetEmailLog(eventScheduleId, trainerId);

                var eventSchedule = await this.icsSentMailManager.FindAllEventScheduleAsync(EntityList, 1);


                var emailLog = emailLogsList.Where(o => o.IsCancel == 0).FirstOrDefault();
                if (emailLog!=null)
                {
                    var sendResult = this.autoSentMailHelper.SendEmail(eventSchedule.FirstOrDefault(), emailLogsList, baseUrl, emailLog.UUID, "Cancle");

                    var logResult = this.icsSentMailManager.ModifyEmailLogAsync(eventScheduleId, trainerId);
                    if (await sendResult && await logResult)
                    {
                        respondStr = "Sent Mail success";
                    }
                }
                else
                {
                    return BadRequest("You have cancel to this trainer before. Please contact admin.");
                }




                return Ok(this.respondHelper.Respond(0, 0, respondStr, null, null));
            }
            catch (Exception ex)
            {
                return BadRequest(this.respondHelper.Respond(0, 0, $"Error : {ex}", null, null));
            }
        }

        [HttpGet("AcceptEventSchedule")]
        public async Task<IActionResult> AcceptEventSchedule(string eventScheduleID, string Trainer, int isAccept, string UUID)
        {
            try
            {
                string respondStr = "";

                var eventScheduleIdDecrypt = this.commonHelper.Decrypt(eventScheduleID);
                var TrainerDecrypt = this.commonHelper.Decrypt(Trainer);
              var  emailLogsList = await this.icsSentMailManager.GetEmailLog(int.Parse(eventScheduleIdDecrypt), int.Parse(TrainerDecrypt));
                var emailLogs = emailLogsList.Where(s => s.IsCancel == 1 && s.UUID == UUID).ToList();
                if (emailLogs.Count==0)
                {
                    var logResult = this.icsSentMailManager.InsertScheduleAcceptedLogAsync(eventScheduleIdDecrypt, TrainerDecrypt, isAccept, UUID);
                    if (await logResult)
                    {
                        respondStr = (isAccept == 1 ? "Accept" : "Decline") + " Schedule Success.";
                    }
                    else
                    {
                        return BadRequest("You have responded to this schedule before. Please contact admin.");
                    }
                }
                else
                {
                    return BadRequest("This schedule has been cancelled. Please contact admin.");

                }
                
                return Ok(respondStr);
            }
            catch (Exception ex)
            {
                return BadRequest(this.respondHelper.Respond(0, 0, $"Error : {ex}", null, null));
            }
        }
    }
}
