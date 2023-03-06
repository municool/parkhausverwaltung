using System;
using System.Collections.Generic;

namespace Parkhausverwaltung.Shared.Models;

public partial class Tarif
{
    public int TarifId { get; set; }

    public int ParkhausId { get; set; }

    public decimal Preis { get; set; }

    public TimeSpan StartTime { get; set; }

    public TimeSpan EndTime { get; set; }

    public DateTime StartDate { get; set; }

    public DateTime? EndDate { get; set; }

    public bool WorkDay { get; set; }

    public virtual Parkhaus Parkhaus { get; set; } = null!;
}
