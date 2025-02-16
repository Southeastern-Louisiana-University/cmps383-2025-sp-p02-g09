using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Selu383.SP25.P02.Api.Data;
using Selu383.SP25.P02.Api.Features.Authentication;
using Selu383.SP25.P02.Api.Features.Users;

namespace Selu383.SP25.P02.Api.Controllers
{
    [Route("api/users")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly DbSet<User> users;
        private readonly DataContext dataContext;

        public UsersController(DataContext dataContext)
        {
            this.dataContext = dataContext;
            this.users = dataContext.Set<User>();
        }

        [HttpGet]
        public IQueryable<UserDto> GetAllUsers()
        {
            return GetUserDtos(users);
        }

        [HttpGet]
        [Route("{id}")]
        public ActionResult<UserDto> GetUserById(int id)
        {
            var result = GetUserDtos(users.Where(x => x.Id == id)).FirstOrDefault();
            if (result == null)
            {
                return NotFound();
            }

            return Ok(result);
        }
        [HttpPost]
        public ActionResult<UserDto> CreateUser([FromBody] UserDto dto)
        {
            if (IsInvalid(dto))
            {
                return BadRequest();
            }

            var user = new User
            {
                UserName = dto.UserName,

            };

            users.Add(user);
            dataContext.SaveChanges();

            dto.Id = user.Id;

            return CreatedAtAction(nameof(GetUserById), new { id = dto.Id }, dto);
        }


        [HttpPut]
        [Route("{id}")]
        public ActionResult<UserDto> UpdateUser(int id, [FromBody] UserDto dto)
        {
            if (IsInvalid(dto))
            {
                return BadRequest();
            }

            var user = users.FirstOrDefault(x => x.Id == id);
            if (user == null)
            {
                return NotFound();
            }

            user.UserName = dto.UserName;

            dataContext.SaveChanges();

            return Ok(dto);
        }

        [HttpDelete]
        [Route("{id}")]
        public ActionResult DeleteUser(int id)
        {
            var user = users.FirstOrDefault(x => x.Id == id);
            if (user == null)
            {
                return NotFound();
            }

            users.Remove(user);
            dataContext.SaveChanges();

            return Ok();
        }

        private static bool IsInvalid(UserDto dto)
        {
            return string.IsNullOrWhiteSpace(dto.UserName);
        }

        private static IQueryable<UserDto> GetUserDtos(IQueryable<User> users)
        {
            return users
                .Select(x => new UserDto
                {
                    Id = x.Id,
                    UserName = x.UserName,
                    Roles = x.Roles.Select(r => r.Role.Name).ToArray()
                });
        }
    }
}
