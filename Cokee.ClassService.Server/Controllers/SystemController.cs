using System.Configuration;

using Cokee.ClassService.Shared;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Cokee.ClassService.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SystemController : ControllerBase
    {
        private readonly GlobalDbContext _dbContext;
        private readonly IConfiguration _configuration;

        public SystemController(GlobalDbContext dbContext, IConfiguration configuration)
        {
            _dbContext = dbContext;
            _configuration = configuration;
        }

        [AllowAnonymous]
        [HttpGet("Status")]
        public async Task<IActionResult> Status(string str)
        {
            return Ok();
        }

        [AllowAnonymous]
        [HttpPut("Status")]
        public async Task<IActionResult> Log(string str)
        {
            return Ok();
        }
    }
}