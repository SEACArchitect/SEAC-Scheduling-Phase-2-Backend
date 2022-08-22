using System;
using System.Collections.Generic;
using System.Linq;
using Nri_Webapplication_Backend.DTO;
using Nri_Webapplication_Backend.Helpers;
using Nri_Webapplication_Backend.Models;
using System.Globalization;
using System.Data;

namespace Nri_Webapplication_Backend.Managers
{
    public interface IReportManager
    {
        List<CourseSummaryModel> ContentSummary(DateTime startDateTime, DateTime endDateTime, List<string> businessType, List<string> contentFormat, List<string> eventCategory);
        List<TrainerSummaryModel> TrainerSummary(DateTime startDateTime, DateTime endDateTime);
        List<ModeratorSummaryModel> ModeratorSummary(DateTime startDateTime, DateTime endDateTime);
        //List<ContentMasterTemplate> ContentMasterTemplate(SearchContentMasterTemplate content_search);
        List<ContentMasterTemplate> ContentMasterTemplate(string ContentName, string BusinessType, string LearningType, string ContentFormat);
        List<TrainerAndModerator> TrainerAndMod(string Name, string NickNameEn, string NickNameTh, string Email, string Contract, string Type);
        List<RCertifiedTrainer> CerTrainer(string ContentName, string BusinessType, string LearningType, string TrainerName, string TrainerType);
        List<CourseSession> CourseSessions(DateTime startDate, DateTime endDate, int contBizType);
        List<CourseSessionFile> CourseSessionsFile(DateTime startDate, DateTime endDate, int contBizType);
        List<CourseOutline> CourseOutline(List<int> evScheduleId);
        List<CourseOutlineFile> CourseOutlineFile(List<int> evScheduleId);
        List<Instructors> Instructors(List<int> evScheduleId);
    }
    public class ReportManager : IReportManager
    {
        public ConnectionHelper Db { get; set; }
        public ReportManager(ConnectionHelper Db)
        {
            this.Db = Db;
        }

