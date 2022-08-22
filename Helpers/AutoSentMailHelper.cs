using Microsoft.Extensions.Configuration;
using Nri_Webapplication_Backend.DTO;
using Nri_Webapplication_Backend.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
//using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using EASendMail;
using System.Net.Mail;
using Ical.Net.CalendarComponents;
using Ical.Net.DataTypes;
using Ical.Net;
using Calendar = Ical.Net.Calendar;
using Ical.Net.Serialization;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Http;
using System.Security.Policy;
using Microsoft.EntityFrameworkCore;

namespace Nri_Webapplication_Backend.Helpers
{
    public interface IAutoSentMailHelper
    {

        Task<Boolean> SendEmail(AutoMailAppoinmentModel autoMailAppoinmentModel, List<EmailLogModel> EmailLogsList, string baseUrl, string IcsUUID, string IsCancle = null);
        Task<Boolean> SendReqMod(RequesModerator e,List<string> email);
    }

    public class AutoSentMailHelper : IAutoSentMailHelper
    {
        public readonly IConfiguration config;
        public readonly ICommonHelper commonHelper;
        public readonly LinkGenerator _linkGenerator;



        public AutoSentMailHelper(IConfiguration config, ICommonHelper commonHelper, LinkGenerator linkGenerator)
        {
            this.config = config;
            this.commonHelper = commonHelper;
            this._linkGenerator = linkGenerator;

        }

