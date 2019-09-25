using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EFCore;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace UserApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly UcenterDbContext _repository;

        public UserController(UcenterDbContext repository)
        {
            _repository = repository;
        }
        [Route("check")]
        [HttpPost]
        public async Task<IActionResult> CheckorCreate(string phone)
        {
            var user = _repository.Users.FirstOrDefault(a => a.UserName == phone);
            if (user==null)
            {
                user = new User()
                {
                    AccessFailedCount = 0,
                    CreationTime = DateTime.Now,
                    IsLockoutEnabled = false,
                    UserName = phone,
                    Password = phone,
                    RegIp = "127.0.0.1"
                };
                await _repository.Users.AddAsync(user);
                await _repository.SaveChangesAsync();
            }

            return Ok(user.Id);
        }
    }
}