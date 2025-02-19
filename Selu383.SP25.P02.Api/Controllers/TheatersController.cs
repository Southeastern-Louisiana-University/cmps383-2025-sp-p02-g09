using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Selu383.SP25.P02.Api.Data;
using Selu383.SP25.P02.Api.Features.Theaters;
using Selu383.SP25.P02.Api.Features.Users;

namespace Selu383.SP25.P02.Api.Controllers
{
    [Route("api/theaters")]
    [ApiController]
    public class TheatersController : ControllerBase
    {
        private readonly DbSet<Theater> theaters;
        private readonly DataContext dataContext;
        private readonly UserManager<User> userManager;

        public TheatersController(DataContext dataContext, UserManager<User> userManager)
        {
            this.dataContext = dataContext;
            this.userManager = userManager;
            theaters = dataContext.Set<Theater>();
        }

        [HttpGet]
        public IQueryable<TheaterDto> GetAllTheaters()
        {
            return GetTheaterDtos(theaters);
        }

        [HttpGet]
        [Route("{id}")]
        public ActionResult<TheaterDto> GetTheaterById(int id)
        {
            var result = GetTheaterDtos(theaters.Where(x => x.Id == id)).FirstOrDefault();
            if (result == null)
            {
                return NotFound();
            }

            return Ok(result);
        }

        [HttpPost]
        [Authorize]
        public async Task<ActionResult<TheaterDto>> CreateTheater(TheaterDto dto)
        {
            var user = await userManager.GetUserAsync(User);
            var isAdmin = user != null && await userManager.IsInRoleAsync(user, "Admin");

            if (!isAdmin)
            {
                return StatusCode(403);
            }

            if (IsInvalid(dto))
            {
                return BadRequest();
            }

            var theater = new Theater
            {
                Name = dto.Name,
                Address = dto.Address,
                SeatCount = dto.SeatCount,
                ManagerId = dto.ManagerId
            };
            theaters.Add(theater);

            await dataContext.SaveChangesAsync();

            dto.Id = theater.Id;

            return CreatedAtAction(nameof(GetTheaterById), new { id = dto.Id }, dto);
        }

        [HttpPut]
        [Route("{id}")]
        [Authorize]
        public async Task<ActionResult<TheaterDto>> UpdateTheater(int id, TheaterDto dto)
        {
            if (IsInvalid(dto))
            {
                return BadRequest();
            }

            var theater = await theaters.FirstOrDefaultAsync(x => x.Id == id);
            if (theater == null)
            {
                return NotFound();
            }

            var user = await userManager.GetUserAsync(User);
            var isAdmin = user != null && await userManager.IsInRoleAsync(user, "Admin");

            if (!isAdmin && (theater.ManagerId != user?.Id))
            {
                return StatusCode(403);
            }

            theater.Name = dto.Name;
            theater.Address = dto.Address;
            theater.SeatCount = dto.SeatCount;

            if (isAdmin)
            {
                theater.ManagerId = dto.ManagerId;
            }

            await dataContext.SaveChangesAsync();

            dto.Id = theater.Id;
            dto.ManagerId = theater.ManagerId;

            return Ok(dto);
        }

        [HttpDelete]
        [Route("{id}")]
        [Authorize]
        public async Task<ActionResult> DeleteTheater(int id)
        {
            var theater = await theaters.FirstOrDefaultAsync(x => x.Id == id);
            if (theater == null)
            {
                return NotFound();
            }

            var user = await userManager.GetUserAsync(User);
            var isAdmin = user != null && await userManager.IsInRoleAsync(user, "Admin");

            if (!isAdmin && (theater.ManagerId != user?.Id))
            {
                return StatusCode(403);
            }

            theaters.Remove(theater);

            await dataContext.SaveChangesAsync();

            return Ok();
        }

        private static bool IsInvalid(TheaterDto dto)
        {
            return string.IsNullOrWhiteSpace(dto.Name) ||
                   dto.Name.Length > 120 ||
                   string.IsNullOrWhiteSpace(dto.Address) ||
                   dto.SeatCount <= 0;
        }

        private static IQueryable<TheaterDto> GetTheaterDtos(IQueryable<Theater> theaters)
        {
            return theaters
                .Select(x => new TheaterDto
                {
                    Id = x.Id,
                    Name = x.Name,
                    Address = x.Address,
                    SeatCount = x.SeatCount,
                    ManagerId = x.ManagerId
                });
        }
    }
}