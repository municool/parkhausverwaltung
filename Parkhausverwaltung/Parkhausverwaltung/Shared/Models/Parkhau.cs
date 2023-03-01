using System;
using System.Collections.Generic;

namespace Parkhausverwaltung.Shared.Models;

public partial class Parkhau
{
    public int ParkhausId { get; set; }

    public string Name { get; set; } = null!;

    public int DayPrice { get; set; }

    public int DefaultPrice { get; set; }

    public virtual ICollection<Floor> Floors { get; } = new List<Floor>();

    public virtual ICollection<Mieter> Mieters { get; } = new List<Mieter>();

    public virtual ICollection<Tarif> Tarifs { get; } = new List<Tarif>();

    public virtual ICollection<Visit> Visits { get; } = new List<Visit>();
}