        public async Task<Boolean> SendReqMod(RequesModerator e,List<string> email)
        {
            SmtpMail oMail = new SmtpMail("ES-E1582190613-00369-73VT53D16C732U23-7E4E7489E64T15CU");
            SmtpServer oServer = new SmtpServer(this.config["EmailSetting:Host"]);
            oServer.User = this.config["EmailSetting:Email"];
            oServer.Password = this.config["EmailSetting:Password"];
            oServer.Port = Convert.ToInt32(this.config["EmailSetting:Port"]);
            oServer.ConnectType = SmtpConnectType.ConnectTryTLS;

            oMail.From = this.config["EmailSetting:Email"];

            if (email != null)
            {
                foreach (var item in email)
                {
                    oMail.To.Add(new EASendMail.MailAddress(item));
                }
            }

            oMail.Subject = "YNU_" + e.ContentName;

            #region HTML GEN
            string mail_body = @"    <!doctype html>
<html>

<head>
    <title>
    </title>
    <meta http-equiv='Content-Language' content='th' />
    <meta http-equiv='Content-Type' content='text/html; charset=UTF-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1'>
    <style type='text/css'>
        .action-button {
            display: inline-block;
            border: solid;
            border-width: 2px;
            text-decoration: none;
            font-family: Segoe UI, Tahoma, Geneva, Verdana, sans-serif;
            font-weight: bold;
            padding: 12px 36px;
            width: 50%;
            border-radius: 10px;
        }
        .action-button-green {
            border-color: #3b0;
            color: #3b0 !important;
        }
        .action-button-green:hover {
            background-color: #3b0d;
            color: #fff !important;
        }
        .action-button-red {
            border-color: #f33;
            color: #f33 !important;
        }
        .action-button-red:hover {
            background-color: #f33d;
            color: #fff !important;
        }
    </style>
</head>

<body
    style='
        background-color: #fff;
        font-family: Segoe UI, Tahoma, Geneva, Verdana, sans-serif;
    '>
    <table width='100%'
        style='
            max-width: 600px;
        '>
        <tbody>
            <tr>
                <td colspan='2'>
                    <h3
                        style='
                            margin: 0px 0px 0px 0px;
                            max-width: 480px;
                            text-align: left;
                        '>
                        Dear Moderator manager
                    </h3></br>
                    <h4>มี event รอการ assign moderator รายละเอียด ดังนี้</h4>
                </td>
            </tr>
            <tr>
                <td colspan='2'>" +
                    "Business Type : " + e.BusinessType + "</br>" +
                    "Content Name : " + e.ContentName + "</br>" +
                    "Date and Time {dateChange}: " + e.StartDate.ToString("dd MMM yyyy hh:mm:ss tt") +
                    " - " + e.EndDate.ToString("dd MMM yyyy hh:mm:ss tt") + "</br>" +
                    "{oldDate}" +
                    "Room {roomChange}: " + e.RoomName + "</br>" +
                    "{oldRoom}" +
                    "Location : " + e.Location + "</br>" +
                    "Link : " + e.Link + "</br>" +
                    "Session : " + e.Session + "</br>" +
                    "Status : " + e.Status + "</br>" +
                    "Company : " + e.Company + "</br>" +
                    //"Project ID/Cost Center : " + e.ProjectIDoCost + "</br>" +
                    "Project Name : " + e.ProjectName + "</br>" +
                    "No Of Participant : " + e.Participant + "</br>" +
                    "Remark : " + e.Remark + "</br>" +
                @"</td>
            </tr>
            <tr>
                <td colspan='2'
                    style='
                        padding:18px 0px;
                        text-align: left;
                    '>
                    <a href='{GotoLink}' target='_self'
                        class='action-button action-button-green' style='text-align:center;'>
                        Link to assign moderator 
                    </a>
                </td>
            </tr>
        </tbody>
    </table>
</body>

</html>
              ";
            #endregion

            EASendMail.SmtpClient oSmtp = new EASendMail.SmtpClient();
            if (e.DateChange == 1)
            {
                //date change
                mail_body = mail_body.Replace("{dateChange}", "change to ").Replace("{oldDate}", "From : " + e.OldStartDate.ToString("dd MMM yyyy hh:mm:ss tt") + " - " + e.OldEndDate.ToString("dd MMM yyyy hh:mm:ss tt") + "</br>");
                //room change
                if (e.OldRoomName != "" && e.OldRoomName != null)
                {
                    mail_body = mail_body.Replace("{roomChange}","change to ").Replace("{oldRoom}", "From : " + e.OldRoomName + "</br>");
                }
                else
                {
                    mail_body = mail_body.Replace("{roomChange}", "").Replace("{oldRoom}", "");
                }
                mail_body = mail_body.Replace("{GotoLink}", e.GotoLink);
            }
            else
            {
                //room change
                if (e.OldRoomName != "" && e.OldRoomName != null)
                {
                    mail_body = mail_body.Replace("{roomChange}", "change to ").Replace("{oldRoom}", "From : " + e.OldRoomName + "</br>");
                }
                mail_body = mail_body.Replace("{GotoLink}", e.GotoLink).Replace("{dateChange}", "").Replace("{oldDate}", "").Replace("{roomChange}", "").Replace("{oldRoom}", "");
            }

            oMail.HtmlBody = mail_body;

            try
            {
                oSmtp.SendMail(oServer, oMail);

                return true;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        public async Task<Boolean> SendEmail(AutoMailAppoinmentModel autoMailAppoinmentModel, List<EmailLogModel> EmailLogsList, string baseUrl, string IcsUUID, string IsCancle = null)
        {


            SmtpMail oMail = new SmtpMail("ES-E1582190613-00369-73VT53D16C732U23-7E4E7489E64T15CU");
            SmtpServer oServer = new SmtpServer(this.config["EmailSetting:Host"]);
            oServer.User = this.config["EmailSetting:Email"];
            oServer.Password = this.config["EmailSetting:Password"];
            oServer.Port = Convert.ToInt32(this.config["EmailSetting:Port"]);
            oServer.ConnectType = SmtpConnectType.ConnectTryTLS;
            oMail.From = this.config["EmailSetting:Email"];

            EASendMail.SmtpClient oSmtp = new EASendMail.SmtpClient();

            var EventSchedule = autoMailAppoinmentModel.EventSchedule;
            var EventRoomMaster = autoMailAppoinmentModel.EventRoomMaster;
            var LearningTypeMaster = autoMailAppoinmentModel.LearningTypeMaster;
            var EventScheduleTrainer = autoMailAppoinmentModel.EventScheduleTrainer;
            var TrainerMaster = autoMailAppoinmentModel.TrainerMaster;
            var ContentBusinessType = autoMailAppoinmentModel.ContentBusinessType;
            var ContentMaster = autoMailAppoinmentModel.ContentMaster;

            oMail.To.Add(new EASendMail.MailAddress(TrainerMaster.Email));

            string MailBcc = System.IO.File.ReadAllText(@"C:\tempICS\bcc_list.txt");
            if (!string.IsNullOrEmpty(MailBcc))
            {
                oMail.Bcc.Add(MailBcc);
            }



            String mail_subject = "";
            var statusStr = autoMailAppoinmentModel.Status != null ? autoMailAppoinmentModel.Status.Status1 : null;
            if (ContentBusinessType.ContentBusinessTypeName.Equals("YourNextU") || ContentBusinessType.ContentBusinessTypeName.Equals("YourNextU Young"))
            {
                mail_subject = ContentBusinessType.ContentBusinessTypeAbbreviate + "_" + ContentMaster.ContentName;

            }
            else if (ContentBusinessType.ContentBusinessTypeName.Equals("SEAC") || ContentBusinessType.ContentBusinessTypeName.Equals("Contextualized Solution"))
            {

                mail_subject = ContentBusinessType.ContentBusinessTypeAbbreviate + "_" + statusStr + "_" + EventSchedule.Company + "_" + EventSchedule.Session;
            }
            else
            {

                mail_subject = ContentBusinessType.ContentBusinessTypeAbbreviate + "_" + ContentMaster.ContentName;
            }
            if (IsCancle != null)
            {
                oMail.Subject = "Cancel_" + mail_subject + "_" + TrainerMaster.Name + " " + (string.IsNullOrEmpty(TrainerMaster.LastName) == true ? "" : TrainerMaster.LastName.Substring(0, 1) + ".") + "_" + EventSchedule.EventScheduleId;
            }
            else
            {
                oMail.Subject = mail_subject + "_" + TrainerMaster.Name + " " + (string.IsNullOrEmpty(TrainerMaster.LastName) == true ? "" : TrainerMaster.LastName.Substring(0, 1) + ".") + "_" + EventSchedule.EventScheduleId;
            }



            #region HTML GEN
            string mail_body = @"    <!doctype html>
<html>

<head>
    <title>
    </title>
    <meta http-equiv='Content-Language' content='th' />
    <meta http-equiv='Content-Type' content='text/html; charset=UTF-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1'>
    <style type='text/css'>
        .action-button {
            display: inline-block;
            border: solid;
            border-width: 2px;
            text-decoration: none;
            font-family: Segoe UI, Tahoma, Geneva, Verdana, sans-serif;
            font-weight: bold;
            padding: 12px 36px;
            width: 50%;
            border-radius: 10px;
        }
        .action-button-green {
            border-color: #3b0;
            color: #3b0 !important;
        }
        .action-button-green:hover {
            background-color: #3b0d;
            color: #fff !important;
        }
        .action-button-red {
            border-color: #f33;
            color: #f33 !important;
        }
        .action-button-red:hover {
            background-color: #f33d;
            color: #fff !important;
        }
    </style>
</head>

<body
    style='
        background-color: #fff;
        font-family: Segoe UI, Tahoma, Geneva, Verdana, sans-serif;
    '>
    <table width='100%'
        style='
            max-width: 480px;
        '>
        <tbody>
            <tr>
                <td colspan='2'>
                    <h3
                        style='
                            margin: 0px 0px 0px 0px;
                            max-width: 480px;
                            text-align: center;
                        '>
                        SEAC Schedule Invitation
                    </h3>
                </td>
            </tr>
            <tr>
                <td colspan='2'>
                    You are invited to teach the class. To let’s us know your status Please confirm by click “Yes I can” or click “No I can’t” if you are not be able to teach this class.
                </td>
            </tr>
            <tr>
                <td width='50%'
                    style='
                        padding:18px 0px;
                        text-align: center;
                    '>
                    <a href='{AcceptLinkURL}' target='_self'
                        class='action-button action-button-green'>
                        YES, I can
                    </a>
                </td>
                <td width='50%'
                    style='
                        padding:18px 0px;
                        text-align: center;
                    '>
                    <a href='{DeclineLinkURL}' target='_self'
                        class='action-button action-button-red'>
                        NO, I can't
                    </a>
                </td>
            </tr>
            <tr>
                <td colspan='2'>
                    <div
                        style='
                            font-weight: 600;
                            font-size: 14px;
                            color: #777;
                            margin-left: 12px;
                            margin-right: 12px;
                        '>
                        {mail_body_detail}
                    </div>
                </td>
            </tr>
        </tbody>
    </table>
</body>

</html>
              ";
            #endregion


            var EventScheduleIdEncrypt = this.commonHelper.encrypt(EventSchedule.EventScheduleId.ToString());

            var TrainerEncrypt = this.commonHelper.encrypt(TrainerMaster.TrainerId.ToString());

            if (IsCancle != null)
            {
                mail_body = "{mail_body_detail}";
            }
            string mail_body_detail = string.Format("Business Type : {0}<br>Content Name : {1}<br>Date and Time : {2}<br>Room : {3}<br>Location : {4}<br>Link : {5}",
                                                  ContentBusinessType.ContentBusinessTypeName,
                                                  ContentMaster.ContentName,
                                                  EventSchedule.StartTime.ToString("dd MMM yyyy hh:mm:ss tt") + " - " + EventSchedule.EndTime?.ToString("dd MMM yyyy hh:mm:ss tt"),
                                                  EventRoomMaster.EventRoomName,
                                                  EventSchedule.Location,
                                                  EventSchedule.Link);
            mail_body_detail += "<br>" + string.Format("Session : {0}<br>Status : {1}<br>Company : {2}<br>Project Name : {3}<br>Project ID/Cost Center : {4}<br>No Of Participant : {5}<br>Book By : {6}<br>Remark : {7}",
                                                  EventSchedule.Session,
                                                  statusStr,
                                                  EventSchedule.Company,
                                                  EventSchedule.ProjectName,
                                                  EventSchedule.ProjectId,
                                                  EventSchedule.NoOfParticipant,
                                                  EventSchedule.BookBy,
                                                  EventSchedule.Remark);
            MemoryStream ms = new MemoryStream();

            int Sequence = 0;
            int AppSequence = 0;
            Sequence = EmailLogsList.Where(s => s.IsCancel == 0).ToList().Count;
            if (Sequence > 0)
            {
                AppSequence = Sequence - 1;
                IcsUUID = EmailLogsList.Where(s => s.IsCancel == 0).ToList().FirstOrDefault().UUID;
            }

            var AcceptURL = _linkGenerator.GetPathByAction("AcceptEventSchedule", "ICSMail", new { eventScheduleID = EventScheduleIdEncrypt, Trainer = TrainerEncrypt, isAccept = 1, UUID = IcsUUID }); //isAccept =1 
            AcceptURL = baseUrl + AcceptURL;

            var DeclineURL = _linkGenerator.GetPathByAction("AcceptEventSchedule", "ICSMail", new { eventScheduleID = EventScheduleIdEncrypt, Trainer = TrainerEncrypt, isAccept = 0, UUID = IcsUUID });
            DeclineURL = baseUrl + DeclineURL;

            if (IsCancle != null)
            {
                oMail.HtmlBody = mail_body.Replace("{mail_body_detail}", mail_body_detail);
                ms = GetIcsFile(autoMailAppoinmentModel, Sequence, AppSequence, IcsUUID, "CANCEL");
            }
            else
            {
                oMail.HtmlBody = mail_body.Replace("{AcceptLinkURL}", AcceptURL).Replace("{DeclineLinkURL}", DeclineURL).Replace("{mail_body_detail}", mail_body_detail);
                ms = GetIcsFile(autoMailAppoinmentModel, Sequence, AppSequence, IcsUUID);
            }



            var byts = ms.ToArray();
            ms.Dispose();

            oMail.AddAttachment("invite.ics", byts);

            oSmtp.SendMail(oServer, oMail);

            // using (var context = new seac_webapplicationContext())
            // {

            //     // Update emailLog
            //     var emailLog = await context.EmailLog.Where(o => o.EventScheduleId == autoMailAppoinmentModel.EventSchedule.EventScheduleId && o.TrainerId == autoMailAppoinmentModel.TrainerMaster.TrainerId && o.IsCancel == 0).FirstOrDefaultAsync();

            //     if (emailLog != null)
            //     {

            //         emailLog.IsCancel = 1;

            //         await context.SaveChangesAsync();
            //     }
            // }

            return true;
        }

        private MemoryStream GetIcsFile(AutoMailAppoinmentModel autoMailAppoinmentModel, int Sequence, int AppSequence, string IcsUUID, string METHOD = "REQUEST")
        {

            StringBuilder sb = new StringBuilder();
            string DateFormat = "yyyyMMddTHHmmssZ";

            string dtnow = DateTime.Now.ToUniversalTime().ToString(DateFormat);

            var EventSchedule = autoMailAppoinmentModel.EventSchedule;
            var EventRoomMaster = autoMailAppoinmentModel.EventRoomMaster;
            var LearningTypeMaster = autoMailAppoinmentModel.LearningTypeMaster;
            var EventScheduleTrainer = autoMailAppoinmentModel.EventScheduleTrainer;
            var TrainerMaster = autoMailAppoinmentModel.TrainerMaster;
            var ContentBusinessType = autoMailAppoinmentModel.ContentBusinessType;
            var ContentMaster = autoMailAppoinmentModel.ContentMaster;

            var EventStartDate = EventSchedule.StartTime.AddHours(-7).ToString(DateFormat, new CultureInfo("en-US"));
            var EventEndDate = EventSchedule.EndTime.Value.AddHours(-7).ToString(DateFormat, new CultureInfo("en-US"));



            String subject = "";
            String Detail = "";
            var statusStr = autoMailAppoinmentModel.Status != null ? autoMailAppoinmentModel.Status.Status1 : null;
            if (ContentBusinessType.ContentBusinessTypeName.Equals("YourNextU") || ContentBusinessType.ContentBusinessTypeName.Equals("YourNextU Young"))
            {
                subject = ContentBusinessType.ContentBusinessTypeAbbreviate + "_" + ContentMaster.ContentName;

            }
            else if (ContentBusinessType.ContentBusinessTypeName.Equals("SEAC") || ContentBusinessType.ContentBusinessTypeName.Equals("Contextualized Solution"))
            {
                subject = ContentBusinessType.ContentBusinessTypeAbbreviate + "_" + statusStr + "_" + EventSchedule.Company + "_" + EventSchedule.Session;
            }
            else
            {
                subject = ContentBusinessType.ContentBusinessTypeAbbreviate + "_" + ContentMaster.ContentName;
            }


            Detail += string.Format("Business Type :{0}<br>Content Name : {1}<br>Date and Time : {2}<br>Room : {3} <br>Location : {4}<br>Link : {5} <br>", ContentBusinessType.ContentBusinessTypeName,
               ContentMaster.ContentName, EventSchedule.StartTime + " - " + EventSchedule.EndTime, EventRoomMaster.EventRoomName, EventSchedule.Location, EventSchedule.Link);
            Detail += string.Format("Session Type : {0}<br>Status : {1}<br>Company : {2}<br>Project Name : {3}<br>No Of Participant : {4}<br>Book By : {5} <br>Remark : {6} ", EventSchedule.Session,
               statusStr, EventSchedule.Company, EventSchedule.ProjectName, EventSchedule.NoOfParticipant, EventSchedule.BookBy, EventSchedule.Remark);



            sb.AppendLine("BEGIN:VCALENDAR");
            sb.AppendLine("METHOD:" + METHOD);
            sb.AppendLine("PRODID:Microsoft Exchange Server 2010");
            sb.AppendLine("VERSION:2.0");
            sb.AppendLine("BEGIN:VEVENT");
            sb.AppendLine("ORGANIZER;CN=seasiacenter.com :mailto:scheduling@seasiacenter.com");
            sb.AppendLine("ATTENDEE;ROLE=REQ-PARTICIPANT;PARTSTAT=NEEDS-ACTION;RSVP=TRUE;CN=" + TrainerMaster.NickNameEn + ":mailto:" + TrainerMaster.Email + "");
            sb.AppendLine("DESCRIPTION;LANGUAGE=en-US:" + Detail);
            sb.AppendLine("UID:" + IcsUUID);
            sb.AppendLine("SUMMARY;LANGUAGE=en-US:" + subject);
            sb.AppendLine("DTSTART;TZID=SE Asia Standard Time:" + EventStartDate);
            sb.AppendLine("DTEND;TZID=SE Asia Standard Time:" + EventEndDate);
            sb.AppendLine("CLASS:PUBLIC");
            sb.AppendLine("PRIORITY:5");
            sb.AppendLine("DTSTAMP:" + dtnow);
            sb.AppendLine("TRANSP:OPAQUE");
            sb.AppendLine("STATUS:CONFIRMED");
            sb.AppendLine("SEQUENCE:" + Sequence.ToString());
            sb.AppendLine("LOCATION;LANGUAGE=en-US:" + EventSchedule.Location);
            sb.AppendLine("X-MICROSOFT-CDO-APPT-SEQUENCE:" + AppSequence.ToString());
            sb.AppendLine("X-MICROSOFT-CDO-OWNERAPPTID:-1449740316");
            sb.AppendLine("X-MICROSOFT-CDO-BUSYSTATUS:TENTATIVE");
            sb.AppendLine("X-MICROSOFT-CDO-INTENDEDSTATUS:BUSY");
            sb.AppendLine("X-MICROSOFT-CDO-ALLDAYEVENT:FALSE");
            sb.AppendLine("X-MICROSOFT-CDO-IMPORTANCE:1");
            sb.AppendLine("X-MICROSOFT-CDO-INSTTYPE:0");
            sb.AppendLine("X-MICROSOFT-DONOTFORWARDMEETING:FALSE");
            sb.AppendLine("X-MICROSOFT-DISALLOW-COUNTER:FALSE");
            sb.AppendLine("X-MICROSOFT-LOCATIONS:");
            sb.AppendLine("BEGIN:VALARM");
            sb.AppendLine("DESCRIPTION:REMINDER");
            sb.AppendLine("TRIGGER;RELATED=START:-PT15M");
            sb.AppendLine("ACTION:DISPLAY");
            sb.AppendLine("END:VALARM");
            sb.AppendLine("END:VEVENT");
            sb.AppendLine("END:VCALENDAR");

            var calendarBytes = Encoding.UTF8.GetBytes(sb.ToString());



            MemoryStream memoryStream = new MemoryStream(calendarBytes);
            return memoryStream;
        }





    }
}
