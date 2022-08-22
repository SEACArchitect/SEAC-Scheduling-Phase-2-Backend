using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.EntityFrameworkCore;

using Nri_Webapplication_Backend.DTO;
using Nri_Webapplication_Backend.Models;

namespace Nri_Webapplication_Backend.Managers
{
    public interface IMyworkManager
    {
        Task<List<MyworkModel>> FindTrainerAsync(int trainerID, DateTime startDateTime, DateTime endDateTime, int isTrainer);
    }
    public class MyworkManager : IMyworkManager
    {
        public MyworkManager()
        {

        }

        public async Task<List<MyworkModel>> FindTrainerAsync(int trainerID, DateTime startDateTime, DateTime endDateTime, int isTrainer)
        {
            try
            {
                using (var context = new seac_webapplicationContext())
                {
                    IQueryable<int> eventIds;
                    if (isTrainer == 1)
                    {
                        eventIds = context.EventScheduleTrainer.Where(x => x.IsActive == 1)
                                                    .Where(x => x.TrainerId == trainerID)
                                                    .Select(x => x.EventScheduleId);
                    }
                    else
                    {
                        eventIds = context.EventScheduleModerator.Where(x => x.IsActive == 1)
                                                    .Where(x => x.ModeratorId == trainerID)
                                                    .Select(x => x.EventScheduleId);
                    }
                    var events = (from evs in context.EventSchedule.Where(o => eventIds.Contains(o.EventScheduleId))
                                    join evm in context.EventRoomMaster
                                    on evs.EventRoomId equals evm.EventRoomId
                                    join ct in context.ContentMaster
                                    on evs.ContentId equals ct.ContentId
                                    join lang in context.Language
                                    on evs.LanguageId equals lang.LanguageId
                                    join ctbt in context.ContentBusinessType
                                    on evs.ContentBusinessTypeId equals ctbt.ContentBusinessTypeId
                                    join ctfm in context.ContentFormatMaster
                                    on evs.ContentFormatId equals ctfm.ContentFormatId
                                    join tnm in context.TrainerMaster
                                    on trainerID equals tnm.TrainerId
                                    where evs.StartTime >= startDateTime && evs.EndTime <= endDateTime
                                    select new MyworkModel()
                                    {
                                        EventScheduleID = evs.EventScheduleId,
                                        NickNameEN = tnm.NickNameEn,
                                        NickNameTH = tnm.NickNameTh,
                                        FullName = String.Format("{0} {1}", tnm.Name, tnm.LastName),
                                        FirstName = tnm.Name,
                                        LastName = tnm.LastName,
                                        RoomName = evm.EventRoomName,
                                        ContentID = ct.ContentId,
                                        StartTime = evs.StartTime,
                                        EndTime = evs.EndTime,
                                        Language = lang.LanguageName,
                                        isActiveSchedule = evs.IsActive,
                                        ContentBusinessTypeName = ctbt.ContentBusinessTypeName,
                                        ContentFormantName = ctfm.ContentFormatName,
                                        Location = evs.Location,
                                        Session = evs.Session,
                                        Company = evs.Company,
                                        ProjectName = evs.ProjectName,
                                        CreatedBy = evs.CreatedBy,
                                        Remark = evs.Remark,
                                        Link = evs.Link,
                                        isTrainer = tnm.IsTrainer,
                                        BookBy = evs.BookBy,
                                        statusID = evs.StatusId,
                                    }).ToList();
                    foreach (var e in events)
                    {
                        if (e.statusID != null)
                        {
                            var resultStatus = context.Status.Where(o => o.StatusId == e.statusID).FirstOrDefault();
                            e.status = resultStatus.Status1;
                        }
                        var getContent = context.ContentMaster.Where(o => o.ContentId == e.ContentID).FirstOrDefault();
                        e.ContentName = getContent.ContentName;
                        var activeTrainers = (from est in context.EventScheduleTrainer
                                                join tnm in context.TrainerMaster
                                                on est.TrainerId equals tnm.TrainerId
                                                where est.EventScheduleId == e.EventScheduleID && est.IsActive == 1
                                                select new TrainerWork()
                                                {
                                                    ID = tnm.TrainerId,
                                                    isTrainer = tnm.IsTrainer,
                                                    Telephone = tnm.Telephone,
                                                    FirstName = tnm.Name,
                                                    LastName = tnm.LastName,
                                                }).ToList();
                        foreach (var trainer in activeTrainers)
                        {
                            e.trainer.Add(trainer);
                        }
                        var activeModerators = (from evm in context.EventScheduleModerator
                                                join tnm in context.TrainerMaster
                                                on evm.ModeratorId equals tnm.TrainerId
                                                where evm.EventScheduleId == e.EventScheduleID && evm.IsActive == 1
                                                select new TrainerWork()
                                                {
                                                    ID = tnm.TrainerId,
                                                    isTrainer = tnm.IsTrainer,
                                                    Telephone = tnm.Telephone,
                                                    FirstName = tnm.Name,
                                                    LastName = tnm.LastName
                                                }).ToList();
                        foreach (var moderator in activeModerators)
                        {
                            e.trainer.Add(moderator);
                        }
                    }
                    return events;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
