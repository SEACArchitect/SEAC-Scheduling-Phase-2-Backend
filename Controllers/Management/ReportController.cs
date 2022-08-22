using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using Nri_Webapplication_Backend.Helpers;
using Nri_Webapplication_Backend.Models;
using Nri_Webapplication_Backend.Managers;
using System.IO;
using ClosedXML.Excel;
using System.Globalization;

namespace Nri_Webapplication_Backend.Controllers.Management
{
    [Route("report")]
    [ApiController]
    public class ReportController : ControllerBase
    {
        public readonly IRespondHelper respondHelper;
        public readonly IReportManager reportManager;
        public readonly ICommonHelper commonHelper;

        public object Server { get; private set; }

        public ReportController(IRespondHelper respondHelper, IReportManager reportManager, ICommonHelper commonHelper)
        {
            this.respondHelper = respondHelper;
            this.reportManager = reportManager;
            this.commonHelper = commonHelper;
        }
        [HttpGet("contentsummary")]
        public IActionResult ContentSummaryReport(DateTime startDateTime, DateTime endDateTime, string businessTypeId = "", string contentFormatId = "", string eventCategoryId = "")
        {
            try
            {
                var jwtToken = Request.Headers["Authorization"];


                if (string.IsNullOrWhiteSpace(jwtToken))
                {
                    return Unauthorized(this.respondHelper.Respond(null, null, "Unauthorized", null, null));
                }

                //Check authen by jwt
                var result = this.commonHelper.DecodeJwtV2(jwtToken);

                if (result == null)
                {
                    return Unauthorized(this.respondHelper.Respond(null, null, "Unauthorized", null, null));
                }

                // Check permission
                if (result.roleId == (int)Roles.viewer)
                {
                    return NotFound(this.respondHelper.Respond(null, null, "Unauthorized", null, null));
                }

                // Logic for CourseSummary
                List<string> listBusinessTypeId = businessTypeId.Split('_', StringSplitOptions.RemoveEmptyEntries).ToList();
                List<string> listContentFormatId = contentFormatId.Split('_', StringSplitOptions.RemoveEmptyEntries).ToList();
                List<string> listEventCategoryId = eventCategoryId.Split('_', StringSplitOptions.RemoveEmptyEntries).ToList();
                var resultFindAll = this.reportManager.ContentSummary(startDateTime, endDateTime, listBusinessTypeId, listContentFormatId, listEventCategoryId);
                var data = new
                {
                    total = resultFindAll.Count(),
                    count = resultFindAll.Count(),
                    message = "Get all success",
                    data = resultFindAll
                };
                return Ok(this.respondHelper.Respond(data.total, data.count, data.message, null, data.data));
            }
            catch (Exception ex)
            {
                return BadRequest(this.respondHelper.Respond(0, 0, $"Error : {ex}", null, null));
            }
        }

