using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using MySqlConnector;
using Nri_Webapplication_Backend.DTO;
using Nri_Webapplication_Backend.Helpers;
using Nri_Webapplication_Backend.Models;
using Org.BouncyCastle.Crypto.Signers;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using ContentBusinessType = Nri_Webapplication_Backend.Models.ContentBusinessType;

namespace Nri_Webapplication_Backend.Managers
{
    public interface IContentManager
    {
        Task<List<LearningTypeModel>> FindLearningTypeAsync();
        Task<List<ContentBusinessType>> FindContentBusinessTypeAsync();
        Task<List<ContentFormat>> FindContentFormatAsync();
        Task<Boolean> InsertAsync(ContentMasterModel entity);

        Task<List<ContentMasterModel>> FindAsync();
        Task<ContentMasterModel> FindByIDAsync(int contentID);
        Task<Boolean> UpdateAsync(ContentMasterModel entity, int contentID);
        Task<Boolean> DeleteAsync(ContentMasterModel entity, int contentID);
        Task<List<TrainerList>> FindTrainerAsync();


    }
    public class ContentManager : IContentManager
    {
        public ConnectionHelper Db { get; set; }
        public ContentManager(ConnectionHelper Db)
        {
            this.Db = Db;
        }

        //Get all data learning type 
        public async Task<List<LearningTypeModel>> FindLearningTypeAsync()
        {
            try
            {

                using (var context = new seac_webapplicationContext())
                {

                    var query = (from ltm in await context.LearningTypeMaster.ToListAsync()
                                 select new LearningTypeModel()
                                 {
                                     id = ltm.LearningTypeId,
                                     value = ltm.LearningTypeName
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

        //Get all data content business type
        public async Task<List<ContentBusinessType>> FindContentBusinessTypeAsync()
        {
            try
            {
                using (var context = new seac_webapplicationContext())
                {

                    var query = (from cbt in await context.ContentBusinessType.ToListAsync()
                                 select new ContentBusinessType()
                                 {
                                     id = cbt.ContentBusinessTypeId,
                                     value = cbt.ContentBusinessTypeName,
                                     name = cbt.ContentBusinessTypeAbbreviate

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

        //Get all data content format
        public async Task<List<ContentFormat>> FindContentFormatAsync()
        {
            try
            {

                try
                {
                    using (var context = new seac_webapplicationContext())
                    {

                        var query = (from cbt in await context.ContentFormatMaster.ToListAsync()
                                     select new ContentFormat()
                                     {
                                         Id = cbt.ContentFormatId,
                                         Value = cbt.ContentFormatName
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
            catch (Exception ex)
            {
                this.Db.connection.Close();
                throw ex;
            }
        }

        //Create content master
        public async Task<Boolean> InsertAsync(ContentMasterModel entity)
        {
            try
            {

                using (var context = new seac_webapplicationContext())
                {
                    var contentMaster = new ContentMaster
                    {
                        ContentCode = entity.Code,
                        ContentName = entity.Name,
                        DisplayName = entity.DisplayName,
                        OriginalContentName = entity.OriginalName,
                        Duration = entity.Duration,
                        IsActive = entity.IsActive,
                        PaxMax = entity.PaxMax,
                        IsInternal = entity.IsInternal,
                        IsPrivillege = entity.IsPrivillege,
                        LearningTypeId = entity.LearningType.Id,
                        ContentBusinessTypeId = entity.BusinessType.Id,
                        CreatedDate = DateTime.Now,
                        CreatedBy = entity.createdBy,
                        OutLineId = entity.OutLineId,
                        CourseId = entity.CourseId,
                        CourseTitle = entity.CourseTitle
                    };

                    await context.AddAsync(contentMaster);

                    await context.SaveChangesAsync();


                    if (entity.TrainerList != null)
                    {
                        foreach (var result in entity.TrainerList)
                        {

                            var certifiedTrainer = new CertifiedTrainer()
                            {
                                ContentId = contentMaster.ContentId,
                                TrainerId = result.Id,
                                IsActive = entity.IsActive,
                                CreatedDate = DateTime.Now,
                                CreatedBy = entity.createdBy

                            };

                            await context.AddAsync(certifiedTrainer);


                        }

                    }

                    if (entity.ContentFormat != null)
                    {
                        foreach (var result in entity.ContentFormat)
                        {

                            var certifiedTrainer = new ContentFormatVariety()
                            {
                                ContentId = contentMaster.ContentId,
                                ContentFormatId = result.Id

                            };

                            await context.AddAsync(certifiedTrainer);



                        }

                    }

                    await context.SaveChangesAsync();
                    return true;
                }

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<List<ContentMasterModel>> FindAsync()
        {
            try
            {

                using (var context = new seac_webapplicationContext())
                {

                    var contentMaster = new List<ContentMasterModel>();

                    var query = (from ctm in await context.ContentMaster.ToListAsync()
                                 join ltm in await context.LearningTypeMaster.ToListAsync()
                                 on ctm.LearningTypeId equals ltm.LearningTypeId
                                 join cbt in await context.ContentBusinessType.ToListAsync()
                                 on ctm.ContentBusinessTypeId equals cbt.ContentBusinessTypeId
                                 select new ContentMasterModel()
                                 {
                                     Id = ctm.ContentId,
                                     Code = ctm.ContentCode,
                                     Name = ctm.ContentName,
                                     DisplayName = ctm.DisplayName,
                                     OriginalName = ctm.OriginalContentName,
                                     Duration = ctm.Duration,
                                     IsActive = ctm.IsActive,
                                     PaxMax = ctm.PaxMax,
                                     IsInternal = ctm.IsInternal,
                                     IsPrivillege = ctm.IsPrivillege,
                                     LearningType = new LearningType()
                                     {
                                         Id = ltm.LearningTypeId,
                                         Value = ltm.LearningTypeName
                                     },
                                     BusinessType = new BusinessType()
                                     {
                                         Id = cbt.ContentBusinessTypeId,
                                         Value = cbt.ContentBusinessTypeName
                                     },
                                     createdDate = ctm.CreatedDate,
                                     createdBy = ctm.CreatedBy,
                                     OutLineId = ctm.OutLineId,
                                     CourseId = ctm.CourseId,
                                     CourseTitle = ctm.CourseTitle


                                 }).ToList();


                    foreach (var result in query)
                    {
                        var varity = await context.CertifiedTrainer.Where(o => o.ContentId == result.Id).ToListAsync();

                        foreach (var resultVarity in varity)
                        {

                            var trainer = await context.TrainerMaster.Where(o => o.TrainerId == resultVarity.TrainerId).FirstOrDefaultAsync();

                            if (trainer != null)
                            {
                                result.TrainerList.Add(new TrainerList
                                {
                                    Id = trainer.TrainerId,
                                    Value = trainer.Name + " " + trainer.LastName
                                });
                            }
                        }

                        var contentFormatVaritety = await context.ContentFormatVariety.Where(o => o.ContentId == result.Id).ToListAsync();

                        foreach (var resultContentFormatVariety in contentFormatVaritety)
                        {

                            var contentFormatMaster = await context.ContentFormatMaster.Where(o => o.ContentFormatId == resultContentFormatVariety.ContentFormatId).FirstOrDefaultAsync();

                            if (contentFormatMaster != null)
                            {
                                result.ContentFormat.Add(new ContentFormat
                                {
                                    Id = contentFormatMaster.ContentFormatId,
                                    Value = contentFormatMaster.ContentFormatName
                                });
                            }
                        }




                    }


                    return query;
                }

            }
            catch (Exception ex)
            {
                this.Db.connection.Close();
                throw ex;
            }
        }

        public async Task<ContentMasterModel> FindByIDAsync(int contentID)
        {
            try
            {

                using (var context = new seac_webapplicationContext())
                {

                    var query = (from ctm in await context.ContentMaster.ToListAsync()
                                 join ltm in await context.LearningTypeMaster.ToListAsync()
                                 on ctm.LearningTypeId equals ltm.LearningTypeId
                                 join cbt in await context.ContentBusinessType.ToListAsync()
                                 on ctm.ContentBusinessTypeId equals cbt.ContentBusinessTypeId
                                 where ctm.ContentId == contentID
                                 select new ContentMasterModel()
                                 {
                                     Id = ctm.ContentId,
                                     Code = ctm.ContentCode,
                                     Name = ctm.ContentName,
                                     DisplayName = ctm.DisplayName,
                                     OriginalName = ctm.OriginalContentName,
                                     Duration = ctm.Duration,
                                     IsActive = ctm.IsActive,
                                     PaxMax = ctm.PaxMax,
                                     IsInternal = ctm.IsInternal,
                                     IsPrivillege = ctm.IsPrivillege,
                                     LearningType = new LearningType()
                                     {
                                         Id = ltm.LearningTypeId,
                                         Value = ltm.LearningTypeName
                                     },
                                     BusinessType = new BusinessType()
                                     {
                                         Id = cbt.ContentBusinessTypeId,
                                         Value = cbt.ContentBusinessTypeName
                                     },
                                     createdDate = ctm.CreatedDate,
                                     createdBy = ctm.CreatedBy


                                 }).FirstOrDefault();

                    return query;
                }

            }
            catch (Exception ex)
            {
                this.Db.connection.Close();
                throw ex;
            }
        }

        public async Task<Boolean> UpdateAsync(ContentMasterModel entity, int contentID)
        {
            try
            {

                using (var context = new seac_webapplicationContext())
                {

                    var content = await context.ContentMaster.Where(o => o.ContentId == contentID).FirstOrDefaultAsync();

                    content.ContentCode = entity.Code;
                    content.ContentName = entity.Name;
                    content.DisplayName = entity.DisplayName;
                    content.OriginalContentName = entity.OriginalName;
                    content.Duration = entity.Duration;
                    content.IsActive = entity.IsActive;
                    content.PaxMax = entity.PaxMax;
                    content.IsInternal = entity.IsInternal;
                    content.IsPrivillege = entity.IsPrivillege;
                    content.LearningTypeId = entity.LearningType.Id;
                    content.ContentBusinessTypeId = entity.BusinessType.Id;
                    content.UpdatedBy = entity.updatedBy;
                    content.UpdatedDate = DateTime.Now;
                    content.OutLineId = entity.OutLineId;
                    content.CourseId = entity.CourseId;
                    content.CourseTitle = entity.CourseTitle;

                    await context.SaveChangesAsync();

                    var trainer = await context.CertifiedTrainer.Where(o => o.ContentId == contentID).ToListAsync();

                    if (trainer != null)
                    {
                        foreach (var result in trainer)
                        {
                            context.Remove(result);

                            await context.SaveChangesAsync();
                        }

                    }

                    if (entity.TrainerList != null)
                    {
                        foreach (var result in entity.TrainerList)
                        {

                            var certifiedTrainer = new CertifiedTrainer()
                            {
                                ContentId = contentID,
                                TrainerId = result.Id,
                                IsActive = entity.IsActive,
                                CreatedDate = DateTime.Now,
                                CreatedBy = entity.createdBy

                            };

                            await context.AddAsync(certifiedTrainer);

                            await context.SaveChangesAsync();
                        }
                    }

                    var contentFormatVariety = await context.ContentFormatVariety.Where(o => o.ContentId == contentID).ToListAsync();

                    if (contentFormatVariety != null)
                    {
                        foreach (var result in contentFormatVariety)
                        {
                            context.Remove(result);

                            await context.SaveChangesAsync();
                        }

                    }

                    if (entity.ContentFormat != null)
                    {

                        foreach (var result in entity.ContentFormat)
                        {

                            var certifiedTrainer = new ContentFormatVariety()
                            {
                                ContentId = contentID,
                                ContentFormatId = result.Id

                            };

                            await context.AddAsync(certifiedTrainer);
                            await context.SaveChangesAsync();




                        }
                    }


                    return true;
                }

            }
            catch (Exception ex)
            {
                this.Db.connection.Close();
                throw ex;
            }
        }

        public async Task<Boolean> DeleteAsync(ContentMasterModel entity, int contentID)
        {
            try
            {
                using (var context = new seac_webapplicationContext())
                {

                    var content = await context.ContentMaster.Where(o => o.ContentId == contentID).FirstOrDefaultAsync();


                    content.IsActive = entity.IsActive;

                    await context.SaveChangesAsync();


                    return true;
                }

            }
            catch (Exception ex)
            {
                this.Db.connection.Close();
                throw ex;
            }
        }

        public async Task<List<TrainerList>> FindByIDContentTrainerAsync(int? contentID)
        {
            try
            {
                using (var context = new seac_webapplicationContext())
                {

                    var query = (from cft in await context.CertifiedTrainer.ToListAsync()
                                 join tm in await context.TrainerMaster.ToListAsync()
                                 on cft.TrainerId equals tm.TrainerId
                                 where cft.ContentId == contentID
                                 select new TrainerList()
                                 {

                                     Id = tm.TrainerId,
                                     Value = tm.Name + " " + tm.LastName

                                 }).ToList();

                    return query;
                }

            }
            catch (Exception ex)
            {

                return null;
            }
        }

        public async Task<List<ContentFormat>> FindByIDContentContentFormatAsync(int? contentID)
        {
            try
            {

                using (var context = new seac_webapplicationContext())
                {

                    var query = (from cfv in await context.ContentFormatVariety.ToListAsync()
                                 join cfm in await context.ContentFormatMaster.ToListAsync()
                                 on cfv.ContentFormatId equals cfm.ContentFormatId
                                 where cfv.ContentId == contentID
                                 select new ContentFormat()
                                 {

                                     Id = cfm.ContentFormatId,
                                     Value = cfm.ContentFormatName

                                 }).ToList();

                    return query;
                }

            }
            catch (Exception ex)
            {
                this.Db.connection.Close();
                return null;
            }
        }

        //Get combobox for 
        public async Task<List<TrainerList>> FindTrainerAsync()
        {
            try
            {
                using (var context = new seac_webapplicationContext())
                {
                    var query = (from tm in await context.TrainerMaster.ToListAsync()
                                 where tm.IsTrainer == 1 && tm.IsActive == 1
                                 orderby tm.Name ascending
                                 select new TrainerList()
                                 {
                                     Id = tm.TrainerId,
                                     Value = tm.Name + " " + tm.LastName
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

    }
}
