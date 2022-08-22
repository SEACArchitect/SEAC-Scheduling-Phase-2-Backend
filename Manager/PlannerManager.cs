using DDay.iCal;
using Microsoft.AspNetCore.Razor.Hosting;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using MySqlX.XDevAPI.Common;
using Nri_Webapplication_Backend.DTO;
using Nri_Webapplication_Backend.Helpers;
using Nri_Webapplication_Backend.Managers;
using Nri_Webapplication_Backend.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.OleDb;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static Nri_Webapplication_Backend.Models.TypeTrainer;
using ContentBusinessType = Nri_Webapplication_Backend.Models.ContentBusinessType;
using TrainerPlanner = Nri_Webapplication_Backend.Models.TrainerPlanner;

namespace Nri_Webapplication_Backend.Managers
{
    public enum typeAction
    {
        Create = 1,
        Update = 2,
        Delete = 3,
        Cancel = 4
    }

    public enum fieldEnum
    {
        StartTime,
        Trainer,
        Room,
        Session,
        Location,
        Language,
        Category,
        ProjectName,
        ProjectId,
        Company,
        BookBy,
        IsBillable,
        UploadDate,
        Link,
        Remark,
        NoOfParticipant,
        Status,
        Moderator
    }
    public interface IPlannerManager
    {
        Task<List<CategoryModel>> FindCategoryAsync();
        Task<List<LanguageModel>> FindLanguageAsync();
        Task<List<PlannerModel>> FindPlannerAsync(DateTime startDateTime, DateTime endDateTime);
        Task<PlannerModel> FindByIdPlannerAsync(int scheduleId);
        List<PlannerModel> FindPlannerByDate(DateTime startDateTime, DateTime endDateTime);
        PlannerModel FindPlannerById(int scheduleId);
        List<PlannerModel> GetEventsForPlanner(int[] ids);
        Task<int> InsertAsync(PlannerModel_Request entity);
        Task<bool> UpdateAsync(PlannerModel_Request entity, int scheduleId, string baseUrl);
        Task<bool> DeleteAsync(int scheduleId, int CreatedBy);
        Task<bool> CancelAsync(int scheduleId, int CreatedBy, string baseUrl);
        Task<List<EventSchedule>> FindAllAsync();
        Task<List<ContentPlanner>> FindAllContentMasterAsync();
        Task<List<ContactTypePlanner>> FindAllContactTypeAsyc();
        Task<List<TrainerContacTypeCombo>> FindAllTrainerContractTypeAsync(DateTime createdDate, string startTime, string endTime, int contentId, string mode);
        Task<List<TrainerContacTypeCombo>> FindAllModeratorMasterAsync(DateTime createdDate, string startTime, string endTime, int bussinessTypeId, int roomId);
        Task<List<TrainerPlanner>> FindAllModeratorOld();
        Task<List<ContentPlanner>> FilterContentName(int businessTypeId, int learningTypeId, int contentFormatId);
        Task<List<StatusPlanner>> FindAllStatusAsync();
        Task<List<RoomPlanner>> FindRoomAllAsync(DateTime createDate, string startTime, string endTime, int contentId, int bussinessTypeId);
        List<RoomPlanner> FindRoomByBusinessTypeAndContentFormat(int roomTypeId, int contentFormatId);
        Task<List<TrainerSchedule>> FindScheduleAsync(int trainerId, DateTime startDate, DateTime endDate);
        Task<int> FindEventCountTrainer(int? trainerId, DateTime createdDate);
        Task<bool> GetICSFile();
        Task<bool> GetICSFileByTrainer(int trainerId);
        Task<bool> DeleteTrainer(int scheduleId, int trainerId);
        Task<List<CurMod>> GetCurMod(int EventScheduleId);
    }

    public class PlannerManager : IPlannerManager
    {
        public ConnectionHelper Db { get; set; }
        public readonly IAutoSentMailHelper autoSentMailHelper;
        public readonly IIcsSentMailManager icsSentMailManager;

        public PlannerManager(ConnectionHelper Db, IAutoSentMailHelper autoSentMailHelper, IIcsSentMailManager icsSentMailManager)
        {
            this.Db = Db;
            this.autoSentMailHelper = autoSentMailHelper;
            this.icsSentMailManager = icsSentMailManager;
        }

