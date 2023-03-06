using System;
using System.Collections.Generic;

namespace Parkhausverwaltung.Shared.Models;

public partial class Visit
{
    public int VisitId { get; set; }

    public int ParkhausId { get; set; }

    public DateTime Arrival { get; set; }

    public DateTime? Departure { get; set; }

    public decimal Cost { get; set; }

    public int? TicketNr { get; set; }

    public int? MieterId { get; set; }

    public int? SlotNr { get; set; }

    public virtual Mieter? Mieter { get; set; }

    public virtual Parkhaus Parkhaus { get; set; } = null!;
}
