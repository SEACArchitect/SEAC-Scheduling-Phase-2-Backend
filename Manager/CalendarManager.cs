using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nri_Webapplication_Backend.Helpers;
using Nri_Webapplication_Backend.Models;
using MySqlConnector;
using System.Data;
using Nri_Webapplication_Backend.DTO;
using Microsoft.EntityFrameworkCore;

namespace Nri_Webapplication_Backend.Managers
{
    public interface ICalendarManager
    {
        Task<int> InsertAsync(DwnLoadIcs entity, List<Calendar> calEvent);
        Task UpdateIcsLink(DwnLoadIcs entity, int idLink);
        Task TruncateAsync();
        Task<int> GetLatestIdLink();
        Task PairingRoom( int usrId,int idLnk);
        Task<int> UpdatePairing(ListApprParams entity,int usrId);
        Task<int> UpdatePlanner(ListApprParams entity, int usrId);
        Task<List<EditLink>> GetCalLnk();
        Task UpdCalLnk(EditLink entity);
        Task DelCalLnk(int idLnk,int usrId);
        Task<List<Classroom>> GetClassRoom(GetClassRoom entity);
        void InsertLogs(Logs entity);
        Task<List<Dictionary<string, string>>> GetClassOverlap(DateTime startSearch, DateTime endSearch);
    }
    public class Logs
    {
        public string Method { get; set; }
        public string From { get; set; }
        public int UsrId { get; set; }
        public string Table { get; set; }
    }
    public class CalendarManager : ICalendarManager
    {

        public ConnectionHelper Db { get; set; }
        public readonly IPlannerManager Pl;
        public CalendarManager(ConnectionHelper Db,IPlannerManager Pl)
        {
            this.Db = Db;
            this.Pl = Pl;
        }
        public MySqlDataReader Reader { get; set; }

        public async Task UpdateIcsLink(DwnLoadIcs entity, int idLink)
        {
            MySqlCommand cmd = new MySqlCommand
            {
                Connection = Db.connection,
                CommandText = "UPDATE `sync_calendar_link` SET `status_link` = 0 WHERE id_link = @idLink; " +
                    "INSERT INTO `sync_calendar_link` " +
                    "(`ics_link`,`status_link`,`upload_by`,`upload_date`,`edit_by`,`edit_date`)" +
                    "VALUES " +
                    "(@icsLink, " +
                    "@status_link," +
                    "@uploadBy," +
                    "@uploadDate, " +
                    "@editBy, " +
                    "@editDate); "
            };
            cmd.Parameters.AddWithValue("@idLink", idLink);
            cmd.Parameters.AddWithValue("@icsLink", entity.IcsPath);
            cmd.Parameters.AddWithValue("@status_link", 1);
            cmd.Parameters.AddWithValue("@uploadBy", entity.updatedBy);
            cmd.Parameters.AddWithValue("@uploadDate", entity.updatedDate);
            cmd.Parameters.AddWithValue("@editBy", entity.updatedBy);
            cmd.Parameters.AddWithValue("@editDate", entity.updatedDate);

            try
            {
                await Db.connection.OpenAsync();
                cmd.Transaction = await Db.connection.BeginTransactionAsync();
                int count = await cmd.ExecuteNonQueryAsync();
                await cmd.Transaction.CommitAsync();

                if (count>0)
                {
                    Logs log = new Logs
                    {
                        Method = "update",
                        From = "UpdateIcsLink",
                        UsrId = (int)entity.updatedBy,
                        Table = "sync_calendar_link"
                    };
                    InsertLogs(log);
                }
                
            }
            catch (Exception ex)
            {
                await cmd.Transaction.RollbackAsync();
                throw ex;
            }
            finally
            {
                await Db.connection.CloseAsync();
            }
        }