        [HttpGet("contentsummaryfile")]
        public IActionResult ContentSummaryReportFile(DateTime startDateTime, DateTime endDateTime, string businessTypeId = "", string contentFormatId = "", string eventCategoryId = "")
        {
            try
            {
                var jwtToken = Request.Headers["Authorization"];


                if (string.IsNullOrWhiteSpace(jwtToken))
                {
                    return Unauthorized(this.respondHelper.Respond(null, null, "Unauthorized", null, null));
                }

                //Check authen by jwt
                var result = this.commonHelper.DecodeJwtV2(jwtToken);

                if (result == null)
                {
                    return Unauthorized(this.respondHelper.Respond(null, null, "Unauthorized", null, null));
                }

                // Check permission
                if (result.roleId == (int)Roles.viewer)
                {
                    return NotFound(this.respondHelper.Respond(null, null, "Unauthorized", null, null));
                }

                // Logic for CourseSummary
                List<string> listBusinessTypeId = businessTypeId.Split('_', StringSplitOptions.RemoveEmptyEntries).ToList();
                List<string> listContentFormatId = contentFormatId.Split('_', StringSplitOptions.RemoveEmptyEntries).ToList();
                List<string> listEventCategoryId = eventCategoryId.Split('_', StringSplitOptions.RemoveEmptyEntries).ToList();
                var resultFindAll = this.reportManager.ContentSummary(startDateTime, endDateTime, listBusinessTypeId, listContentFormatId, listEventCategoryId);

                var wbContent = new XLWorkbook();
                var wsContent = wbContent.Worksheets.Add("ContentSummary");
                //Header    
                int noOfYear = Convert.ToInt16(endDateTime.Year - startDateTime.Year) + 1;
                int noOfMonth = ((endDateTime.Year - startDateTime.Year) * 12) + ((endDateTime.Month - startDateTime.Month)+1);
                int noOfStartMonth = startDateTime.Month;
                int noOfStartYear = startDateTime.Year;
                int idxMonth = 0;
                int idxYear = 0;
                wsContent.Cell(1, 1).Value = "Content Name";
                wsContent.Range(1, 1, 2, 1).Merge();
                wsContent.Cell(1, 1).Style.Alignment.SetVertical(XLAlignmentVerticalValues.Top);
                wsContent.Cell(1, 1).Style.Font.SetBold();
                string monthName = "";
                for (int x = 0; x < noOfMonth; x++)
                {
                    idxMonth = noOfStartMonth + x;
                    if ((noOfStartMonth + x) > 12)
                    {
                        idxYear = (noOfStartMonth + x) / 12;
                        if ((noOfStartMonth + x) == (12 * idxYear))
                        {
                            idxMonth = 12;
                            idxYear = idxYear - 1;
                        }
                        else
                        {
                            idxMonth = (noOfStartMonth + x) - (12 * idxYear);
                        }
                    }
                    wsContent.Cell(1, 2 + x).Value = noOfStartYear + idxYear;
                    wsContent.Cell(1, 2 + x).Style.Font.SetBold();
                    monthName = "'" + CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(idxMonth);
                    wsContent.Cell(2, 2 + x).Value = monthName;
                    wsContent.Cell(2, 2 + x).Style.Font.SetBold();
                }

                //Input all content info in each row
                var DistinctItems = resultFindAll.GroupBy(x => x.contentID).Select(y => y.First()).ToList();
                int idx = 0;
                for (int x = 0; x < DistinctItems.Count(); x++)
                {
                    wsContent.Cell(3 + x, 1).Value = DistinctItems[x].course;
                    foreach (var details in resultFindAll)
                    {
                        for (int i = 0; i < noOfMonth; i++)
                        {
                            idx = noOfStartMonth + i;
                            if ((noOfStartMonth + i) > 12)
                            {
                                idx = (noOfStartMonth + i) - 12;
                            }if (details.course == DistinctItems[x].course && details.monthName == wsContent.Cell(2, 2 + i).Value.ToString() && details.year == wsContent.Cell(1, 2 + i).Value.ToString())
                            
                            {
                                wsContent.Cell(3 + x, 2 + i).Value = details.numberOfCourse;
                            }
                        }
                    }
                }

                //Merge Header
                int idxStartMerge = 2;
                int countColumn = 1;
                bool xM = true;
                for (int i = 0; i <= noOfMonth; i++)
                {
                    if (i == 0)
                    {
                        xM = true;
                    }
                    else
                    {
                        if (wsContent.Cell(1, 2 + i).Value.ToString() == wsContent.Cell(1, 2 + i - 1).Value.ToString())
                        {
                            xM = true;
                            countColumn++;
                        }
                        else
                        {
                            xM = false;
                            if (xM == false && countColumn > 1)
                            {
                                wsContent.Range(1, idxStartMerge, 1, i + 1).Merge();
                                wsContent.Range(1, idxStartMerge, 1, i + 1).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
                            }
                            idxStartMerge = i + 2;
                        }
                    }
                }
                //input 0 to empty cells
                for (int x = 3; x < DistinctItems.Count() + 3; x++)
                {
                    for (int i = 2; i < noOfMonth+2; i++)
                    {
                        if (wsContent.Cell(x, i).Value.ToString() == "")
                        {
                            wsContent.Cell(x, i).Value = 0;
                        }
                    }
                }

                wsContent.Columns().AdjustToContents();
                using (MemoryStream stream = new MemoryStream())
                {
                    wbContent.SaveAs(stream);                    
                    return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "contentsummary_" + DateTime.Now.ToString("yyyy-MM-ddTHH-mm-ss") + ".xlsx");
                }
            }
            catch (Exception ex)
            {
                return BadRequest(this.respondHelper.Respond(0, 0, $"Error : {ex}", null, null));
            }
        }

