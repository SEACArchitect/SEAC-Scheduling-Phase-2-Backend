using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Nri_Webapplication_Backend.Managers;
using Nri_Webapplication_Backend.Helpers;
using Nri_Webapplication_Backend.Models;

namespace Nri_Webapplication_Backend.Controllers.Management
{
    [Route("mywork")]
    [ApiController]
    public class MyWorkController : ControllerBase
    {
        public readonly IRespondHelper respondHelper;
        public readonly ICommonHelper commonHelper;
        public readonly IMyworkManager myworkManager;
        public readonly IUserManager userManager;
        public readonly ITrainerManager trainerManager;

        public MyWorkController(IRespondHelper respondHelper, ICommonHelper commonHelper, IMyworkManager myworkManager, IUserManager userManager, ITrainerManager trainerManager)
        {
            this.userManager = userManager;
            this.respondHelper = respondHelper;
            this.commonHelper = commonHelper;
            this.myworkManager = myworkManager;
            this.trainerManager = trainerManager;
        }

        [HttpGet("")]
        public async Task<IActionResult> getMyWork(DateTime startDateTime, DateTime endDateTime)
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

                if (result.roleId == (int)Roles.admin || result.roleId == (int)Roles.adminReadOnly)
                {
                    return NotFound(this.respondHelper.Respond(null, null, "Unauthorized", null, null));
                }

                //Check user (Viewer)
                var chkViewer = await this.userManager.FindByIDViewerAsync(Convert.ToInt32(result.userId));

                //if (string.IsNullOrWhiteSpace(chkViewer))
                // {
                //     return Ok(this.respondHelper.Respond(null, null, "You cannot view work", null, null));
                // }

                //Check trainer or moderator

                var chkTrainer = await this.trainerManager.FindChkTrainer(chkViewer);

               // if (chkTrainer == null)
               // {
               //     return Ok(this.respondHelper.Respond(null,null,"Cannot find trainer in table trainer master",null,null));
              //  }

                List<MyworkModel> myWorkModel = await this.myworkManager.FindTrainerAsync(chkTrainer.TrainerId, startDateTime, endDateTime, chkTrainer.IsTrainer);
          

                var data = new
                {
                    total = myWorkModel.Count(),
                    count = myWorkModel.Count(),
                    message = "Get all success",
                    data = myWorkModel
                };

                return Ok(this.respondHelper.Respond(data.total, data.count, data.message, null, data.data));
            }
            catch (Exception ex)
            {
                return BadRequest(this.respondHelper.Respond(0, 0, $"Error : {ex}", null, null));
            }
        }

    }
}
