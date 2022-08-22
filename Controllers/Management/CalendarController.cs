using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nri_Webapplication_Backend.Helpers;
using Nri_Webapplication_Backend.Managers;
using Nri_Webapplication_Backend.Models;
using System.Net;
using System.IO;
using System.Data;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;

namespace Nri_Webapplication_Backend.Controllers.Management
{
    [Route("calendar_management")]
    [ApiController]
    public class CalendarController : ControllerBase
    {
        public readonly IRespondHelper respondHelper;
        public readonly ICalendarManager calendarManager;
        public readonly ICommonHelper commonHelper;
        public CalendarController(IRespondHelper respondHelper, ICalendarManager calendarManager, ICommonHelper commonHelper)
        {
            this.respondHelper = respondHelper;
            this.calendarManager = calendarManager;
            this.commonHelper = commonHelper;
        }

        [HttpPost("get_calendar")]
        public async Task<IActionResult> GetCalendar(DwnLoadIcs dwnLoadIcs)
        {
            try
            {
                //Check authen by jwt
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

                if (dwnLoadIcs.IcsPath == null)
                {
                    return BadRequest(this.respondHelper.Respond(null, null, "Calendar path not found", null, null));
                }

                //var webReq = WebRequest.Create(dwnLoadIcs.IcsPath.ToString());
                //var webRes = webReq.GetResponseAsync();
                //var content = webRes.Result;
                //var reader = new StreamReader("");

                //var client = new RestClient(dwnLoadIcs.IcsPath.ToString());
                //var req = new RestRequest("");
                //var res = client.GetAsync(req);
                //var resResult = res.Result.Content;

                //var reader = new StreamReader(@"C:\Users\tiwa.ho\Documents\ONG\ZONG\Project\Ext\nri\ical\calProblem.ics");

                //WebClient client = new WebClient();
                //client.DownloadFile(dwnLoadIcs.IcsPath.ToString(), @"C:\Users\tiwa.ho\Documents\ONG\ZONG\Project\Ext\nri\ical\calProblem.ics");
                //Stream sdata = client.OpenRead(dwnLoadIcs.IcsPath.ToString());
                //StreamReader reader = new StreamReader(@"C:\Users\tiwa.ho\Documents\ONG\ZONG\Project\Ext\nri\ical\calendar.ics");
                //string icsCalendar = reader.ReadToEnd();

                var client = new RestClient(dwnLoadIcs.IcsPath.ToString());
                var req = new RestRequest("");
                var res = client.GetAsync(req);
                var resResult = res.Result.Content;
                string icsCalendar = resResult.ToString();

                List<Calendar> calDatas = new List<Calendar>();
                Ical.Net.Calendar calendar = Ical.Net.Calendar.Load(icsCalendar);
 
                foreach (var item in calendar.Events)
                {
                    if (item.DtStart.AsDateTimeOffset.DateTime >= DateTime.Now)
                    {
                        Calendar calData = new Calendar
                        {
                            Summary = item.Summary,
                            DtStart = item.DtStart.AsDateTimeOffset.DateTime,
                            DtEnd = item.DtEnd.AsDateTimeOffset.DateTime,
                            Location = item.Location
                        };
                        calDatas.Add(calData);
                    } 
                }

                //insert database
                int dwnResult = await this.calendarManager.InsertAsync(dwnLoadIcs, calDatas);

                var data = new
                {
                    total = dwnResult,
                    message = "Download Success."
                };
                
                return Ok(this.respondHelper.Respond(data.total, null, data.message, null, null));
            }
            catch (Exception ex)
            {
                return BadRequest(this.respondHelper.Respond(null, null, $"Error : {ex}", null, null));
                throw ex;
            }
        }

        [HttpPost("sync_calendar")]
        public async Task<IActionResult> Sync()
        {
            try
            {
                //Check authen by jwt
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

                if (result.userId == null || Convert.ToInt32(result.userId) == 0)
                {
                    return BadRequest(this.respondHelper.Respond(null, null, "Not User ID", null, null));
                }

                int usrId = Convert.ToInt32(result.userId);
                //RoomParing entity = new RoomParing();
                int idLnk = await this.calendarManager.GetLatestIdLink();

                await this.calendarManager.PairingRoom(usrId, idLnk);

                return Ok( this.respondHelper.Respond(1, null, "Pairing success", null, null));
            }
            catch (Exception ex)
            {
                return BadRequest(this.respondHelper.Respond(null, null, $"Error : {ex}", null, null));
                throw ex;
            }
        }

        [HttpPost("get_ics_link")]
        public async Task<IActionResult> GetLink()
        {
            try
            {
                //Check authen by jwt
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

                var resultLink = await this.calendarManager.GetCalLnk();

                var data = new
                {
                    total = resultLink.Count(),
                    count = resultLink.Count(),
                    message = "Get all success",
                    data = resultLink
                };

                return Ok(this.respondHelper.Respond(data.total, data.count, data.message, null, data.data));
            }
            catch (Exception ex)
            {
                return BadRequest(this.respondHelper.Respond(null, null, $"Error : {ex}", null, null));
                throw ex;
            }
        }
        [HttpPut("update_ics_link")]
        public async Task<IActionResult> UpdateLink(EditLink entity)
        {
            try
            {
                //Check authen by jwt
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

                if(entity.IcsLink == null || entity.IdLink.ToString() == null || entity.Status.ToString() == null || entity.EditBy.ToString() == null)
                {
                    return BadRequest(this.respondHelper.Respond(null, null, "IcsLink,IdLink,Status or EditBy are missing.", null, null));
                }

                entity.EditBy = Convert.ToInt32(result.userId);

                await this.calendarManager.UpdCalLnk(entity);

                return Ok(this.respondHelper.Respond(1, null, "Update success", null, null));
            }
            catch (Exception ex)
            {
                return BadRequest(this.respondHelper.Respond(null, null, $"Error : {ex}", null, null));
                throw ex;
            }
        }

