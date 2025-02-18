using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Selu383.SP25.P02.Api.Data;
using Selu383.SP25.P02.Api.Features.Authentication;
using Selu383.SP25.P02.Api.Features.Users;

namespace Selu383.SP25.P02.Api.Controllers
{
    [Authorize]
    [Route("api/users")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly UserManager<User> _userManager;
        private readonly DataContext _dataContext;

        public UsersController(UserManager<User> userManager, DataContext dataContext)
        {
            _userManager = userManager;
            _dataContext = dataContext;
        }

        [HttpPost]
        public async Task<ActionResult<UserDto>> CreateUser([FromBody] CreateUserDto dto)
        {
            if (dto == null || string.IsNullOrWhiteSpace(dto.UserName) || string.IsNullOrWhiteSpace(dto.Password))
            {
                return BadRequest("Username and password are required.");
            }

            bool existingUser = _dataContext.Users.Any(u => u.UserName == dto.UserName); //checks if any user has same name
            if (existingUser)
            {
                return BadRequest("Username already exists.");
            }
            
            if (dto.Roles == null || dto.Roles.Length == 0)//checks if roles is empty or none
            {
                return BadRequest("Roles required");
            }

            if (dto.Roles.Any(role => !_dataContext.Roles.Any(r => r.Name == role)))//checks if role exist in dto
            {
                return BadRequest("Role does not exist");
            }


            var user = new User
            {
                UserName = dto.UserName,

            };
            var result = await _userManager.CreateAsync(user, dto.Password);//checks to see if password meets requirements
            if (!result.Succeeded)
            {
                return BadRequest("Password does not meet requirements.");
            }

            foreach (var role in dto.Roles) 
            {
                await _userManager.AddToRoleAsync(user, role); //assigns role
            }

                var userDto = new UserDto
                {
                    Id = user.Id,
                    UserName = user.UserName,
                    Roles = user.Roles.Select(r => r.Role.Name).ToArray()
                };

            return Ok(userDto);
            }
        }
    }



