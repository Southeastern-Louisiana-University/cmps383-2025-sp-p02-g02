using Microsoft.AspNetCore.Authorization;
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

        public TheatersController(DataContext dataContext)
        {
            this.dataContext = dataContext;
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
        [Authorize] //401 if user not authenticated
        public ActionResult<TheaterDto> CreateTheater(TheaterDto dto)
        {
            if (IsInvalid(dto))
            {
                return BadRequest();
            }

            if (!User.IsInRole("Admin"))
            {
                return Forbid(); // Returns 403 if user is not an admin
            }

            //check if the user exists first and if not, return bad request
            User? manager = null;
            if (dto.ManagerId.HasValue)
            {
                manager = dataContext.Set<User>().Find(dto.ManagerId.Value);
                if (manager == null)
                {
                    return BadRequest("Invalid ManagerId: User not found.");
                }
            }

            var theater = new Theater
            {
                Name = dto.Name,
                Address = dto.Address,
                SeatCount = dto.SeatCount,
                Manager = manager
            };
            theaters.Add(theater);

            dataContext.SaveChanges();

            dto.Id = theater.Id;

            return CreatedAtAction(nameof(GetTheaterById), new { id = dto.Id }, dto); 
        }

        [HttpPut]
        [Route("{id}")]
        [Authorize]
        public ActionResult<TheaterDto> UpdateTheater(int id, TheaterDto dto)
        {
            if (IsInvalid(dto))
            {
                return BadRequest();
            }

            var theater = theaters.FirstOrDefault(x => x.Id == id);
            if (theater == null)
            {
                return NotFound();
            }

            if (!int.TryParse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value, out var userId))
            {
                return Unauthorized(); // 401 Unauthorized if user ID cannot be parsed
            }

            bool isAdmin = User.IsInRole("Admin");
            bool isManager = theater.Manager?.Id == userId;

            if (!isAdmin && !isManager)
            {
                return Forbid("Only admins or the manager can update this theater."); 
            }

            if (!isAdmin && dto.ManagerId.HasValue && dto.ManagerId != theater.Manager?.Id)
            {
                return Forbid("Only admins can change the ManagerId.");
            }

            User? newManager = theater.Manager;
            if (isAdmin && dto.ManagerId.HasValue && dto.ManagerId != theater.Manager?.Id)
            {
                newManager = dataContext.Set<User>().Find(dto.ManagerId.Value);
                if (newManager == null)
                {
                    return BadRequest();
                }
            }

            theater.Name = dto.Name;
            theater.Address = dto.Address;
            theater.SeatCount = dto.SeatCount;

            // Only admins can change the manager
            if (isAdmin) 
            {
                theater.Manager = newManager;
            }

            dataContext.SaveChanges();

            dto.Id = theater.Id;

            return Ok(dto);
        }

        [HttpDelete]
        [Route("{id}")]
        public ActionResult DeleteTheater(int id)
        {
            var theater = theaters.FirstOrDefault(x => x.Id == id);
            if (theater == null)
            {
                return NotFound();
            }

            theaters.Remove(theater);

            dataContext.SaveChanges();

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
                });
        }
    }
}
