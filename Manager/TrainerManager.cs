using Microsoft.AspNetCore.Mvc.Formatters.Xml;
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
using SkillDto = Nri_Webapplication_Backend.DTO.Skill;
using Skill = Nri_Webapplication_Backend.Models.Skill;
using TrainerType = Nri_Webapplication_Backend.Models.TrainerType;
using Type = Nri_Webapplication_Backend.Models.Type;
using Microsoft.CodeAnalysis.CSharp;
using Org.BouncyCastle.Ocsp;
using System.Runtime.InteropServices;

namespace Nri_Webapplication_Backend.Managers
{
    public interface ITrainerManager
    {
        //Changed !!!!!!!!!
        Task<List<RoomType>> FindEventRoomTypeAsync();
        Task<List<Skill>> FindSkillAsync();
        Task<List<TrainerType>> FindTrainerAsync();
        Task<List<ContractType>> FindContractTypeAsync();

        Task<Boolean> InsertAsync(TrainerModel entity);
        Task<List<TrainerModel>> FindAllAsync();
        Task<Boolean> UpdateAsync(TrainerModel entity, int id);
        Task<List<TrainerModel>> FindContacType(int contactTypeID);

        Task<TrainerMaster> FindChkTrainer(string username);
    }
    public class TrainerManager : ITrainerManager
    {
        public ConnectionHelper Db { get; set; }
        public TrainerManager(ConnectionHelper Db)
        {
            this.Db = Db;
        }