        [HttpGet("trainersummary")]
        public IActionResult TrainerSummaryReport(DateTime startDateTime, DateTime endDateTime)
        {
            try
            {
                var jwtToken = Request.Headers["Authorization"];


                if (string.IsNullOrWhiteSpace(jwtToken))
                {
                    return Unauthorized(this.respondHelper.Respond(null, null, "Unauthorized", null, null));
                }

                //Check authen by jwt
                var result = this.commonHelper.DecodeJwtV2(jwtToken);

                if (result == null)
                {
                    return Unauthorized(this.respondHelper.Respond(null, null, "Unauthorized", null, null));
                }

                // Check permission
                if (result.roleId == (int)Roles.viewer)
                {
                    return NotFound(this.respondHelper.Respond(null, null, "Unauthorized", null, null));
                }
              

                var resultFindAll = this.reportManager.TrainerSummary(startDateTime, endDateTime);
                var data = new
                {
                    total = resultFindAll.Count(),
                    count = resultFindAll.Count(),
                    message = "Get all success",
                    data = resultFindAll
                };
                return Ok(this.respondHelper.Respond(data.total, data.count, data.message, null, data.data));
            }
            catch (Exception ex)
            {
                return BadRequest(this.respondHelper.Respond(0, 0, $"Error : {ex}", null, null));
            }
        }

