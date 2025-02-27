using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Selu383.SP25.P02.Api.Data;
using Selu383.SP25.P02.Api.Features.Theaters;
using Selu383.SP25.P02.Api.Features.Users;
using System.Security.Claims;

namespace Selu383.SP25.P02.Api.Controllers
{
    [Route("api/theaters")]
    [ApiController]
    public class TheatersController : ControllerBase
    {
        private readonly DataContext _context;
        private readonly DbSet<Theater> _theaters;
        private readonly DbSet<User> _users;
        public TheatersController(DataContext context)
        {
            _context = context;
            _theaters = context.Set<Theater>();
            _users = context.Set<User>();
        }
        [HttpGet]
        public IQueryable<TheaterDto> GetAllTheaters()
        {
            return _theaters.Select(x => new TheaterDto
            {
                Id = x.Id,
                Name = x.Name,
                Address = x.Address,
                SeatCount = x.SeatCount,
                ManagerId = x.Manager == null ? null : x.Manager.Id
            });
        }
        [HttpGet("{id}")]
        public ActionResult<TheaterDto> GetTheaterById(int id)
        {
            var dto = _theaters.Where(x => x.Id == id).Select(x => new TheaterDto
            {
                Id = x.Id,
                Name = x.Name,
                Address = x.Address,
                SeatCount = x.SeatCount,
                ManagerId = x.Manager == null ? null : x.Manager.Id
            }).FirstOrDefault();
            if (dto == null)
            {
                return NotFound();
            }
            return Ok(dto);
        }
        [HttpPost]
        [Authorize]
        public ActionResult<TheaterDto> CreateTheater([FromBody] TheaterDto dto)
        {
            if (IsInvalid(dto))
            {
                return BadRequest();
            }
            bool isAdmin = User.IsInRole("Admin");
            if (!isAdmin)
            {
                return Forbid();
            }
            User? manager = null;
            if (dto.ManagerId.HasValue)
            {
                manager = _users.Find(dto.ManagerId.Value);
                if (manager == null)
                {
                    return BadRequest();
                }
            }
            var theater = new Theater
            {
                Name = dto.Name,
                Address = dto.Address,
                SeatCount = dto.SeatCount,
                Manager = manager
            };
            _theaters.Add(theater);
            _context.SaveChanges();
            var resultDto = new TheaterDto
            {
                Id = theater.Id,
                Name = theater.Name,
                Address = theater.Address,
                SeatCount = theater.SeatCount,
                ManagerId = theater.Manager == null ? null : theater.Manager.Id
            };
            return CreatedAtAction(nameof(GetTheaterById), new { id = resultDto.Id }, resultDto);
        }
        [HttpPut("{id}")]
        [Authorize]
        public ActionResult<TheaterDto> UpdateTheater(int id, [FromBody] TheaterDto dto)
        {
            if (IsInvalid(dto))
            {
                return BadRequest();
            }
            var theater = _theaters.Include(x => x.Manager).FirstOrDefault(x => x.Id == id);
            if (theater == null)
            {
                return NotFound();
            }
            if (!int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId))
            {
                return Unauthorized();
            }
            bool isAdmin = User.IsInRole("Admin");
            bool isManager = theater.Manager?.Id == userId;
            if (!isAdmin && !isManager)
            {
                return Forbid();
            }
            if (!isAdmin && dto.ManagerId.HasValue && dto.ManagerId != theater.Manager?.Id)
            {
                return Forbid();
            }
            User? newManager = theater.Manager;
            if (isAdmin && dto.ManagerId.HasValue && dto.ManagerId != theater.Manager?.Id)
            {
                newManager = _users.Find(dto.ManagerId.Value);
                if (newManager == null)
                {
                    return BadRequest();
                }
            }
            theater.Name = dto.Name;
            theater.Address = dto.Address;
            theater.SeatCount = dto.SeatCount;
            if (isAdmin)
            {
                theater.Manager = newManager;
            }
            _context.SaveChanges();
            var updatedDto = new TheaterDto
            {
                Id = theater.Id,
                Name = theater.Name,
                Address = theater.Address,
                SeatCount = theater.SeatCount,
                ManagerId = theater.Manager == null ? null : theater.Manager.Id
            };
            return Ok(updatedDto);
        }
        [HttpDelete("{id}")]
        [Authorize]
        public ActionResult DeleteTheater(int id)
        {
            var theater = _theaters.FirstOrDefault(x => x.Id == id);
            if (theater == null)
            {
                return NotFound();
            }
            if (!User.IsInRole("Admin"))
            {
                return Forbid();
            }
            _theaters.Remove(theater);
            _context.SaveChanges();
            return Ok();
        }
        private static bool IsInvalid(TheaterDto dto)
        {
            return string.IsNullOrWhiteSpace(dto.Name) || dto.Name.Length > 120 || string.IsNullOrWhiteSpace(dto.Address) || dto.SeatCount <= 0;
        }
    }
}