        [HttpPut("delete_ics_link")]
        public async Task<IActionResult> DeleteLink(int idLnk)
        {
            try
            {
                //Check authen by jwt
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

                if (idLnk == 0 || idLnk.ToString() == null)
                {
                    return BadRequest(this.respondHelper.Respond(null, null, "Not calendar id link", null, null));
                }

                await this.calendarManager.DelCalLnk(idLnk,Convert.ToInt32(result.userId));

                return Ok(this.respondHelper.Respond(1, null, "Delete success", null, null));
            }
            catch (Exception ex)
            {
                return BadRequest(this.respondHelper.Respond(null, null, $"Error : {ex}", null, null));
                throw ex;
            }
        }

        [HttpPost("get_classroom")]
        public async Task<IActionResult> GetClassRoom(GetClassRoom entity)
        {
            try
            {
                //Check authen by jwt
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

                if(entity == null)
                {
                    return BadRequest(this.respondHelper.Respond(null, null, "Body is empty", null, null));
                }

                //if (entity.StartSearchDate.ToString() == null || entity.EndSearchDate.ToString() == null || entity.EndSearchDate.ToString() == null)
                //{
                //    return BadRequest(this.respondHelper.Respond(null, null, "search date or business type id are missing.", null, null));
                //}
                //Get all data
                var resultGetAll = await this.calendarManager.GetClassRoom(entity);

                var data = new
                {
                    total = resultGetAll.Count(),
                    count = resultGetAll.Count(),
                    message = "Get all success",
                    data = resultGetAll
                };

                return Ok(this.respondHelper.Respond(data.total, data.count, data.message, null, data.data));
            }
            catch (Exception ex)
            {
                return BadRequest(this.respondHelper.Respond(null, null, $"Error : {ex}", null, null));
                throw ex;
            }
        }

        [HttpPost("get_classroom_overlaping")]
        public async Task<IActionResult> GetClassroomOverlaping(SearchClassroomOverlaping entity)
        {
            try
            {
                //Check authen by jwt
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

                if (entity.StartDate == null || entity.EndDate == null)
                {
                    return BadRequest(this.respondHelper.Respond(null, null, "Body is empty", null, null));
                }

                var getAll = await calendarManager.GetClassOverlap(entity.StartDate, entity.EndDate);

                var data = new
                {
                    total = getAll.Count(),
                    count = getAll.Count(),
                    message = "Get all success",
                    data = getAll
                };

                return Ok(this.respondHelper.Respond(data.total, data.count, data.message, null, data.data));
            }
            catch (Exception ex)
            {
                return BadRequest(this.respondHelper.Respond(null, null, $"Error : {ex}", null, null));
                throw ex;
            }
        }
                
        [HttpPut("update_classroom")]
        public async Task<IActionResult> ApprPairingClassRoom(ListApprParams entity)
        {
            try
            {
                //Check authen by jwt
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
                if (result.userId == null)
                {
                    return NotFound(this.respondHelper.Respond(null, null, "Unauthorized", null, null));
                }
                foreach (var item in entity.ParamsList)
                {
                    if (item.ZoomLink == null || item.PairingId.ToString() == null || item.ApprStat.ToString() == null)
                    {
                        return BadRequest(this.respondHelper.Respond(null, null, "please enter zoom link,pairing id or approve.", null, null));
                    }
                }

                int countUpd = await calendarManager.UpdatePairing(entity,Convert.ToInt32(result.userId));
                if (countUpd == 0)
                {

                    return BadRequest(this.respondHelper.Respond(null, null, "Calendar data or Planner data not found", null, null));
                }

                var data = new
                {
                    //total_Create_schedule = countCreate,
                    total_Update_Link = countUpd,
                    message = "Update Success"
                };

                return Ok(this.respondHelper.Respond(1, null, data.message, null, data));
            }
            catch (Exception ex)
            {
                return BadRequest(this.respondHelper.Respond(null, null, $"Error : {ex}", null, null));
                throw ex;
            }
        }
        [HttpPut("update_classroom_nopairing")]
        public async Task<IActionResult> ApprNoPairingClassRoom(ListApprParams entity)
        {
            try
            {
                //Check authen by jwt
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
                if (result.userId == null)
                {
                    return NotFound(this.respondHelper.Respond(null, null, "Unauthorized", null, null));
                }
                foreach (var item in entity.ParamsList)
                {
                    if (item.ZoomLink == null || item.PairingId.ToString() == null || item.ApprStat.ToString() == null)
                    {
                        return BadRequest(this.respondHelper.Respond(null, null, "please enter zoom link,pairing id or approve.", null, null));
                    }
                }

                int countUpd = await calendarManager.UpdatePlanner(entity, Convert.ToInt32(result.userId));
                if (countUpd == 0)
                {

                    return BadRequest(this.respondHelper.Respond(null, null, "Calendar data or Planner data not found", null, null));
                }

                var data = new
                {
                    //total_Create_schedule = countCreate,
                    total_Update_Link = countUpd,
                    message = "Update Success"
                };

                return Ok(this.respondHelper.Respond(1, null, data.message, null, data));
            }
            catch (Exception ex)
            {
                return BadRequest(this.respondHelper.Respond(null, null, $"Error : {ex}", null, null));
                throw ex;
            }
        }
    }

}
