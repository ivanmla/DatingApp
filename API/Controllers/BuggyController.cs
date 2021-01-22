using System;
using API.Data;
using API.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    public class BuggyController : BaseApiController
    {
        private readonly DataContext _context;

        public BuggyController(DataContext context)
        {
            _context = context;
        }

        [Authorize]
        [HttpGet("auth")]
        public ActionResult<string> GetSecret()
        {
            return "secret text";
        }

        [HttpGet("not-found")]
        public ActionResult<AppUser> GetNotFound()
        {
            var nesto = _context.Users.Find(-1);
            if (nesto == null)
                return NotFound();
            return Ok(nesto);
        }

        [HttpGet("server-error")]
        public ActionResult<string> GetServerError()
        {
            var nesto = _context.Users.Find(-1);
            var nestoToReturn = nesto.ToString();// nullReferenceExc

            return nestoToReturn;
        }

        [HttpGet("bad-request")]
        public ActionResult<string> GetBadRequest()
        {
            return BadRequest("Ovo nije dobar Request...");
        }
    }
}