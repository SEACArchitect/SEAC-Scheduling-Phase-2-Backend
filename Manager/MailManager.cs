using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

using Nri_Webapplication_Backend.DTO;
using Nri_Webapplication_Backend.Models;

namespace Nri_Webapplication_Backend.Managers
{
    public interface IMailManager
    {
        Task<List<MailModel>> FindAllAsync();
    }
    public class MailManager : IMailManager
    {
        public MailManager()
        {

        }

        public async Task<List<MailModel>> FindAllAsync()
        {
            try
            {
                using (var context = new seac_webapplicationContext())
                {
                    List<MailModel> mails = new List<MailModel>();
                    var events = await context.EventSchedule.Where(o => o.IsActive == 1 && o.IsEmailSent == 0).ToListAsync();
                    foreach (var e in events)
                    {
                        var eventActiveTrainer  = await context.EventScheduleTrainer.Where(o => o.IsActive == 1 && o.EventScheduleId == e.EventScheduleId).FirstOrDefaultAsync();
                        if (eventActiveTrainer  != null)
                        {
                            var eventActiveMods  = await context.EventScheduleModerator.Where(o => o.IsActive == 1 && o.EventScheduleId == e.EventScheduleId).ToListAsync();
                            string trainerName = null;
                            if (eventActiveTrainer  != null)
                            {
                                var trainer = await context.TrainerMaster.Where(o => o.IsActive == 1 && o.TrainerId == eventActiveTrainer.TrainerId && o.IsTrainer == 1).FirstOrDefaultAsync();
                                trainerName = trainer != null ? String.Format("{0} {1}", trainer.Name, trainer.LastName) : null;
                            }
                            List<string> moderatorNames = new List<string>();
                            foreach (var eventMod in eventActiveMods)
                            {
                                var moderator = await context.TrainerMaster.Where(o => o.IsActive == 1 && o.TrainerId == eventMod.ModeratorId && o.IsTrainer == 0).FirstOrDefaultAsync();
                                moderatorNames.Add(moderator != null ? String.Format("{0} {1}", moderator.Name, moderator.LastName) : null);
                            }
                            var content = await context.ContentMaster.Where(o => o.ContentId == e.ContentId).FirstOrDefaultAsync();
                            mails.Add(new MailModel()
                            {
                                ScheduleId = e.EventScheduleId,
                                Schedule = content.ContentName,
                                StartTime = e.StartTime,
                                EndTime = e.EndTime,
                                Trainer = trainerName,
                                Moderator = moderatorNames,
                                Link = e != null ? e.Link : null,
                                CreatedDate = e.CreatedDate,
                                UpdatedDate = e.UpdatedDate,
                            });
                        }
                    }
                    return mails;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
