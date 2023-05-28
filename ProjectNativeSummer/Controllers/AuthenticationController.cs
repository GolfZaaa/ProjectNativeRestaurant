using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjectNativeSummer.DTOs;
using ProjectNativeSummer.Models;

namespace ProjectNativeSummer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthenticationController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> userManager;

        public AuthenticationController(UserManager<ApplicationUser> userManager)
        {
            this.userManager = userManager;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var result = await userManager.Users.ToListAsync();
            List<Object> users = new();
            foreach (var user in result)
            {
                var userRole = await userManager.GetRolesAsync(user);
                var email = await userManager.GetEmailAsync(user);
                var securitystamp = await userManager.GetSecurityStampAsync(user);
                users.Add(new { user.UserName, userRole, securitystamp, email });
            }
            return Ok(users);
        }

        [HttpPost]
        public async Task<IActionResult> Register([FromForm] RegisterDto registerDto)
        {
            // เช็ค error
            var check = await userManager.FindByEmailAsync(registerDto.Email);

            if (check != null)
            {
                return StatusCode(StatusCodes.Status403Forbidden);
            }

            // สร้าง user
            var createuser = new ApplicationUser
            {
                UserName = registerDto.Username,
                Email = registerDto.Email,
                SecurityStamp = Guid.NewGuid().ToString(),
            };
            var result = await userManager.CreateAsync(createuser, registerDto.Password);
            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(error.Code, error.Description);
                }
                return ValidationProblem();
            }

            await userManager.AddToRoleAsync(createuser, registerDto.Role);
            return Ok(result);
        }
    }
}