        public async Task<int> FindEventCountTrainer(int? trainerId, DateTime createDate)
        {
            try
            {
                using (var context = new seac_webapplicationContext())
                {
                    var filterSchedule = await context.EventSchedule.Where(o => o.StartTime.Month == createDate.Month && o.StartTime.Year == createDate.Year && o.IsActive == 1).ToListAsync();

                    List<TrainerCount> idEventTrainer = new List<TrainerCount>();
                    List<EventSchduleTrainerCount> eventScCount = new List<EventSchduleTrainerCount>();

                    foreach (var itemSchedule in filterSchedule)
                    {
                        if (itemSchedule != null)
                        {
                            var resultSchedultTrainer = await context.EventScheduleTrainer.Where(o => o.EventScheduleId == itemSchedule.EventScheduleId && o.IsActive == 1).ToListAsync();

                            foreach (var item in resultSchedultTrainer)
                            {
                                eventScCount.Add(new EventSchduleTrainerCount { id = item.TrainerId, value = item.EventScheduleId });
                            }
                        }
                    }


                    var eventCount = eventScCount.GroupBy(o => o.id).ToList();

                    var filterEventCount = eventCount.Where(o => o.Key == trainerId);

                    return filterEventCount.SelectMany(eventScCount => eventScCount).Count();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public async Task<List<PlannerModel>> FindPlannerAsync(DateTime startDateTime, DateTime endDateTime)
        {
            try
            {

                using (var context = new seac_webapplicationContext())
                {

                    var queryTrainer = (from evs in await context.EventSchedule.ToListAsync()
                                        join ctb in await context.ContentBusinessType.ToListAsync()

                                        on evs.ContentBusinessTypeId equals ctb.ContentBusinessTypeId
                                        join lnt in await context.LearningTypeMaster.ToListAsync()

                                        on evs.LearningTypeId equals lnt.LearningTypeId
                                        join ctfm in await context.ContentFormatMaster.ToListAsync()

                                        on evs.ContentFormatId equals ctfm.ContentFormatId
                                        join lang in await context.Language.ToListAsync()

                                        on evs.LanguageId equals lang.LanguageId
                                        join evt in await context.EventCategory.ToListAsync()
                                    on evs.EventCategoryId equals evt.EventCategoryId into cata
                                        from resCata in cata.DefaultIfEmpty()

                                        join ctm in await context.ContentMaster.ToListAsync()

                                        on evs.ContentId equals ctm.ContentId
                                        join evst in await context.EventScheduleTrainer.ToListAsync()

                                        on evs.EventScheduleId equals evst.EventScheduleId
                                        join evem in await context.EventRoomMaster.ToListAsync()

                                        on evs.EventRoomId equals evem.EventRoomId
                                        where evs.StartTime >= startDateTime && evs.EndTime <= endDateTime && evs.IsActive == 1
                                        select new PlannerModel
                                        {
                                            Id = evs.EventScheduleId,
                                            BusinessType = new BusinessType
                                            {
                                                Id = ctb.ContentBusinessTypeId,
                                                Name = ctb.ContentBusinessTypeAbbreviate
                                            },
                                            LearningType = new LearningType
                                            {
                                                Id = lnt.LearningTypeId,
                                                Value = lnt.LearningTypeName
                                            },
                                            ContentFormat = new ContentFormat
                                            {
                                                Id = ctfm.ContentFormatId,
                                                Value = ctfm.ContentFormatName
                                            },
                                            Lang = lang.LanguageName,
                                            Language = new LanguagePlanner
                                            {
                                                Id = lang.LanguageId,
                                                Value = lang.LanguageName
                                            },
                                            Category = new Category
                                            {
                                                Id = resCata != null ? (int?)resCata.EventCategoryId : null,
                                                Value = resCata != null ? resCata.EventCategoryName : null,
                                            },
                                            ContentName = new ContentName
                                            {
                                                Id = ctm.ContentId,
                                                Value = ctm.ContentName,
                                                Duration = ctm.Duration
                                            },
                                            StartTime = evs.StartTime,
                                            EndTime = evs.EndTime,
                                            Room = new Room
                                            {
                                                Id = evem.EventRoomId,
                                                Name = evem.EventRoomName,
                                                IsActive = evem.IsActive,
                                            },
                                            IsActive = evs.IsActive,
                                            IsTrainerReply = evst.IsTrainerReply,
                                            IsSendEmail = evs.IsEmailSent,
                                            Trainer = new Trainer
                                            {
                                                Id = evst.TrainerId
                                            },
                                            IsSchedule = evs.IsActive,
                                            IsActiveTrainer = evst.IsActive

                                        }).ToList();


                    // Find trainer
                    foreach (var result in queryTrainer)
                    {

                        var trainerMaster = (from est in await context.EventScheduleTrainer.Where(o => o.IsActive == 1).ToListAsync()
                                             join tnm in await context.TrainerMaster.ToListAsync()
                                             on est.TrainerId equals tnm.TrainerId
                                             where est.EventScheduleId == result.Id
                                             select new Moderator()
                                             {
                                                 Id = tnm.TrainerId,
                                                 Name = tnm.Name,
                                                 lastname = tnm.LastName,
                                                 MaxEventPerMonth = tnm.MaximumEventPerMonth,
                                                 isTrainer = tnm.IsTrainer,
                                                 IsActive = tnm.IsActive
                                             }).ToList().FirstOrDefault();


                        if (trainerMaster != null)
                        {


                            result.Trainer = new Trainer
                            {
                                Id = trainerMaster.Id,
                                Name = trainerMaster.Name,
                                lastname = trainerMaster.lastname,
                                MaxEventPerMonth = trainerMaster.MaxEventPerMonth,
                                IsTrainer = trainerMaster.isTrainer,
                                IsActive = trainerMaster.IsActive,


                            };

                            var trainerAccept = await context.ScheduleAcceptedLog.Where(o => o.EventScheduleId == result.Id).OrderByDescending(o => o.ScheduleAcceptedId).FirstOrDefaultAsync();

                            if (trainerAccept != null)
                            {
                                result.IsTrainerAccept = trainerAccept.IsAccepted;
                            }
                            else
                            {
                                result.IsTrainerAccept = null;
                            }
                        }
                        else
                        {
                            result.Trainer = null;
                        }



                    }

                    var querymoderator = (from evs in await context.EventSchedule.ToListAsync()
                                          join ctb in await context.ContentBusinessType.ToListAsync()
                                         on evs.ContentBusinessTypeId equals ctb.ContentBusinessTypeId
                                          join lnt in await context.LearningTypeMaster.ToListAsync()
                                         on evs.LearningTypeId equals lnt.LearningTypeId
                                          join ctfm in await context.ContentFormatMaster.ToListAsync()
                                         on evs.ContentFormatId equals ctfm.ContentFormatId
                                          join lang in await context.Language.ToListAsync()
                                         on evs.LanguageId equals lang.LanguageId
                                          join evt in await context.EventCategory.ToListAsync()
                                     on evs.EventCategoryId equals evt.EventCategoryId into cata
                                          from resCata in cata.DefaultIfEmpty()
                                          join ctm in await context.ContentMaster.ToListAsync()
                                         on evs.ContentId equals ctm.ContentId
                                          join evsm in await context.EventScheduleModerator.ToListAsync()
                                         on evs.EventScheduleId equals evsm.EventScheduleId
                                          join evem in await context.EventRoomMaster.ToListAsync()
                                         on evs.EventRoomId equals evem.EventRoomId
                                          join sal in await context.ScheduleAcceptedLog.ToListAsync()
                                         on evs.EventScheduleId equals sal.EventScheduleId into ps
                                          from y1 in ps.DefaultIfEmpty()
                                          where evs.StartTime >= startDateTime && evs.EndTime <= endDateTime && evs.IsActive == 1
                                          select new PlannerModel
                                          {
                                              Id = evs.EventScheduleId,
                                              BusinessType = new BusinessType
                                              {
                                                  Id = ctb.ContentBusinessTypeId,
                                                  //Value = ctb.,
                                                  Name = ctb.ContentBusinessTypeName
                                              },
                                              LearningType = new LearningType
                                              {
                                                  Id = lnt.LearningTypeId,
                                                  Value = lnt.LearningTypeName
                                              },
                                              ContentFormat = new ContentFormat
                                              {
                                                  Id = ctfm.ContentFormatId,
                                                  Value = ctfm.ContentFormatName
                                              },
                                              Lang = lang.LanguageName,
                                              Language = new LanguagePlanner
                                              {
                                                  Id = lang.LanguageId,
                                                  Value = lang.LanguageName
                                              },
                                              Category = new Category
                                              {
                                                  Id = resCata != null ? (int?)resCata.EventCategoryId : null,
                                                  Value = resCata != null ? resCata.EventCategoryName : null,
                                              },
                                              ContentName = new ContentName
                                              {
                                                  Id = ctm.ContentId,
                                                  Value = ctm.ContentName
                                              },
                                              StartTime = evs.StartTime,
                                              EndTime = evs.EndTime,
                                              Room = new Room
                                              {
                                                  Id = evem.EventRoomId,
                                                  Name = evem.EventRoomName,
                                                  IsActive = evem.IsActive,
                                              },
                                              IsActive = evs.IsActive,
                                              IsSendEmail = evs.IsEmailSent,
                                              Moderator = new List<Moderator>(),
                                              IsSchedule = evs.IsActive,
                                              IsActiveTrainer = evsm.IsActive,

                                          }).ToList();



                    foreach (var result in querymoderator)
                    {

                        var modList = (from est in await context.EventScheduleModerator.Where(o => o.IsActive == 1).ToListAsync()
                                       join tnm in await context.TrainerMaster.ToListAsync()
                                       on est.ModeratorId equals tnm.TrainerId
                                       where est.EventScheduleId == result.Id
                                       select new Moderator()
                                       {
                                           Id = tnm.TrainerId,
                                           Name = tnm.Name + " (" + tnm.NickNameEn + ") ",
                                           lastname = tnm.LastName,
                                           MaxEventPerMonth = tnm.MaximumEventPerMonth,
                                           isTrainer = tnm.IsTrainer,
                                           IsActive = tnm.IsActive
                                       }).ToList();

                        foreach (var mod in modList)
                        {
                            result.Moderator.Add(mod);
                        }
                    }

                    var resultPlanner = (from trainer in queryTrainer
                                         join moderator in querymoderator
                                         on trainer.Id equals moderator.Id into ps
                                         from y1 in ps.DefaultIfEmpty()
                                         select new PlannerModel
                                         {
                                             Id = trainer.Id,
                                             BusinessType = new BusinessType
                                             {
                                                 Id = trainer.BusinessType.Id,
                                                 //Value = ctb.,
                                                 Name = trainer.BusinessType.Name
                                             },
                                             LearningType = new LearningType
                                             {
                                                 Id = trainer.LearningType.Id,
                                                 Value = trainer.LearningType.Value
                                             },
                                             ContentFormat = new ContentFormat
                                             {
                                                 Id = trainer.ContentFormat.Id,
                                                 Value = trainer.ContentFormat.Value
                                             },
                                             Lang = trainer.Lang,
                                             Category = new Category
                                             {
                                                 Id = trainer.Category.Id,
                                                 Value = trainer.Category.Value
                                             },
                                             ContentName = new ContentName
                                             {
                                                 Id = trainer.ContentName.Id,
                                                 Value = trainer.ContentName.Value
                                             },
                                             StartTime = trainer.StartTime,
                                             EndTime = trainer.EndTime,
                                             Room = new Room
                                             {
                                                 Id = trainer.Room.Id,
                                                 Name = trainer.Room.Name,
                                                 IsActive = trainer.Room.IsActive,
                                             },
                                             IsActive = trainer.IsActive,
                                             IsSendEmail = trainer.IsSendEmail,
                                             Trainer = trainer.Trainer,
                                             Moderator = y1 != null ? y1.Moderator : new List<Moderator>(),
                                             IsSchedule = trainer.IsSchedule,
                                             IsActiveTrainer = trainer.IsActiveTrainer,
                                             IsActiveModerator = y1 != null ? y1.IsActiveModerator == null ? 0 : y1.IsActiveModerator : 0,
                                             IsTrainerReply = trainer.IsTrainerReply,
                                             IsTrainerAccept = trainer != null ? trainer.IsTrainerAccept : null

                                         });


                    var eventSchedule = new List<int>();
                    List<PlannerModel> resultPlannerCondition = new List<PlannerModel>();


                    foreach (var res in resultPlanner)
                    {
                        // Check for groupBy
                        var chk = eventSchedule.Where(o => o == res.Id);

                        if (chk.Count() == 0)
                        {


                            if (res.Trainer == null)
                            {
                                res.IsTrainerReply = 0;
                            }
                            if (res.IsActive == 1)
                            {
                                resultPlannerCondition.Add(res);
                            }
                            // if (res.IsActive == 1 && (res.IsActiveTrainer == 1 || res.IsActiveModerator == 1))
                            // {
                            //     resultPlannerCondition.Add(res);
                            // }
                            // else if (res.IsActive == 1 && res.IsActiveTrainer == 1 && res.IsActiveModerator == 0)
                            // {
                            //     resultPlannerCondition.Add(res);
                            // }
                            // else if (res.IsActive == 1 && res.IsActiveTrainer == 0 && res.IsActiveModerator == 1)
                            // {
                            //     resultPlannerCondition.Add(res);
                            // }
                            // else if (res.IsActive == 1 && res.IsActiveTrainer == 0 && res.IsActiveModerator == 0)
                            // {
                            //     resultPlannerCondition.Add(res);
                            // }
                        }

                        eventSchedule.Add(res.Id);
                    }


                    return resultPlannerCondition;


                }

            }
            catch (Exception ex)
            {
                this.Db.connection.Close();
                throw ex;
            }
        }
        public async Task<PlannerModel> FindByIdPlannerAsync(int scheduleId)
        {
            try
            {

                using (var context = new seac_webapplicationContext())
                {
                    #region Query trainer
                    var queryTrainer = (from evs in await context.EventSchedule.ToListAsync()
                                        join ctb in await context.ContentBusinessType.ToListAsync()
                                       on evs.ContentBusinessTypeId equals ctb.ContentBusinessTypeId

                                        join lnt in await context.LearningTypeMaster.ToListAsync()
                                       on evs.LearningTypeId equals lnt.LearningTypeId

                                        join ctfm in await context.ContentFormatMaster.ToListAsync()
                                       on evs.ContentFormatId equals ctfm.ContentFormatId

                                        join lang in await context.Language.ToListAsync()
                                       on evs.LanguageId equals lang.LanguageId

                                        join evt in await context.EventCategory.ToListAsync()
                                       on evs.EventCategoryId equals evt.EventCategoryId into cata
                                        from resCata in cata.DefaultIfEmpty()

                                        join ctm in await context.ContentMaster.ToListAsync()
                                       on evs.ContentId equals ctm.ContentId

                                        join evst in await context.EventScheduleTrainer.ToListAsync()
                                       on evs.EventScheduleId equals evst.EventScheduleId

                                        join tm in await context.TrainerMaster.ToListAsync()
                                       on evst.TrainerId equals tm.TrainerId

                                        join tct in await context.TrainerContractType.ToListAsync()
                                       on tm.TraninerContractTypeId equals tct.TraninerContractTypeId

                                        join evem in await context.EventRoomMaster.ToListAsync()
                                       on evs.EventRoomId equals evem.EventRoomId

                                        join sal in await context.ScheduleAcceptedLog.ToListAsync()
                                       on evs.EventScheduleId equals sal.EventScheduleId into ps
                                        from y1 in ps.DefaultIfEmpty()

                                        join sta in await context.Status.ToListAsync()
                                         on evs.StatusId equals sta.StatusId into ps2
                                        from y2 in ps2.DefaultIfEmpty()

                                        where evs.IsActive == 1 && evs.EventScheduleId == scheduleId
                                        select new PlannerModel
                                        {
                                            Id = evs.EventScheduleId,
                                            BusinessType = new BusinessType
                                            {
                                                Id = ctb.ContentBusinessTypeId,
                                                Value = ctb.ContentBusinessTypeName,
                                                Name = ctb.ContentBusinessTypeAbbreviate
                                            },
                                            LearningType = new LearningType
                                            {
                                                Id = lnt.LearningTypeId,
                                                Value = lnt.LearningTypeName
                                            },
                                            ContentFormat = new ContentFormat
                                            {
                                                Id = ctfm.ContentFormatId,
                                                Value = ctfm.ContentFormatName
                                            },
                                            Lang = lang.LanguageName,
                                            Language = new LanguagePlanner
                                            {
                                                Id = lang.LanguageId,
                                                Value = lang.LanguageName
                                            },
                                            Category = new Category
                                            {
                                                Id = resCata != null ? (int?)resCata.EventCategoryId : null,
                                                Value = resCata != null ? resCata.EventCategoryName : null,
                                            },
                                            ContentName = new ContentName
                                            {
                                                Id = ctm.ContentId,
                                                Value = ctm.ContentName,
                                                Duration = ctm.Duration
                                            },
                                            StartTime = evs.StartTime,
                                            EndTime = evs.EndTime,

                                            Room = new Room
                                            {
                                                Id = evem.EventRoomId,
                                                Name = evem.EventRoomName,
                                                IsActive = evem.IsActive,
                                            },
                                            IsActive = evs.IsActive,
                                            IsTrainerReply = evst.IsTrainerReply,
                                            IsSendEmail = evs.IsEmailSent,
                                            Trainer = new Trainer
                                            {
                                                Id = evst.TrainerId
                                            },
                                            IsSchedule = evs.IsActive,
                                            IsActiveTrainer = evst.IsActive,
                                            IsTrainerAccept = y1 != null ? y1.IsAccepted : null,
                                            IsBillable = evs.IsBillable,
                                            ProjectName = evs.ProjectName,
                                            ProjectId = evs.ProjectId,
                                            Remark = evs.Remark,
                                            Link = evs.Link,
                                            Company = evs.Company,
                                            BookBy = evs.BookBy,
                                            Location = evs.Location,
                                            Session = evs.Session,
                                            NoOfParticipant = evs.NoOfParticipant,
                                            status = new StatusPlanner
                                            {
                                                Id = y2 != null ? (int?)y2.StatusId : null,
                                                Value = y2 != null ? y2.Status1 : null,
                                            },
                                            uploadDate = evs.UploadToInstancyDate,
                                            contractType = new ContactTypePlanner
                                            {
                                                Id = tct.TraninerContractTypeId,
                                                Value = tct.TraninerContractTypeName
                                            },
                                            IsModerator = evs.IsModerator

                                        }).ToList();

                    #endregion



                    // Find trainer
                    foreach (var result in queryTrainer)
                    {

                        var trainerMaster = (from est in await context.EventScheduleTrainer.Where(o => o.IsActive == 1).ToListAsync()
                                             join tnm in await context.TrainerMaster.ToListAsync()
                                             on est.TrainerId equals tnm.TrainerId
                                             where est.EventScheduleId == result.Id
                                             select new Moderator()
                                             {
                                                 Id = tnm.TrainerId,
                                                 Name = tnm.Name,
                                                 lastname = tnm.LastName,
                                                 MaxEventPerMonth = tnm.MaximumEventPerMonth,
                                                 isTrainer = tnm.IsTrainer,

                                                 IsActive = tnm.IsActive
                                             }).ToList().FirstOrDefault();


                        if (trainerMaster != null)
                        {

                            result.Trainer = new Trainer
                            {
                                Id = trainerMaster.Id,
                                Name = trainerMaster.Name,
                                lastname = trainerMaster.lastname,
                                MaxEventPerMonth = trainerMaster.MaxEventPerMonth,
                                IsTrainer = trainerMaster.isTrainer,
                                IsActive = trainerMaster.IsActive
                            };
                        }
                        else
                        {
                            result.Trainer = null;
                        }

                        // Add count event schedule
                        var eventCount = await context.EventScheduleTrainer.Where(o => o.EventScheduleId == result.Id).ToListAsync();

                        result.EventCount = eventCount.Count();

                    }

                    var querymoderator = (from evs in await context.EventSchedule.ToListAsync()
                                          join ctb in await context.ContentBusinessType.ToListAsync()
                                         on evs.ContentBusinessTypeId equals ctb.ContentBusinessTypeId

                                          join lnt in await context.LearningTypeMaster.ToListAsync()
                                         on evs.LearningTypeId equals lnt.LearningTypeId

                                          join ctfm in await context.ContentFormatMaster.ToListAsync()
                                         on evs.ContentFormatId equals ctfm.ContentFormatId

                                          join lang in await context.Language.ToListAsync()
                                         on evs.LanguageId equals lang.LanguageId

                                          join evt in await context.EventCategory.ToListAsync()
                                      on evs.EventCategoryId equals evt.EventCategoryId into cata
                                          from resCata in cata.DefaultIfEmpty()

                                          join ctm in await context.ContentMaster.ToListAsync()
                                         on evs.ContentId equals ctm.ContentId

                                          join evsm in await context.EventScheduleModerator.ToListAsync()
                                         on evs.EventScheduleId equals evsm.EventScheduleId

                                          join tm in await context.TrainerMaster.ToListAsync()
                                         on evsm.ModeratorId equals tm.TrainerId

                                          join tct in await context.TrainerContractType.ToListAsync()
                                         on tm.TraninerContractTypeId equals tct.TraninerContractTypeId

                                          join evem in await context.EventRoomMaster.ToListAsync()
                                         on evs.EventRoomId equals evem.EventRoomId

                                          join sal in await context.ScheduleAcceptedLog.ToListAsync()
                                         on evs.EventScheduleId equals sal.EventScheduleId into ps
                                          from y1 in ps.DefaultIfEmpty()

                                          join status in await context.Status.ToListAsync()
                                         on evs.StatusId equals status.StatusId into ps2
                                          from y2 in ps2.DefaultIfEmpty()

                                          where evs.IsActive == 1 && evs.EventScheduleId == scheduleId
                                          select new PlannerModel
                                          {
                                              Id = evs.EventScheduleId,
                                              BusinessType = new BusinessType
                                              {
                                                  Id = ctb.ContentBusinessTypeId,
                                                  Value = ctb.ContentBusinessTypeName,
                                                  Name = ctb.ContentBusinessTypeName
                                              },
                                              LearningType = new LearningType
                                              {
                                                  Id = lnt.LearningTypeId,
                                                  Value = lnt.LearningTypeName
                                              },
                                              ContentFormat = new ContentFormat
                                              {
                                                  Id = ctfm.ContentFormatId,
                                                  Value = ctfm.ContentFormatName
                                              },
                                              Lang = lang.LanguageName,
                                              Language = new LanguagePlanner
                                              {
                                                  Id = lang.LanguageId,
                                                  Value = lang.LanguageName
                                              },
                                              Category = new Category
                                              {
                                                  Id = resCata != null ? (int?)resCata.EventCategoryId : null,
                                                  Value = resCata != null ? resCata.EventCategoryName : null,
                                              },
                                              ContentName = new ContentName
                                              {
                                                  Id = ctm.ContentId,
                                                  Value = ctm.ContentName,
                                                  Duration = ctm.Duration
                                              },
                                              StartTime = evs.StartTime,
                                              EndTime = evs.EndTime,
                                              Room = new Room
                                              {
                                                  Id = evem.EventRoomId,
                                                  Name = evem.EventRoomName,
                                                  IsActive = evem.IsActive,
                                              },
                                              IsActive = evs.IsActive,
                                              IsSendEmail = evs.IsEmailSent,
                                              Moderator = new List<Moderator>(),
                                              IsSchedule = evs.IsActive,
                                              IsActiveTrainer = evsm.IsActive,
                                              IsTrainerAccept = y1 != null ? y1.IsAccepted : null,
                                              IsBillable = evs.IsBillable,
                                              ProjectName = evs.ProjectName,
                                              ProjectId = evs.ProjectId,
                                              Remark = evs.Remark,
                                              Link = evs.Link,
                                              Company = evs.Company,
                                              BookBy = evs.BookBy,
                                              Location = evs.Location,
                                              Session = evs.Session,
                                              NoOfParticipant = evs.NoOfParticipant,
                                              status = new StatusPlanner
                                              {
                                                  Id = y2 != null ? (int?)y2.StatusId : null,
                                                  Value = y2 != null ? y2.Status1 : null,
                                              },
                                              uploadDate = evs.UploadToInstancyDate,
                                              contractType = new ContactTypePlanner
                                              {
                                                  Id = tct.TraninerContractTypeId,
                                                  Value = tct.TraninerContractTypeName
                                              },
                                              //IsModerator = evs.IsModerator

                                          }).ToList();



                    foreach (var result in querymoderator)
                    {
                        // Add count event schedule
                        var eventCount = await context.EventScheduleModerator.Where(o => o.EventScheduleId == result.Id).ToListAsync();

                        //result.EventCount = eventCount.Count();

                        var modList = (from est in await context.EventScheduleModerator.Where(o => o.IsActive == 1).ToListAsync()
                                       join tnm in await context.TrainerMaster.ToListAsync()
                                       on est.ModeratorId equals tnm.TrainerId
                                       where est.EventScheduleId == result.Id
                                       select new Moderator()
                                       {
                                           Id = tnm.TrainerId,
                                           Name = tnm.Name,
                                           Nickname = tnm.NickNameEn,
                                           lastname = tnm.LastName,
                                           EventCount = eventCount.Count(),
                                           MaxEventPerMonth = tnm.MaximumEventPerMonth,
                                           isTrainer = tnm.IsTrainer,
                                           IsActive = tnm.IsActive
                                       }).ToList();

                        foreach (var mod in modList)
                        {
                            result.Moderator.Add(mod);
                        }

                        

                    }

                    var resultPlanner = (from trainer in queryTrainer
                                         join moderator in querymoderator
                                         on trainer.Id equals moderator.Id into ps
                                         from y1 in ps.DefaultIfEmpty()
                                         select new PlannerModel
                                         {
                                             Id = trainer.Id,
                                             BusinessType = new BusinessType
                                             {
                                                 Id = trainer.BusinessType.Id,
                                                 Value = trainer.BusinessType.Value,
                                                 Name = trainer.BusinessType.Name
                                             },
                                             LearningType = new LearningType
                                             {
                                                 Id = trainer.LearningType.Id,
                                                 Value = trainer.LearningType.Value
                                             },
                                             ContentFormat = new ContentFormat
                                             {
                                                 Id = trainer.ContentFormat.Id,
                                                 Value = trainer.ContentFormat.Value
                                             },
                                             Lang = trainer.Lang,
                                             Language = new LanguagePlanner
                                             {
                                                 Id = trainer.Language.Id,
                                                 Value = trainer.Language.Value
,
                                             },
                                             Category = new Category
                                             {
                                                 Id = trainer.Category.Id,
                                                 Value = trainer.Category.Value
                                             },
                                             ContentName = new ContentName
                                             {
                                                 Id = trainer.ContentName.Id,
                                                 Value = trainer.ContentName.Value,
                                                 Duration = trainer.ContentName.Duration
                                             },
                                             StartTime = trainer.StartTime,
                                             EndTime = trainer.EndTime,
                                             Room = new Room
                                             {
                                                 Id = trainer.Room.Id,
                                                 Name = trainer.Room.Name,
                                                 IsActive = trainer.Room.IsActive,
                                             },
                                             IsActive = trainer.IsActive,
                                             IsSendEmail = trainer.IsSendEmail,
                                             Trainer = trainer.Trainer,
                                             Moderator = y1 != null ? y1.Moderator : new List<Moderator>(),
                                             IsSchedule = trainer.IsSchedule,
                                             IsActiveTrainer = trainer.IsActiveTrainer,
                                             IsActiveModerator = y1 != null ? y1.IsActiveModerator == null ? 0 : y1.IsActiveModerator : 0,
                                             IsTrainerReply = trainer.IsTrainerReply,
                                             IsTrainerAccept = trainer != null ? trainer.IsTrainerAccept : null,
                                             IsBillable = trainer.IsBillable,
                                             ProjectName = trainer.ProjectName,
                                             ProjectId = trainer.ProjectId,
                                             Remark = trainer.Remark,
                                             Link = trainer.Link,
                                             Company = trainer.Company,
                                             BookBy = trainer.BookBy,
                                             Location = trainer.Location,
                                             Session = trainer.Session,
                                             NoOfParticipant = trainer.NoOfParticipant,
                                             status = new StatusPlanner
                                             {
                                                 Id = trainer.status != null ? (int?)trainer.status.Id : null,
                                                 Value = trainer.status != null ? trainer.status.Value : null,
                                             },
                                             uploadDate = trainer.uploadDate,
                                             contractType = trainer.contractType,
                                             EventCount = trainer.EventCount,
                                             IsModerator = trainer.IsModerator
                                         });


                    var eventSchedule = new List<int>();
                    List<PlannerModel> resultPlannerCondition = new List<PlannerModel>();




                    foreach (var res in resultPlanner.OrderBy(o => o.Id))
                    {
                        // Check for groupBy
                        var chk = eventSchedule.Where(o => o == res.Id);

                        if (chk.Count() == 0)
                        {


                            if (res.Trainer == null)
                            {
                                res.IsTrainerReply = 0;
                            }
                            if (res.IsActive == 1 && res.IsActiveTrainer == 1 && res.IsActiveModerator == 1)
                            {
                                resultPlannerCondition.Add(res);
                            }
                            else if (res.IsActive == 1 && res.IsActiveTrainer == 1 && res.IsActiveModerator == 0)
                            {
                                resultPlannerCondition.Add(res);
                            }
                            else if (res.IsActive == 1 && res.IsActiveTrainer == 0 && res.IsActiveModerator == 1)
                            {
                                resultPlannerCondition.Add(res);
                            }
                            else if (res.IsActive == 1 && res.IsActiveTrainer == 0 && res.IsActiveModerator == 0)
                            {
                                resultPlannerCondition.Add(res);
                            }
                        }

                        eventSchedule.Add(res.Id);
                    }


                    return resultPlannerCondition.FirstOrDefault();


                }

            }
            catch (Exception ex)
            {
                this.Db.connection.Close();
                throw ex;
            }
        }
        public List<PlannerModel> FindPlannerByDate(DateTime startDateTime, DateTime endDateTime)
        {
            try
            {
                using (var context = new seac_webapplicationContext())
                {

                    var events = context.EventSchedule
                                    .Where(x => x.StartTime >= startDateTime && x.EndTime <= endDateTime && x.IsActive == 1)
                                    .Select(x => x.EventScheduleId);

                    return GetEventsForPlanner(events.ToArray());
                }
            }
            catch (Exception ex)
            {
                this.Db.connection.Close();
                throw ex;
            }
        }
        public PlannerModel FindPlannerById(int scheduleId)
        {
            return GetEventsForPlanner(new int[] { scheduleId }).First();
        }
        public List<PlannerModel> GetEventsForPlanner(int[] ids)
        {
            try
            {
                using (var context = new seac_webapplicationContext())
                {
                    var events = (from evs in context.EventSchedule.Where(o => ids.Contains(o.EventScheduleId))
                                  join ctb in context.ContentBusinessType
                                  on evs.ContentBusinessTypeId equals ctb.ContentBusinessTypeId
                                  join lnt in context.LearningTypeMaster
                                  on evs.LearningTypeId equals lnt.LearningTypeId
                                  join ctfm in context.ContentFormatMaster
                                  on evs.ContentFormatId equals ctfm.ContentFormatId
                                  join evt in context.EventCategory
                                  on evs.EventCategoryId equals evt.EventCategoryId
                                  join ctm in context.ContentMaster
                                  on evs.ContentId equals ctm.ContentId
                                  join lang in context.Language
                                  on evs.LanguageId equals lang.LanguageId
                                  join evem in context.EventRoomMaster
                                  on evs.EventRoomId equals evem.EventRoomId
                                  join sta in context.Status
                                  on evs.StatusId equals sta.StatusId into temp_lj1
                                  from lj_status in temp_lj1.DefaultIfEmpty()
                                  where evs.IsActive == 1
                                  select new PlannerModel
                                  {
                                      Id = evs.EventScheduleId,
                                      BusinessType = new BusinessType
                                      {
                                          Id = ctb.ContentBusinessTypeId,
                                          Value = ctb.ContentBusinessTypeName,
                                          Name = ctb.ContentBusinessTypeAbbreviate
                                      },
                                      LearningType = new LearningType
                                      {
                                          Id = lnt.LearningTypeId,
                                          Value = lnt.LearningTypeName
                                      },
                                      ContentFormat = new ContentFormat
                                      {
                                          Id = ctfm.ContentFormatId,
                                          Value = ctfm.ContentFormatName
                                      },
                                      Category = new Category
                                      {
                                          Id = evt != null ? (int?)evt.EventCategoryId : null,
                                          Value = evt != null ? evt.EventCategoryName : null,
                                      },
                                      ContentName = new ContentName
                                      {
                                          Id = ctm.ContentId,
                                          Value = ctm.ContentName,
                                          Duration = ctm.Duration
                                      },
                                      Lang = lang.LanguageName,
                                      Language = new LanguagePlanner
                                      {
                                          Id = lang.LanguageId,
                                          Value = lang.LanguageName
                                      },
                                      StartTime = evs.StartTime,
                                      EndTime = evs.EndTime,
                                      Room = new Room
                                      {
                                          Id = evem.EventRoomId,
                                          Name = evem.EventRoomName,
                                          IsActive = evem.IsActive,
                                      },

                                      IsBillable = evs.IsBillable,
                                      ProjectName = evs.ProjectName,
                                      ProjectId = evs.ProjectId,
                                      Remark = evs.Remark,
                                      Link = evs.Link,
                                      Company = evs.Company,
                                      BookBy = evs.BookBy,
                                      Location = evs.Location,
                                      Session = evs.Session,
                                      NoOfParticipant = evs.NoOfParticipant,
                                      uploadDate = evs.UploadToInstancyDate,
                                      status = new StatusPlanner
                                      {
                                          Id = lj_status != null ? (int?)lj_status.StatusId : null,
                                          Value = lj_status != null ? lj_status.Status1 : null,
                                      },
                                      contractType = null,

                                      IsActive = evs.IsActive,
                                      IsSchedule = evs.IsActive,
                                      IsSendEmail = evs.IsEmailSent,

                                      Trainer = null,
                                      Moderator = new List<Moderator>(),
                                      // IsActiveTrainer = null,
                                      // IsActiveModerator = null,
                                      IsTrainerReply = null,
                                      IsTrainerAccept = null,
                                      IsModerator = evs.IsModerator
                                  }).ToList();

                    foreach (var e in events)
                    {
                        var latestTrainer = (from est in context.EventScheduleTrainer.Where(o => o.EventScheduleId == e.Id)
                                                                                        .Where(o => o.IsActive == 1)
                                                                                        .OrderByDescending(o => o.Id)
                                             join tnm in context.TrainerMaster
                                             on est.TrainerId equals tnm.TrainerId
                                             join tct in context.TrainerContractType
                                             on tnm.TraninerContractTypeId equals tct.TraninerContractTypeId
                                             select new
                                             {
                                                 IsTrainerReply = est.IsTrainerReply,
                                                 Id = tnm.TrainerId,
                                                 Name = tnm.Name,
                                                 lastname = tnm.LastName,
                                                 MaxEventPerMonth = tnm.MaximumEventPerMonth,
                                                 IsTrainer = tnm.IsTrainer,
                                                 IsActive = tnm.IsActive,
                                                 TraninerContractTypeId = tnm.TraninerContractTypeId,
                                                 TraninerContractTypeName = tct.TraninerContractTypeName
                                             }).FirstOrDefault();
                        if (latestTrainer != null)
                        {
                            e.IsTrainerReply = latestTrainer.IsTrainerReply;
                            e.Trainer = new Trainer
                            {
                                Id = latestTrainer.Id,
                                Name = latestTrainer.Name,
                                lastname = latestTrainer.lastname,
                                MaxEventPerMonth = latestTrainer.MaxEventPerMonth,
                                IsTrainer = latestTrainer.IsTrainer,
                                IsActive = latestTrainer.IsActive,
                            };
                            e.contractType = new ContactTypePlanner
                            {
                                Id = latestTrainer.TraninerContractTypeId,
                                Value = latestTrainer.TraninerContractTypeName
                            };
                            var lastTrainerReply = context.ScheduleAcceptedLog.Where(o => o.EventScheduleId == e.Id)
                                                                                .Where(o => o.TrainerId == e.Trainer.Id)
                                                                                .OrderByDescending(o => o.ScheduleAcceptedId)
                                                                                .FirstOrDefault();
                            if (lastTrainerReply != null)
                            {
                                e.IsTrainerAccept = lastTrainerReply.IsAccepted;
                            }
                        }
                        var activeMods = (from est in context.EventScheduleModerator.Where(o => o.EventScheduleId == e.Id)
                                                                                        .Where(o => o.IsActive == 1)
                                          join tnm in context.TrainerMaster
                                          on est.ModeratorId equals tnm.TrainerId
                                          select new Moderator()
                                          {
                                              Id = tnm.TrainerId,
                                              Name = tnm.Name,
                                              Nickname = tnm.NickNameEn,
                                              lastname = tnm.LastName,
                                              MaxEventPerMonth = tnm.MaximumEventPerMonth,
                                              isTrainer = tnm.IsTrainer,
                                              IsActive = tnm.IsActive
                                          }).ToList();
                        foreach (var mod in activeMods)
                        {
                            e.Moderator.Add(mod);
                        }
                    }
                    return events;
                }
            }
            catch (Exception ex)
            {
                this.Db.connection.Close();
                throw ex;
            }
        }
        public async Task<List<CategoryModel>> FindCategoryAsync()
        {
            try
            {

                using (var context = new seac_webapplicationContext())
                {

                    var query = (from ec in await context.EventCategory.ToListAsync()
                                 select new CategoryModel()
                                 {
                                     Id = ec.EventCategoryId,
                                     Value = ec.EventCategoryName
                                 }).ToList();

                    return query;
                }

            }
            catch (Exception ex)
            {
                this.Db.connection.Close();
                throw ex;
            }
        }
        public async Task<List<LanguageModel>> FindLanguageAsync()
        {
            try
            {

                using (var context = new seac_webapplicationContext())
                {

                    var query = (from lang in await context.Language.ToListAsync()
                                 select new LanguageModel()
                                 {
                                     Id = lang.LanguageId,
                                     Value = lang.LanguageName
                                 }).ToList();

                    return query;
                }

            }
            catch (Exception ex)
            {
                this.Db.connection.Close();
                throw ex;
            }
        }
        public async Task<int> InsertAsync(PlannerModel_Request entity)
        {
            try
            {
                using (var context = new seac_webapplicationContext())
                {


                    foreach (var eventSchedule in await context.EventSchedule.Where(o => o.IsActive == 1).Where(o => o.StartTime <= entity.EndDate && entity.StartDate <= o.EndTime).ToListAsync())
                    {
                        //Validate type 1 = ynu, 2 = ynuy
                        if (entity.BusinessTypeId == 1 || entity.BusinessTypeId == 2)
                        {

                            if (eventSchedule.EventRoomId == entity.RoomId)
                            {
                                return -1;
                            }
                        }


                        var resultScheduleTrainer = await context.EventScheduleTrainer.Where(o => o.EventScheduleId == eventSchedule.EventScheduleId && o.IsActive == 1).FirstOrDefaultAsync();

                        if (resultScheduleTrainer != null)
                        {
                            if (resultScheduleTrainer.TrainerId == entity.TrainerId)
                            {
                                return -1;
                            }
                        }

                    }

                    #region 
                    //// Check trainer and room already exist.
                    //var getSchedule = (from evs in await context.EventSchedule.Where(o => o.IsActive == 1).ToListAsync()
                    //                   join evst in await context.EventScheduleTrainer.Where(o => o.IsActive == 1).ToListAsync() on evs.EventScheduleId equals evst.EventScheduleId
                    //                   where evs.EventRoomId == entity.RoomId
                    //                     || evst.TrainerId == entity.TrainerId
                    //                     && evs.StartTime <= entity.EndDate
                    //                     && entity.StartDate <= evs.EndTime
                    //                   select new
                    //                   {
                    //                       evs = evs.EventScheduleId
                    //                   }).ToList();

                    //if (getSchedule.Count() > 0)
                    //{
                    //    return -1;
                    //}
                    #endregion


                    EventSchedule schedule = new EventSchedule
                    {
                        ContentId = entity.ContentId,
                        EventRoomId = entity.RoomId,
                        EventCategoryId = entity.CategoryId,
                        LearningTypeId = entity.LearningTypeId,
                        ContentFormatId = entity.ContentFormatId,
                        StartTime = entity.StartDate,
                        EndTime = entity.EndDate,
                        Duration = (int)entity.EndDate.Subtract(entity.StartDate).TotalMinutes,
                        Link = entity.Link,
                        DisplayName = "",//Fix
                        ProjectId = entity.ProjectId,
                        ProjectName = entity.ProjectName,
                        Company = entity.Company,
                        NoOfParticipant = entity.NoOfParticipant,
                        Session = entity.Session,
                        IsBillable = entity.IsBillable,
                        Remark = entity.Remarks,
                        IsEmailSent = 0,//Fix
                        LanguageId = entity.LangId,
                        UploadToInstancyDate = entity.UploadDate,
                        CreatedDate = DateTime.Now,
                        UpdatedDate = DateTime.Now,
                        CreatedBy = entity.CreatedBy,
                        IsActive = 1,
                        ContentBusinessTypeId = entity.BusinessTypeId,
                        Location = entity.Location,
                        BookBy = entity.BookBy,
                        StatusId = entity.StatusId,
                        IsModerator = entity.IsModerator
                    };

                    // Insert event schedule
                    await context.AddAsync(schedule);

                    await context.SaveChangesAsync();

                    EventScheduleTrainer scheduleTrainer = new EventScheduleTrainer
                    {
                        EventScheduleId = schedule.EventScheduleId,
                        TrainerId = entity.TrainerId,
                        IsActive = 1,
                        IsTrainerReply = 0,
                        CreatedDate = DateTime.Now,
                        CreatedBy = entity.CreatedBy,
                    };

                    // Insert schedule trainer
                    await context.AddAsync(scheduleTrainer);

                    await context.SaveChangesAsync();

                    EventScheduleLogs scheduleLog = new EventScheduleLogs
                    {
                        EventScheduleId = schedule.EventScheduleId,
                        UserId = entity.CreatedBy,
                        Action = typeAction.Create.ToString(),
                        ContentId = entity.ContentId,
                        LoggedDateTime = DateTime.Now
                    };

                    // Insert schedule log
                    await context.AddAsync(scheduleLog);

                    await context.SaveChangesAsync();

                    return schedule.EventScheduleId;
                }


            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public async Task<List<EventSchedule>> FindAllAsync()
        {
            try
            {


                using (var context = new seac_webapplicationContext())
                {
                    var result = await context.EventSchedule.Where(o => o.IsActive == 1).ToListAsync();

                    return result;
                }

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public async Task<List<ContentPlanner>> FindAllContentMasterAsync()
        {
            try
            {


                using (var context = new seac_webapplicationContext())
                {
                    var result = await context.ContentMaster.Where(o => o.IsActive == 1).OrderBy(o => o.DisplayName).ToListAsync();

                    if (result == null)
                    {
                        return null;
                    }

                    List<ContentPlanner> content = new List<ContentPlanner>();
                    foreach (var res in result)
                    {
                        content.Add(new ContentPlanner { Id = res.ContentId, Value = res.DisplayName });
                    }

                    return content;
                }

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public async Task<List<ContactTypePlanner>> FindAllContactTypeAsyc()
        {
            try
            {


                using (var context = new seac_webapplicationContext())
                {
                    var result = await context.TrainerContractType.ToListAsync();

                    if (result == null)
                    {
                        return null;
                    }

                    List<ContactTypePlanner> contactTypes = new List<ContactTypePlanner>();
                    foreach (var res in result)
                    {
                        contactTypes.Add(new ContactTypePlanner { Id = res.TraninerContractTypeId, Value = res.TraninerContractTypeName });
                    }

                    return contactTypes;
                }

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public async Task<List<TrainerContacTypeCombo>> FindAllModeratorMasterAsync(DateTime createdDate, string startTime, string endTime, int bussinessTypeId, int roomId)
        {
            try
            {

                return await getModerator(createdDate, startTime, endTime, bussinessTypeId, roomId);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public async Task<List<TrainerPlanner>> FindAllModeratorOld()
        {
            try
            {

                #region this old 
                using (var context = new seac_webapplicationContext())
                {
                    var result = await context.TrainerMaster.Where(o => o.IsTrainer == 0 && o.IsActive == 1).ToListAsync();

                    if (result == null)
                    {
                        return null;
                    }

                    List<TrainerPlanner> trainer = new List<TrainerPlanner>();

                    foreach (var res in result)
                    {
                        trainer.Add(new TrainerPlanner
                        {
                            Id = res.TrainerId,
                            Value = res.Name + " " + res.LastName
                        });
                    }





                    return trainer;
                }
                #endregion
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public class TrainerCount
        {
            public int Id { get; set; }
            public int Count { get; set; }
        }
        public class EventSchduleTrainerCount
        {
            public int id { get; set; }
            public int value { get; set; }
        }
        public async Task<List<TrainerContacTypeCombo>> FindAllTrainerContractTypeAsync(DateTime createDate, string startTime, string endTime, int contentId, string mode)
        {
            try
            {
                return await getTrainerMaster(createDate, startTime, endTime, contentId, mode);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public async Task<bool> UpdateAsync(PlannerModel_Request entity, int scheduleId, string baseUrl)
        {
            try
            {
                using (var context = new seac_webapplicationContext())
                {

                    var schedule = await context.EventSchedule.Where(o => o.EventScheduleId == scheduleId && o.IsActive == 0).FirstOrDefaultAsync();

                    if (schedule != null)
                    {
                        return false;
                    }

                    var resultSchedule = await context.EventSchedule.Where(o => o.EventScheduleId == scheduleId && o.IsActive == 1).FirstOrDefaultAsync();

                    // Update table event schedule log
                    await UpdateScheduleLogs(entity, scheduleId);

                    // Update event trainer
                    await UpdateTrainer(entity, scheduleId, baseUrl);

                    // Update event schedule
                    await RemoveSchedule(entity, scheduleId, resultSchedule);

                    // Update content 
                    await UpdateModelator(entity, scheduleId);



                }

                return true;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public async Task<bool> UpdateScheduleLogs(PlannerModel_Request entity, int scheduleId)
        {
            try
            {
                using (var context = new seac_webapplicationContext())
                {

                    DateTime loggedDatatTime = DateTime.Now;


                    var resultEventSchedule = await context.EventScheduleLogs.Where(o => o.EventScheduleId == scheduleId).ToListAsync();

                    var resultSchedule = await context.EventSchedule.Where(o => o.EventScheduleId == scheduleId).FirstOrDefaultAsync();

                    #region Parameter startTime send email
                    var resStartTime = resultEventSchedule.Where(o => o.Field == fieldEnum.StartTime.ToString()).FirstOrDefault();


                    if (entity.StartDate != resultSchedule.StartTime || entity.EndDate != resultSchedule.EndTime)
                    {
                        // Insert table EventScheduleLog
                        EventScheduleLogs eventLog = new EventScheduleLogs
                        {
                            EventScheduleId = scheduleId,
                            UserId = entity.CreatedBy,
                            Action = typeAction.Update.ToString(),
                            ContentId = entity.ContentId,
                            Field = fieldEnum.StartTime.ToString(),
                            FromValue = resultSchedule.StartTime.ToString("dd MMM yyyy hh:mm"),
                            ToValue = entity.StartDate.ToString("dd MMM yyyy hh:mm"),
                            LoggedDateTime = loggedDatatTime
                        };

                        await context.AddAsync(eventLog);

                        await context.SaveChangesAsync();

                        // Update table EventSchedule
                        if (resultSchedule.IsEmailSent != 0)
                        {
                            resultSchedule.IsEmailSent = 0;

                            await context.SaveChangesAsync();
                        }
                    }

                    #endregion

                    #region Parameter trainer


                    var resTrainer = resultEventSchedule.Where(o => o.Field == fieldEnum.Trainer.ToString()).OrderByDescending(o => o.LoggedDateTime).FirstOrDefault();

                    string fromName = null;

                    if (resTrainer != null)
                    {
                        fromName = resTrainer.ToValue;
                    }
                    else
                    {
                        var resultTrainer = (from evs in await context.EventSchedule.ToListAsync()
                                             join evst in await context.EventScheduleTrainer.ToListAsync()
                                             on evs.EventScheduleId equals evst.EventScheduleId
                                             join tm in await context.TrainerMaster.ToListAsync()
                                             on evst.TrainerId equals tm.TrainerId
                                             where evs.EventScheduleId == resultSchedule.EventScheduleId && evs.IsActive == 1
                                             orderby evst.CreatedDate descending
                                             select new TrainerMaster
                                             {
                                                 TrainerId = tm.TrainerId,
                                                 Name = tm.Name,
                                                 LastName = tm.LastName
                                             }).FirstOrDefault();

                        fromName = resultTrainer.Name + " " + resultTrainer.LastName;
                    }


                    // string fromName = resTrainer != null ? resTrainer.ToValue : resultTrainer.Name + " " + resultTrainer.LastName;
                    string toName = null;
                    var checkInsertTrainer = false;

                    var getTrainerbyTrainer = await context.TrainerMaster.Where(o => o.TrainerId == entity.TrainerId).FirstOrDefaultAsync();

                    if (getTrainerbyTrainer != null)
                    {
                        string name = getTrainerbyTrainer.Name + " " + getTrainerbyTrainer.LastName;

                        if (fromName != name)
                        {
                            toName = name;
                            checkInsertTrainer = true;
                        }
                    }



                    if (checkInsertTrainer)
                    {

                        EventScheduleLogs eventLog = new EventScheduleLogs
                        {
                            EventScheduleId = scheduleId,
                            UserId = entity.CreatedBy,
                            Action = typeAction.Update.ToString(),
                            ContentId = entity.ContentId,
                            Field = fieldEnum.Trainer.ToString(),
                            FromValue = fromName,
                            ToValue = toName,
                            LoggedDateTime = loggedDatatTime
                        };


                        await context.AddAsync(eventLog);

                        await context.SaveChangesAsync();
                    }


                    #endregion

                    #region Parameter Room
                    var resRoom = resultEventSchedule.Where(o => o.Field == fieldEnum.Room.ToString()).FirstOrDefault();

                    var filterRoom = await context.EventRoomMaster.Where(o => o.EventRoomId == resultSchedule.EventRoomId).FirstOrDefaultAsync();
                    if (entity.RoomId != resultSchedule.EventRoomId)
                    {
                        EventScheduleLogs eventLog = new EventScheduleLogs
                        {
                            EventScheduleId = scheduleId,
                            UserId = entity.CreatedBy,
                            Action = typeAction.Update.ToString(),
                            ContentId = entity.ContentId,
                            Field = fieldEnum.Room.ToString(),
                            FromValue = filterRoom.EventRoomName,
                            ToValue = entity.RoomName.ToString(),
                            LoggedDateTime = loggedDatatTime
                        };


                        await context.AddAsync(eventLog);

                        await context.SaveChangesAsync();
                    }
                    #endregion

                    #region Parameter Session
                    var resSession = resultEventSchedule.Where(o => o.Field == fieldEnum.Session.ToString()).FirstOrDefault();

                    if (entity.Session != resultSchedule.Session)
                    {
                        EventScheduleLogs eventLog = new EventScheduleLogs
                        {
                            EventScheduleId = scheduleId,
                            UserId = entity.CreatedBy,
                            Action = typeAction.Update.ToString(),
                            ContentId = entity.ContentId,
                            Field = fieldEnum.Session.ToString(),
                            FromValue = resultSchedule.Session,
                            ToValue = entity.Session,
                            LoggedDateTime = loggedDatatTime
                        };


                        await context.AddAsync(eventLog);

                        await context.SaveChangesAsync();
                    }
                    #endregion

                    #region Parameter Location send email
                    var resLocation = resultEventSchedule.Where(o => o.Field == fieldEnum.Location.ToString()).FirstOrDefault();


                    if (entity.Location != resultSchedule.Location)
                    {
                        // Insert table EventScheduleLog
                        EventScheduleLogs eventLog = new EventScheduleLogs
                        {
                            EventScheduleId = scheduleId,
                            UserId = entity.CreatedBy,
                            Action = typeAction.Update.ToString(),
                            ContentId = entity.ContentId,
                            Field = fieldEnum.Location.ToString(),
                            FromValue = resultSchedule.Location,
                            ToValue = entity.Location,
                            LoggedDateTime = loggedDatatTime

                        };

                        await context.AddAsync(eventLog);

                        await context.SaveChangesAsync();

                        // Update table EventScheduke
                        if (resultSchedule.IsEmailSent != 0)
                        {
                            resultSchedule.IsEmailSent = 0;

                            await context.SaveChangesAsync();
                        }
                    }
                    #endregion

                    #region Parameter language
                    var resLanguage = resultEventSchedule.Where(o => o.Field == fieldEnum.Language.ToString()).FirstOrDefault();

                    var filterLanguageOld = await context.Language.Where(o => o.LanguageId == resultSchedule.LanguageId).FirstOrDefaultAsync();
                    var filterLanguageNew = await context.Language.Where(o => o.LanguageId == entity.LangId).FirstOrDefaultAsync();

                    if (entity.LangId != resultSchedule.LanguageId)
                    {
                        EventScheduleLogs eventLog = new EventScheduleLogs
                        {
                            EventScheduleId = scheduleId,
                            UserId = entity.CreatedBy,
                            Action = typeAction.Update.ToString(),
                            ContentId = entity.ContentId,
                            Field = fieldEnum.Language.ToString(),
                            FromValue = filterLanguageOld.LanguageName,
                            ToValue = filterLanguageNew.LanguageName,
                            LoggedDateTime = loggedDatatTime

                        };


                        await context.AddAsync(eventLog);

                        await context.SaveChangesAsync();
                    }
                    #endregion

                    #region Parameter Cate
                    var resCategory = resultEventSchedule.Where(o => o.Field == fieldEnum.Category.ToString()).FirstOrDefault();

                    var filterOld = await context.EventCategory.Where(o => o.EventCategoryId == resultSchedule.EventCategoryId).FirstOrDefaultAsync();
                    var filterNew = await context.EventCategory.Where(o => o.EventCategoryId == entity.CategoryId).FirstOrDefaultAsync();

                    if (entity.CategoryId != resultSchedule.EventCategoryId)
                    {
                        EventScheduleLogs eventLog = new EventScheduleLogs
                        {
                            EventScheduleId = scheduleId,
                            UserId = entity.CreatedBy,
                            Action = typeAction.Update.ToString(),
                            ContentId = entity.ContentId,
                            Field = fieldEnum.Category.ToString(),
                            FromValue = filterOld.EventCategoryName,
                            ToValue = filterNew.EventCategoryName,
                            LoggedDateTime = loggedDatatTime

                        };


                        await context.AddAsync(eventLog);

                        await context.SaveChangesAsync();
                    }
                    #endregion

                    #region Parameter ProjectName
                    var resProjectName = resultEventSchedule.Where(o => o.Field == fieldEnum.ProjectName.ToString()).FirstOrDefault();

                    if (entity.ProjectName != resultSchedule.ProjectName)
                    {
                        EventScheduleLogs eventLog = new EventScheduleLogs
                        {
                            EventScheduleId = scheduleId,
                            UserId = entity.CreatedBy,
                            Action = typeAction.Update.ToString(),
                            ContentId = entity.ContentId,
                            Field = fieldEnum.ProjectName.ToString(),
                            FromValue = resultSchedule.ProjectName,
                            ToValue = entity.ProjectName,
                            LoggedDateTime = loggedDatatTime

                        };


                        await context.AddAsync(eventLog);

                        await context.SaveChangesAsync();
                    }
                    #endregion

                    #region Parameter ProjectId
                    var resProjectId = resultEventSchedule.Where(o => o.Field == fieldEnum.ProjectId.ToString()).FirstOrDefault();

                    if (entity.ProjectId != resultSchedule.ProjectId)
                    {
                        EventScheduleLogs eventLog = new EventScheduleLogs
                        {
                            EventScheduleId = scheduleId,
                            UserId = entity.CreatedBy,
                            Action = typeAction.Update.ToString(),
                            ContentId = entity.ContentId,
                            Field = fieldEnum.ProjectId.ToString(),
                            FromValue = resultSchedule.ProjectId,
                            ToValue = entity.ProjectId,
                            LoggedDateTime = loggedDatatTime

                        };


                        await context.AddAsync(eventLog);

                        await context.SaveChangesAsync();
                    }
                    #endregion

                    #region Parameter Company
                    var resCompany = resultEventSchedule.Where(o => o.Field == fieldEnum.Company.ToString()).FirstOrDefault();

                    if (entity.Company != resultSchedule.Company)
                    {
                        EventScheduleLogs eventLog = new EventScheduleLogs
                        {
                            EventScheduleId = scheduleId,
                            UserId = entity.CreatedBy,
                            Action = typeAction.Update.ToString(),
                            ContentId = entity.ContentId,
                            Field = fieldEnum.Company.ToString(),
                            FromValue = resultSchedule.Company,
                            ToValue = entity.Company,
                            LoggedDateTime = loggedDatatTime

                        };


                        await context.AddAsync(eventLog);

                        await context.SaveChangesAsync();
                    }
                    #endregion

                    #region Parameter BookBy
                    var resBookBy = resultEventSchedule.Where(o => o.Field == fieldEnum.BookBy.ToString()).FirstOrDefault();

                    if (entity.BookBy != resultSchedule.BookBy)
                    {
                        EventScheduleLogs eventLog = new EventScheduleLogs
                        {
                            EventScheduleId = scheduleId,
                            UserId = entity.CreatedBy,
                            Action = typeAction.Update.ToString(),
                            ContentId = entity.ContentId,
                            Field = fieldEnum.BookBy.ToString(),
                            FromValue = resultSchedule.BookBy,
                            ToValue = entity.BookBy,
                            LoggedDateTime = loggedDatatTime

                        };


                        await context.AddAsync(eventLog);

                        await context.SaveChangesAsync();
                    }
                    #endregion

                    #region Parameter BookBy
                    var resIsBillable = resultEventSchedule.Where(o => o.Field == fieldEnum.IsBillable.ToString()).FirstOrDefault();

                    if (entity.IsBillable != resultSchedule.IsBillable)
                    {
                        EventScheduleLogs eventLog = new EventScheduleLogs
                        {
                            EventScheduleId = scheduleId,
                            UserId = entity.CreatedBy,
                            Action = typeAction.Update.ToString(),
                            ContentId = entity.ContentId,
                            Field = fieldEnum.IsBillable.ToString(),
                            FromValue = resultSchedule.IsBillable.ToString(),
                            ToValue = entity.IsBillable.ToString(),
                            LoggedDateTime = loggedDatatTime

                        };


                        await context.AddAsync(eventLog);

                        await context.SaveChangesAsync();
                    }
                    #endregion

                    #region Parameter UpLoadDate
                    var resUpLoadDate = resultEventSchedule.Where(o => o.Field == fieldEnum.UploadDate.ToString()).FirstOrDefault();

                    if (entity.UploadDate != resultSchedule.UploadToInstancyDate)
                    {
                        EventScheduleLogs eventLog = new EventScheduleLogs
                        {
                            EventScheduleId = scheduleId,
                            UserId = entity.CreatedBy,
                            Action = typeAction.Update.ToString(),
                            ContentId = entity.ContentId,
                            Field = fieldEnum.UploadDate.ToString(),
                            FromValue = resultSchedule.UploadToInstancyDate.ToString("dd MMM yyyy hh:mm"),
                            ToValue = entity.UploadDate.ToString("dd MMM yyyy hh:mm"),
                            LoggedDateTime = loggedDatatTime

                        };


                        await context.AddAsync(eventLog);

                        await context.SaveChangesAsync();
                    }
                    #endregion

                    #region Parameter Link send email
                    var resLink = resultEventSchedule.Where(o => o.Field == fieldEnum.Link.ToString()).FirstOrDefault();

                    if (entity.Link != resultSchedule.Link)
                    {
                        // Insert table EventScheduleLog
                        EventScheduleLogs eventLog = new EventScheduleLogs
                        {
                            EventScheduleId = scheduleId,
                            UserId = entity.CreatedBy,
                            Action = typeAction.Update.ToString(),
                            ContentId = entity.ContentId,
                            Field = fieldEnum.Link.ToString(),
                            FromValue = resultSchedule.Link,
                            ToValue = entity.Link != null ? entity.Link.ToString() : null,
                            LoggedDateTime = loggedDatatTime

                        };


                        await context.AddAsync(eventLog);

                        await context.SaveChangesAsync();

                        // Update table EventScheduke
                        if (resultSchedule.IsEmailSent != 0)
                        {
                            resultSchedule.IsEmailSent = 0;

                            await context.SaveChangesAsync();
                        }

                    }
                    #endregion

                    #region Parameter Remark
                    var resRemark = resultEventSchedule.Where(o => o.Field == fieldEnum.Remark.ToString()).FirstOrDefault();

                    if (entity.Remarks != resultSchedule.Remark)
                    {
                        EventScheduleLogs eventLog = new EventScheduleLogs
                        {
                            EventScheduleId = scheduleId,
                            UserId = entity.CreatedBy,
                            Action = typeAction.Update.ToString(),
                            ContentId = entity.ContentId,
                            Field = fieldEnum.Remark.ToString(),
                            FromValue = resultSchedule.Remark,
                            ToValue = entity.Remarks != null ? entity.Remarks.ToString() : null,
                            LoggedDateTime = loggedDatatTime

                        };


                        await context.AddAsync(eventLog);

                        await context.SaveChangesAsync();

                    }
                    #endregion

                    #region Parameter noParti
                    var resNoParti = resultEventSchedule.Where(o => o.Field == fieldEnum.NoOfParticipant.ToString()).FirstOrDefault();

                    if (entity.NoOfParticipant != resultSchedule.NoOfParticipant)
                    {
                        EventScheduleLogs eventLog = new EventScheduleLogs
                        {
                            EventScheduleId = scheduleId,
                            UserId = entity.CreatedBy,
                            Action = typeAction.Update.ToString(),
                            ContentId = entity.ContentId,
                            Field = fieldEnum.NoOfParticipant.ToString(),
                            FromValue = resultSchedule.NoOfParticipant.ToString(),
                            ToValue = entity.NoOfParticipant.ToString(),
                            LoggedDateTime = loggedDatatTime

                        };


                        await context.AddAsync(eventLog);

                        await context.SaveChangesAsync();
                    }
                    #endregion

                    #region Parameter status
                    var resStatus = resultEventSchedule.Where(o => o.Field == fieldEnum.Status.ToString()).FirstOrDefault();

                    var filterOldStatus = await context.Status.Where(o => o.StatusId == resultSchedule.StatusId).FirstOrDefaultAsync();
                    var filterNewStatus = await context.Status.Where(o => o.StatusId == entity.StatusId).FirstOrDefaultAsync();

                    if (entity.StatusId != resultSchedule.StatusId)
                    {
                        EventScheduleLogs eventLog = new EventScheduleLogs
                        {
                            EventScheduleId = scheduleId,
                            UserId = entity.CreatedBy,
                            Action = typeAction.Update.ToString(),
                            ContentId = entity.ContentId,
                            Field = fieldEnum.Status.ToString(),
                            FromValue = filterOldStatus != null ? filterOldStatus.Status1 : null,
                            ToValue = filterNewStatus != null ? filterNewStatus.Status1 : null,
                            LoggedDateTime = loggedDatatTime

                        };


                        await context.AddAsync(eventLog);

                        await context.SaveChangesAsync();
                    }
                    #endregion

                    #region Moderator
                    var resModerator = resultEventSchedule.Where(o => o.Field == fieldEnum.Moderator.ToString()).OrderByDescending(o => o.LoggedDateTime).FirstOrDefault();

                    var preModeratorLst = resModerator != null ? resModerator.ToValue.Split(",").Select(o => o.Trim()).ToList() : null;
                    var lastModeratorLst = entity.Moderator;

                    List<string> nameModeratorLst = new List<string>();
                    var checkInsert = false;

                    if (lastModeratorLst.Count() == 0 && preModeratorLst != null)
                    {
                        checkInsert = true;
                    }

                    foreach (var preModerator in lastModeratorLst)
                    {
                        var getTrainer = await context.TrainerMaster.Where(o => o.TrainerId == preModerator && o.IsTrainer == 0).FirstOrDefaultAsync();


                        string name = getTrainer != null ? getTrainer.Name + " " + getTrainer.LastName : null;

                        nameModeratorLst.Add(name);

                        if (preModeratorLst != null)
                        {
                            if (preModeratorLst.Count() == lastModeratorLst.Count())
                            {

                                var chkMod = preModeratorLst.Where(o => o == name).FirstOrDefault();

                                if (chkMod == null)
                                {
                                    checkInsert = true;
                                }
                            }
                            else
                            {
                                checkInsert = true;
                            }
                        }
                        else
                        {
                            checkInsert = true;
                        }

                    }

                    if (checkInsert)
                    {
                        EventScheduleLogs eventLog = new EventScheduleLogs
                        {
                            EventScheduleId = scheduleId,
                            UserId = entity.CreatedBy,
                            Action = typeAction.Update.ToString(),
                            ContentId = entity.ContentId,
                            Field = fieldEnum.Moderator.ToString(),
                            FromValue = resModerator != null ? resModerator.ToValue : null,
                            ToValue = nameModeratorLst != null ? string.Join(" , ", nameModeratorLst) : null,
                            LoggedDateTime = loggedDatatTime
                        };


                        await context.AddAsync(eventLog);

                        await context.SaveChangesAsync();
                    }
                    #endregion

                    return true;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public async Task<bool> RemoveSchedule(PlannerModel_Request entity, int scheduleId, EventSchedule evs)
        {
            try
            {

                using (var context = new seac_webapplicationContext())
                {

                    var eventSchedule = await context.EventSchedule.Where(o => o.EventScheduleId == scheduleId).FirstOrDefaultAsync();

                    #region Update schedule
                    eventSchedule.ContentId = entity.ContentId;
                    eventSchedule.EventRoomId = entity.RoomId;
                    eventSchedule.EventCategoryId = entity.CategoryId;
                    eventSchedule.LearningTypeId = entity.LearningTypeId;
                    eventSchedule.ContentFormatId = entity.ContentFormatId;
                    eventSchedule.StartTime = entity.StartDate;
                    eventSchedule.EndTime = entity.EndDate;
                    eventSchedule.Duration = (int)entity.EndDate.Subtract(entity.StartDate).TotalMinutes;
                    eventSchedule.Link = entity.Link;
                    eventSchedule.DisplayName = "";//Fix
                    eventSchedule.ProjectId = entity.ProjectId;
                    eventSchedule.ProjectName = entity.ProjectName;
                    eventSchedule.Company = entity.Company;
                    eventSchedule.NoOfParticipant = entity.NoOfParticipant;
                    eventSchedule.Session = entity.Session;
                    eventSchedule.IsBillable = entity.IsBillable;
                    eventSchedule.LanguageId = entity.LangId;
                    eventSchedule.UploadToInstancyDate = entity.UploadDate;
                    eventSchedule.UpdatedDate = DateTime.Now;
                    eventSchedule.CreatedBy = entity.CreatedBy;
                    eventSchedule.IsActive = 1;
                    eventSchedule.ContentBusinessTypeId = entity.BusinessTypeId;
                    eventSchedule.Location = entity.Location;
                    eventSchedule.BookBy = entity.BookBy;
                    eventSchedule.StatusId = entity.StatusId;
                    eventSchedule.Remark = entity.Remarks;
                    eventSchedule.IsModerator = entity.IsModerator;
                    #endregion

                    await context.SaveChangesAsync();
                }

                return true;


            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public async Task<bool> UpdateTrainer(PlannerModel_Request entity, int scheduleId, string baseUrl)
        {
            try
            {
                using (var context = new seac_webapplicationContext())
                {
                    // Update schedule trainer old
                    var resultSchedule = await context.EventScheduleTrainer.Where(o => o.EventScheduleId == scheduleId && o.IsActive == 1).FirstOrDefaultAsync();

                    if (resultSchedule != null)
                    {

                        if (entity.TrainerId != resultSchedule.TrainerId)
                        {

                            #region old
                            //// Send email
                            //var chkEmailLog = await context.EmailLog.Where(o => o.TrainerId == resultSchedule.TrainerId && o.EventScheduleId == ).FirstOrDefaultAsync();

                            //if (chkEmailLog != null)
                            //{
                            //     List<int> scheduleIdLst = new List<int>();
                            //    scheduleIdLst.Add(scheduleId);

                            //     // * Must send email to trainer
                            //   var autuMailAppoinment = await this.icsSentMailManager.FindAllEventScheduleAsync(scheduleIdLst, 1);

                            //    foreach (var item in autuMailAppoinment)
                            //   {
                            //       await this.autoSentMailHelper.SendEmail(item, new List<EmailLogModel>(), baseUrl, chkEmailLog.Uuid, "true");

                            //       chkEmailLog.IsCancel = 1;

                            //       await context.SaveChangesAsync();
                            //   }


                            // }
                            #endregion

                            await CancelEmail(scheduleId, resultSchedule.TrainerId, baseUrl);

                            // Update scheduleTrainer
                            resultSchedule.IsActive = 0;
                            resultSchedule.UpdatedDate = DateTime.Now;
                            resultSchedule.UpdatedBy = entity.UpdatedBy;

                            await context.SaveChangesAsync();

                            // Insert scheduleTrainer
                            EventScheduleTrainer eventSchedule = new EventScheduleTrainer
                            {
                                EventScheduleId = scheduleId,
                                TrainerId = entity.TrainerId,
                                IsActive = 1,
                                IsTrainerReply = 0,
                                CreatedDate = DateTime.Now,
                                CreatedBy = entity.CreatedBy,
                                UpdatedDate = DateTime.Now,
                                UpdatedBy = entity.UpdatedBy
                            };

                            await context.AddAsync(eventSchedule);
                            await context.SaveChangesAsync();

                            // Update isEmailSent
                            var schedule = await context.EventSchedule.Where(o => o.EventScheduleId == scheduleId && o.IsActive == 1).FirstOrDefaultAsync();
                            schedule.IsEmailSent = 0;
                            await context.SaveChangesAsync();
                        }

                    }
                    else
                    {
                        // Insert scheduleTrainer
                        EventScheduleTrainer eventSchedule = new EventScheduleTrainer
                        {
                            EventScheduleId = scheduleId,
                            TrainerId = entity.TrainerId,
                            IsActive = 1,
                            IsTrainerReply = 0,
                            CreatedDate = DateTime.Now,
                            CreatedBy = entity.CreatedBy,
                            UpdatedDate = DateTime.Now,
                            UpdatedBy = entity.UpdatedBy
                        };

                        await context.AddAsync(eventSchedule);
                        await context.SaveChangesAsync();
                    }

                    return true;
                }

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public async Task<bool> UpdateModelator(PlannerModel_Request entity, int scheduleId)
        {
            try
            {
                using (var context = new seac_webapplicationContext())
                {



                    var eventMod = await context.EventScheduleModerator.Where(o => o.EventScheduleId == scheduleId).ToListAsync();


                    foreach (var item in eventMod)
                    {

                        item.IsActive = 0;
                        await context.SaveChangesAsync();

                    }

                    foreach (var incomeMod in entity.Moderator)
                    {
                        Console.WriteLine(incomeMod);

                        var eventModerator = await context.EventScheduleModerator.Where(o => o.ModeratorId == incomeMod && o.EventScheduleId == scheduleId && o.IsActive == 0).FirstOrDefaultAsync();

                        if (eventModerator != null)
                        {
                            eventModerator.IsActive = 1;
                            await context.SaveChangesAsync();
                        }
                        else
                        {
                            //  Console.WriteLine('else : '+incomeMod);
                            EventScheduleModerator eventScheduleMod = new EventScheduleModerator
                            {
                                EventScheduleId = scheduleId,
                                ModeratorId = incomeMod,
                                IsActive = 1,
                                CreatedDate = DateTime.Now,
                                CreatedBy = entity.CreatedBy,
                            };
                            // Console.WriteLine('Object : '+eventScheduleMod.ToString());

                            await context.AddAsync(eventScheduleMod);
                            await context.SaveChangesAsync();
                        }


                    }

                    return true;


                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public async Task<List<ContentPlanner>> FilterContentName(int businessTypeId, int learningTypeId, int contentFormatId)
        {
            try
            {
                using (var context = new seac_webapplicationContext())
                {
                    var result = (from cm in await context.ContentMaster.ToListAsync()
                                  join ctv in await context.ContentFormatVariety.ToListAsync()
                                  on cm.ContentId equals ctv.ContentId
                                  where cm.ContentBusinessTypeId == businessTypeId && cm.LearningTypeId == learningTypeId && ctv.ContentFormatId == contentFormatId && cm.IsActive == 1
                                  select new ContentPlanner
                                  {
                                      Id = cm.ContentId,
                                      Value = cm.ContentName,
                                      Duration = cm.Duration
                                  }).OrderBy(o => o.Value).ToList();

                    return result;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public async Task<List<StatusPlanner>> FindAllStatusAsync()
        {
            try
            {
                using (var context = new seac_webapplicationContext())
                {
                    var result = await context.Status.ToListAsync();

                    if (result == null)
                    {
                        return null;
                    }

                    List<StatusPlanner> content = new List<StatusPlanner>();
                    foreach (var res in result)
                    {
                        content.Add(new StatusPlanner { Id = res.StatusId, Value = res.Status1 });
                    }

                    return content;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public async Task<List<RoomPlanner>> FindRoomAllAsync(DateTime createDate, string startTime, string endTime, int contentId, int bussinessTypeId)
        {
            try
            {
                using (var context = new seac_webapplicationContext())
                {

                    var result = (from cfv in await context.ContentFormatVariety.ToListAsync()
                                  join erm in await context.EventRoomMaster.ToListAsync() on cfv.ContentFormatId equals erm.ContentFormatId
                                  join ert in await context.EventRoomType.ToListAsync() on erm.EventRoomTypeId equals ert.EventRoomTypeId
                                  join ctf in await context.ContentFormatMaster.ToListAsync() on cfv.ContentFormatId equals ctf.ContentFormatId
                                  where erm.IsActive == 1 && cfv.ContentId == contentId
                                  select new RoomPlanner
                                  {
                                      Id = erm.EventRoomId,
                                      Value = erm.EventRoomName,
                                      Type = new RoomTypePlanner
                                      {
                                          Id = ert.EventRoomTypeId,
                                          Value = ert.EventRoomTypeName
                                      },
                                      ContentFormat = new ContentFormatPlanner
                                      {
                                          Id = ctf.ContentFormatId,
                                          Value = ctf.ContentFormatName
                                      }
                                  }).OrderBy(o => o.Id).ToList();


                    DateTime checkthisdate = new DateTime(createDate.Year, createDate.Month, createDate.Day);
                    //This is Sample Time for check at 20:00 or 8 PM.
                    var resStartTime = startTime.Split(":");
                    TimeSpan checkstarttime = new TimeSpan(Convert.ToInt32(resStartTime[0]), Convert.ToInt32(resStartTime[1]), Convert.ToInt32(resStartTime[2]));

                    var resEndTime = endTime.Split(":");
                    TimeSpan checkendtime = new TimeSpan(Convert.ToInt32(resEndTime[0]), Convert.ToInt32(resEndTime[1]), Convert.ToInt32(resEndTime[2]));

                    //This is how to create start / end time period for each content
                    DateTime checkStartDate = checkthisdate + checkstarttime;
                    DateTime checkEndDate = checkthisdate + checkendtime;

                    if (bussinessTypeId != 3 && bussinessTypeId != 4)
                    {
                        var resultSchedule = await context.EventSchedule.Where(o => o.StartTime <= checkStartDate && o.EndTime >= checkStartDate || o.StartTime <= checkEndDate && o.EndTime >= checkEndDate || (o.StartTime > checkStartDate && o.EndTime < checkEndDate)).ToListAsync();

                        foreach (var res in resultSchedule.Where(o => o.IsActive == 1))
                        {

                            result.RemoveAll(O => O.Id == res.EventRoomId);

                        }
                    }





                    return result;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public List<RoomPlanner> FindRoomByBusinessTypeAndContentFormat(int roomTypeId, int contentFormatId)
        {
            try
            {

                using (var context = new seac_webapplicationContext())
                {

                    var result = (from erm in context.EventRoomMaster
                                  join ert in context.EventRoomType on erm.EventRoomTypeId equals ert.EventRoomTypeId
                                  join ctf in context.ContentFormatMaster on erm.ContentFormatId equals ctf.ContentFormatId
                                  where erm.IsActive == 1 && erm.EventRoomTypeId == roomTypeId && erm.ContentFormatId == contentFormatId
                                  select new RoomPlanner
                                  {
                                      Id = erm.EventRoomId,
                                      Value = erm.EventRoomName,
                                      Type = new RoomTypePlanner
                                      {
                                          Id = ert.EventRoomTypeId,
                                          Value = ert.EventRoomTypeName
                                      },
                                      ContentFormat = new ContentFormatPlanner
                                      {
                                          Id = ctf.ContentFormatId,
                                          Value = ctf.ContentFormatName
                                      }
                                  }).ToList();

                    return result;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public async Task<bool> DeleteAsync(int scheduleId, int CreatedBy)
        {
            try
            {

                return await CancelEvent(scheduleId, CreatedBy, "delete", "");
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public async Task<bool> CancelAsync(int scheduleId, int CreatedBy, string baseUrl)
        {
            try
            {

                return await CancelEvent(scheduleId, CreatedBy, "cancel", baseUrl);

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public async Task<List<TrainerSchedule>> FindScheduleAsync(int trainerId, DateTime startDate, DateTime endDate)
        {
            try
            {
                List<TrainerSchedule> trainerSchedule = new List<TrainerSchedule>();

                using (var context = new seac_webapplicationContext())
                {


                    var resultTrainer = await context.TrainerMaster.Where(o => o.TrainerId == trainerId).FirstOrDefaultAsync();

                    if (resultTrainer != null)
                    {

                        var data = new iCalendarObject(resultTrainer.Name + " " + resultTrainer.LastName, resultTrainer.ICalendarLink, startDate, endDate);

                        var result = data.parseICStoSchedule(startDate, endDate);

                        var scheduleTrainer = await context.EventScheduleTrainer.Where(o => o.TrainerId == resultTrainer.TrainerId && o.IsActive == 1).ToListAsync();

                        foreach (var item in scheduleTrainer)
                        {
                            var schedule = (from evs in await context.EventSchedule.Where(o => o.IsActive == 1).ToListAsync()
                                            join cm in await context.ContentMaster.Where(o => o.IsActive == 1).ToListAsync() on evs.ContentId equals cm.ContentId
                                            join rm in await context.EventRoomMaster.Where(o => o.IsActive == 1).ToListAsync() on evs.EventRoomId equals rm.EventRoomId
                                            join ctfm in await context.ContentFormatMaster.ToListAsync() on evs.ContentFormatId equals ctfm.ContentFormatId
                                            where evs.EventScheduleId == item.EventScheduleId
                                            select new TrainerSchedule()
                                            {
                                                eventScheduleId = evs.EventScheduleId,
                                                contentBusinessTypeId = evs.ContentBusinessTypeId,
                                                statusId = evs.StatusId,
                                                langId = evs.LanguageId,
                                                startDate = evs.StartTime,
                                                endDate = evs.EndTime,
                                                contentName = cm.ContentName,
                                                room = rm.EventRoomName,
                                                contentFormat = ctfm.ContentFormatName,
                                                location = evs.Location,
                                                session = evs.Session,
                                                company = evs.Company,
                                                projectName = evs.ProjectName,
                                                bookBy = evs.BookBy,
                                                remarks = evs.Remark,
                                                flag = "WORK",
                                            });


                            foreach (var sch in schedule)
                            {
                                var contentBusinessType = await context.ContentBusinessType.Where(o => o.ContentBusinessTypeId == sch.contentBusinessTypeId).FirstOrDefaultAsync();
                                sch.businessTypeAbbreviation = contentBusinessType.ContentBusinessTypeAbbreviate;

                                if (sch.statusId != null)
                                {
                                    var statusId = await context.Status.Where(o => o.StatusId == sch.statusId).FirstOrDefaultAsync();
                                    sch.status = statusId.Status1;
                                }

                                var lang = await context.Language.Where(o => o.LanguageId == sch.langId).FirstOrDefaultAsync();
                                sch.language = lang.LanguageName;

                                result.Add(sch);
                            }


                        }

                        return result;


                    }
                    return null;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public class IcsFileModel
        {
            public string name { get; set; }
            public string url { get; set; }
            public DateTime startDate { get; set; }
        }
        public async Task<bool> GetICSFile()
        {
            try
            {
                using (var context = new seac_webapplicationContext())
                {
                    var trainer = await context.TrainerMaster.Where(o => o.IsActive == 1).ToListAsync();

                    foreach (var item in trainer)
                    {
                        iCalendarObject calendar = new iCalendarObject(item.Name + " " + item.LastName, item.ICalendarLink, DateTime.Now);
                    }


                }


                return true;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<bool> CancelEvent(int scheduleId, int CreatedBy, string mode, string baseUrl)
        {
            try
            {

                using (var context = new seac_webapplicationContext())
                {

                    if (mode == "cancel")
                    {
                        await SentCancelEmail(scheduleId, baseUrl);
                    }

                    var resultScheduleModerator = await context.EventScheduleModerator.Where(o => o.EventScheduleId == scheduleId).ToListAsync();

                    if (resultScheduleModerator != null)
                    {
                        foreach (var item in resultScheduleModerator)
                        {
                            item.IsActive = 0;
                            await context.SaveChangesAsync();
                        }
                    }

                    //Table event schedule trainer
                    var resultScheduleTrainer = await context.EventScheduleTrainer.Where(o => o.EventScheduleId == scheduleId).FirstOrDefaultAsync();

                    if (resultScheduleTrainer != null)
                    {

                        resultScheduleTrainer.IsActive = 0;
                        await context.SaveChangesAsync();
                    }

                    //Table event schedule
                    var resultSchedule = await context.EventSchedule.Where(o => o.EventScheduleId == scheduleId).FirstOrDefaultAsync();

                    if (resultSchedule != null)
                    {
                        resultSchedule.IsActive = 0;
                        await context.SaveChangesAsync();

                        EventScheduleLogs scheduleLog = new EventScheduleLogs
                        {
                            EventScheduleId = scheduleId,
                            UserId = CreatedBy,
                            Action = typeAction.Cancel.ToString(),
                            ContentId = resultSchedule.ContentId,
                            LoggedDateTime = DateTime.Now
                        };

                        // Insert schedule log
                        await context.AddAsync(scheduleLog);
                        await context.SaveChangesAsync();
                    }


                    //Update email log
                    var emailLog = await context.EmailLog.Where(o => o.EventScheduleId == scheduleId).ToListAsync();

                    foreach (var item in emailLog)
                    {
                        item.IsCancel = 1;

                        await context.SaveChangesAsync();
                    }



                    if (mode == "cancel")
                    {
                        await SentCancelEmail(scheduleId, baseUrl);
                    }

                }

                return true;


            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<List<TrainerContacTypeCombo>> getTrainerMaster(DateTime createDate, string startTime, string endTime, int contentId, string mode)
        {
            try
            {
                DateTime present = DateTime.Now;
                using (var context = new seac_webapplicationContext())
                {

                    List<TrainerContacTypeCombo> scheduleTrainer = new List<TrainerContacTypeCombo>();

                    List<TrainerCount> trainerCount = new List<TrainerCount>();

                    // Fileter event per month
                    var filterSchedule = await context.EventSchedule.Where(o => o.StartTime.Month == createDate.Month && o.StartTime.Year == createDate.Year && o.IsActive == 1).ToListAsync();

                    List<TrainerCount> idEventTrainer = new List<TrainerCount>();
                    List<EventSchduleTrainerCount> eventScCount = new List<EventSchduleTrainerCount>();
                    List<int> trainerId = new List<int>();

                    foreach (var itemSchedule in filterSchedule)
                    {
                        if (itemSchedule != null)
                        {
                            var resultSchedultTrainer = await context.EventScheduleTrainer.Where(o => o.EventScheduleId == itemSchedule.EventScheduleId && o.IsActive == 1).ToListAsync();

                            foreach (var item in resultSchedultTrainer)
                            {
                                eventScCount.Add(new EventSchduleTrainerCount { id = item.TrainerId, value = item.EventScheduleId });
                            }
                        }
                    }


                    var eventCount = eventScCount.GroupBy(o => o.id).ToList();


                    var certifiedTrainer = await context.CertifiedTrainer.Where(o => o.IsActive == 1 && o.ContentId == contentId).ToListAsync();



                    foreach (var itemCerti in certifiedTrainer)
                    {

                        var trainerMaster = await context.TrainerMaster.Where(o => o.IsActive == 1 && o.TrainerId == itemCerti.TrainerId).ToListAsync();

                        var filterTrainer = await checkTrainerAvali(createDate, startTime, endTime, trainerMaster);

                        foreach (var itemTrainer in filterTrainer)
                        {

                            var contactType = await context.TrainerContractType.Where(o => o.TraninerContractTypeId == itemTrainer.TraninerContractTypeId).FirstOrDefaultAsync();

                            var schduleTrainer = await context.EventScheduleTrainer.Where(o => o.TrainerId == itemTrainer.TrainerId && o.IsActive == 1).ToListAsync();

                            if (schduleTrainer != null)
                            {

                                var countEventPermonth = eventCount.Where(o => o.Key == itemTrainer.TrainerId);

                                scheduleTrainer.Add(new TrainerContacTypeCombo
                                {
                                    Id = itemTrainer.TrainerId,
                                    name = itemTrainer.Name,
                                    lastname = itemTrainer.LastName,
                                    eventCount = countEventPermonth != null ? countEventPermonth.SelectMany(eventScCount => eventScCount).Count() : 0,
                                    maxEventPerMonth = itemTrainer.MaximumEventPerMonth,
                                    contractType = new ContactTypePlanner
                                    {
                                        Id = contactType.TraninerContractTypeId,
                                        Value = contactType.TraninerContractTypeName
                                    }

                                });
                            }
                        }

                    }


                    DateTime checkthisdate = new DateTime(createDate.Year, createDate.Month, createDate.Day);
                    //This is Sample Time for check at 20:00 or 8 PM.
                    var resStartTime = startTime.Split(":");
                    TimeSpan checkstarttime = new TimeSpan(Convert.ToInt32(resStartTime[0]), Convert.ToInt32(resStartTime[1]), Convert.ToInt32(resStartTime[2]));

                    var resEndTime = endTime.Split(":");
                    TimeSpan checkendtime = new TimeSpan(Convert.ToInt32(resEndTime[0]), Convert.ToInt32(resEndTime[1]), Convert.ToInt32(resEndTime[2]));

                    //This is how to create start / end time period for each content
                    DateTime checkStartDate = checkthisdate + checkstarttime;
                    DateTime checkEndDate = checkthisdate + checkendtime;

                    List<EventSchedule> resultSchedule = new List<EventSchedule>();

                    //if (mode.ToLower().Equals("create"))
                    //{


                    resultSchedule = await context.EventSchedule.Where(
                        o => o.StartTime <= checkStartDate && o.EndTime > checkStartDate ||
                        o.StartTime < checkEndDate && o.EndTime >= checkEndDate ||
                        (o.StartTime > checkStartDate && o.EndTime < checkEndDate)).ToListAsync();
                    //}
                    //else if (mode.ToLower().Equals("edit"))
                    //{

                    //    resultSchedule = (from evs in await context.EventSchedule.ToListAsync()
                    //                      join evst in await context.EventScheduleTrainer.ToListAsync()
                    //                      on evs.EventScheduleId equals evst.EventScheduleId
                    //                      where
                    //                      evs.StartTime <= checkStartDate && evs.EndTime > checkStartDate ||
                    //                      evs.StartTime < checkEndDate && evs.EndTime >= checkEndDate ||
                    //                      evs.StartTime > checkStartDate && evs.EndTime < checkEndDate &&
                    //                      evs.IsActive == 1 && evst.IsActive == 1
                    //                      select new EventSchedule
                    //                      {
                    //                          EventScheduleId = evs.EventScheduleId
                    //                      }).ToList();
                    //}

                    foreach (var res in resultSchedule.Where(o => o.IsActive == 1))
                    {
                        var resultEventSchedule = await context.EventScheduleTrainer.Where(o => o.EventScheduleId == res.EventScheduleId && o.IsActive == 1).ToListAsync();

                        foreach (var item in resultEventSchedule)
                        {
                            scheduleTrainer.RemoveAll(O => O.Id == item.TrainerId);
                        }
                    }



                    return scheduleTrainer;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<List<TrainerContacTypeCombo>> getModerator(DateTime createDate, string startTime, string endTime, int businessTypeId, int roomId)
        {
            try
            {
                DateTime present = DateTime.Now;
                using (var context = new seac_webapplicationContext())
                {

                    List<TrainerContacTypeCombo> scheduleTrainer = new List<TrainerContacTypeCombo>();

                    List<TrainerCount> trainerCount = new List<TrainerCount>();

                    // Fileter event per month
                    var filterSchedule = await context.EventSchedule.Where(o => o.StartTime.Month == createDate.Month && o.StartTime.Year == createDate.Year && o.IsActive == 1).ToListAsync();

                    List<TrainerCount> idEventTrainer = new List<TrainerCount>();
                    List<EventSchduleTrainerCount> eventScCount = new List<EventSchduleTrainerCount>();
                    List<int> trainerId = new List<int>();

                    foreach (var itemSchedule in filterSchedule)
                    {
                        if (itemSchedule != null)
                        {
                            var resultSchedultModerator = await context.EventScheduleModerator.Where(o => o.EventScheduleId == itemSchedule.EventScheduleId && o.IsActive == 1).ToListAsync();

                            foreach (var item in resultSchedultModerator)
                            {
                                eventScCount.Add(new EventSchduleTrainerCount { id = item.ModeratorId, value = item.EventScheduleId });
                            }
                        }
                    }

                    var eventCount = eventScCount.GroupBy(o => o.id).ToList();


                    var resultSupportBusinessType = await context.ModeratorSupportBusinessType.Where(o => o.ContentBusinessTypeId == businessTypeId).ToListAsync();


                    foreach (var supportBusinessType in resultSupportBusinessType)
                    {
                        var resultRoomMaster = await context.EventRoomMaster.Where(o => o.EventRoomId == roomId).FirstOrDefaultAsync();

                        if (resultRoomMaster != null)
                        {

                            var resultRoomType = await context.ModeratorSupportRoomType.Where(o => o.ModeratorId == supportBusinessType.ModeratorId && o.EventRoomTypeId == resultRoomMaster.EventRoomTypeId).ToListAsync();

                            foreach (var roomType in resultRoomType)
                            {

                                var trainerMaster = await context.TrainerMaster.Where(o => o.IsActive == 1 && o.TrainerId == roomType.ModeratorId).OrderBy(o => o.TrainerId).ToListAsync();

                                var filterTrainer = await checkTrainerAvali(createDate, startTime, endTime, trainerMaster);

                                foreach (var itemTrainer in filterTrainer)
                                {

                                    var contactType = await context.TrainerContractType.Where(o => o.TraninerContractTypeId == itemTrainer.TraninerContractTypeId).FirstOrDefaultAsync();

                                    var schduleModerator = await context.EventScheduleModerator.Where(o => o.ModeratorId == itemTrainer.TrainerId && o.IsActive == 1).ToListAsync();

                                    if (schduleModerator != null)
                                    {
                                        //string nickName = "";

                                        //if (itemTrainer.NickNameEn != null && itemTrainer.NickNameEn != "")
                                        //{
                                        //    nickName = "(" + itemTrainer.NickNameEn + ") ";
                                        //}

                                        //scheduleTrainer.Add(new TrainerPlanner
                                        //{
                                        //    Id = itemTrainer.TrainerId,
                                        //    Value = itemTrainer.Name + nickName + itemTrainer.LastName

                                        //});

                                        //foreach (var item in schduleModerator)
                                        //{
                                        //    eventScCount.Add(new EventSchduleTrainerCount { id = item.ModeratorId, value = item.EventScheduleId });
                                        //}
                                        //var eventCount = eventScCount.GroupBy(o => o.id).ToList();

                                        var countEventPermonth = eventCount.Where(o => o.Key == itemTrainer.TrainerId);

                                        scheduleTrainer.Add(new TrainerContacTypeCombo
                                        {
                                            Id = itemTrainer.TrainerId,
                                            name = itemTrainer.Name,
                                            lastname = itemTrainer.LastName,
                                            nickname = itemTrainer.NickNameEn,
                                            eventCount = countEventPermonth != null ? countEventPermonth.SelectMany(eventScCount => eventScCount).Count() : 0,
                                            maxEventPerMonth = itemTrainer.MaximumEventPerMonth,
                                            contractType = new ContactTypePlanner
                                            {
                                                Id = contactType.TraninerContractTypeId,
                                                Value = contactType.TraninerContractTypeName
                                            }

                                        });

                                    }
                                }

                            }
                        }

                    }


                    DateTime checkthisdate = new DateTime(createDate.Year, createDate.Month, createDate.Day);
                    //This is Sample Time for check at 20:00 or 8 PM.
                    var resStartTime = startTime.Split(":");
                    TimeSpan checkstarttime = new TimeSpan(Convert.ToInt32(resStartTime[0]), Convert.ToInt32(resStartTime[1]), Convert.ToInt32(resStartTime[2]));

                    var resEndTime = endTime.Split(":");
                    TimeSpan checkendtime = new TimeSpan(Convert.ToInt32(resEndTime[0]), Convert.ToInt32(resEndTime[1]), Convert.ToInt32(resEndTime[2]));

                    //This is how to create start / end time period for each content
                    DateTime checkStartDate = checkthisdate + checkstarttime;
                    DateTime checkEndDate = checkthisdate + checkendtime;



                    var resultSchedule = await context.EventSchedule.Where(o => o.StartTime <= checkStartDate && o.EndTime > checkStartDate || o.StartTime < checkEndDate && o.EndTime >= checkEndDate || (o.StartTime > checkStartDate && o.EndTime < checkEndDate)).ToListAsync();

                    foreach (var res in resultSchedule.Where(o => o.IsActive == 1))
                    {
                        var resultEventSchedule = await context.EventScheduleModerator.Where(o => o.EventScheduleId == res.EventScheduleId && o.IsActive == 1).ToListAsync();


                        foreach (var item in resultEventSchedule)
                        {
                            scheduleTrainer.RemoveAll(O => O.Id == item.ModeratorId);
                        }
                    }



                    return scheduleTrainer;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<bool> GetICSFileByTrainer(int trainerId)
        {
            try
            {
                using (var context = new seac_webapplicationContext())
                {

                    var trainer = await context.TrainerMaster.Where(o => o.IsActive == 1 && o.TrainerId == trainerId).ToListAsync();

                    foreach (var item in trainer)
                    {
                        string filesToDelete = item.Name + " " + item.LastName + "*";
                        string[] fileList = System.IO.Directory.GetFiles(iCalendarObject.tempICSPath, filesToDelete);
                        foreach (string file in fileList)
                        {
                            System.IO.File.Delete(file);
                        }
                        iCalendarObject calendar = new iCalendarObject(item.Name + " " + item.LastName, item.ICalendarLink, DateTime.Now);
                    }


                }


                return true;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        // Function sent email for cancel
        public async Task<bool> SentCancelEmail(int scheduleId, string baseUrl)
        {
            try
            {
                using (var context = new seac_webapplicationContext())
                {
                    var resultSchedule = await context.EventScheduleTrainer.Where(o => o.EventScheduleId == scheduleId && o.IsActive == 1).FirstOrDefaultAsync();

                    if (resultSchedule != null)
                    {
                        // Check log email
                        var chkEmailLog = await context.EmailLog.Where(o => o.TrainerId == resultSchedule.TrainerId).FirstOrDefaultAsync();

                        if (chkEmailLog != null)
                        {
                            List<int> scheduleIdLst = new List<int>();
                            scheduleIdLst.Add(scheduleId);

                            // * Must send email to trainer
                            var autuMailAppoinment = await this.icsSentMailManager.FindAllEventScheduleAsync(scheduleIdLst, 1);
                            List<EmailLogModel> EmailLogsList = new List<EmailLogModel>();
                            foreach (var item in autuMailAppoinment)
                            {

                                await CancelEmail(scheduleId, resultSchedule.TrainerId, baseUrl);

                                chkEmailLog.IsCancel = 1;

                                await context.SaveChangesAsync();
                            }


                        }
                    }
                }

                return true;

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private async Task<bool> CancelEmail(int eventScheduleId, int trainerId, string baseUrl)
        {
            try
            {
                string respondStr = "";
                List<int> EntityList = new List<int>();
                EntityList.Add(eventScheduleId);
                List<EmailLogModel> emailLogsList = new List<EmailLogModel>();
                emailLogsList = await this.icsSentMailManager.GetEmailLog(eventScheduleId, trainerId);

                var eventSchedule = await this.icsSentMailManager.FindAllEventScheduleAsync(EntityList, 1);


                var emailLog = emailLogsList.Where(o => o.IsCancel == 0).FirstOrDefault();
                if (emailLog != null)
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
                    return false;
                }




                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public async Task<bool> DeleteTrainer(int scheduleId, int trainerId)
        {
            try
            {
                using (var context = new seac_webapplicationContext())
                {
                    var scheduleTrainer = await context.EventScheduleTrainer.Where(o => o.EventScheduleId == scheduleId && o.TrainerId == trainerId && o.IsActive == 1).FirstOrDefaultAsync();

                    if (scheduleTrainer != null)
                    {
                        scheduleTrainer.IsActive = 0;

                        await context.SaveChangesAsync();
                    }

                    var scheduleModerator = await context.EventScheduleModerator.Where(o => o.EventScheduleId == scheduleId && o.IsActive == 1).ToListAsync();

                    foreach (var scheduleMod in scheduleModerator)
                    {
                        scheduleMod.IsActive = 0;

                        await context.SaveChangesAsync();
                    }

                }


                return true;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<List<CurMod>> GetCurMod(int EventScheduleId)
        {
            MySqlConnector.MySqlCommand cmd = new MySqlConnector.MySqlCommand()
            {
                CommandText = "select * from view_current_moderator where EventScheduleID = @EventScheduleId;",
                Connection = Db.connection
            };
            try
            {
                cmd.Parameters.AddWithValue("@EventScheduleId", EventScheduleId);
                await Db.connection.OpenAsync();
                MySqlConnector.MySqlDataReader reader = await cmd.ExecuteReaderAsync();
                List<CurMod> lMod = new List<CurMod>();

                while (await reader.ReadAsync())
                {
                    CurMod mod = new CurMod()
                    {
                        ModeratorId = (int?)(reader.IsDBNull("ModeratorID") ? null : reader["ModeratorID"]),
                        Name = reader.IsDBNull("Name") ? null : reader["Name"].ToString(),
                        Nickname = reader.IsDBNull("NickNameEN") ? null : reader["NickNameEN"].ToString(),
                        lastname = reader.IsDBNull("LastName") ? null : reader["LastName"].ToString(),
                        //isTrainer = (byte)(reader.IsDBNull("IsTrainer") ? (int?)null : reader["IsTrainer"]),
                        //IsActive = (byte)(reader.IsDBNull("IsActive") ? (int?)null : reader["IsActive"]),
                    };
                    lMod.Add(mod);
                }
                await reader.CloseAsync();

                return lMod;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                await Db.connection.CloseAsync();
            }
        }
        public async Task<bool> CheckModAvailable(int modId)
        {
            MySqlConnector.MySqlCommand cmd = new MySqlConnector.MySqlCommand()
            {
                CommandText = "",
                Connection = Db.connection
            };
            bool chkMod = false;
            try
            {
                await Db.connection.OpenAsync();

                return chkMod;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                await Db.connection.CloseAsync();
            }
        }

        #region Check trainer avai
        public async Task<List<TrainerMaster>> checkTrainerAvali(DateTime checkThisDate, string checkStartTime, string checkEndTime, List<TrainerMaster> trainers)
        {
            try
            {
                Console.WriteLine("iCalendar Handle - NRI Project [CORE C# Code]");

                //1) Get All Trainer that can Teach this Content.
                //Do The Query Job or Linq here.
                //Get TrainerName , iCalendar Data that can Teach this Content.
                //allData

                //2.1) Get Date that you need to check isAvailableTime for Traniner.
                //Do get Data from FrontEnds.
                //This is Sample Date that we need to check.
                DateTime checkthisdate = new DateTime(checkThisDate.Year, checkThisDate.Month, checkThisDate.Day);
                //This is Sample Time for check at 20:00 or 8 PM.
                var resStartTime = checkStartTime.Split(":");
                TimeSpan checkstarttime = new TimeSpan(Convert.ToInt32(resStartTime[0]), Convert.ToInt32(resStartTime[1]), Convert.ToInt32(resStartTime[2]));

                var resEndTime = checkEndTime.Split(":");
                TimeSpan checkendtime = new TimeSpan(Convert.ToInt32(resEndTime[0]), Convert.ToInt32(resEndTime[1]), Convert.ToInt32(resEndTime[2]));

                //This is how to create start / end time period for each content
                DateTime checkStartDate = checkthisdate + checkstarttime;
                DateTime checkEndDate = checkthisdate + checkendtime;

                //2.2) Create List of TrainerData and load Data from Database to this List.
                List<TrainerCalendarData> allData = new List<TrainerCalendarData>();

                foreach (var item in trainers)
                {
                    allData.Add(new TrainerCalendarData(item.Name + " " + item.LastName, item.ICalendarLink));
                }
                //This Sample have 3 Teach can do this content from 1).
                //allData.Add(new TrainerCalendarData("Rachadee Chuenphakdee", "https://calendar.google.com/calendar/ical/rachadee%40aquilastudio.net/public/basic.ics"));
                //allData.Add(new TrainerCalendarData("Aquila Studio - Holiday Calendar", "https://calendar.google.com/calendar/ical/c_84ld26njq9lu4mn7bfjp11qt54%40group.calendar.google.com/public/basic.ics"));
                //allData.Add(new TrainerCalendarData("Error Checker", "https://calendar.google.com/calendar/ical/rachadee%40aquilastudiasdfsadfo.net/public/basic.ics"));

                //Create List to Storage Trainer that Available.
                List<String> avaiTrainer = new List<string>();

                //Loop All Trainer Data to this iCalendar Object to Create Data of Available or Not Available.
                Parallel.ForEach(allData, a =>
                {
                    string trainerName = a.getTrainerName();
                    iCalendarObject calendar = new iCalendarObject(a.getTrainerName(), a.getTrainerICalendarURL(), checkthisdate);
                    //Input Date that you need to check in this method.
                    //isAvailableTime will Check all Slot Data of Trainer 
                    if (calendar.isAvailableTime(checkStartDate, checkEndDate))
                    {
                        avaiTrainer.Add(a.getTrainerName());
                    }
                    else
                    {
                        //Do something for Unavai.Trainer
                    }
                });

                //Print all Avai Trainer here
                Console.WriteLine("This Avai. Trainer List : ");
                foreach (String name in avaiTrainer)
                {
                    Console.WriteLine(name);
                }
                Console.WriteLine("End of List.");


                List<TrainerMaster> trainer = new List<TrainerMaster>();

                foreach (var item in avaiTrainer)
                {
                    var name = Regex.Replace(item, "\\s+", ",").Split(",");

                    string firstName = name[0];
                    string lastName = name.Length > 1 ? name[1] : "";

                    var resultFilter = trainers.Where(o => o.Name == firstName.Trim() || o.LastName == lastName.Trim());

                    foreach (var res in resultFilter)
                    {
                        trainer.Add(res);
                    }
                }

                return trainer;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        #endregion
    }
}

class TrainerCalendarData
{
    private String TrainerName;
    private String TrainerICalendarURL;
    public TrainerCalendarData(String TrainerName, String TrainerICalendarURL)
    {
        this.TrainerName = TrainerName;
        this.TrainerICalendarURL = TrainerICalendarURL;
    }
    public String getTrainerName()
    {
        return TrainerName;
    }
    public String getTrainerICalendarURL()
    {
        return TrainerICalendarURL;
    }

}

class iCalendarObject
{
    String iCalendarURL = "";
    public static String tempICSPath = "C:\\tempICS\\";
    String tempICSFileName = "temp.ics";

    int DAY_STAMP = DateTime.Today.Day;
    int MONTH_STAMP = DateTime.Today.Month;
    int YEAR_STAMP = DateTime.Today.Year;

    //Rang of Data collection
    int GMT = 7;

    IICalendarCollection calendars;
    IList<Occurrence> occurrences;

    public iCalendarObject(String Name, String URL, DateTime StartDate, DateTime EndDate)
    {
        this.tempICSFileName = Name + "_" + DAY_STAMP + "_" + MONTH_STAMP + "_" + YEAR_STAMP;
        this.iCalendarURL = URL;
        Boolean isAlreadyGetDataToday = File.Exists(tempICSPath + tempICSFileName);
        if (isAlreadyGetDataToday)
        {
            Console.WriteLine("Already Download ICS File Today of Trainer : " + Name);
            Console.WriteLine("Skip to Prasing Data...");
            //parseICStoSchedule(StartDate, EndDate);
        }
        else
        {
            Console.WriteLine("Start Downloading File of Trainer : " + Name);
            getCalendar();
            Console.WriteLine("Prasing Data...");
            //parseICStoSchedule(StartDate, EndDate);
        }

    }

    public List<TrainerSchedule> parseICStoSchedule(DateTime StartEventDate, DateTime EndEventDate)
    {
        try
        {
            Boolean isExists = File.Exists(tempICSPath + tempICSFileName);
            Console.WriteLine("File exists : " + isExists);

            if (!isExists)
            {
                calendars = iCalendar.LoadFromFile(tempICSPath + "calendar");
            }
            else
            {
                long filesize = new System.IO.FileInfo(tempICSPath + tempICSFileName).Length;

                if (filesize > 10000000)
                {
                    calendars = iCalendar.LoadFromFile(tempICSPath + "calendar");
                }
                else
                {
                    calendars = iCalendar.LoadFromFile(tempICSPath + tempICSFileName);
                    // Console.WriteLine("Show current Time on Server Engine : " + DateTime.Now);
                }
            }
            occurrences = calendars.GetOccurrences(StartEventDate, EndEventDate);
            //printCalendarData();

            List<TrainerSchedule> tList = new List<TrainerSchedule>();
            foreach (Occurrence occ in occurrences)
            {
                if (occ.Period != null)
                {
                    if (occ.Period.StartTime.IsUniversalTime)
                    {
                        GMT = 7;
                    }
                    else
                    {
                        GMT = 0;
                    }

                    TrainerSchedule t = new TrainerSchedule();
                    DateTime occEventStart = occ.Period.StartTime.Local.AddHours(GMT);
                    DateTime occEventEnd = occ.Period.EndTime.Local.AddHours(GMT);

                    t.startDate = occEventStart;
                    t.endDate = occEventEnd;
                    t.contentName = null;
                    t.room = null;
                    t.language = null;
                    t.businessTypeAbbreviation = null;
                    t.contentFormat = null;
                    t.location = null;
                    t.session = null;
                    t.company = null;
                    t.projectName = null;
                    t.bookBy = null;
                    t.remarks = null;
                    t.flag = "BUSY";

                    tList.Add(t);
                }
            }

            return tList;
        }
        catch (Exception e)
        {
            Console.WriteLine("Error in parsing data from temp-location." + e);
            throw e;
        }
    }
    public iCalendarObject(String Name, String URL, DateTime StartDate)
    {
        this.tempICSFileName = Name + "_" + DAY_STAMP + "_" + MONTH_STAMP + "_" + YEAR_STAMP;
        this.iCalendarURL = URL;
        Boolean isAlreadyGetDataToday = File.Exists(tempICSPath + tempICSFileName);
        if (isAlreadyGetDataToday)
        {
            Console.WriteLine("Already Download ICS File Today of Trainer : " + Name);
            Console.WriteLine("Skip to Prasing Data...");
            parseICStoOCC(StartDate);
        }
        else
        {
            Console.WriteLine("Start Downloading File of Trainer : " + Name);
            getCalendar();
            Console.WriteLine("Prasing Data...");
            parseICStoOCC(StartDate);
        }


    }

    public Boolean isAvailableTime(DateTime StartDate, DateTime EndDate)
    {
        Boolean isAvailable = false;
        if (occurrences != null)
        {
            if (occurrences.Count >= 1)
            {
                foreach (Occurrence occ in occurrences)
                {
                    Console.WriteLine("UTC : " + occ.Period.StartTime.IsUniversalTime);
                    if (occ.Period.StartTime.IsUniversalTime)
                    {
                        GMT = 7;
                    }
                    else
                    {
                        GMT = 0;
                    }
                    DateTime occEventStart = occ.Period.StartTime.Local.AddHours(GMT);
                    DateTime occEventEnd = occ.Period.EndTime.Local.AddHours(GMT);

                    Boolean isStartRange = (occEventStart <= StartDate && occEventEnd > StartDate);
                    Boolean isEndInRange = (occEventStart < EndDate && occEventEnd >= EndDate);
                    Boolean isStartEndCoverRange = (occEventStart > StartDate && occEventEnd < EndDate);
                    Boolean isInRange = isStartRange || isEndInRange || isStartEndCoverRange;

                    Console.WriteLine("isInRange of Time(Event/Booking) : (" + occEventStart + " | " + StartDate + ") - (" + occEventEnd + " | " + EndDate + ") : " + isInRange);
                    if (isInRange)
                    {
                        isAvailable = false;
                        Console.WriteLine("Fail to Booking Start : " + StartDate + " End Booking : " + EndDate);
                        Console.WriteLine("Start Busy : " + occEventStart + " End Busy : " + occEventEnd);
                        break;
                    }
                    else
                    {

                    }
                    isAvailable = true;
                }
            }
            else
            {
                isAvailable = true;
            }
        }
        return isAvailable;
    }

    private Boolean getCalendar()
    {
        Boolean isDownloadCompleted = false;
        using (var client = new WebClient())
        {
            try
            {
                client.DownloadFile(iCalendarURL, tempICSPath + tempICSFileName);
                ICSFormateHandle(tempICSPath + tempICSFileName);
                isDownloadCompleted = true;
            }
            catch (Exception)
            {
                Console.WriteLine("Error to get Data from iCalendar URL.");
            }

        }
        return isDownloadCompleted;
    }

    //New Version
    private void parseICStoOCC(DateTime StartEventDate)
    {
        try
        {
            Boolean isExists = File.Exists(tempICSPath + tempICSFileName);
            Console.WriteLine("File exists : " + isExists);

            if (!isExists)
            {
                calendars = iCalendar.LoadFromFile(tempICSPath + "calendar");
            }
            else
            {
                long filesize = new System.IO.FileInfo(tempICSPath + tempICSFileName).Length;

                if (filesize > 10000000)
                {
                    calendars = iCalendar.LoadFromFile(tempICSPath + "calendar");
                }
                else
                {
                    calendars = iCalendar.LoadFromFile(tempICSPath + tempICSFileName);
                    // Console.WriteLine("Show current Time on Server Engine : " + DateTime.Now);
                }
            }



            Console.WriteLine("iCalendar Data of Date : " + StartEventDate);
            occurrences = calendars.GetOccurrences(StartEventDate, StartEventDate.AddDays(1));
            printCalendarData();
        }
        catch (Exception e)
        {
            Console.WriteLine("Error in parsing data from temp-location." + e);
        }

    }

    private void printCalendarData()
    {

        foreach (Occurrence occ in occurrences)
        {
            DateTime occEventStart = occ.Period.StartTime.Local.AddHours(GMT);
            DateTime occEventEnd = occ.Period.EndTime.Local.AddHours(GMT);
            IRecurringComponent rc = occ.Source as IRecurringComponent;
            if (rc != null)
                Console.WriteLine(rc.Summary + ": " + occEventStart.ToShortTimeString() + " to " + occEventEnd.ToShortTimeString());
        }


    }

    private void ICSFormateHandle(string ICSPath)
    {
        int counter = 0;
        string line;
        Boolean isBegin = false;
        int begin_end_counter = 0;
        int start_delete_line = 0;
        int end_delete_line = 0;

        if (File.Exists(ICSPath))
        {
            using (StreamReader file = new StreamReader(ICSPath))
            {
                while ((line = file.ReadLine()) != null)
                {
                    counter++;
                    if (line.StartsWith("BEGIN:VTIMEZONE"))
                    {
                        isBegin = true;
                    }

                    if (line.StartsWith("END:VTIMEZONE") && isBegin == true)
                    {
                        isBegin = false;
                        begin_end_counter = begin_end_counter + 1;
                        if (begin_end_counter == 1)
                        {
                            start_delete_line = counter + 1;
                        }
                    }

                    if (line.StartsWith("BEGIN:VEVENT"))
                    {
                        end_delete_line = counter - 1;
                        break;
                    }
                }
            }
        }


        Console.WriteLine("BEGIN/END STANDARD LOOP : " + begin_end_counter);
        Console.WriteLine("Start Delete Line : " + start_delete_line + " to " + end_delete_line);


        if (end_delete_line - start_delete_line >= 0 && begin_end_counter >= 1)
        {
            // Delete Line & Save here
            string cline = null;
            int line_number = 0;
            using (StreamReader reader = new StreamReader(ICSPath))
            {
                using (StreamWriter writer = new StreamWriter(ICSPath + "_edit"))
                {
                    while ((cline = reader.ReadLine()) != null)
                    {
                        line_number++;

                        if (line_number >= start_delete_line && line_number <= end_delete_line)
                        {
                            Console.WriteLine("Try to delete this Line : " + cline);
                            continue;
                        }


                        writer.WriteLine(cline);
                    }
                }
            }
            System.IO.File.Delete(ICSPath);
            System.IO.File.Move(ICSPath + "_edit", ICSPath);
        }

        Console.WriteLine("Start : TZ Overload Handle");
        string sline = null;
        using (StreamReader reader = new StreamReader(ICSPath))
        {
            using (StreamWriter writer = new StreamWriter(ICSPath + "_edit"))
            {
                while ((sline = reader.ReadLine()) != null)
                {
                    if (sline.Contains(";TZID"))
                    {
                        String new_line = "";
                        //SET PREFIX VALUE
                        foreach (char c in sline)
                        {
                            if (c.Equals(';'))
                            {
                                new_line = new_line + ":";
                                break;
                            }
                            new_line = new_line + c;
                        }
                        //ASSIGN DATE
                        Boolean date_index = false;
                        foreach (char c in sline)
                        {
                            if (date_index)
                            {
                                new_line = new_line + c;
                            }
                            if (c.Equals(':'))
                            {
                                date_index = true;
                            }
                        }
                        sline = new_line;
                    }
                    writer.WriteLine(sline);
                }
            }
        }
        System.IO.File.Delete(ICSPath);
        System.IO.File.Move(ICSPath + "_edit", ICSPath);



    }

}