        [HttpGet("trainersummaryfile")]
        public IActionResult TrainerSummaryReportFile(DateTime startDateTime, DateTime endDateTime)
        {
            try
            {
                var jwtToken = Request.Headers["Authorization"];


                if (string.IsNullOrWhiteSpace(jwtToken))
                {
                    return Unauthorized(this.respondHelper.Respond(null, null, "Unauthorized", null, null));
                }

                //Check authen by jwt
                var result = this.commonHelper.DecodeJwtV2(jwtToken);

                if (result == null)
                {
                    return Unauthorized(this.respondHelper.Respond(null, null, "Unauthorized", null, null));
                }

                // Check permission
                if (result.roleId == (int)Roles.viewer)
                {
                    return NotFound(this.respondHelper.Respond(null, null, "Unauthorized", null, null));
                }

                var resultFindAll = this.reportManager.TrainerSummary(startDateTime, endDateTime);
                var wbTrainer = new XLWorkbook();
                var wsTrainer = wbTrainer.Worksheets.Add("TrainerSummary");
                //Header
                wsTrainer.Cell(1, 1).Value = "Start Time";
                wsTrainer.Cell(1, 2).Value = "End Time";
                wsTrainer.Cell(1, 3).Value = "Trainer Type";
                wsTrainer.Cell(1, 4).Value = "Contract Type";
                wsTrainer.Cell(1, 5).Value = "Trainer Name";
                wsTrainer.Cell(1, 6).Value = "Booking Type";
                wsTrainer.Cell(1, 7).Value = "Content Format";
                wsTrainer.Cell(1, 8).Value = "Content Name";
                wsTrainer.Cell(1, 9).Value = "Session Name";
                wsTrainer.Cell(1, 10).Value = "Is Billable";
                wsTrainer.Cell(1, 11).Value = "Actual Mins";
                wsTrainer.Cell(1, 12).Value = " Actual Hours";
                wsTrainer.Cell(1, 13).Value = "Calculate Day";
                wsTrainer.Cell(1, 14).Value = "Workload";
                wsTrainer.Cell(1, 15).Value = "Billable day";
                wsTrainer.Cell(1, 16).Value = "Project ID/Cost Center";
                wsTrainer.Cell(1, 17).Value = "Company";
                wsTrainer.Cell(1, 18).Value = "Book By";
                wsTrainer.Cell(1, 19).Value = "Year";
                wsTrainer.Cell(1, 20).Value = "Month";
                var sourceTrainer = wsTrainer.Cell(1, 1).InsertTable(resultFindAll, "TrainerSummary", true);
                var header = wsTrainer.Range(1, 1, 1, 20);
                var dataRangeTrainer = wsTrainer.Range(header.FirstCell(), wsTrainer.Cell(resultFindAll.Count + 1, 20));
                //pivot sheet#1
                var ptSheetTrainer1 = wbTrainer.Worksheets.Add("PivotTable1");
                ptSheetTrainer1.Cell(1,1).Value = "Billable Summary as of " + DateTime.Now.Date.ToString("dd-MMM-yyyy");
                ptSheetTrainer1.Cell(1,1).Style.Font.Bold = true; 
                var ptTrainer1 = ptSheetTrainer1.PivotTables.AddNew("PivotTable1", ptSheetTrainer1.Cell(3, 1), dataRangeTrainer);
                ptTrainer1.RowLabels.Add("contractType");
                ptTrainer1.RowLabels.Add("trainerName");
                ptTrainer1.ColumnLabels.Add("year");
                ptTrainer1.ColumnLabels.Add("month");
                ptTrainer1.Values.Add("billableDay");
                ptTrainer1.ReportFilters.Add("trainerType");
                ptTrainer1.Subtotals = XLPivotSubtotals.AtBottom;
                ptSheetTrainer1.Column(1).Width = 20;
                ptSheetTrainer1.Column(1).Width = 40;
                //pivot sheet#2
                var ptSheetTrainer2 = wbTrainer.Worksheets.Add("PivotTable2");
                ptSheetTrainer2.Cell(1,1).Value = "Billable Summary by Business Type as of " + DateTime.Now.Date.ToString("dd-MMM-yyyy"); 
                ptSheetTrainer2.Cell(1,1).Style.Font.Bold = true; 
                var ptTrainer2 = ptSheetTrainer2.PivotTables.AddNew("PivotTable2", ptSheetTrainer2.Cell(3, 1), dataRangeTrainer);
                ptTrainer2.RowLabels.Add("contractType");
                ptTrainer2.RowLabels.Add("trainerName");
                ptTrainer2.ColumnLabels.Add("year");
                ptTrainer2.ColumnLabels.Add("month");
                ptTrainer2.ColumnLabels.Add("bookingType");
                ptTrainer2.Values.Add("billableDay");
                ptTrainer2.ReportFilters.Add("trainerType");
                ptTrainer2.Subtotals = XLPivotSubtotals.AtBottom;
                ptSheetTrainer2.Column(1).Width = 20;
                ptSheetTrainer2.Column(1).Width = 40;

                //pivot sheet#3
                var ptSheetTrainer3 = wbTrainer.Worksheets.Add("PivotTable3");
                ptSheetTrainer3.Cell(1,1).Value = "Workload Summary as of " + DateTime.Now.Date.ToString("dd-MMM-yyyy"); 
                ptSheetTrainer3.Cell(1,1).Style.Font.Bold = true; 
                var ptTrainer3 = ptSheetTrainer3.PivotTables.AddNew("PivotTable3", ptSheetTrainer3.Cell(3, 1), dataRangeTrainer);
                ptTrainer3.RowLabels.Add("contractType");
                ptTrainer3.RowLabels.Add("trainerName");
                ptTrainer3.ColumnLabels.Add("year");
                ptTrainer3.ColumnLabels.Add("month");
                ptTrainer3.ColumnLabels.Add("bookingType");
                ptTrainer3.Values.Add("workload");
                ptTrainer3.ReportFilters.Add("trainerType");
                ptTrainer3.Subtotals = XLPivotSubtotals.AtBottom;
                ptSheetTrainer3.Column(1).Width = 20;
                ptSheetTrainer3.Column(1).Width = 40;

                wsTrainer.Column(22).Delete();
                wsTrainer.Column(21).Delete();
                wsTrainer.Columns().AdjustToContents();
                using (MemoryStream stream = new MemoryStream())
                {
                    wbTrainer.SaveAs(stream);                    
                    return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "trainersummary_" + DateTime.Now.ToString("yyyy-MM-ddTHH-mm-ss") + ".xlsx");
                }
            }
            catch (Exception ex)
            {
                return BadRequest(this.respondHelper.Respond(0, 0, $"Error : {ex}", null, null));
            }
        }

        [HttpGet("moderatorsummary")]
        public IActionResult ModeratorSummaryReport(DateTime startDateTime, DateTime endDateTime)
        {
            try
            {
                var jwtToken = Request.Headers["Authorization"];


                if (string.IsNullOrWhiteSpace(jwtToken))
                {
                    return Unauthorized(this.respondHelper.Respond(null, null, "Unauthorized", null, null));
                }

                //Check authen by jwt
                var result = this.commonHelper.DecodeJwtV2(jwtToken);

                if (result == null)
                {
                    return Unauthorized(this.respondHelper.Respond(null, null, "Unauthorized", null, null));
                }

                // Check permission
                if (result.roleId == (int)Roles.viewer)
                {
                    return NotFound(this.respondHelper.Respond(null, null, "Unauthorized", null, null));
                }

                var resultFindAll = this.reportManager.ModeratorSummary(startDateTime, endDateTime);
                var data = new
                {
                    total = resultFindAll.Count(),
                    count = resultFindAll.Count(),
                    message = "Get all success",
                    data = resultFindAll
                };
                return Ok(this.respondHelper.Respond(data.total, data.count, data.message, null, data.data));
            }
            catch (Exception ex)
            {
                return BadRequest(this.respondHelper.Respond(0, 0, $"Error : {ex}", null, null));
            }
        }

        [HttpGet("moderatorsummaryfile")]
        public IActionResult ModeratorSummaryReportFile(DateTime startDateTime, DateTime endDateTime)
        {
            try
            {
                var jwtToken = Request.Headers["Authorization"];


                if (string.IsNullOrWhiteSpace(jwtToken))
                {
                    return Unauthorized(this.respondHelper.Respond(null, null, "Unauthorized", null, null));
                }

                //Check authen by jwt
                var result = this.commonHelper.DecodeJwtV2(jwtToken);

                if (result == null)
                {
                    return Unauthorized(this.respondHelper.Respond(null, null, "Unauthorized", null, null));
                }

                // Check permission
                if (result.roleId == (int)Roles.viewer)
                {
                    return NotFound(this.respondHelper.Respond(null, null, "Unauthorized", null, null));
                }

                var resultFindAll = this.reportManager.ModeratorSummary(startDateTime, endDateTime);
                var wbModerator = new XLWorkbook();
                var wsModerator = wbModerator.Worksheets.Add("ModeratorSummary");
                //Header
                wsModerator.Cell(1, 1).Value = "Start Time";
                wsModerator.Cell(1, 2).Value = "End Time";
                wsModerator.Cell(1, 3).Value = "Moderator Type";
                wsModerator.Cell(1, 4).Value = "Contract Type";
                wsModerator.Cell(1, 5).Value = "Moderator Name";
                wsModerator.Cell(1, 6).Value = "Nickname (EN)";
                wsModerator.Cell(1, 7).Value = "Booking Type";
                wsModerator.Cell(1, 8).Value = "Content Name";
                wsModerator.Cell(1, 9).Value = "trainer name";
                wsModerator.Cell(1, 10).Value = "No.of hrs";
                wsModerator.Cell(1, 11).Value = "No.of event";
                wsModerator.Cell(1, 12).Value = "Year";
                wsModerator.Cell(1, 13).Value = "Month";
                var sourceModerator = wsModerator.Cell(1, 1).InsertTable(resultFindAll, "ModeratorSummary", true);
                var headerModerator = wsModerator.Range(1, 1, 1, 13);
                var dataRangeModerator = wsModerator.Range(headerModerator.FirstCell(), wsModerator.Cell(resultFindAll.Count + 1, 13));
                var ptSheetModerator = wbModerator.Worksheets.Add("PivotTable");
                var ptModerator = ptSheetModerator.PivotTables.AddNew("PivotTable", ptSheetModerator.Cell(1, 1), dataRangeModerator);
                //ptSheetModerator.Columns().AdjustToContents();
                ptSheetModerator.Column(1).Width = 40;
                ptModerator.RowLabels.Add("moderatorName");
                ptModerator.ColumnLabels.Add("year");
                ptModerator.ColumnLabels.Add("month");
                ptModerator.ColumnLabels.Add("bookingType");
                ptModerator.Values.Add("noOfSession");
                ptModerator.ReportFilters.Add("moderatorType");
                ptModerator.ReportFilters.Add("contractType");
                wsModerator.Columns().AdjustToContents();
                using (MemoryStream stream = new MemoryStream())
                {
                    wbModerator.SaveAs(stream);                    
                    return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "moderatorsummary_" + DateTime.Now.ToString("yyyy-MM-ddTHH-mm-ss") + ".xlsx");
                }
            }
            catch (Exception ex)
            {
                return BadRequest(this.respondHelper.Respond(0, 0, $"Error : {ex}", null, null));
            }
        }

        [HttpGet("contentmaster")]
        public IActionResult ContentMasterReport(string ContentName,string BusinessType, string LearningType, string ContentFormat)
        {
            try
            {
                var jwtToken = Request.Headers["Authorization"];

                if (string.IsNullOrWhiteSpace(jwtToken))
                {
                    return Unauthorized(this.respondHelper.Respond(null, null, "Unauthorized", null, null));
                }

                //Check authen by jwt
                var result = this.commonHelper.DecodeJwtV2(jwtToken);

                if (result == null)
                {
                    return Unauthorized(this.respondHelper.Respond(null, null, "Unauthorized", null, null));
                }

                // Check permission
                if (result.roleId == (int)Roles.viewer)
                {
                    return NotFound(this.respondHelper.Respond(null, null, "Unauthorized", null, null));
                }

                var resultSearch = this.reportManager.ContentMasterTemplate(ContentName,BusinessType,LearningType,ContentFormat);
                var data = new
                {
                    total = resultSearch.Count(),
                    count = resultSearch.Count(),
                    message = "Get all success",
                    data = resultSearch
                };

                return Ok(this.respondHelper.Respond(data.total, data.count, data.message, null, data.data));
            }
            catch (Exception ex)
            {
                return BadRequest(this.respondHelper.Respond(0, 0, $"Error : {ex}", null, null));
                throw ex;
            }
        }

        [HttpGet("contentmasterfile")]
        //public IActionResult ContentMasterReportFile(SearchContentMasterTemplate entity)
        public IActionResult ContentMasterReportFile(string ContentName, string BusinessType, string LearningType, string ContentFormat)
        {
            try
            {
                var jwtToken = Request.Headers["Authorization"];

                if (string.IsNullOrWhiteSpace(jwtToken))
                {
                    return Unauthorized(this.respondHelper.Respond(null, null, "Unauthorized", null, null));
                }

                //Check authen by jwt
                var result = this.commonHelper.DecodeJwtV2(jwtToken);

                if (result == null)
                {
                    return Unauthorized(this.respondHelper.Respond(null, null, "Unauthorized", null, null));
                }

                // Check permission
                if (result.roleId == (int)Roles.viewer)
                {
                    return NotFound(this.respondHelper.Respond(null, null, "Unauthorized", null, null));
                }

                var resultSearch = this.reportManager.ContentMasterTemplate(ContentName, BusinessType, LearningType, ContentFormat);

                var wbContentMaster = new XLWorkbook();
                var wsContentMaster = wbContentMaster.Worksheets.Add("ContentMaster");                
                var sourceContentMaster = wsContentMaster.Cell(1, 1).InsertTable(resultSearch, "ContentMaster", true);
                wsContentMaster.Columns().AdjustToContents();
                using MemoryStream stream = new MemoryStream();
                wbContentMaster.SaveAs(stream);
                return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "ContentMaster_" + DateTime.Now.ToString("yyyy-MM-ddTHH-mm-ss") + ".xlsx");
            }
            catch (Exception ex)
            {
                return BadRequest(this.respondHelper.Respond(0, 0, $"Error : {ex}", null, null));
                throw ex;
            }
        }

        [HttpGet("trainerandmoderator")]
        public IActionResult TrainerAndModerator(string Name, string NickNameEn, string NickNameTh,string Email, string Contract,string Type)
        {
            try
            {
                var jwtToken = Request.Headers["Authorization"];

                if (string.IsNullOrWhiteSpace(jwtToken))
                {
                    return Unauthorized(this.respondHelper.Respond(null, null, "Unauthorized", null, null));
                }

                //Check authen by jwt
                var result = this.commonHelper.DecodeJwtV2(jwtToken);

                if (result == null)
                {
                    return Unauthorized(this.respondHelper.Respond(null, null, "Unauthorized", null, null));
                }

                // Check permission
                if (result.roleId == (int)Roles.viewer)
                {
                    return NotFound(this.respondHelper.Respond(null, null, "Unauthorized", null, null));
                }

                var resultSearch = this.reportManager.TrainerAndMod(Name, NickNameEn, NickNameTh, Email, Contract, Type);
                var data = new
                {
                    total = resultSearch.Count(),
                    count = resultSearch.Count(),
                    message = "Get all success",
                    data = resultSearch
                };

                return Ok(this.respondHelper.Respond(data.total, data.count, data.message, null, data.data));
            }
            catch (Exception ex)
            {
                return BadRequest(this.respondHelper.Respond(0, 0, $"Error : {ex}", null, null));
                throw ex;
            }
        }

        [HttpGet("trainerandmoderatorfile")]
        public IActionResult TrainerAndModeratorFile(string Name, string NickNameEn, string NickNameTh, string Email, string Contract, string Type)
        {
            try
            {
                var jwtToken = Request.Headers["Authorization"];

                if (string.IsNullOrWhiteSpace(jwtToken))
                {
                    return Unauthorized(this.respondHelper.Respond(null, null, "Unauthorized", null, null));
                }

                //Check authen by jwt
                var result = this.commonHelper.DecodeJwtV2(jwtToken);

                if (result == null)
                {
                    return Unauthorized(this.respondHelper.Respond(null, null, "Unauthorized", null, null));
                }

                // Check permission
                if (result.roleId == (int)Roles.viewer)
                {
                    return NotFound(this.respondHelper.Respond(null, null, "Unauthorized", null, null));
                }

                var resultSearch = this.reportManager.TrainerAndMod(Name, NickNameEn, NickNameTh, Email, Contract, Type);

                var wbTrainerAndMod = new XLWorkbook();
                var wsTrainerAndMod = wbTrainerAndMod.Worksheets.Add("TrainerAndMod");
                var sourceTrainerAndMod = wsTrainerAndMod.Cell(1, 1).InsertTable(resultSearch, "TrainerAndMod", true);
                wsTrainerAndMod.Columns().AdjustToContents();
                using MemoryStream stream = new MemoryStream();
                wbTrainerAndMod.SaveAs(stream);
                return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "TrainerAndModerator_" + DateTime.Now.ToString("yyyy-MM-ddTHH-mm-ss") + ".xlsx");
            }
            catch (Exception ex)
            {
                return BadRequest(this.respondHelper.Respond(0, 0, $"Error : {ex}", null, null));
                throw ex;
            }
        }

        [HttpGet("certifiedtrainer")]
        public IActionResult CertTrainer(string ContentName, string BusinessType, string LearningType, string TrainerName,string TrainerType)
        {
            try
            {
                var jwtToken = Request.Headers["Authorization"];

                if (string.IsNullOrWhiteSpace(jwtToken))
                {
                    return Unauthorized(this.respondHelper.Respond(null, null, "Unauthorized", null, null));
                }

                //Check authen by jwt
                var result = this.commonHelper.DecodeJwtV2(jwtToken);

                if (result == null)
                {
                    return Unauthorized(this.respondHelper.Respond(null, null, "Unauthorized", null, null));
                }

                // Check permission
                if (result.roleId == (int)Roles.viewer)
                {
                    return NotFound(this.respondHelper.Respond(null, null, "Unauthorized", null, null));
                }

                var resultSearch = this.reportManager.CerTrainer(ContentName, BusinessType,LearningType, TrainerName, TrainerType);
                var data = new
                {
                    total = resultSearch.Count(),
                    count = resultSearch.Count(),
                    message = "Get all success",
                    data = resultSearch
                };

                return Ok(this.respondHelper.Respond(data.total, data.count, data.message, null, data.data));
            }
            catch (Exception ex)
            {
                return BadRequest(this.respondHelper.Respond(0, 0, $"Error : {ex}", null, null));
                throw ex;
            }
        }

        [HttpGet("certifiedtrainerfile")]
        //public IActionResult CertTrainerFile(SearchCertifiedTrainer entity)
        public IActionResult CertTrainerFile(string ContentName, string BusinessType, string LearningType, string TrainerName, string TrainerType)
        {
            try
            {
                var jwtToken = Request.Headers["Authorization"];

                if (string.IsNullOrWhiteSpace(jwtToken))
                {
                    return Unauthorized(this.respondHelper.Respond(null, null, "Unauthorized", null, null));
                }

                //Check authen by jwt
                var result = this.commonHelper.DecodeJwtV2(jwtToken);

                if (result == null)
                {
                    return Unauthorized(this.respondHelper.Respond(null, null, "Unauthorized", null, null));
                }

                // Check permission
                if (result.roleId == (int)Roles.viewer)
                {
                    return NotFound(this.respondHelper.Respond(null, null, "Unauthorized", null, null));
                }

                var resultSearch = this.reportManager.CerTrainer(ContentName, BusinessType, LearningType, TrainerName, TrainerType);

                var wbCertTrainer = new XLWorkbook();
                var wsCertTrainer = wbCertTrainer.Worksheets.Add("CertTrainer");
                var sourceCertTrainer = wsCertTrainer.Cell(1, 1).InsertTable(resultSearch, "CertTrainer", true);
                wsCertTrainer.Columns().AdjustToContents();
                using MemoryStream stream = new MemoryStream();
                wbCertTrainer.SaveAs(stream);
                return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "CertTrainer_" + DateTime.Now.ToString("yyyy-MM-ddTHH-mm-ss") + ".xlsx");
            }
            catch (Exception ex)
            {
                return BadRequest(this.respondHelper.Respond(0, 0, $"Error : {ex}", null, null));
                throw ex;
            }
        }

        [HttpGet("schedulecentral")]
        public IActionResult ScheduleCentral(DateTime startDate, DateTime endDate, int businessType)
        {
            try
            {
                var jwtToken = Request.Headers["Authorization"];

                if (string.IsNullOrWhiteSpace(jwtToken))
                {
                    return Unauthorized(this.respondHelper.Respond(null, null, "Unauthorized", null, null));
                }

                //Check authen by jwt
                var result = this.commonHelper.DecodeJwtV2(jwtToken);

                if (result == null)
                {
                    return Unauthorized(this.respondHelper.Respond(null, null, "Unauthorized", null, null));
                }

                // Check permission
                if (result.roleId == (int)Roles.viewer)
                {
                    return NotFound(this.respondHelper.Respond(null, null, "Unauthorized", null, null));
                }

                var courseSession = this.reportManager.CourseSessions(startDate, endDate,businessType);
                if (courseSession.Count == 0)
                {
                    return Ok(this.respondHelper.Respond(null, null, "The Report you searched for was not found", null, null));
                }

                List<int> evScheduleId = new List<int>();
                foreach (var item in courseSession)
                {
                    evScheduleId.Add((int)item.EventScheduleID);
                }

                var courseOutline = this.reportManager.CourseOutline(evScheduleId);
                var instructors = this.reportManager.Instructors(evScheduleId);

                Dictionary<string, object> lOb = new Dictionary<string, object>();
                lOb.Add("CourseSession", courseSession);
                lOb.Add("CourseOutline", courseOutline);
                lOb.Add("Instructors", instructors);
                var data = new
                {
                    total = courseSession.Count(),
                    count = courseSession.Count(),
                    message = "Get all success",
                    data = lOb
                };
                return Ok(this.respondHelper.Respond(data.total, data.count, data.message, null, data.data));
            }
            catch (Exception ex)
            {
                return BadRequest(this.respondHelper.Respond(0, 0, $"Error : {ex}", null, null));
                throw ex;
            }
        }
        [HttpGet("schedulecentralfile")]
        public IActionResult ScheduleCentralFile(DateTime startDate, DateTime endDate,int businessType)
        {
            try
            {
                var jwtToken = Request.Headers["Authorization"];

                if (string.IsNullOrWhiteSpace(jwtToken))
                {
                    return Unauthorized(this.respondHelper.Respond(null, null, "Unauthorized", null, null));
                }

                //Check authen by jwt
                var result = this.commonHelper.DecodeJwtV2(jwtToken);

                if (result == null)
                {
                    return Unauthorized(this.respondHelper.Respond(null, null, "Unauthorized", null, null));
                }

                // Check permission
                if (result.roleId == (int)Roles.viewer)
                {
                    return NotFound(this.respondHelper.Respond(null, null, "Unauthorized", null, null));
                }

                // get data for report
                var courseSession = this.reportManager.CourseSessions(startDate, endDate, businessType);
                if (courseSession.Count == 0)
                {
                    return Ok(this.respondHelper.Respond(null, null, "The Report you searched for was not found", null, null));
                }

                var courseSessionFile = this.reportManager.CourseSessionsFile(startDate, endDate, businessType);

                List<int> evScheduleId = new List<int>();
                foreach (var item in courseSession)
                {
                    evScheduleId.Add((int)item.EventScheduleID);
                }

                var courseOutlineFile = this.reportManager.CourseOutlineFile(evScheduleId);
                var instructors = this.reportManager.Instructors(evScheduleId);

                //create workbook
                var wbScheduleCentral = new XLWorkbook();
                //work sheet course session
                var wscourseSessionFile = wbScheduleCentral.Worksheets.Add("course_session");
                wscourseSessionFile.Cell(1, 1).InsertTable(courseSessionFile, "course_session", true);
                wscourseSessionFile.Columns().AdjustToContents();
                //work sheet course outline
                var wscourseOutlineFile = wbScheduleCentral.Worksheets.Add("course_outline");
                wscourseOutlineFile.Cell(1, 1).InsertTable(courseOutlineFile, "course_outline", true);
                wscourseOutlineFile.Columns().AdjustToContents();
                //work sheet instructors
                var wsinstructors = wbScheduleCentral.Worksheets.Add("instructors");
                wsinstructors.Cell(1, 1).InsertTable(instructors, "instructors", true);
                wsinstructors.Columns().AdjustToContents();
                //save workbook
                using MemoryStream stream = new MemoryStream();
                wbScheduleCentral.SaveAs(stream);
                return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "schedule_central_" + DateTime.Now.ToString("yyyy-MM-ddTHH-mm-ss") + ".xlsx");

            }
            catch (Exception ex)
            {
                return BadRequest(this.respondHelper.Respond(0, 0, $"Error : {ex}", null, null));
                throw ex;
            }
        }
    }
}