        public async Task<List<RoomType>> FindEventRoomTypeAsync()
        {
            try
            {
                using (var context = new seac_webapplicationContext())
                {

                    var query = (from ert in await context.EventRoomType.ToListAsync()
                                 select new RoomType()
                                 {
                                     id = ert.EventRoomTypeId,
                                     value = ert.EventRoomTypeName
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

        public async Task<List<Skill>> FindSkillAsync()
        {
            try
            {
                using (var context = new seac_webapplicationContext())
                {

                    var query = (from sk in await context.Skill.ToListAsync()
                                 select new Skill()
                                 {
                                     id = sk.SkillId,
                                     value = sk.SkillName
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

        public async Task<List<TrainerType>> FindTrainerAsync()
        {
            try
            {
                using (var context = new seac_webapplicationContext())
                {

                    var query = (from tn in await context.TrainerType.ToListAsync()
                                 select new TrainerType()
                                 {
                                     id = tn.TrainerTypeId,
                                     value = tn.TypeName
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

        public async Task<List<ContractType>> FindContractTypeAsync()
        {
            try
            {
                using (var context = new seac_webapplicationContext())
                {

                    var query = (from tct in await context.TrainerContractType.ToListAsync()
                                 select new ContractType()
                                 {
                                     id = tct.TraninerContractTypeId,
                                     value = tct.TraninerContractTypeName
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
        public async Task<Boolean> InsertAsync(TrainerModel entity)
        {
            try
            {
                using (var context = new seac_webapplicationContext())
                {
                    var trainerMaster = new TrainerMaster
                    {
                        Name = entity.name.Trim(),
                        LastName = !string.IsNullOrWhiteSpace(entity.lastname) ? entity.lastname.Trim() : entity.lastname,
                        NickNameEn = !string.IsNullOrWhiteSpace(entity.nicknameEN) ? entity.nicknameEN.Trim() : entity.nicknameEN,
                        NickNameTh = !string.IsNullOrWhiteSpace(entity.nicknameTH) ? entity.nicknameTH.Trim() : entity.nicknameTH,
                        Telephone = !string.IsNullOrWhiteSpace(entity.tel) ? entity.tel.Trim() : entity.tel,
                        Email = entity.email.Trim(),
                        ICalendarLink = !string.IsNullOrWhiteSpace(entity.iCalendarLink) ? entity.iCalendarLink.Trim() : entity.iCalendarLink,
                        TrainerTypeId = entity.type.id,
                        TraninerContractTypeId = entity.contractType.id,
                        IsActive = entity.isActive,
                        IsTrainer = entity.isTrainer,
                        MaximumEventPerMonth = entity.maxEventPerMonth,
                        CreatedDate = DateTime.Now,
                        CreatedBy = entity.createdBy

                    };

                    //Insert trainer master
                    await context.AddAsync(trainerMaster);

                    await context.SaveChangesAsync();


                    if (entity.isTrainer == 0)
                    {

                        if (entity.businessType != null)
                        {
                            foreach (var result in entity.businessType)
                            {
                                var businessType = new ModeratorSupportBusinessType
                                {
                                    ModeratorId = trainerMaster.TrainerId,
                                    ContentBusinessTypeId = result.Id,
                                    IsActive = entity.isActive,
                                    CreatedDate = DateTime.Now,
                                    CreatedBy = entity.createdBy
                                };

                                //Insert moderatorSuppoerBusinessType
                                await context.AddAsync(businessType);
                            }
                        }

                        if (entity.skill != null)
                        {

                            foreach (var result in entity.skill)
                            {
                                var skill = new ModeratorSkill
                                {
                                    ModeratorId = trainerMaster.TrainerId,
                                    SkillId = result.id,
                                    IsActive = entity.isActive,
                                    CreatedDate = DateTime.Now,
                                    CreatedBy = entity.createdBy

                                };

                                //Insert ModeratorSkill
                                await context.AddAsync(skill);
                            }
                        }

                        if (entity.roomType != null)
                        {

                            foreach (var result in entity.roomType)
                            {
                                var roomType = new ModeratorSupportRoomType
                                {
                                    ModeratorId = trainerMaster.TrainerId,
                                    EventRoomTypeId = result.id,
                                    IsActive = entity.isActive,
                                    CreatedDate = DateTime.Now,
                                    CreatedBy = entity.createdBy

                                };

                                //Insert ModeratorSkill
                                await context.AddAsync(roomType);

                            }
                        }

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
        public async Task<List<TrainerModel>> FindAllAsync()
        {
            try
            {
                using (var context = new seac_webapplicationContext())
                {

                    var query = (from ert in await context.TrainerMaster.ToListAsync()
                                 join type in await context.TrainerType.ToListAsync()
                                    on ert.TrainerTypeId equals type.TrainerTypeId
                                 join ctt in await context.TrainerContractType.ToListAsync()
                                    on ert.TraninerContractTypeId equals ctt.TraninerContractTypeId
                                 select new TrainerModel()
                                 {
                                     id = ert.TrainerId,
                                     name = ert.Name,
                                     lastname = ert.LastName,
                                     nicknameTH = ert.NickNameTh,
                                     nicknameEN = ert.NickNameEn,
                                     tel = ert.Telephone,
                                     email = ert.Email,
                                     isActive = ert.IsActive,
                                     iCalendarLink = ert.ICalendarLink,
                                     isTrainer = ert.IsTrainer,
                                     maxEventPerMonth = ert.MaximumEventPerMonth,
                                     contractType = new ContractType
                                     {
                                         id = ctt.TraninerContractTypeId,
                                         value = ctt.TraninerContractTypeName
                                     },
                                     type = new TrainerType
                                     {
                                         id = type.TrainerTypeId,
                                         value = type.TypeName
                                     }


                                 }).ToList();

                    foreach (var result in query)
                    {
                        var modSupTypes = await context.ModeratorSupportBusinessType.Where(o => o.ModeratorId == result.id).ToListAsync();

                        foreach (var modSupType in modSupTypes)
                        {
                            var contentBusinessType = await context.ContentBusinessType.Where(o => o.ContentBusinessTypeId == modSupType.ContentBusinessTypeId).FirstOrDefaultAsync();

                            if (contentBusinessType != null)
                            {
                                result.businessType.Add(new BusinessType
                                {
                                    Id = contentBusinessType.ContentBusinessTypeId,
                                    Value = contentBusinessType.ContentBusinessTypeName
                                });
                            }
                            else
                            {
                                result.businessType = null;
                            }
                        }



                        var modeSkills = await context.ModeratorSkill.Where(o => o.ModeratorId == result.id).ToListAsync();



                        foreach (var modeSkill in modeSkills)
                        {
                            var skill = await context.Skill.Where(o => o.SkillId == modeSkill.SkillId).FirstOrDefaultAsync();

                            if (skill != null)
                            {
                                result.skill.Add(new Skill
                                {
                                    id = skill.SkillId,
                                    value = skill.SkillName
                                });
                            }
                            else
                            {
                                result.skill = null;
                            }
                        }



                        var roomTypes = await context.ModeratorSupportRoomType.Where(o => o.ModeratorId == result.id).ToListAsync();

                        foreach (var roomType in roomTypes)
                        {
                            var type = await context.EventRoomType.Where(o => o.EventRoomTypeId == roomType.EventRoomTypeId).FirstOrDefaultAsync();

                            if (type != null)
                            {
                                result.roomType.Add(new RoomType
                                {
                                    id = type.EventRoomTypeId,
                                    value = type.EventRoomTypeName
                                });
                            }
                            else
                            {
                                result.roomType = null;
                            }
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

        public async Task<Boolean> UpdateAsync(TrainerModel entity, int id)
        {
            try
            {
                using (var context = new seac_webapplicationContext())
                {

                    var trainerMaster = await context.TrainerMaster.Where(o => o.TrainerId == id).FirstOrDefaultAsync();

                    trainerMaster.Name = entity.name.Trim();
                    trainerMaster.LastName = !string.IsNullOrWhiteSpace(entity.lastname) ? entity.lastname.Trim() : entity.lastname;
                    trainerMaster.NickNameTh = !string.IsNullOrWhiteSpace(entity.nicknameTH) ? entity.nicknameTH.Trim() : entity.nicknameTH;
                    trainerMaster.NickNameEn = !string.IsNullOrWhiteSpace(entity.nicknameEN) ? entity.nicknameEN.Trim() : entity.nicknameEN;
                    trainerMaster.Telephone = !string.IsNullOrWhiteSpace(entity.tel) ? entity.tel.Trim() : entity.tel;
                    trainerMaster.Email = entity.email.Trim();
                    trainerMaster.TrainerTypeId = entity.type.id;
                    trainerMaster.MaximumEventPerMonth = entity.maxEventPerMonth;
                    trainerMaster.TraninerContractTypeId = entity.contractType.id;
                    trainerMaster.MaximumEventPerMonth = entity.maxEventPerMonth;
                    trainerMaster.IsTrainer = entity.isTrainer;
                    trainerMaster.IsActive = entity.isActive;
                    trainerMaster.ICalendarLink = !string.IsNullOrWhiteSpace(entity.iCalendarLink) ? entity.iCalendarLink.Trim() : entity.iCalendarLink;

                    await context.SaveChangesAsync();

                    var businessTypes = await context.ModeratorSupportBusinessType.Where(o => o.ModeratorId == id).ToListAsync();

                    if (businessTypes != null)
                    {
                        foreach (var result in businessTypes)
                        {
                            context.Remove(result);

                            await context.SaveChangesAsync();
                        }
                    }

                    if (entity.businessType != null)
                    {
                        foreach (var result in entity.businessType)
                        {
                            //var query = await context.ModeratorSupportBusinessType.Where(o => o.ContentBusinessTypeId == result.Id).FirstOrDefaultAsync();

                            //if (query != null)
                            //{
                            var businessType = new ModeratorSupportBusinessType
                            {
                                ModeratorId = trainerMaster.TrainerId,
                                ContentBusinessTypeId = result.Id,
                                IsActive = entity.isActive,
                                CreatedDate = DateTime.Now,
                                CreatedBy = entity.createdBy
                            };

                            //Insert moderatorSuppoerBusinessType
                            await context.AddAsync(businessType);

                            await context.SaveChangesAsync();
                            //}


                        }
                    }

                    var moderatorSkills = await context.ModeratorSkill.Where(o => o.ModeratorId == id).ToListAsync();

                    if (moderatorSkills != null)
                    {
                        foreach (var result in moderatorSkills)
                        {
                            context.Remove(result);

                            await context.SaveChangesAsync();
                        }
                    }

                    if (entity.skill != null)
                    {
                        foreach (var result in entity.skill)
                        {

                            //var query = await context.ModeratorSkill.Where(o => o.SkillId == result.id).FirstOrDefaultAsync();

                            //if (query != null)
                            //{
                            var skill = new ModeratorSkill
                            {
                                ModeratorId = trainerMaster.TrainerId,
                                SkillId = result.id,
                                IsActive = entity.isActive,
                                CreatedDate = DateTime.Now,
                                CreatedBy = entity.createdBy

                            };

                            //Insert ModeratorSkill
                            await context.AddAsync(skill);

                            await context.SaveChangesAsync();
                            // }
                        }
                    }

                    var ModeratorSupportRoomTypes = await context.ModeratorSupportRoomType.Where(o => o.ModeratorId == id).ToListAsync();

                    if (ModeratorSupportRoomTypes != null)
                    {
                        foreach (var result in ModeratorSupportRoomTypes)
                        {
                            context.Remove(result);

                            await context.SaveChangesAsync();
                        }
                    }

                    if (entity.roomType != null)
                    {

                        foreach (var result in entity.roomType)
                        {

                            var query = await context.ModeratorSupportRoomType.Where(o => o.EventRoomTypeId == result.id).FirstOrDefaultAsync();

                            if (query != null)
                            {
                                var roomType = new ModeratorSupportRoomType
                                {
                                    ModeratorId = trainerMaster.TrainerId,
                                    EventRoomTypeId = query.EventRoomTypeId,
                                    IsActive = entity.isActive,
                                    CreatedDate = DateTime.Now,
                                    CreatedBy = entity.createdBy

                                };

                                //Insert ModeratorSkill
                                await context.AddAsync(roomType);

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

        public async Task<List<TrainerModel>> FindContacType(int contactTypeID)
        {
            try
            {
                using (var context = new seac_webapplicationContext())
                {

                    var query = (from ert in await context.TrainerMaster.ToListAsync()
                                 join ctt in await context.TrainerContractType.ToListAsync()
                                    on ert.TraninerContractTypeId equals ctt.TraninerContractTypeId
                                 where ctt.TraninerContractTypeId == contactTypeID
                                 select new TrainerModel()
                                 {
                                     id = ert.TrainerId,
                                     name = ert.Name,
                                     lastname = ert.LastName,
                                     nicknameTH = ert.NickNameTh,
                                     nicknameEN = ert.NickNameEn,
                                     tel = ert.Telephone,
                                     email = ert.Email,
                                     isActive = ert.IsActive,
                                     iCalendarLink = ert.ICalendarLink,
                                     isTrainer = ert.IsTrainer,
                                     maxEventPerMonth = ert.MaximumEventPerMonth,
                                     contractType = new ContractType
                                     {
                                         id = ctt.TraninerContractTypeId,
                                         value = ctt.TraninerContractTypeName
                                     }


                                 }).ToList();



                    return query;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<TrainerMaster> FindChkTrainer(string username)
        {
            try
            {
                using (var context = new seac_webapplicationContext())
                {
                    var query = (from tm in await context.TrainerMaster.ToListAsync()
                                 where tm.Email == username
                                 select new TrainerMaster()
                                 {
                                     TrainerId = tm.TrainerId,
                                     IsTrainer = tm.IsTrainer

                                 });

                    if (query == null)
                    {
                        return null;
                    }

                    return query.FirstOrDefault();

                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
