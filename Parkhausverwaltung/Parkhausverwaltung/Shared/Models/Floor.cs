using System;
using System.Collections.Generic;

namespace Parkhausverwaltung.Shared.Models;

public partial class Floor
{
    public int FloorId { get; set; }

    public int ParkhausId { get; set; }

    public int FloorNr { get; set; }

    public int SlotCount { get; set; }

    public virtual Parkhau Parkhaus { get; set; } = null!;
}
