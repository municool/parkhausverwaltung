using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Parkhausverwaltung.Server.Infrastructure;
using Parkhausverwaltung.Shared.Models;

namespace Mieterverwaltung.Server.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class MieterController : Controller
    {
        public MieterController(IDbContextFactory<ParkhausverwaltungContext> dbContextFactory)
        {
            _dbContextFactory = dbContextFactory;
        }


        [HttpGet]
        public IEnumerable<Mieter> GetAllMieters()
        {
            using (var context = _dbContextFactory.CreateDbContext())
            {
                return context.Mieters.ToList();
            }
        }

        [HttpGet("{id}")]
        public Mieter? GetById(int id)
        {
            using (var context = _dbContextFactory.CreateDbContext())
            {
                return context.Mieters.FirstOrDefault(v => v.MieterId == id);
            }
        }

        [HttpPost]
        public IActionResult Create([FromBody] Mieter mieter)
        {
            var rnd = new Random();
            mieter.MieterCode = rnd.Next(10000000, 99999999);

            using (var context = _dbContextFactory.CreateDbContext())
            {
                context.Mieters.Add(mieter);
                context.SaveChanges();
            }
            return Ok();
        }

        [HttpPost("{id}")]
        public IActionResult Update(int id, [FromBody] Mieter mieter)
        {
            using (var context = _dbContextFactory.CreateDbContext())
            {
                var oldMieter = context.Mieters.Find(mieter.MieterId);

                if (oldMieter != null)
                {
                    oldMieter.Name = mieter.Name;
                    oldMieter.SlotNr = mieter.SlotNr;
                    oldMieter.EndDate = mieter.EndDate;
                    oldMieter.PaymentOpen = mieter.PaymentOpen;

                    context.Mieters.Update(oldMieter);
                    context.SaveChanges();
                }
            }
            return Ok();
        }

        private readonly IDbContextFactory<ParkhausverwaltungContext> _dbContextFactory;
    }
}
