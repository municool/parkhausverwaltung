using System;
using System.Collections.Generic;

namespace Parkhausverwaltung.Shared.Models;

public partial class Mieter
{
    public int MieterId { get; set; }

    public string Name { get; set; } = null!;

    public int MieterCode { get; set; }

    public int ParkhausId { get; set; }

    public int SlotNr { get; set; }

    public DateTime StartDate { get; set; }

    public DateTime? EndDate { get; set; }

    public bool PaymentOpen { get; set; }

    public virtual Parkhau Parkhaus { get; set; } = null!;

    public virtual ICollection<Visit> Visits { get; } = new List<Visit>();
}