        public List<CourseSummaryModel> ContentSummary(DateTime startDateTime, DateTime endDateTime, List<string> businessTypeId, List<string> contentFormatId, List<string> eventCategoryId)
        {
            try
            {
                using (var context = new seac_webapplicationContext())
                {
                    //find all active contentmaster
                    var ctmData = (from cn in context.ContentMaster
                                   join bus in context.ContentBusinessType on cn.ContentBusinessTypeId equals bus.ContentBusinessTypeId
                                   join cfv in context.ContentFormatVariety on cn.ContentId equals cfv.ContentId
                                   where (businessTypeId.Count == 0 || businessTypeId.Contains(cn.ContentBusinessTypeId.ToString()))
                                   && (contentFormatId.Count == 0 || contentFormatId.Contains(cfv.ContentFormatId.ToString()))
                                   && cn.IsActive == 1
                                   orderby cn.ContentName ascending
                                   select new CourseSummaryModel()
                                   {
                                       contentID = cn.ContentId.ToString(),
                                       course = cn.ContentName,
                                       monthNo = startDateTime.Month.ToString(),
                                       monthName = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(startDateTime.Month),
                                       year = startDateTime.Year.ToString(),
                                       numberOfCourse = 0

                                   }).ToList();

                    var scheduleData = (from evs in context.EventSchedule
                                        join cn in context.ContentMaster on evs.ContentId equals cn.ContentId
                                        join cat in context.EventCategory on evs.EventCategoryId equals cat.EventCategoryId
                                        join bus in context.ContentBusinessType on cn.ContentBusinessTypeId equals bus.ContentBusinessTypeId
                                        join cf in context.ContentFormatMaster on evs.ContentFormatId equals cf.ContentFormatId
                                        where (contentFormatId.Count == 0 || contentFormatId.Contains(evs.ContentFormatId.ToString()))
                                        && (eventCategoryId.Count == 0 || eventCategoryId.Contains(evs.EventCategoryId.ToString()))
                                        && evs.StartTime >= startDateTime && evs.EndTime <= endDateTime
                                        && evs.IsActive == 1
                                        group evs by new { evs.ContentId, cn.ContentName, evs.StartTime.Month, evs.StartTime.Year, } into grp
                                        orderby grp.Key.ContentId, grp.Key.Year, grp.Key.Month ascending
                                        select new CourseSummaryModel()
                                        {
                                            contentID = grp.Key.ContentId.ToString(),
                                            course = grp.Key.ContentName,
                                            monthNo = grp.Key.Month.ToString(),
                                            monthName = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(grp.Key.Month),
                                            year = grp.Key.Year.ToString(),
                                            numberOfCourse = grp.Count()
                                        }).ToList();

                    List<CourseSummaryModel> finalResult = new List<CourseSummaryModel> { };
                    foreach (var result in ctmData)
                    {
                        bool chk = true;
                        foreach (var x in scheduleData)
                        {
                            if (result.contentID == x.contentID)
                            {
                                finalResult.Add(new CourseSummaryModel { contentID = x.contentID, course = x.course, monthName = x.monthName, monthNo = x.monthNo, year = x.year, numberOfCourse = x.numberOfCourse });
                                chk = false;
                            }
                        }
                        if (chk)
                        {
                            finalResult.Add(new CourseSummaryModel { contentID = result.contentID, course = result.course, monthName = result.monthName, monthNo = result.monthNo, year = result.year, numberOfCourse = result.numberOfCourse });
                        }
                    }
                    return finalResult;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public List<TrainerSummaryModel> TrainerSummary(DateTime startDateTime, DateTime endDateTime)
        {
            try
            {
                using (var context = new seac_webapplicationContext())
                {
                    var query = (from evs in context.EventSchedule
                                 join cn in context.ContentMaster on evs.ContentId equals cn.ContentId
                                 join cf in context.ContentFormatMaster on evs.ContentFormatId equals cf.ContentFormatId
                                 join cb in context.ContentBusinessType on evs.ContentBusinessTypeId equals cb.ContentBusinessTypeId
                                 join evt in context.EventScheduleTrainer on evs.EventScheduleId equals evt.EventScheduleId
                                 join tm in context.TrainerMaster on evt.TrainerId equals tm.TrainerId
                                 join tc in context.TrainerContractType on tm.TraninerContractTypeId equals tc.TraninerContractTypeId
                                 join tt in context.TrainerType on tm.TrainerTypeId equals tt.TrainerTypeId
                                 where evs.StartTime >= startDateTime && evs.EndTime <= endDateTime
                                 && evs.IsActive == 1 && evt.IsActive == 1
                                 orderby evs.StartTime.Year, evs.StartTime.Month, cn.ContentName ascending
                                 select new TrainerSummaryModel()
                                 {
                                     startTime = evs.StartTime.ToString(),
                                     endTime = evs.EndTime.ToString(),
                                     trainerType = tt.TypeName,
                                     contractType = tc.TraninerContractTypeName,
                                     trainerName = string.Format("{0} {1}", tm.Name, string.IsNullOrEmpty(tm.LastName) == true ? "" : tm.LastName),
                                     bookingType = cb.ContentBusinessTypeName,
                                     contentFormat = cf.ContentFormatName,
                                     contentName = cn.ContentName,
                                     sessionName = evs.Session,
                                     isBillable = evs.IsBillable.ToString(),
                                     actualMins = Math.Round(Convert.ToDateTime(evs.EndTime).Subtract(Convert.ToDateTime(evs.StartTime)).TotalMinutes, 2),
                                     actualHours = Math.Round(Convert.ToDateTime(evs.EndTime).Subtract(Convert.ToDateTime(evs.StartTime)).TotalHours, 2),
                                     calculateDay = ((evs.ContentFormatId == 1) && (evs.ContentBusinessTypeId == 1 || evs.ContentBusinessTypeId == 2)) ? 0.5 :
                                                    ((evs.ContentFormatId == 2) && (evs.ContentBusinessTypeId == 1 || evs.ContentBusinessTypeId == 2)) ? 0.5 :
                                                    Math.Round(Convert.ToDateTime(evs.EndTime).Subtract(Convert.ToDateTime(evs.StartTime)).TotalHours, 2) > 6 ? 1 :
                                                    Math.Round(Convert.ToDateTime(evs.EndTime).Subtract(Convert.ToDateTime(evs.StartTime)).TotalHours, 2) > 2 ? 0.5 : 0.25,
                                     projectID = evs.ProjectId,
                                     company = evs.Company,
                                     bookBy = evs.BookBy,
                                     contentBusinessTypeId = evs.ContentBusinessTypeId,
                                     contractTypeId = tm.TraninerContractTypeId,
                                     year = evs.StartTime.Year,
                                     month = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(evs.StartTime.Month)
                                 }).ToList();

                    foreach (var result in query)
                    {
                        //Calculate workload 
                        if (result.contentBusinessTypeId == 1 || result.contentBusinessTypeId == 2 || result.contentBusinessTypeId == 4)
                        {
                            result.workload = result.calculateDay;
                        }
                        else
                        {
                            if (result.contractTypeId == 1)
                            {
                                result.workload = result.calculateDay;
                            }
                            else
                            {
                                result.workload = result.calculateDay * 1.5;
                            }
                        }
                        //Calculate billableday
                        if (result.contentBusinessTypeId == 1 || result.contentBusinessTypeId == 2)
                        {
                            result.billableDay = result.workload;
                        }
                        else
                        {
                            if (result.isBillable == "1")
                            {
                                result.billableDay = result.workload;
                            }
                            else
                            {
                                result.billableDay = 0;
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

        public List<ModeratorSummaryModel> ModeratorSummary(DateTime startDateTime, DateTime endDateTime)
        {
            try
            {
                using (var context = new seac_webapplicationContext())
                {
                    var query = (from evs in context.EventSchedule
                                 join cn in context.ContentMaster on evs.ContentId equals cn.ContentId
                                 join cb in context.ContentBusinessType on evs.ContentBusinessTypeId equals cb.ContentBusinessTypeId
                                 join evm in context.EventScheduleModerator on evs.EventScheduleId equals evm.EventScheduleId
                                 join mm in context.TrainerMaster on evm.ModeratorId equals mm.TrainerId //retrieve moderator name
                                 join evt in context.EventScheduleTrainer on evs.EventScheduleId equals evt.EventScheduleId
                                 join tm in context.TrainerMaster on evt.TrainerId equals tm.TrainerId
                                 join tc in context.TrainerContractType on mm.TraninerContractTypeId equals tc.TraninerContractTypeId
                                 join tt in context.TrainerType on mm.TrainerTypeId equals tt.TrainerTypeId
                                 where evs.StartTime >= startDateTime && evs.EndTime <= endDateTime
                                 && evs.IsActive == 1 && evm.IsActive == 1 && evt.IsActive == 1
                                 orderby evs.StartTime.Year, evs.StartTime.Month ascending
                                 select new ModeratorSummaryModel()
                                 {
                                     startTime = evs.StartTime.ToString(),
                                     endTime = evs.EndTime.ToString(),
                                     moderatorType = tt.TypeName,
                                     contractType = tc.TraninerContractTypeName,
                                     moderatorName = string.Format("{0} {1}", mm.Name, string.IsNullOrEmpty(mm.LastName) == true ? "" : mm.LastName),
                                     nickNameEn = mm.NickNameEn,
                                     bookingType = cb.ContentBusinessTypeName,
                                     contentName = cn.ContentName,
                                     trainerName = string.Format("{0} {1}", tm.Name, string.IsNullOrEmpty(tm.LastName) == true ? "" : tm.LastName),
                                     noOfHours = Math.Round(Convert.ToDateTime(evs.EndTime).Subtract(Convert.ToDateTime(evs.StartTime)).TotalHours, 2),
                                     noOfSession = 1,
                                     year = evs.StartTime.Year,
                                     month = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(evs.StartTime.Month)
                                 }).ToList();
                    return query;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        //public List<ContentMasterTemplate> ContentMasterTemplate(SearchContentMasterTemplate content_search)
        public List<ContentMasterTemplate> ContentMasterTemplate(string ContentName, string BusinessType, string LearningType, string ContentFormat)
        {
            MySqlConnector.MySqlCommand cmd = new MySqlConnector.MySqlCommand()
            {
                CommandText = "SELECT * FROM seac_webapplication.view_content_master " +
                "WHERE ContentName LIKE @contentName " +
                "AND LearningTypeName LIKE @LearningType " +
                "AND ContentBusinessTypeName LIKE @contentBiz " +
                "AND ContentFormatName LIKE @contentFormat;",
                Connection = Db.connection
            };
            MySqlConnector.MySqlDataReader reader;
            try
            {
                cmd.Parameters.AddWithValue("@contentName", "%" + ContentName + "%");
                cmd.Parameters.AddWithValue("@LearningType", "%" + LearningType + "%");
                cmd.Parameters.AddWithValue("@contentBiz", "%" + BusinessType + "%");
                cmd.Parameters.AddWithValue("@contentFormat", "%" + ContentFormat + "%");
                Db.connection.Open();
                reader = cmd.ExecuteReader();
                List<ContentMasterTemplate> listData = new List<ContentMasterTemplate>();
                while (reader.Read())
                {
                    ContentMasterTemplate data = new ContentMasterTemplate()
                    {
                        ContentCode = reader.IsDBNull("ContentCode") ? null : reader["ContentCode"].ToString(),
                        ContentName = reader.IsDBNull("ContentName") ? null : reader["ContentName"].ToString(),
                        DisplayName = reader.IsDBNull("DisplayName") ? null : reader["DisplayName"].ToString(),
                        OriginalContentName = reader.IsDBNull("OriginalContentName") ? null : reader["OriginalContentName"].ToString(),
                        OutLineID = reader.IsDBNull("OutLineID") ? null : reader["OutLineID"].ToString(),
                        CourseID = reader.IsDBNull("CourseID") ? null : reader["CourseID"].ToString(),
                        CourseTitle = reader.IsDBNull("CourseTitle") ? null : reader["CourseTitle"].ToString(),
                        Duration = reader.IsDBNull("Duration") ? null : reader["Duration"].ToString(),
                        Paxmax = reader.IsDBNull("Paxmax") ? null : reader["Paxmax"].ToString(),
                        IsInternal = reader.IsDBNull("IsInternal") ? null : reader["IsInternal"].ToString(),
                        BusinessType = reader.IsDBNull("ContentBusinessTypeName") ? null : reader["ContentBusinessTypeName"].ToString(),
                        LearningType = reader.IsDBNull("LearningTypeName") ? null : reader["LearningTypeName"].ToString(),
                        ContentFormat = reader.IsDBNull("ContentFormatName") ? null : reader["ContentFormatName"].ToString(),
                        Active = reader.IsDBNull("Active") ? null : reader["Active"].ToString()
                    };
                    listData.Add(data);
                }
                reader.Close();

                return listData;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                Db.connection.Close();
            }
        }
        public List<TrainerAndModerator> TrainerAndMod(string Name, string NickNameEn, string NickNameTh, string Email, string Contract, string Type)
        {
            MySqlConnector.MySqlCommand cmd = new MySqlConnector.MySqlCommand()
            {
                CommandText = "select * from seac_webapplication.view_trainer_and_moderator_master " +
                "where (`Name` like @Name or `LastName` like @Name) " +
                "and `NickNameEN` like @NickNameEn " +
                "and `NickNameTH` like @NickNameTh " +
                "and `Email` like @Email " +
                "and `TraninerContractTypeName` like @Contract " +
                "and `TypeName` like @Type " +
                "order by `Name` asc; ",
                Connection = Db.connection
            };
            try
            {
                cmd.Parameters.AddWithValue("@Name", "%" + Name + "%");
                cmd.Parameters.AddWithValue("@NickNameEn", "%" + NickNameEn + "%");
                cmd.Parameters.AddWithValue("@NickNameTh", "%" + NickNameTh + "%");
                cmd.Parameters.AddWithValue("@Email", "%" + Email + "%");
                cmd.Parameters.AddWithValue("@Contract", "%" + Contract + "%");
                cmd.Parameters.AddWithValue("@Type", "%" + Type + "%");
                Db.connection.Open();
                MySqlConnector.MySqlDataReader reader;
                reader = cmd.ExecuteReader();

                List<TrainerAndModerator> listTnM = new List<TrainerAndModerator>();
                while (reader.Read())
                {
                    TrainerAndModerator TnM = new TrainerAndModerator()
                    {
                        Name = reader.IsDBNull("Name") ? null : reader["Name"].ToString(),
                        LName = reader.IsDBNull("LastName") ? null : reader["LastName"].ToString(),
                        NName = reader.IsDBNull("NickNameEN") ? null : reader["NickNameEN"].ToString(),
                        NNameTh = reader.IsDBNull("NickNameTH") ? null : reader["NickNameTH"].ToString(),
                        Telno = reader.IsDBNull("Telephone") ? null : reader["Telephone"].ToString(),
                        Email = reader.IsDBNull("Email") ? null : reader["Email"].ToString(),
                        Contract = reader.IsDBNull("TraninerContractTypeName") ? null : reader["TraninerContractTypeName"].ToString(),
                        Type = reader.IsDBNull("TypeName") ? null : reader["TypeName"].ToString(),
                        Trainer = reader.IsDBNull("Trainer") ? null : reader["Trainer"].ToString(),
                        Active = reader.IsDBNull("Active") ? null : reader["Active"].ToString()
                    };
                    listTnM.Add(TnM);
                }
                reader.Close();
                
                return listTnM;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                Db.connection.Close();
            }
        }
        public List<RCertifiedTrainer> CerTrainer(string ContentName, string BusinessType, string LearningType, string TrainerName, string TrainerType)
        {
            MySqlConnector.MySqlCommand cmd = new MySqlConnector.MySqlCommand()
            {
                CommandText = "select * from seac_webapplication.view_certified_trainer_master " +
                "where `ContentName` like @ContentName " +
                "and `ContentBusinessTypeName` like @BusinessType " +
                "and `LearningTypeName` like @LearningType " +
                "and `TrainerName` like @TrainerName " +
                "and `TraninerContractTypeName` like @TrainerType " +
                "order by `ContentName` asc; ",
                Connection = Db.connection,
            };
            try
            {
                cmd.Parameters.AddWithValue("@ContentName", "%" + ContentName + "%");
                cmd.Parameters.AddWithValue("@BusinessType", "%" + BusinessType + "%");
                cmd.Parameters.AddWithValue("@LearningType", "%" + LearningType + "%");
                cmd.Parameters.AddWithValue("@TrainerName", "%" + TrainerName + "%");
                cmd.Parameters.AddWithValue("@TrainerType", "%" + TrainerType + "%");
                Db.connection.Open();
                MySqlConnector.MySqlDataReader reader = cmd.ExecuteReader();
                List<RCertifiedTrainer> listCTrainer = new List<RCertifiedTrainer>();

                while (reader.Read())
                {
                    RCertifiedTrainer cerTrainer = new RCertifiedTrainer()
                    {
                        ContentName = reader.IsDBNull("ContentName") ? null : reader["ContentName"].ToString(),
                        LearningTypeName = reader.IsDBNull("LearningTypeName") ? null : reader["LearningTypeName"].ToString(),
                        ContentBusinessTypeName = reader.IsDBNull("ContentBusinessTypeName") ? null : reader["ContentBusinessTypeName"].ToString(),
                        TrainerName = reader.IsDBNull("TrainerName") ? null : reader["TrainerName"].ToString(),
                        TraninerContractTypeName = reader.IsDBNull("TraninerContractTypeName") ? null : reader["TraninerContractTypeName"].ToString(),
                        Active = reader.IsDBNull("Active") ? null : reader["Active"].ToString()
                    };
                    listCTrainer.Add(cerTrainer);
                }

                return listCTrainer;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                Db.connection.Close();
            }
        }

        public List<CourseSession> CourseSessions(DateTime startDate,DateTime endDate,int contBizType)
        {
            MySqlConnector.MySqlCommand cmd = new MySqlConnector.MySqlCommand()
            {
                CommandText = "select * from seac_webapplication.view_course_sessions where UploadToInstancyDate between @startDate and @endDate ;",
                //"where(CourseOutLineID like @wordSearch or Location like @wordSearch or WebinarTool like @wordSearch) " +
                //"and `Language` like @Lang " +
                //"where StartTime between @startDate and @endDate ",
                //"where UploadToInstancyDate between @startDate and @endDate " +
                //"and (InstructorEmails like @trainerEmail " +
                //"or ModeratorEmails like @modEmail)" +
                Connection = Db.connection
            };
            try
            {
                //cmd.Parameters.AddWithValue("@wordSearch", "%" + wordSearch + "%");
                //cmd.Parameters.AddWithValue("@Lang", "%" + Lang + "%");
                cmd.Parameters.AddWithValue("@startDate", startDate);
                cmd.Parameters.AddWithValue("@endDate", endDate);
                //cmd.Parameters.AddWithValue("@trainerEmail", "%" + trainerEmail + "%");
                //cmd.Parameters.AddWithValue("@modEmail", "%" + modEmail + "%");
                if (contBizType != 0)
                {
                    string sqlQuery = cmd.CommandText.ToString();
                    sqlQuery = sqlQuery.Replace(";", "");
                    sqlQuery += "and ContentBusinessTypeID = " + contBizType.ToString();
                    sqlQuery += " ;";
                    cmd.CommandText = sqlQuery;
                }
                

                Db.connection.Open();
                MySqlConnector.MySqlDataReader reader = cmd.ExecuteReader();

                List<CourseSession> lCourse = new List<CourseSession>();
                while (reader.Read())
                {
                    CourseSession courseSession = new CourseSession()
                    {
                        EventScheduleID= (int?)(reader.IsDBNull("EventScheduleID") ? null : reader["EventScheduleID"]),
                        ContentID = (int?)(reader.IsDBNull("ContentID") ? null : reader["ContentID"]),
                        CourseOutlineId = reader.IsDBNull("CourseOutLineID")?null:reader["CourseOutLineID"].ToString(),
                        Seats = reader.IsDBNull("Seats") ? null : reader["Seats"].ToString(),
                        Language = reader.IsDBNull("Language") ?null:reader["Language"].ToString(),
                        WebinarTool = reader.IsDBNull("WebinarTool") ?null:reader["WebinarTool"].ToString(),
                        Location = reader.IsDBNull("Location") ?null:reader["Location"].ToString(),
                        ParticipantURL = reader.IsDBNull("ParticipantURL") ?null:reader["ParticipantURL"].ToString(),
                        StartDate = reader.IsDBNull("StartDate") ?null:reader["StartDate"].ToString(),
                        StartMonth = reader.IsDBNull("StartMonth") ?null:reader["StartMonth"].ToString(),
                        StartYear = reader.IsDBNull("StartYear") ?null:reader["StartYear"].ToString(),
                        StartTime = reader.IsDBNull("StartTime(HH:MM)") ?null:reader["StartTime(HH:MM)"].ToString(),
                        EndDate = reader.IsDBNull("EndDate") ?null:reader["EndDate"].ToString(),
                        EndMonth = reader.IsDBNull("EndMonth") ?null:reader["EndMonth"].ToString(),
                        EndYear = reader.IsDBNull("EndYear") ?null:reader["EndYear"].ToString(),
                        EndTime = reader.IsDBNull("EndTime(HH:MM)") ?null:reader["EndTime(HH:MM)"].ToString(),
                        TimeZoneUTC = reader.IsDBNull("TimeZone(UTC)") ?null:reader["TimeZone(UTC)"].ToString(),
                        InstructorEmails = reader.IsDBNull("InstructorEmails") ?null:reader["InstructorEmails"].ToString(),
                        PrivateClass = reader.IsDBNull("Private") ?null:reader["Private"].ToString(),
                        ModeratorEmails = reader.IsDBNull("ModeratorEmails") ?null:reader["ModeratorEmails"].ToString()
                        //ContentBusinessType = (int)reader["ContentBusinessTypeID"]
                    };
                    lCourse.Add(courseSession);
                }
                reader.Close();

                return lCourse;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                Db.connection.Close();
            }
        }
        public List<CourseSessionFile> CourseSessionsFile(DateTime startDate, DateTime endDate,int contBizType)
        {
            MySqlConnector.MySqlCommand cmd = new MySqlConnector.MySqlCommand()
            {
                CommandText = "select * from seac_webapplication.view_course_sessions where UploadToInstancyDate between @startDate and @endDate ;",
                //"where(CourseOutLineID like @wordSearch or Location like @wordSearch or WebinarTool like @wordSearch) " +
                //"and `Language` like @Lang " +
                //"where StartTime between @startDate and @endDate ",
                //"where UploadToInstancyDate between @startDate and @endDate " +
                //"and (InstructorEmails like @trainerEmail " +
                //"or ModeratorEmails like @modEmail)" +
                Connection = Db.connection
            };
            try
            {
                //cmd.Parameters.AddWithValue("@wordSearch", "%" + wordSearch + "%");
                //cmd.Parameters.AddWithValue("@Lang", "%" + Lang + "%");
                cmd.Parameters.AddWithValue("@startDate", startDate);
                cmd.Parameters.AddWithValue("@endDate", endDate);
                //cmd.Parameters.AddWithValue("@trainerEmail", "%" + trainerEmail + "%");
                //cmd.Parameters.AddWithValue("@modEmail", "%" + modEmail + "%");
                if (contBizType != 0)
                {
                    string sqlQuery = cmd.CommandText.ToString();
                    sqlQuery = sqlQuery.Replace(";", "");
                    sqlQuery += "and ContentBusinessTypeID = " + contBizType.ToString();
                    sqlQuery += " ;";
                    cmd.CommandText = sqlQuery;
                }

                Db.connection.Open();
                MySqlConnector.MySqlDataReader reader = cmd.ExecuteReader();

                List<CourseSessionFile> lCourse = new List<CourseSessionFile>();
                while (reader.Read())
                {
                    CourseSessionFile courseSession = new CourseSessionFile()
                    {
                        CourseOutlineId = reader.IsDBNull("CourseOutLineID") ? null : reader["CourseOutLineID"].ToString(),
                        Seats = reader.IsDBNull("Seats") ? null : reader["Seats"].ToString(),
                        Language = reader.IsDBNull("Language") ? null : reader["Language"].ToString(),
                        WebinarTool = reader.IsDBNull("WebinarTool") ? null : reader["WebinarTool"].ToString(),
                        Location = reader.IsDBNull("Location") ? null : reader["Location"].ToString(),
                        ParticipantURL = reader.IsDBNull("ParticipantURL") ? null : reader["ParticipantURL"].ToString(),
                        StartDate = reader.IsDBNull("StartDate") ? null : reader["StartDate"].ToString(),
                        StartMonth = reader.IsDBNull("StartMonth") ? null : reader["StartMonth"].ToString(),
                        StartYear = reader.IsDBNull("StartYear") ? null : reader["StartYear"].ToString(),
                        StartTime = reader.IsDBNull("StartTime(HH:MM)") ? null : reader["StartTime(HH:MM)"].ToString(),
                        EndDate = reader.IsDBNull("EndDate") ? null : reader["EndDate"].ToString(),
                        EndMonth = reader.IsDBNull("EndMonth") ? null : reader["EndMonth"].ToString(),
                        EndYear = reader.IsDBNull("EndYear") ? null : reader["EndYear"].ToString(),
                        EndTime = reader.IsDBNull("EndTime(HH:MM)") ? null : reader["EndTime(HH:MM)"].ToString(),
                        TimeZoneUTC = reader.IsDBNull("TimeZone(UTC)") ? null : reader["TimeZone(UTC)"].ToString(),
                        InstructorEmails = reader.IsDBNull("InstructorEmails") ? null : reader["InstructorEmails"].ToString(),
                        PrivateClass = reader.IsDBNull("Private") ? null : reader["Private"].ToString(),
                        ModeratorEmails = reader.IsDBNull("ModeratorEmails") ? null : reader["ModeratorEmails"].ToString()
                        //ContentBusinessType = (int)reader["ContentBusinessTypeID"]
                    };
                    lCourse.Add(courseSession);
                }
                reader.Close();

                return lCourse;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                Db.connection.Close();
            }
        }
        public List<CourseOutline> CourseOutline(List<int> evScheduleId)
        {
            MySqlConnector.MySqlCommand cmd = new MySqlConnector.MySqlCommand();
            string sqlTxt = "select * from seac_webapplication.view_course_outlines where EventScheduleID in (";
            for (int i = 0; i < evScheduleId.Count; i++)
            {
                sqlTxt = $"{sqlTxt} @{i},";
            }
            sqlTxt += ");";
            sqlTxt = sqlTxt.ToString().Remove(sqlTxt.LastIndexOf(","), 1);
            cmd.CommandText = sqlTxt;
            cmd.Connection = Db.connection;
            try
            {
                for (int i = 0; i < evScheduleId.Count; i++)
                {
                    cmd.Parameters.AddWithValue("@" + i, evScheduleId[i]);
                }

                Db.connection.Open();
                MySqlConnector.MySqlDataReader reader = cmd.ExecuteReader();
                List<CourseOutline> lCourse = new List<CourseOutline>();
                while (reader.Read())
                {
                    CourseOutline courseOutline = new CourseOutline()
                    {
                        CreatedAt = reader.IsDBNull("CreateAt") ? null : reader["CreateAt"].ToString(),
                        CourseOutlineId = reader.IsDBNull("CourseOutlineId") ? null : reader["CourseOutlineId"].ToString(),
                        CourseTitle = reader.IsDBNull("CourseTitle") ? null : reader["CourseTitle"].ToString(),
                        Title = reader.IsDBNull("Title") ? null : reader["Title"].ToString(),
                        Category = reader.IsDBNull("Category") ? null : reader["Category"].ToString(),
                        ContentId = (int?)(reader.IsDBNull("ContentID") ? null : reader["ContentID"])
                    };
                    lCourse.Add(courseOutline);
                }
                reader.Close();

                return lCourse;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                Db.connection.Close();
            }
        }
        public List<CourseOutlineFile> CourseOutlineFile(List<int> evScheduleId)
        {
            MySqlConnector.MySqlCommand cmd = new MySqlConnector.MySqlCommand();
            string sqlTxt = "select * from seac_webapplication.view_course_outlines where EventScheduleID in (";
            for (int i = 0; i < evScheduleId.Count; i++)
            {
                sqlTxt = $"{sqlTxt} @{i},";
            }
            sqlTxt += ");";
            sqlTxt = sqlTxt.ToString().Remove(sqlTxt.LastIndexOf(","), 1);
            cmd.CommandText = sqlTxt;
            cmd.Connection = Db.connection;
            try
            {
                for (int i = 0; i < evScheduleId.Count; i++)
                {
                    cmd.Parameters.AddWithValue("@" + i, evScheduleId[i]);
                }

                Db.connection.Open();
                MySqlConnector.MySqlDataReader reader = cmd.ExecuteReader();
                List<CourseOutlineFile> lCourse = new List<CourseOutlineFile>();
                while (reader.Read())
                {
                    CourseOutlineFile courseOutline = new CourseOutlineFile()
                    {
                        CreatedAt = reader.IsDBNull("CreateAt") ? null : reader["CreateAt"].ToString(),
                        CourseOutlineId = reader.IsDBNull("CourseOutlineId") ? null : reader["CourseOutlineId"].ToString(),
                        CourseTitle = reader.IsDBNull("CourseTitle") ? null : reader["CourseTitle"].ToString(),
                        Title = reader.IsDBNull("Title") ? null : reader["Title"].ToString(),
                        Category = reader.IsDBNull("Category") ? null : reader["Category"].ToString()
                    };
                    lCourse.Add(courseOutline);
                }
                reader.Close();

                return lCourse;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                Db.connection.Close();
            }
        }
        public List<Instructors> Instructors(List<int> evScheduleId)
        {
            MySqlConnector.MySqlCommand cmd = new MySqlConnector.MySqlCommand();
            string sqlTxt = "select * from seac_webapplication.view_instructors where EventScheduleID in (";
            for (int i = 0; i < evScheduleId.Count; i++)
            {
                sqlTxt = $"{sqlTxt} @{i},";
            }
            sqlTxt += ");";
            sqlTxt = sqlTxt.ToString().Remove(sqlTxt.LastIndexOf(","), 1);
            cmd.CommandText = sqlTxt;
            cmd.Connection = Db.connection;
            try
            {
                for (int i = 0; i < evScheduleId.Count; i++)
                {
                    cmd.Parameters.AddWithValue("@" + i, evScheduleId[i]);
                }

                Db.connection.Open();
                MySqlConnector.MySqlDataReader reader = cmd.ExecuteReader();
                List<Instructors> lTrainer = new List<Instructors>();
                while (reader.Read())
                {
                    Instructors trainer = new Instructors()
                    {
                        Id = reader.IsDBNull("ContentID") ? null : reader["ContentID"].ToString(),
                        Email = reader.IsDBNull("Email") ? null : reader["Email"].ToString(),
                        FirstName = reader.IsDBNull("Name") ? null : reader["Name"].ToString(),
                        LastName = reader.IsDBNull("LastName") ? null : reader["LastName"].ToString()
                    };
                    lTrainer.Add(trainer);
                }
                reader.Close();

                return lTrainer;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                Db.connection.Close();
            }
        }
    }
}
