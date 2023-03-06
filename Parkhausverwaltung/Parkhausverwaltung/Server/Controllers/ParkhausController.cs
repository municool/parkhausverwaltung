using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Parkhausverwaltung.Server.Infrastructure;
using Parkhausverwaltung.Shared;
using Parkhausverwaltung.Shared.Models;

namespace Parkhausverwaltung.Server.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ParkhausController : ControllerBase
    {
        public ParkhausController(IDbContextFactory<ParkhausverwaltungContext> dbContextFactory)
        {
            _dbContextFactory = dbContextFactory;
        }


        [HttpGet]
        public IEnumerable<Parkhaus> GetAllParkhauss()
        {
            using (var context = _dbContextFactory.CreateDbContext())
            {
                return context.Parkhaus.ToList();
            }
        }

        [HttpGet("{id}")]
        public Parkhaus? GetById(int id)
        {
            using (var context = _dbContextFactory.CreateDbContext())
            {
                return context.Parkhaus.FirstOrDefault(v => v.ParkhausId == id);
            }
        }

        [HttpGet("{parkhouseId}")]
        public IEnumerable<KeyValuePair<int, SlotState>> GetParkingSlots(int parkhouseId)
        {
            var visits = new List<Visit>();
            List<Floor> floors;
            var slots = new List<KeyValuePair<int, SlotState>>();

            using (var context = _dbContextFactory.CreateDbContext())
            {
                visits = context.Visits.Where(v => v.ParkhausId == parkhouseId && v.Departure == null).ToList();
                floors = context.Floors.Where(f => f.ParkhausId == parkhouseId).OrderBy(f => f.FloorNr).ToList();
            }

            foreach (var floor in floors)
            {
                for (int i = 1; i <= floor.SlotCount; i++)
                {
                    var slotNr = floor.FloorNr * 100 + i;
                    var state = SlotState.Free;

                    if(visits.Any(v => v.SlotNr == slotNr))
                    {
                        state = SlotState.Visitor;

                        if(visits.Any(v => v.SlotNr == slotNr && v.MieterId != null))
                        {
                            state = SlotState.Customer;
                        }
                    }

                    slots.Add(new KeyValuePair<int, SlotState>(slotNr, state));
                }
            }

            return slots;
        }

        [HttpPost]
        public IActionResult Create([FromBody] Parkhaus parkhaus)
        {
            using (var context = _dbContextFactory.CreateDbContext())
            {
                context.Parkhaus.Add(parkhaus);
                context.SaveChanges();
            }
            return Ok();
        }

        [HttpPost("{id}")]
        public IActionResult Update(int id, [FromBody] Parkhaus parkhaus)
        {
            using (var context = _dbContextFactory.CreateDbContext())
            {
                var oldParkhaus = context.Parkhaus.Find(parkhaus.ParkhausId);

                if (oldParkhaus != null)
                {
                    oldParkhaus.Name = parkhaus.Name;
                    oldParkhaus.DayPrice = parkhaus.DayPrice;
                    oldParkhaus.DefaultPrice = parkhaus.DefaultPrice;

                    context.Parkhaus.Update(oldParkhaus);
                    context.SaveChanges();
                }
            }
            return Ok();
        }

        private readonly IDbContextFactory<ParkhausverwaltungContext> _dbContextFactory;
    }
}
