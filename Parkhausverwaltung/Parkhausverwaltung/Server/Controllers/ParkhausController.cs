using Azure.Identity;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Parkhausverwaltung.Server.Infrastructure;
using Parkhausverwaltung.Shared;
using Parkhausverwaltung.Shared.Models;
using System.Security.Cryptography;
using System.Text;

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

        #region CRUD

        [HttpGet]
        public IEnumerable<Parkhaus> GetAllParkhauses()
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

        [HttpGet("GetFloors/{id}")]
        public List<Floor> GetFloorsByParkhausId(int id)
        {
            using (var context = _dbContextFactory.CreateDbContext())
            {
                return context.Floors.Where(v => v.ParkhausId == id).ToList();
            }
        }

        [HttpGet("GetTarifs/{id}")]
        public List<Tarif> GetTarifsByParkhausId(int id)
        {
            using (var context = _dbContextFactory.CreateDbContext())
            {
                return context.Tarifs.Where(v => v.ParkhausId == id).ToList();
            }
        }

        [HttpGet("GetParkingSlots/{parkhouseId}")]
        public IEnumerable<KeyValuePair<int, SlotState>> GetParkingSlots(int parkhouseId)
        {
            return GetParkingSlotsByParkhausId(parkhouseId);
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

        [HttpPost("UpdateFloor")]
        public IActionResult UpdateFloor([FromBody] Floor floor)
        {
            using (var context = _dbContextFactory.CreateDbContext())
            {
                var oldFloor = context.Floors.Find(floor.FloorId);

                if (oldFloor != null)
                {
                    oldFloor.FloorNr = floor.FloorNr;
                    oldFloor.SlotCount = floor.SlotCount;

                    context.Floors.Update(oldFloor);
                    context.SaveChanges();
                }
            }
            return Ok();
        }
        [HttpPost("UpdateTarif")]
        public IActionResult UpdateTarif([FromBody] Tarif tarif)
        {
            using (var context = _dbContextFactory.CreateDbContext())
            {
                var oldTarif = context.Tarifs.Find(tarif.TarifId);

                if (oldTarif != null)
                {
                    oldTarif.StartDate = tarif.StartDate;
                    oldTarif.EndDate = tarif.EndDate;
                    oldTarif.StartTime = tarif.StartTime;
                    oldTarif.EndTime = tarif.EndTime;
                    oldTarif.Preis = tarif.Preis;

                    context.Tarifs.Update(oldTarif);
                    context.SaveChanges();
                }
            }
            return Ok();
        }

        [HttpPost("AddFloor")]
        public IActionResult AddFloor([FromBody] Floor floor)
        {
            using (var context = _dbContextFactory.CreateDbContext())
            {
                floor.ParkhausId = floor.Parkhaus.ParkhausId;
                floor.Parkhaus = null;

                context.Floors.Add(floor);
                context.SaveChanges();
            }
            return Ok();
        }

        [HttpPost("AddTarif")]
        public IActionResult AddTarif([FromBody] Tarif tarif)
        {
            using (var context = _dbContextFactory.CreateDbContext())
            {
                tarif.ParkhausId = tarif.Parkhaus.ParkhausId;
                tarif.Parkhaus = null;

                context.Tarifs.Add(tarif);
                context.SaveChanges();
            }
            return Ok();
        }
        #endregion


        [HttpGet("GetParkticket/{parkhausId}")]
        public ActionResult<Visit> GetParkticket(int parkhausId)
        {
            try
            {
                var visit = new Visit
                {
                    Arrival = DateTime.Now,
                    TicketNr = GetNewTicketNr(),
                    ParkhausId = parkhausId
                };

                var rand = new Random();
                var freeSlots = GetParkingSlotsByParkhausId(parkhausId).Where(s => s.Value == SlotState.Free).ToArray();
                visit.SlotNr = freeSlots[rand.Next(freeSlots.Length) - 1].Key;

                using (var context = _dbContextFactory.CreateDbContext())
                {
                    context.Visits.Add(visit);
                    context.SaveChanges();
                }

                return Ok(visit);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("ParkLogin/{parkhausId}")]
        public ActionResult<Visit> ParkLogin(int parkhausId, [FromBody] int mieterCode)
        {
            try
            {
                var visit = new Visit
                {
                    Arrival = DateTime.Now,
                    ParkhausId = parkhausId
                };

                using (var context = _dbContextFactory.CreateDbContext())
                {
                    var mieter = context.Mieters.FirstOrDefault(m => m.MieterCode == mieterCode);

                    if (mieter == null)
                    {
                        return NotFound("Kein Mieter gefunden!");
                    }

                    visit.MieterId = mieter.MieterId;
                    visit.SlotNr = mieter.SlotNr;

                    context.Visits.Add(visit);
                    context.SaveChanges();
                }

                return Ok(visit);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("ParkLogout/{parkhausId}")]
        public ActionResult ParkLogout(int parkhausId, [FromBody] (string code, bool isMieter) body)
        {
            using (var context = _dbContextFactory.CreateDbContext())
            {
                Visit? visit;
                if (body.isMieter)
                {
                    try
                    {
                        visit = context.Visits.Include(v => v.Mieter).FirstOrDefault(v => v.Mieter != null && v.Mieter.MieterCode == int.Parse(body.code) && v.Departure == null && !v.HasLeft && v.ParkhausId == parkhausId);
                    }
                    catch (Exception ex)
                    {
                        return BadRequest(ex.Message);
                    }
                }
                else
                {
                    visit = context.Visits.FirstOrDefault(v => v.TicketNr == body.code && !v.HasLeft && v.ParkhausId == parkhausId);

                    if(visit != null && visit.Departure == null)
                    {
                        return BadRequest("Ticket wurde nicht bezahlt!");
                    }
                }

                if (visit == null)
                {
                    return NotFound("Mieter ist nicht in diesem Parkhaus!");
                }

                visit.Departure ??= DateTime.Now;
                visit.HasLeft = true;

                context.Visits.Update(visit);
                context.SaveChanges();
            }
            return Ok();
        }

        private IEnumerable<KeyValuePair<int, SlotState>> GetParkingSlotsByParkhausId(int parkhouseId)
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

                    if (visits.Any(v => v.SlotNr == slotNr))
                    {
                        state = SlotState.Visitor;

                        if (visits.Any(v => v.SlotNr == slotNr && v.MieterId != null))
                        {
                            state = SlotState.Customer;
                        }
                    }

                    slots.Add(new KeyValuePair<int, SlotState>(slotNr, state));
                }
            }

            return slots;
        }

        private static string GetNewTicketNr()
        {
            var chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890".ToCharArray();
            var size = 10;

            byte[] data = new byte[4 * size];
            using (var crypto = RandomNumberGenerator.Create())
            {
                crypto.GetBytes(data);
            }

            var result = new StringBuilder(size);
            for (int i = 0; i < size; i++)
            {
                var rnd = BitConverter.ToUInt32(data, i * 4);
                var idx = rnd % chars.Length;

                result.Append(chars[idx]);
            }

            return result.ToString();
        }


        private readonly IDbContextFactory<ParkhausverwaltungContext> _dbContextFactory;
    }
}
