using Microsoft.CodeAnalysis.CSharp;
using Microsoft.EntityFrameworkCore;
using Nri_Webapplication_Backend.DTO;
using Nri_Webapplication_Backend.Helpers;
using Nri_Webapplication_Backend.Models;
using Org.BouncyCastle.Math.EC.Multiplier;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Nri_Webapplication_Backend.Managers
{
    public interface IIcsSentMailManager
    {
        Task<List<AutoMailAppoinmentModel>> FindAllEventScheduleAsync(List<int> eventScheduleId, byte IsEmailSend);
        Task<List<AutoMailAppoinmentModel>> FindAllEventScheduleAsync();
        Task<List<EmailLogModel>> GetEmailLog();

        Task<List<EmailLogModel>> GetEmailLog(int EventScheduleID, int TrainerID);
        Task<Boolean> InsertEmailLogAsync(string EventScheduleIdDecrypt, int TrainerID, string IcsUUID, byte IsAccep);

        Task<Boolean> ModifyEmailLogAsync(int EventScheduleIdDecrypt, int TrainerID);
        Task<Boolean> InsertScheduleAcceptedLogAsync(string EventScheduleIdDecrypt, string TrainerDecrypt, int IsAccep, string UUID);


    }
    public class IcsSentMailManager : IIcsSentMailManager
    {
        public ConnectionHelper Db { get; set; }
        public IcsSentMailManager(ConnectionHelper Db)
        {
            this.Db = Db;
        }

        public async Task<List<AutoMailAppoinmentModel>> FindAllEventScheduleAsync(List<int> eventScheduleId, byte IsEmailSend)
        {
            try
            {

                using (var context = new seac_webapplicationContext())
                {

                    var query = (from evs in await context.EventSchedule.ToListAsync()
                                 join cbt in await context.ContentBusinessType.ToListAsync() on evs.ContentBusinessTypeId equals cbt.ContentBusinessTypeId
                                 join ctm in await context.ContentMaster.ToListAsync() on evs.ContentId equals ctm.ContentId
                                 join evr in await context.EventRoomMaster.ToListAsync() on evs.EventRoomId equals evr.EventRoomId
                                 join lmt in await context.LearningTypeMaster.ToListAsync() on evs.LearningTypeId equals lmt.LearningTypeId
                                 join evt in await context.EventScheduleTrainer.ToListAsync() on evs.EventScheduleId equals evt.EventScheduleId
                                 join trm in await context.TrainerMaster.ToListAsync() on evt.TrainerId equals trm.TrainerId
                                 where evs.IsActive == 1
                                    && evt.IsActive == 1
                                    && eventScheduleId.Contains(evs.EventScheduleId)
                                 && evs.IsEmailSent == IsEmailSend
                                 select new AutoMailAppoinmentModel()
                                 {
                                     EventSchedule = evs,
                                     ContentMaster = ctm,
                                     ContentBusinessType = cbt,
                                     EventRoomMaster = evr,
                                     LearningTypeMaster = lmt,
                                     EventScheduleTrainer = evt,
                                     TrainerMaster = trm,
                                     Status = null

                                 }).ToList();


                    foreach (var item in query)
                    {
                        var status = await context.Status.Where(o => o.StatusId == item.EventSchedule.StatusId).FirstOrDefaultAsync();

                        if (status != null)
                        {
                            item.Status = status;
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

        public async Task<List<AutoMailAppoinmentModel>> FindAllEventScheduleAsync()
        {
            try
            {

                using (var context = new seac_webapplicationContext())
                {

                    var query = (from evs in await context.EventSchedule.ToListAsync()
                                 join cbt in await context.ContentBusinessType.ToListAsync() on evs.ContentBusinessTypeId equals cbt.ContentBusinessTypeId
                                 join ctm in await context.ContentMaster.ToListAsync() on evs.ContentId equals ctm.ContentId
                                 join st in await context.Status.ToListAsync() on evs.StatusId equals st.StatusId into tDetail
                                 from st in tDetail.DefaultIfEmpty()
                                 join evr in await context.EventRoomMaster.ToListAsync() on evs.EventRoomId equals evr.EventRoomId
                                 join lmt in await context.LearningTypeMaster.ToListAsync() on evs.LearningTypeId equals lmt.LearningTypeId
                                 join evt in await context.EventScheduleTrainer.ToListAsync() on evs.EventScheduleId equals evt.EventScheduleId
                                 join trm in await context.TrainerMaster.ToListAsync() on evt.TrainerId equals trm.TrainerId

                                 where evs.IsActive == 1
                                        && evt.IsActive == 1
                                        && evs.IsEmailSent == 0
                                 select new AutoMailAppoinmentModel()
                                 {
                                     EventSchedule = evs,
                                     ContentMaster = ctm,
                                     ContentBusinessType = cbt,
                                     EventRoomMaster = evr,
                                     LearningTypeMaster = lmt,
                                     EventScheduleTrainer = evt,
                                     TrainerMaster = trm,
                                     Status = st
                                 }).ToList();


                    return query;


                }
            }


            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<List<EmailLogModel>> GetEmailLog()
        {
            try
            {

                using (var context = new seac_webapplicationContext())
                {

                    var query = (from e in await context.EmailLog.ToListAsync()

                                 select new EmailLogModel()
                                 {
                                     Id = e.Id,
                                     EventScheduleID = e.EventScheduleId,
                                     IsEmailSentSuccess = e.IsEmailSentSuccess,
                                     MailTimeStamp = e.MailTimeStamp,
                                     IsCancel = e.IsCancel,
                                     TrainerID = e.TrainerId,
                                     UUID = e.Uuid
                                 }).ToList();


                    return query;


                }
            }


            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<List<EmailLogModel>> GetEmailLog(int EventScheduleID, int TrainerID)
        {
            try
            {

                using (var context = new seac_webapplicationContext())
                {

                    var query = (from e in await context.EmailLog.ToListAsync()
                                 where e.EventScheduleId == EventScheduleID && e.TrainerId == TrainerID
                                 select new EmailLogModel()
                                 {
                                     Id = e.Id,
                                     EventScheduleID = e.EventScheduleId,
                                     IsEmailSentSuccess = e.IsEmailSentSuccess,
                                     MailTimeStamp = e.MailTimeStamp,
                                     IsCancel = e.IsCancel,
                                     TrainerID = e.TrainerId,
                                     UUID = e.Uuid
                                 }).ToList();


                    return query;


                }
            }


            catch (Exception ex)
            {
                throw ex;
            }
        }
        public async Task<Boolean> InsertEmailLogAsync(string EventScheduleIdDecrypt, int TrainerID, string IcsUUID, byte IsAccep)
        {
            try
            {
                using (var context = new seac_webapplicationContext())
                {
                    var emailLog = new EmailLog
                    {

                        EventScheduleId = int.Parse(EventScheduleIdDecrypt),
                        IsEmailSentSuccess = IsAccep,
                        Uuid = IcsUUID,
                        TrainerId = TrainerID,
                        IsCancel = 0,
                        MailTimeStamp = DateTime.Now
                    };

                    await context.AddAsync(emailLog);

                    await context.SaveChangesAsync();

                    // Update table EventSchedule
                    var schedule = await context.EventSchedule.Where(o => o.EventScheduleId == int.Parse(EventScheduleIdDecrypt)).FirstOrDefaultAsync();

                    if (schedule != null)
                    {
                        schedule.IsEmailSent = 1;


                        await context.SaveChangesAsync();
                    }


                    // Update table EventSchedule Trainer


                }

                return true;

            }
            catch (Exception ex)
            {
                this.Db.connection.Close();
                throw ex;
            }
        }


        public async Task<Boolean> ModifyEmailLogAsync(int EventScheduleId, int TrainerID)
        {
            try
            {
                using (var context = new seac_webapplicationContext())
                {

                    // Update table EventSchedule
                    var EmailLog = await context.EmailLog.Where(o => o.EventScheduleId == EventScheduleId && o.TrainerId == TrainerID && o.IsCancel == 0).FirstOrDefaultAsync();

                    if (EmailLog != null)
                    {
                        EmailLog.IsCancel = 1;


                        await context.SaveChangesAsync();
                    }



                }

                return true;

            }
            catch (Exception ex)
            {
                this.Db.connection.Close();
                throw ex;
            }
        }




        public async Task<Boolean> InsertScheduleAcceptedLogAsync(string EventScheduleIdDecrypt, string TrainerDecrypt, int IsAccept, string UUID)
        {
            try
            {
                using (var context = new seac_webapplicationContext())
                {


                    var schedule = await context.EventScheduleTrainer.Where(o => o.EventScheduleId == int.Parse(EventScheduleIdDecrypt) && o.IsActive == 1).FirstOrDefaultAsync();

                    schedule.IsTrainerReply = 1;


                    var scheduleLog = await context.ScheduleAcceptedLog.Where(o => o.EventScheduleId == int.Parse(EventScheduleIdDecrypt) && o.TrainerId == int.Parse(TrainerDecrypt) && o.Uuid == UUID).FirstOrDefaultAsync();

                    var EmailLog = await context.EmailLog.Where(o => o.TrainerId == int.Parse(TrainerDecrypt) && o.Uuid == UUID && o.IsCancel == 1).FirstOrDefaultAsync();

                    if (scheduleLog != null)
                    {
                        return false;
                    }

                    var scheduleAcceptedLog = new ScheduleAcceptedLog
                    {

                        EventScheduleId = int.Parse(EventScheduleIdDecrypt),
                        TrainerId = int.Parse(TrainerDecrypt),
                        IsAccepted = (byte)IsAccept,
                        Uuid = UUID
                    };

                    await context.AddAsync(scheduleAcceptedLog);

                    await context.SaveChangesAsync();
                }

                return true;

            }
            catch (Exception ex)
            {
                this.Db.connection.Close();
                throw ex;
            }
        }




    }
}