        public async Task TruncateAsync()
        {
            MySqlCommand cmd = new MySqlCommand
            {
                Connection = Db.connection,
                CommandText = "TRUNCATE TABLE `sync_calendar`;"
            };

            try
            {
                await Db.connection.OpenAsync();
                cmd.Transaction = await Db.connection.BeginTransactionAsync();
                await cmd.ExecuteNonQueryAsync();
                await cmd.Transaction.CommitAsync();
            }
            catch (Exception ex)
            {
                await cmd.Transaction.RollbackAsync();
                throw ex;
            }
            finally
            {
                await Db.connection.CloseAsync();
            }
        }

        public async Task<int> GetLatestIdLink()
        {
            try
            {
                string sqlQuery = "SELECT `id_link` FROM `sync_calendar_link` WHERE `status_link` = 1 ORDER BY `id_link` DESC LIMIT 1; ";

                int idLink = SQLCommon.GetCntDataFromDB(Db.connection.ConnectionString, sqlQuery);

                return idLink;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                //await Reader.CloseAsync();
                await Db.connection.CloseAsync();
            }
        }

        //Insert Calendar from ICS File
        public async Task<int> InsertAsync(DwnLoadIcs entity,List<Calendar> calEvent)
        {
            int idLink = await GetLatestIdLink();

            await UpdateIcsLink(entity, idLink);

            await this.TruncateAsync();

            idLink = await GetLatestIdLink();
            string sqlQuery = "";
            foreach (var item in calEvent)
            {
                sqlQuery += "INSERT INTO `sync_calendar` " +
                    "(`summary_sync`,`dtstart_sync`,`dtend_sync`,`location_sync`,`getdate_sync`,`getuser_sync`,`id_link`) " +
                    "VALUES " +
                    "(\"" + item.Summary + "\", " +
                    "\"" + item.DtStart.ToString("yyyy-MM-dd HH:mm:ss") + "\", " +
                    "\"" + item.DtEnd.ToString("yyyy-MM-dd HH:mm:ss") + "\", " +
                    "\"" + item.Location + "\", " +
                    "\"" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "\", " +
                    "\"" + entity.createdBy + "\", " +
                    "\"" + idLink + "\"); ";
            }

            try
            {
                int cInsert = SQLCommon.ComExecute(Db.connection.ConnectionString, sqlQuery);

                if (cInsert > 0)
                {
                    Logs log = new Logs
                    {
                        Method = "update,truncate,insert",
                        From = "InsertAsync",
                        UsrId = (int)entity.createdBy,
                        Table = "sync_calendar,sync_calendar_link"
                    };
                    InsertLogs(log);
                }
                
                return cInsert;
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

        public async Task PairingRoom(int usrId, int idLnk)
        {
            try
            {
                await this.Db.connection.OpenAsync();
                //substring for rooms code for virtual rooms (e.g. Virtual(B1) to B1)
                var cmd = new MySqlCommand("sp_add_roomcode", this.Db.connection);
                cmd.Parameters.AddWithValue("usrId",usrId);
                cmd.Parameters.AddWithValue("idLink", idLnk);
                cmd.Parameters["usrId"].Direction = ParameterDirection.Input;
                cmd.Parameters["idLink"].Direction = ParameterDirection.Input;
                cmd.CommandType = CommandType.StoredProcedure;

                int cInsert = await cmd.ExecuteNonQueryAsync();


                string query = "truncate  sync_calendar_overlaps; " +
                    "insert into sync_calendar_overlaps " +
                "(pairingId, scheduleId, calendar_start_class, calendar_end_class, schedule_start_time, schedule_end_time, idLink, time_stamp) " +
                "select id_pairing, EventScheduleID, start_class, end_class, StartTime, EndTime, id_link, now() " +
                "from view_class_room_overlaping " +
                "where overlaps_status = 1; ";

                int count = SQLCommon.ComExecute(Db.connection.ConnectionString, query);

                if (count > 0)
                {
                    Logs log = new Logs
                    {
                        Method = "insert,truncate",
                        From = "PairingRoom",
                        UsrId = usrId,
                        Table = "sync_calendar_room_pairing,sync_calendar_overlaps"
                    };
                    InsertLogs(log);
                }

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
        public async Task<int> UpdatePairing(ListApprParams entity,int usrId)
        {
            MySqlCommand cmd = new MySqlCommand
            {
                CommandText = "",
                Connection = Db.connection
            };
            try
            {
                var context = new seac_webapplicationContext();
                Dictionary<string, string> OldVal = new Dictionary<string, string>();

                foreach (var lEvItem in entity.ParamsList)
                {
                    if (lEvItem.ApprStat == 1)
                    {
                        //update event schedule's zoom link
                        cmd.CommandText += "update event_schedule " +
                        "set Link = '" + lEvItem.ZoomLink + "', " +
                        "EventRoomID = " + lEvItem.RoomId + ", " +
                        "UpdatedBy = " + usrId + ", " +
                        "UpdatedDate = now() " +
                        "where EventScheduleID = " + lEvItem.EventScheduleId + "; ";

                        //insert approve log
                        cmd.CommandText += "insert into schedule_approved " +
                        "(EventScheduleID,ApproveStatus) " +
                        "values" +
                        "(" + lEvItem.EventScheduleId + ", " + lEvItem.ApprStat + "); ";

                        //update calendar's zoom link
                        cmd.CommandText += "update sync_calendar_room_pairing " +
                            "set link_paring = '" + lEvItem.ZoomLink + "', " +
                            "approve_status = " + lEvItem.ApprStat + ", " +
                            "update_date = now(), " +
                            "update_by = " + usrId + ", " +
                            "schedule_id = " + lEvItem.EventScheduleId + " " +
                            "where id_pairing = " + lEvItem.PairingId + "; ";

                        var schedule = await context.EventSchedule.Where(w => w.EventScheduleId == lEvItem.EventScheduleId).FirstOrDefaultAsync();
                        OldVal.Add(schedule.EventScheduleId.ToString(), schedule.Link + " , " + schedule.EventRoomId);
                    }
                }
                await cmd.Connection.OpenAsync();
                cmd.Transaction = await Db.connection.BeginTransactionAsync();
                int count = await cmd.ExecuteNonQueryAsync();
                await cmd.Transaction.CommitAsync();

                
                foreach (var item in entity.ParamsList)
                {
                    if (item.ApprStat == 1)
                    {
                        var schedule = await context.EventSchedule.Where(w => w.EventScheduleId == item.EventScheduleId).FirstOrDefaultAsync();

                        foreach (var item2 in OldVal)
                        {
                            if (item2.Key == schedule.EventScheduleId.ToString())
                            {
                                EventScheduleLogs scheduleLog = new EventScheduleLogs
                                {
                                    EventScheduleId = item.EventScheduleId,
                                    UserId = usrId,
                                    Action = "Approve",
                                    ContentId = schedule.ContentId,
                                    FromValue = item2.Value,
                                    ToValue = item.ZoomLink + " , " + item.RoomId,
                                    Field = "Link , Room ID",
                                    LoggedDateTime = DateTime.Now
                                };

                                // Insert schedule log
                                await context.AddAsync(scheduleLog);

                                await context.SaveChangesAsync();
                            }
                        }
                        
                    }
                    
                }
                
                return count;
            }
            catch (Exception ex)
            {
                await cmd.Transaction.RollbackAsync();
                throw ex;
            }
            finally
            {
                await cmd.Connection.CloseAsync();
            }
        }

        public async Task<int> UpdatePlanner(ListApprParams entity, int usrId)
        {
            MySqlCommand cmd = new MySqlCommand
            {
                CommandText = "",
                Connection = Db.connection
            };
            try
            {
                var context = new seac_webapplicationContext();
                Dictionary<string, string> OldVal = new Dictionary<string, string>();

                foreach (var lEvItem in entity.ParamsList)
                {
                    if (lEvItem.ApprStat == 1)
                    {
                        //update event schedule zoom's link
                        cmd.CommandText += "update event_schedule " +
                        "set Link = '" + lEvItem.ZoomLink + "', " +
                        "EventRoomID = " + lEvItem.RoomId + ", " +
                        "UpdatedBy = " + usrId + ", " +
                        "UpdatedDate = now() " +
                        "where EventScheduleID = " + lEvItem.EventScheduleId + "; ";

                        //insert approve log
                        cmd.CommandText += "insert into schedule_approved " +
                        "(EventScheduleID,ApproveStatus) " +
                        "values" +
                        "(" + lEvItem.EventScheduleId + ", " + lEvItem.ApprStat + "); ";

                        var schedule = await context.EventSchedule.Where(w => w.EventScheduleId == lEvItem.EventScheduleId).FirstOrDefaultAsync();
                        OldVal.Add(schedule.EventScheduleId.ToString(), schedule.Link + " , " + schedule.EventRoomId);
                    }
                }

                await cmd.Connection.OpenAsync();
                cmd.Transaction = await Db.connection.BeginTransactionAsync();
                int count = await cmd.ExecuteNonQueryAsync();
                await cmd.Transaction.CommitAsync();

                foreach (var item in entity.ParamsList)
                {
                    if (item.ApprStat == 1)
                    {
                        var schedule = await context.EventSchedule.Where(w => w.EventScheduleId == item.EventScheduleId).FirstOrDefaultAsync();

                        foreach (var item2 in OldVal)
                        {
                            if (item2.Key == item.EventScheduleId.ToString())
                            {
                                EventScheduleLogs scheduleLog = new EventScheduleLogs
                                {
                                    EventScheduleId = item.EventScheduleId,
                                    UserId = usrId,
                                    Action = "Approve",
                                    ContentId = schedule.ContentId,
                                    FromValue = item2.Value,
                                    ToValue = item.ZoomLink + " , " + item.RoomId,
                                    Field = "Link , Room ID",
                                    LoggedDateTime = DateTime.Now
                                };
                                // Insert schedule log
                                await context.AddAsync(scheduleLog);

                                await context.SaveChangesAsync();
                            }
                        }

                    }   
                }

                return count;
            }
            catch (Exception ex)
            {
                await cmd.Transaction.RollbackAsync();
                throw ex;
            }
            finally
            {
                await cmd.Connection.CloseAsync();
            }
        }

        public async Task<List<EditLink>> GetCalLnk()
        {
            try
            {
                string sqlQuery = "SELECT * FROM `sync_calendar_link` ORDER BY edit_date desc,upload_date desc";
                DataTable dt = new DataTable();
                dt = SQLCommon.GetDataTable(Db.connection.ConnectionString, sqlQuery);
                List<EditLink> links = new List<EditLink>();
                foreach (DataRow rw in dt.Rows)
                {
                    EditLink editLnk = new EditLink
                    {
                        IdLink = (int)(rw.IsNull("id_link") ? (int?)null : rw["id_link"]),
                        IcsLink = (string)(rw.IsNull("ics_link") ? (int?)null : rw["ics_link"]),
                        Status = (int)(rw.IsNull("status_link") ? (int?)null : rw["status_link"]),
                        updatedDate = (DateTime)(rw.IsNull("upload_date") ? (DateTime?)null : rw["upload_date"]),
                        updatedBy = (int?)(rw.IsNull("upload_by") ? (int?)null : rw["upload_by"]),
                        EditDate = (DateTime)(rw.IsNull("edit_date") ? (DateTime?)null : rw["edit_date"]),
                        EditBy = (int)(rw.IsNull("edit_by") ? (int?)null : rw["edit_by"])
                    };
                    links.Add(editLnk);
                }

                return links;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                //await this.Reader.CloseAsync();
                await Db.connection.CloseAsync();
            }
        }

        public async Task UpdCalLnk(EditLink entity)
        {
            MySqlCommand cmd = new MySqlCommand
            {
                CommandText = "UPDATE `sync_calendar_link` SET " +
                    "`ics_link` = @icsLnk, " +
                    "`status_link` = @stat, " +
                    "`edit_by` = @editBy, " +
                    "`edit_date` = @editDt " +
                    "WHERE " +
                    "`id_link` = @idLnk ;",
                Connection = Db.connection
            };
            cmd.Parameters.AddWithValue("@idLnk", entity.IdLink);
            cmd.Parameters.AddWithValue("@icsLnk", entity.IcsLink);
            cmd.Parameters.AddWithValue("@stat", entity.Status);
            cmd.Parameters.AddWithValue("@editBy", entity.EditBy);
            cmd.Parameters.AddWithValue("@editDt", DateTime.Now);
            try
            {
                await Db.connection.OpenAsync();
                cmd.Transaction = await Db.connection.BeginTransactionAsync();
                int count = await cmd.ExecuteNonQueryAsync();
                await cmd.Transaction.CommitAsync();

                if (count > 0)
                {
                    Logs log = new Logs
                    {
                        Method = "update",
                        From = "UpdCalLnk",
                        UsrId = entity.EditBy,
                        Table = "sync_calendar_link"
                    };
                    InsertLogs(log);
                }

            }
            catch (Exception ex)
            {
                await cmd.Transaction.RollbackAsync();
                throw ex;
            }
            finally
            {
                await Db.connection.CloseAsync();
            }
        }

        public async Task DelCalLnk(int idLnk,int usrId)
        {
            MySqlCommand cmd = new MySqlCommand
            {
                CommandText = "DELETE FROM `sync_calendar_link` " +
                "WHERE `id_link` = @idLnk ;",
                Connection = Db.connection
            };
            cmd.Parameters.AddWithValue("@idLnk", idLnk);
            try
            {
                await Db.connection.OpenAsync();
                cmd.Transaction = await Db.connection.BeginTransactionAsync();
                int count = await cmd.ExecuteNonQueryAsync();
                await cmd.Transaction.CommitAsync();

                if (count > 0)
                {
                    Logs log = new Logs
                    {
                        Method = "delete",
                        From = "DelCalLnk",
                        UsrId = usrId,
                        Table = "sync_calendar_link"
                    };
                    InsertLogs(log);
                }
            }
            catch (Exception ex)
            {
                await cmd.Transaction.RollbackAsync();
                throw ex;
            }
            finally
            {
                await Db.connection.CloseAsync();
            }
        }

        public async Task<List<Classroom>> GetClassRoom(GetClassRoom entity)
        {
            string query = "SELECT * FROM seac_webapplication.view_class_room_overlaping " +
                "where (" +
                "event_room_name like '%" + entity.Search + "%' " +
                "or topic like '%" + entity.Search + "%' " +
                "or id_pairing like '%" + entity.Search + "%'" + ") " +
                "and business_type_id = " + entity.BusinessTypeId + "; ";

            try
            {
                DataTable dt = new DataTable();
                dt = SQLCommon.GetDataTable(Db.connection.ConnectionString, query);

                List<Classroom> classrooms = new List<Classroom>();
                foreach (DataRow rw in dt.Rows)
                {
                    Classroom classRoom = new Classroom()
                    {
                        PairingId = (int?)(rw.IsNull("id_pairing") ? null : rw["id_pairing"]),
                        RoomCode = rw.IsNull("room_pairing") ? null : rw["room_pairing"].ToString(),
                        BizTypeId = (int?)(rw.IsNull("business_type_id") ? null : rw["business_type_id"]),
                        EventRoomId = (int?)(rw.IsNull("event_room_id") ? null : rw["event_room_id"]),
                        RoomName = rw.IsNull("event_room_name") ? null : rw["event_room_name"].ToString(),
                        LinkZoomClassRoom = rw.IsNull("link_paring") ? null : rw["link_paring"].ToString(),
                        Topic = rw.IsNull("topic") ? null : rw["topic"].ToString(),
                        ClassStart = (DateTime?)(rw.IsNull("start_class") ? null : rw["start_class"]),
                        ClassEnd = (DateTime?)(rw.IsNull("end_class") ? null : rw["end_class"]),
                        CalendarIdLink = (int?)(rw.IsNull("id_link") ? null : rw["id_link"]),
                        ApproveStatus = (int?)(rw.IsNull("approve_status") ? null : (int?)(long)rw["approve_status"]),
                        EventScheduleId = (int?)(rw.IsNull("EventScheduleID") ? null : rw["EventScheduleID"]),
                        ScheduleDateStart = (DateTime?)(rw.IsNull("StartTime") ? null : rw["StartTime"]),
                        ScheduleDateEnd = (DateTime?)(rw.IsNull("EndTime") ? null : rw["EndTime"])
                    };

                    classrooms.Add(classRoom);
                }
                

                return classrooms;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                //await this.Reader.CloseAsync();
                await Db.connection.CloseAsync();
            }
        }
        public void InsertLogs(Logs entity)
        {
            string query = "insert into sync_calendar_logs " +
                "(`method`,`from`,`user`,`table`,`timestamp`) " +
                "values ( " +
                "'" + entity.Method.ToString() + "', " +
                "'" + entity.From.ToString() + "', " +
                "" + entity.UsrId.ToString() + ", " +
                "'" + entity.Table.ToString() + "', " +
                "now() " +
                " ); ";
            try
            {
                SQLCommon.ComExecute(Db.connection.ConnectionString, query);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<List<Dictionary<string, string>>> GetClassOverlap(DateTime startSearch, DateTime endSearch)
        {
            List<Dictionary<string,string>> overlapClasses = new List<Dictionary<string,string>>();
            MySqlCommand cmd = new MySqlCommand
            {
                CommandText = "select sr.event_room_name,so.* from sync_calendar_overlaps so " +
                "inner join sync_calendar_room_pairing sr on so.pairingId = sr.id_pairing " +
                "where so.schedule_start_time between @startDate and @endDate " +
                "order by so.schedule_start_time asc; ",
                Connection = Db.connection
            };
            cmd.Parameters.AddWithValue("@startDate", startSearch);
            cmd.Parameters.AddWithValue("@endDate", endSearch);

            try
            {
                await cmd.Connection.OpenAsync();
                Reader = await cmd.ExecuteReaderAsync();
                while (await Reader.ReadAsync())
                {
                    Dictionary<string, string> classes = new Dictionary<string, string>();
                    classes.Add("room", Reader["event_room_name"].ToString());
                    classes.Add("pairingId", Reader["pairingId"].ToString());
                    classes.Add("scheduleId",  Reader["scheduleId"].ToString());
                    classes.Add("calendar_link_id", Reader["idLink"].ToString());
                    classes.Add("calendar_start_class", Convert.ToDateTime(Reader["calendar_start_class"]).ToString("yyyy-MM-ddTHH:mm:ss"));
                    classes.Add("calendar_end_class", Convert.ToDateTime(Reader["calendar_end_class"]).ToString("yyyy-MM-ddTHH:mm:ss"));
                    classes.Add("schedule_start_time", Convert.ToDateTime(Reader["schedule_start_time"]).ToString("yyyy-MM-ddTHH:mm:ss"));
                    classes.Add("schedule_end_time", Convert.ToDateTime(Reader["schedule_end_time"]).ToString("yyyy-MM-ddTHH:mm:ss"));
                    overlapClasses.Add(classes);
                }

                return overlapClasses;

            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                await Reader.CloseAsync();
                await Db.connection.CloseAsync();
            }
        }
    }
}
