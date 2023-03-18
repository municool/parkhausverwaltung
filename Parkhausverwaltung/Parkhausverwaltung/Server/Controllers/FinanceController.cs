using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Parkhausverwaltung.Server.Infrastructure;
using Parkhausverwaltung.Shared;
using Parkhausverwaltung.Shared.Models;

namespace Parkhausverwaltung.Server.Controllers
{
    [ApiController]
    [Route("finance")]
    public class FinanceController : ControllerBase
    {
        public FinanceController(IDbContextFactory<ParkhausverwaltungContext> dbContextFactory)
        {
            _dbContextFactory = dbContextFactory;
        }

        [HttpGet]
        public ActionResult<Visit> PayParkTicket(int parkhausId, string ticketNr)
        {
            Parkhaus? parkhaus;
            Visit? visit;

            using (var context = _dbContextFactory.CreateDbContext())
            {
                visit = context.Visits.FirstOrDefault(v => v.TicketNr == ticketNr && v.ParkhausId == parkhausId);
                parkhaus = context.Parkhaus.Include(p => p.Tarifs).FirstOrDefault(p => p.ParkhausId == parkhausId);
            }

            if (parkhaus == null)
            {
                return BadRequest("Parkhaus existiert nicht!");
            }
            if (visit == null)
            {
                return BadRequest("Kein Ticket mit dieser Nummer gefunden!");
            }

            visit.Departure = DateTime.Now;
            var timeSpent = visit.Departure.Value - visit.Arrival;

            if (timeSpent.TotalHours > 24)
            {
                var daysCount = Math.Ceiling(timeSpent.TotalHours / 24);
                visit.Cost = (decimal)daysCount * parkhaus.DayPrice;
            }
            else
            {
                visit.Cost = CalculateCostHourRates(parkhaus, visit.Arrival, visit.Departure.Value);
            }

            using (var context = _dbContextFactory.CreateDbContext())
            {
                context.Visits.Update(visit);
                context.SaveChanges();
            }

            return Ok(visit);
        }

        [HttpGet]
        public ActionResult<List<RevenueSummary>> GetRevenueSummery(int parkhausId, DateTime start, DateTime end)
        {
            Parkhaus? parkhaus;
            using (var context = _dbContextFactory.CreateDbContext())
            {
                parkhaus = context.Parkhaus.Include(p => p.Visits).Include(p => p.Mieters).FirstOrDefault(p => p.ParkhausId == parkhausId);
            }
            
            if(parkhaus == null)
            {
                return BadRequest("Parkhaus existiert nicht!");
            }

            var revenueSummaries = new List<RevenueSummary>();

            //Complete Summary
            var completeSummary = new RevenueSummary()
            {
                StartDate = start,
                EndDate = end,
                MieterCount = parkhaus.Mieters.Count(m => m.EndDate == null || m.EndDate > start),
                MieterRevenue = parkhaus.Mieters.Count(m => m.EndDate == null || m.EndDate > start) * parkhaus.MonthlyPrice,
                VisitorCount = parkhaus.Visits.Count(v => v.Arrival >= start && v.Departure <= end),
                VisitorRevenue = parkhaus.Visits.Where(v => v.Arrival >= start && v.Departure <= end).Sum(v => v.Cost)
            };
            revenueSummaries.Add(completeSummary);

            // Summaries by month
            var currentMoment = start;
            while(currentMoment != end)
            {
                var monthStart = currentMoment.AddDays(-1 * currentMoment.Day + 1);
                var monthEnd = monthStart.AddMonths(1).AddSeconds(-1);
                if(monthEnd >= end)
                {
                    currentMoment = end;
                }

                var monthlySummary = new RevenueSummary()
                {
                    StartDate = currentMoment,
                    EndDate = monthEnd,
                    MieterCount = parkhaus.Mieters.Count(m => m.EndDate == null || m.EndDate > currentMoment),
                    MieterRevenue = parkhaus.Mieters.Count(m => m.EndDate == null || m.EndDate > currentMoment) * parkhaus.MonthlyPrice,
                    VisitorCount = parkhaus.Visits.Count(v => v.Arrival >= currentMoment && v.Departure <= monthEnd),
                    VisitorRevenue = parkhaus.Visits.Where(v => v.Arrival >= currentMoment && v.Departure <= monthEnd).Sum(v => v.Cost)
                };

                currentMoment = monthStart.AddMonths(1);
            }

            return Ok(revenueSummaries);
        }

        private Tarif GetNextTarifChange(List<Tarif> tarifs, TimeSpan currentMoment)
        {
            var currentTarif = tarifs.FirstOrDefault(t => t.StartTime.TimeOfDay < currentMoment && t.EndTime.TimeOfDay > currentMoment);

            TimeSpan checkTime;
            if(currentTarif == null)
            {
                checkTime = currentMoment;
            }
            {
                checkTime = currentTarif.EndTime.TimeOfDay;
            }

            var nextTarif = tarifs.OrderBy(t => t.StartTime).FirstOrDefault(t => t.StartTime.TimeOfDay > checkTime);

            if(nextTarif == null)
            {
                return new Tarif();
            }

            return nextTarif;
        }

        private decimal CalculateCostHourRates(Parkhaus parkhaus, DateTime start, DateTime end)
        {
            decimal cost = 0;
            var currentMoment = start.TimeOfDay;

            while (currentMoment != end.TimeOfDay)
            {
                TimeSpan timeSpent;

                var nextTarifChange = GetNextTarifChange(parkhaus.Tarifs.ToList(), currentMoment).StartTime.TimeOfDay;
                var currentTarifPrice = parkhaus.Tarifs.FirstOrDefault(t => t.StartTime.TimeOfDay < currentMoment && t.EndTime.TimeOfDay > currentMoment)?.Preis;

                if (nextTarifChange > end.TimeOfDay)
                {
                    timeSpent = end.TimeOfDay - currentMoment;
                    currentMoment = end.TimeOfDay;
                }
                else
                {
                    timeSpent = nextTarifChange - currentMoment;
                    currentMoment = nextTarifChange;
                }

                var qHours = (decimal)Math.Ceiling(timeSpent.TotalMinutes / 15);

                if(currentTarifPrice == null || currentTarifPrice == 0)
                {
                    currentTarifPrice = parkhaus.DefaultPrice;
                }

                cost += qHours * (currentTarifPrice.Value / 4);
            }
            return cost;
        }

        private readonly IDbContextFactory<ParkhausverwaltungContext> _dbContextFactory;
    }
}
