using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nri_Webapplication_Backend.Managers;
using Nri_Webapplication_Backend.Helpers;

namespace Nri_Webapplication_Backend.Controllers.Management
{
    [Route("api/[controller]")]
    [ApiController]
    public class ICSController : ControllerBase
    {
        public readonly IRespondHelper respondHelper;
        public readonly IPlannerManager plannerManager;
        public ICSController(IRespondHelper respondHelper, IPlannerManager plannerManager)
        {
            this.respondHelper = respondHelper;
            this.plannerManager = plannerManager;
        }

        [HttpGet("")]
        public async Task<IActionResult> GetICSFile()
        {
            try
            {
                var jwtToken = Request.Headers["Authorization"];
                if (string.IsNullOrWhiteSpace(jwtToken))
                {
                    return Unauthorized(this.respondHelper.Respond(null, null, "Unauthorized", null, null));
                }

                await this.plannerManager.GetICSFile();

                return Ok(this.respondHelper.Respond(0, 0, "Success", null, null));
            }
            catch (Exception ex)
            {
                return BadRequest(this.respondHelper.Respond(0, 0, $"Error : {ex}", null, null));
            }
        }


        [HttpGet("/trainer/{trainerId}")]
        public async Task<IActionResult> GetICSFile(int trainerId)
        {
            try
            {
                var jwtToken = Request.Headers["Authorization"];
                if (string.IsNullOrWhiteSpace(jwtToken))
                {
                    return Unauthorized(this.respondHelper.Respond(null, null, "Unauthorized", null, null));
                }

                await this.plannerManager.GetICSFileByTrainer(trainerId);

                return Ok(this.respondHelper.Respond(0, 0, "Success", null, null));
            }
            catch (Exception ex)
            {
                return BadRequest(this.respondHelper.Respond(0, 0, $"Error : {ex}", null, null));
            }
        }
    }

}

