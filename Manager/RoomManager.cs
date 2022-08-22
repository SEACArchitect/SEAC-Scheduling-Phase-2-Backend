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

namespace Nri_Webapplication_Backend.Managers
{
    public interface IRoomManager
    {
        Task<List<RoomModel>> FindAsync();
        Task<RoomModel> FindIdAsync(int eventRoomID);
        Task<Boolean> InsertAsync(RoomModel entity);

        Task<Boolean> UpdateAsync(RoomModel entity, int id);

        Task<Boolean> DeleteAsync(RoomModel entity, int id);
        Task<List<Type>> FindRoomTypeAsync();

        Task<List<AvailableForContentFormat>> FindContentFormat();
       // Task<List<RoomModel>> FindRoomNameAsync(string roomName);
    }
    public class RoomManager : IRoomManager
    {
        public ConnectionHelper Db { get; set; }
        public RoomManager(ConnectionHelper Db)
        {
            this.Db = Db;
        }

        public async Task<List<RoomModel>> FindAsync()
        {
            try
            {
                using (var context = new seac_webapplicationContext())
                {

                    var query = (from erm in await context.EventRoomMaster.ToListAsync()
                                 join ert in await context.EventRoomType.ToListAsync()
                                 on erm.EventRoomTypeId equals ert.EventRoomTypeId
                                 join cfm in await context.ContentFormatMaster.ToListAsync()
                                 on erm.ContentFormatId equals cfm.ContentFormatId
                                 select new RoomModel()
                                 {
                                     Id = erm.EventRoomId,
                                     Name = erm.EventRoomName,
                                     type = new Type
                                     {
                                         Id = ert.EventRoomTypeId,
                                         Value = ert.EventRoomTypeName
                                     },
                                     InstancyLocationID = erm.InstancyLocationId,
                                     InstancyDisplayName = erm.InstancyDisplayName,
                                     AvailableForContentFormat = new AvailableForContentFormat
                                     {
                                         Id = cfm.ContentFormatId,
                                         Value = cfm.ContentFormatName
                                     },
                                     IsActive = erm.IsActive
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

        public async Task<RoomModel> FindIdAsync(int eventRoomID)
        {
            try
            {

                using (var context = new seac_webapplicationContext())
                {

                    var query = (from erm in await context.EventRoomMaster.ToListAsync()
                                 join ert in await context.EventRoomType.ToListAsync()
                                 on erm.EventRoomTypeId equals ert.EventRoomTypeId
                                 join cfm in await context.ContentFormatMaster.ToListAsync()
                                 on erm.ContentFormatId equals cfm.ContentFormatId
                                 where erm.EventRoomId == eventRoomID
                                 select new RoomModel()
                                 {
                                     Id = erm.EventRoomId,
                                     Name = erm.EventRoomName,
                                     type = new Type
                                     {
                                         Id = ert.EventRoomTypeId,
                                         Value = ert.EventRoomTypeName
                                     },
                                     InstancyLocationID = erm.InstancyLocationId,
                                     InstancyDisplayName = erm.InstancyDisplayName,
                                     AvailableForContentFormat = new AvailableForContentFormat
                                     {
                                         Id = cfm.ContentFormatId,
                                         Value = cfm.ContentFormatName
                                     },
                                     IsActive = erm.IsActive
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

        public async Task<Boolean> InsertAsync(RoomModel entity)
        {
            try
            {
                using (var context = new seac_webapplicationContext())
                {
                    var eventRoomMaster = new EventRoomMaster
                    {
                        EventRoomId = entity.Id,
                        EventRoomName = entity.Name,
                        EventRoomTypeId = entity.type.Id,
                        InstancyLocationId = entity.InstancyLocationID,
                        InstancyDisplayName = entity.InstancyDisplayName,
                        ContentFormatId = entity.AvailableForContentFormat.Id,
                        IsActive = entity.IsActive,
                        CreatedBy = entity.createdBy,
                        CreatedDate = DateTime.Now,
                    };

                    await context.AddAsync(eventRoomMaster);

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

        public async Task<Boolean> UpdateAsync(RoomModel entity, int id)
        {
            try
            {
                using (var context = new seac_webapplicationContext())
                {

                    var room = await context.EventRoomMaster.Where(o => o.EventRoomId == id).FirstOrDefaultAsync();

                    room.EventRoomName = entity.Name;
                    room.EventRoomTypeId = entity.type.Id;
                    room.InstancyLocationId = entity.InstancyLocationID;
                    room.InstancyDisplayName = entity.InstancyDisplayName;
                    room.ContentFormatId = entity.AvailableForContentFormat.Id;
                    room.IsActive = entity.IsActive;
                    room.UpdatedBy = entity.updatedBy;
                    room.UpdatedDate = DateTime.Now;

                    context.SaveChanges();

                    return true;
                }
                
            }
            catch (Exception ex)
            {
                this.Db.connection.Close();
                throw ex;
            }
        }

        public async Task<Boolean> DeleteAsync(RoomModel entity, int id)
        {
            try
            {
                using (var context = new seac_webapplicationContext())
                {

                    var room = await context.EventRoomMaster.Where(o => o.EventRoomId == id).FirstOrDefaultAsync();

                    room.IsActive = entity.IsActive;
                    room.UpdatedBy = entity.updatedBy;
                    room.UpdatedDate = DateTime.Now;

                    context.SaveChanges();

                    return true;
                }
            }
            catch (Exception ex)
            {
                this.Db.connection.Close();
                throw ex;
            }
        }

        public async Task<List<Type>> FindRoomTypeAsync()
        {
            try
            {
                using (var context = new seac_webapplicationContext())
                {

                    var query = (from ert in await context.EventRoomType.ToListAsync()
                                 select new Type
                                 {
                                     Id = ert.EventRoomTypeId,
                                     Value = ert.EventRoomTypeName

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

        public async Task<List<AvailableForContentFormat>> FindContentFormat()
        {
            try
            {
                using (var context = new seac_webapplicationContext())
                {

                    var query = (from cfm in await context.ContentFormatMaster.ToListAsync()
                                 select new AvailableForContentFormat
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
                throw ex;
            }
        }

        //public async Task<List<RoomModel>> FindRoomNameAsync(string roomName)
        //{
        //    try
        //    {

        //        using (var context = new seac_webapplicationContext())
        //        {

        //            var query = (from erm in await context.EventRoomMaster.ToListAsync()
        //                         join ert in await context.EventRoomType.ToListAsync()
        //                         on erm.EventRoomTypeId equals ert.EventRoomTypeId
        //                         join cfm in await context.ContentFormatMaster.ToListAsync()
        //                         on erm.ContentFormatId equals cfm.ContentFormatId
        //                         where erm.EventRoomName.Trim().ToLower() == roomName.Trim().ToLower()
        //                         select new RoomModel()
        //                         {
        //                             Id = erm.EventRoomId,
        //                             Name = erm.EventRoomName,
        //                             type = new Type
        //                             {
        //                                 Id = ert.EventRoomTypeId,
        //                                 Value = ert.EventRoomTypeName
        //                             },
        //                             InstancyLocationID = erm.InstancyLocationId,
        //                             InstancyDisplayName = erm.InstancyDisplayName,
        //                             AvailableForContentFormat = new AvailableForContentFormat
        //                             {
        //                                 Id = cfm.ContentFormatId,
        //                                 Value = cfm.ContentFormatName
        //                             },
        //                             IsActive = erm.IsActive
        //                         });

        //            return query;
        //        }

        //    }
        //    catch (Exception ex)
        //    {
        //        this.Db.connection.Close();
        //        throw ex;
        //    }
        //}


    }
}
