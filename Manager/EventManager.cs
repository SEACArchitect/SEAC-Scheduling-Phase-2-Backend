using Microsoft.EntityFrameworkCore;
using MySqlConnector;
using Nri_Webapplication_Backend.DTO;
using Nri_Webapplication_Backend.Helpers;
using Nri_Webapplication_Backend.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Type = Nri_Webapplication_Backend.Models.Type;
using System.Dynamic;

namespace Nri_Webapplication_Backend.Managers
{
    public interface IEventManager
    {
        Task<List<EventScheduleLogsModel>> EventLogsAsync(DateTime startDateTime, DateTime endDateTime, List<string> fields, List<string> actions);
        Task<List<OptionModel>> GetUpdateFields();
        Task<List<EventExportForYNUModels>> EventExportYNU(DateTime startDateTime, DateTime endDateTime);
        Task<List<EventExportForYNUYModels>> EventExportYNUY(DateTime startDateTime, DateTime endDateTime);
        Task<List<EventExportForAllModels>> EventExportAll(List<string> businessType, DateTime startDateTime, DateTime endDateTime);
    }
    public class EventManager : IEventManager
    {
        public ConnectionHelper Db { get; set; }
        public EventManager(ConnectionHelper Db)
        {
            this.Db = Db;
        }
        public async Task<List<EventScheduleLogsModel>> EventLogsAsync(DateTime startDateTime, DateTime endDateTime, List<string> fields, List<string> actions)
        {
            try
            {
                using (var context = new seac_webapplicationContext())
                {
                    var query = (from evsl in await context.EventScheduleLogs.ToListAsync()
                                 join cn in await context.ContentMaster.ToListAsync()
                                 on evsl.ContentId equals cn.ContentId
                                 join us in await context.Users.ToListAsync()
                                 on evsl.UserId equals us.UserId
                                 join ev in await context.EventSchedule.ToListAsync()
                                 on evsl.EventScheduleId equals ev.EventScheduleId
                                 join cat in await context.EventCategory.ToListAsync()
                                 on ev.EventCategoryId equals cat.EventCategoryId
                                 join bus in await context.ContentBusinessType.ToListAsync()
                                 on cn.ContentBusinessTypeId equals bus.ContentBusinessTypeId
                                 where (fields.Count == 0 || fields.Contains(evsl.Field.ToLower()))
                                 && (actions.Count == 0 || actions.Contains(evsl.Action.ToLower()))
                                 && evsl.LoggedDateTime >= startDateTime && evsl.LoggedDateTime <= endDateTime
                                 orderby evsl.LoggedDateTime descending
                                 select new EventScheduleLogsModel()
                                 {
                                     // separate date and time
                                     date = evsl.LoggedDateTime.ToString("yyyy-MM-dd"),
                                     time = evsl.LoggedDateTime.ToString("HH:mm"),
                                     eventId = evsl.EventScheduleId,
                                     eventName = cn.ContentName,
                                     field = evsl.Field,
                                     fromValue = evsl.FromValue,
                                     toValue = evsl.ToValue,
                                     changedBy = us.UserName,
                                     action = evsl.Action,
                                     category = cat.EventCategoryName,
                                     businessType = bus.ContentBusinessTypeName,
                                     eventDate = ev.StartTime.ToString("yyyy-MM-dd HH:mm:ss"),
                                     remark = ev.Remark,
                                     company = ev.Company,
                                     projectId = ev.ProjectId,
                                     bookBy = ev.BookBy
                                 }).ToList();
                    return query;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public async Task<List<OptionModel>> GetUpdateFields()
        {
            try
            {
                using (var context = new seac_webapplicationContext())
                {
                    var query = (from fls in await context.EventScheduleLogs.ToListAsync()
                                 where fls.Action == "Update"
                                 select new OptionModel()
                                 {
                                     value = fls.Field
                                 });
                    var x = query.GroupBy(fields => fields.value).Select(grp => grp.First()).ToList();
                    return x;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public async Task<List<EventExportForYNUModels>> EventExportYNU(DateTime startDateTime, DateTime endDateTime)
        {
            try
            {
                using (var context = new seac_webapplicationContext())
                {
                    var query = (from ev in await context.EventSchedule.ToListAsync()
                                 join cn in await context.ContentMaster.ToListAsync()
                                 on ev.ContentId equals cn.ContentId
                                 join evt in await context.EventScheduleTrainer.ToListAsync()
                                 on ev.EventScheduleId equals evt.EventScheduleId
                                 join t in await context.TrainerMaster.ToListAsync()
                                 on evt.TrainerId equals t.TrainerId
                                 join r in await context.EventRoomMaster.ToListAsync()
                                 on ev.EventRoomId equals r.EventRoomId
                                 where evt.IsActive == 1
                                 && ev.ContentBusinessTypeId == 1
                                 && ev.UploadToInstancyDate >= startDateTime && ev.UploadToInstancyDate <= endDateTime
                                 && ev.IsActive == 1
                                 select new EventExportForYNUModels()
                                 {
                                     contentName = cn.ContentName,
                                     tagLine = "",
                                     description = "",
                                     longDescription = "",
                                     keywords = cn.IsPrivillege.ToString(), //dummy data for checking content privillege
                                     learningObjectives = ev.EventCategoryId.ToString(), //dummy data for checking event category
                                     courseOutline = ev.LearningTypeId.ToString(), //dummy data for checking LearningType
                                     activationDate = ev.ContentFormatId.ToString(), //dummy data for checking ContentFormat
                                     trainer = t.Email,
                                     additionalInstructors = "",
                                     eventType = "",
                                     location = r.InstancyLocationId.ToString(),
                                     allowInstances = "",
                                     parentEventName = cn.ContentName,
                                     webinarTool = "",
                                     enrollmentLimit = cn.PaxMax.ToString(),
                                     duration = cn.Duration.ToString(),
                                     startDateTime = ev.StartTime.ToString("MM/dd/yyyy HH:mm"),
                                     endDateTime = ev.EndTime.HasValue ? ev.EndTime.Value.ToString("MM/dd/yyyy HH:mm") : string.Empty,
                                     timeZone = "SE Asia Standard Time",
                                     facilitatorURL = ev.Link != null ? ev.Link + "&facilitator=true" : string.Empty,
                                     participantURL = ev.Link,
                                     relatedContent = "",
                                     rules = "",
                                     assignToComponents = "",
                                     categories = "",
                                     skills = "",
                                     solutions = "",
                                     modules = "",
                                     label = "2"
                                 }).ToList();

                    foreach (var result in query)
                    {
                        if (result.courseOutline == "1" && result.activationDate == "1")
                        {
                            result.eventType = "46";
                            result.assignToComponents = this.assignComponent(result.learningObjectives, result.keywords, "374_153");
                        }
                        else if (result.courseOutline == "1" && result.activationDate == "2")
                        {
                            result.eventType = "55";
                            result.assignToComponents = this.assignComponent(result.learningObjectives, result.keywords, "374_153, 483_153");

                            if (result.contentName.Contains("Unpacking") && result.assignToComponents != "")
                            {
                                result.assignToComponents = "374_153, 483_153, 482_153";
                            }
                        }
                        else if (result.courseOutline == "2" && result.activationDate == "1")
                        {
                            result.eventType = "48";
                            result.assignToComponents = this.assignComponent(result.learningObjectives, result.keywords, "374_153");
                        }
                        else
                        {
                            result.eventType = "55";
                            result.assignToComponents = this.assignComponent(result.learningObjectives, result.keywords, "374_153, 483_153");
                        }
                        result.keywords = ""; //clear dummy data for checking content privillege
                        result.learningObjectives = ""; //clear dummy data for checking event category
                        result.courseOutline = ""; //clear dummy data for checking LearningType
                        result.activationDate = ""; //clear dummy data for checking ContentFormat
                    }
                    return query;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private String assignComponent(string publicEvent, string privillegeContent, string assignComponent)
        {
            if (privillegeContent == "0" && publicEvent == "1") // not privillage content of public event
            {
                return assignComponent;
            }
            else
            {
                return "";
            }
        }

        public async Task<List<EventExportForYNUYModels>> EventExportYNUY(DateTime startDateTime, DateTime endDateTime)
        {
            try
            {
                using (var context = new seac_webapplicationContext())
                {
                    var query = (from ev in await context.EventSchedule.ToListAsync()
                                 join cn in await context.ContentMaster.ToListAsync()
                                 on ev.ContentId equals cn.ContentId
                                 join evt in await context.EventScheduleTrainer.ToListAsync()
                                 on ev.EventScheduleId equals evt.EventScheduleId
                                 join t in await context.TrainerMaster.ToListAsync()
                                 on evt.TrainerId equals t.TrainerId
                                 join r in await context.EventRoomMaster.ToListAsync()
                                 on ev.EventRoomId equals r.EventRoomId
                                 where evt.IsActive == 1
                                 //from ev in await context.EventSchedule.ToListAsync()
                                 //join cn in await context.ContentMaster.ToListAsync()
                                 //on ev.ContentId equals cn.ContentId
                                 //join evt in await context.EventScheduleTrainer.ToListAsync()
                                 //on ev.EventScheduleId equals evt.EventScheduleId
                                 //join t in await context.TrainerMaster.ToListAsync()
                                 //on evt.TrainerId equals t.TrainerId
                                 //join cf in await context.ContentFormatMaster.ToListAsync()
                                 //on ev.ContentFormatId equals cf.ContentFormatId
                                 //where evt.IsActive == 1
                                 && ev.ContentBusinessTypeId == 2
                                 && ev.UploadToInstancyDate >= startDateTime && ev.UploadToInstancyDate <= endDateTime
                                 && ev.IsActive == 1
                                 select new EventExportForYNUYModels()
                                 {
                                     //startDateTime = ev.StartTime.ToString("MM/dd/yyyy HH:mm"),
                                     //endDateTime = ev.EndTime.HasValue ? ev.EndTime.Value.ToString("MM/dd/yyyy HH:mm") : string.Empty,
                                     //contentType = cf.ContentFormatName,
                                     //contentName = cn.ContentName,
                                     //trainer = t.Name + " " + t.LastName,
                                     //link = ev.Link
                                     contentName = cn.ContentName,
                                     tagLine = "",
                                     description = "",
                                     longDescription = "",
                                     keywords = cn.IsPrivillege.ToString(), //dummy data for checking content privillege
                                     learningObjectives = ev.EventCategoryId.ToString(), //dummy data for checking event category
                                     courseOutline = ev.LearningTypeId.ToString(), //dummy data for checking LearningType
                                     activationDate = ev.ContentFormatId.ToString(), //dummy data for checking ContentFormat
                                     trainer = t.Email,
                                     additionalInstructors = "",
                                     eventType = "",
                                     location = r.InstancyLocationId.ToString(),
                                     allowInstances = "",
                                     parentEventName = cn.ContentName,
                                     webinarTool = "",
                                     enrollmentLimit = cn.PaxMax.ToString(),
                                     duration = cn.Duration.ToString(),
                                     startDateTime = ev.StartTime.ToString("MM/dd/yyyy HH:mm"),
                                     endDateTime = ev.EndTime.HasValue ? ev.EndTime.Value.ToString("MM/dd/yyyy HH:mm") : string.Empty,
                                     timeZone = "SE Asia Standard Time",
                                     facilitatorURL = ev.Link != null ? ev.Link + "&facilitator=true" : string.Empty,
                                     participantURL = ev.Link,
                                     relatedContent = "",
                                     rules = "",
                                     assignToComponents = "",
                                     categories = "",
                                     skills = "",
                                     solutions = "",
                                     modules = "",
                                     label = "5"
                                 }).ToList();

                    foreach (var result in query)
                    {
                        if (result.courseOutline == "1" && result.activationDate == "1")
                        {
                            result.eventType = "46";
                            //result.assignToComponents = this.assignComponent(result.learningObjectives, result.keywords, "374_153");
                            result.assignToComponents = this.assignComponent(result.learningObjectives, result.keywords, "900_153");
                        }
                        else if (result.courseOutline == "1" && result.activationDate == "2")
                        {
                            result.eventType = "55";
                            //result.assignToComponents = this.assignComponent(result.learningObjectives, result.keywords, "374_153, 483_153");
                            result.assignToComponents = this.assignComponent(result.learningObjectives, result.keywords, "900_153");

                            if (result.contentName.Contains("Unpacking") && result.assignToComponents != "")
                            {
                                //result.assignToComponents = "374_153, 483_153, 482_153";
                                result.assignToComponents = "900_153";
                            }
                        }
                        else if (result.courseOutline == "2" && result.activationDate == "1")
                        {
                            result.eventType = "48";
                            //result.assignToComponents = this.assignComponent(result.learningObjectives, result.keywords, "374_153");
                            result.assignToComponents = this.assignComponent(result.learningObjectives, result.keywords, "900_153");
                        }
                        else
                        {
                            result.eventType = "55";
                            //result.assignToComponents = this.assignComponent(result.learningObjectives, result.keywords, "374_153, 483_153");
                            result.assignToComponents = this.assignComponent(result.learningObjectives, result.keywords, "900_153");
                        }
                        result.keywords = ""; //clear dummy data for checking content privillege
                        result.learningObjectives = ""; //clear dummy data for checking event category
                        result.courseOutline = ""; //clear dummy data for checking LearningType
                        result.activationDate = ""; //clear dummy data for checking ContentFormat
                        if (result.eventType == "55")
                        {
                            result.location = "";
                        }
                    }
                    return query;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public async Task<List<EventExportForAllModels>> EventExportAll(List<string> businessType, DateTime startDateTime, DateTime endDateTime)
        {
            try
            {
                using (var context = new seac_webapplicationContext())
                {
                    var query = (from ev in await context.EventSchedule.ToListAsync()
                                 join cn in await context.ContentMaster.ToListAsync()
                                 on ev.ContentId equals cn.ContentId
                                 join evt in await context.EventScheduleTrainer.ToListAsync()
                                 on ev.EventScheduleId equals evt.EventScheduleId
                                 join t in await context.TrainerMaster.ToListAsync()
                                 on evt.TrainerId equals t.TrainerId
                                 join cf in await context.ContentFormatMaster.ToListAsync()
                                 on ev.ContentFormatId equals cf.ContentFormatId
                                 join r in await context.EventRoomMaster.ToListAsync()
                                 on ev.EventRoomId equals r.EventRoomId
                                 join cg in await context.EventCategory.ToListAsync()
                                 on ev.EventCategoryId equals cg.EventCategoryId
                                 join lt in await context.LearningTypeMaster.ToListAsync()
                                 on ev.LearningTypeId equals lt.LearningTypeId
                                 join lg in await context.Language.ToListAsync()
                                 on ev.LanguageId equals lg.LanguageId
                                 join bus in await context.ContentBusinessType.ToListAsync()
                                 on ev.ContentBusinessTypeId equals bus.ContentBusinessTypeId
                                 where evt.IsActive == 1
                                 && businessType.Contains(ev.ContentBusinessTypeId.ToString())
                                 && ev.UploadToInstancyDate >= startDateTime && ev.UploadToInstancyDate <= endDateTime
                                 && ev.IsActive == 1
                                 select new EventExportForAllModels()
                                 {
                                     eventScheduleID = ev.EventScheduleId.ToString(),
                                     contentName = cn.ContentName,
                                     roomName = r.EventRoomName,
                                     trainer = t.Name + ' ' + t.LastName,
                                     category = cg.EventCategoryName,
                                     learningType = lt.LearningTypeName,
                                     contentFormat = cf.ContentFormatName,
                                     startDateTime = ev.StartTime.ToString("MM/dd/yyyy HH:mm"),
                                     endDateTime = ev.EndTime.HasValue ? ev.EndTime.Value.ToString("MM/dd/yyyy HH:mm") : string.Empty,
                                     duration = cn.Duration.ToString(),
                                     link = ev.Link,
                                     projectId = ev.ProjectId,
                                     projectName = ev.ProjectName,
                                     company = ev.Company,
                                     noOfParticipant = ev.NoOfParticipant.ToString(),
                                     session = ev.Session,
                                     remark = ev.Remark,
                                     language = lg.LanguageName,
                                     contentBusinessType = bus.ContentBusinessTypeName,
                                     location = ev.Location,
                                     bookBy = ev.BookBy,
                                     isBillable = ev.IsBillable.HasValue? ev.IsBillable.ToString()=="1"? "true": "false" : string.Empty,
                                     status = ev.StatusId.ToString()
                                 }).ToList();
                    foreach (var chkResult in query)
                    {
                        if (chkResult.status != null && chkResult.status != "")
                        {
                            var resultStatus = await context.Status.Where(o => o.StatusId.ToString() == chkResult.status).FirstOrDefaultAsync();

                            chkResult.status = resultStatus.Status1;
                        }
                    }


                    foreach (var result in query)
                    {
                        var modList = (from est in await context.EventScheduleModerator.Where(o => o.IsActive == 1).ToListAsync()
                                       join tnm in await context.TrainerMaster.ToListAsync()
                                       on est.ModeratorId equals tnm.TrainerId
                                       where est.EventScheduleId.ToString() == result.eventScheduleID
                                       select new
                                       {
                                           Id = tnm.TrainerId,
                                           Name = tnm.Name + " " + tnm.LastName                                           

                                       }).ToList();

                        foreach (var mod in modList)
                        {
                            result.moderator = result.moderator + ", " + mod.Name ;
                        }
                        result.moderator = result.moderator != null? result.moderator.Substring(1): result.moderator;
                    }
                    
                    return query;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
