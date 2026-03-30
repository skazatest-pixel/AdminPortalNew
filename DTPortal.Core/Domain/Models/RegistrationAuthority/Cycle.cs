using System;
using System.Collections.Generic;

namespace DTPortal.Core.Domain.Models.RegistrationAuthority;

public partial class Cycle
{
    public int Id { get; set; }

    public int ProgramId { get; set; }

    public DateOnly? CycleStartDate { get; set; }

    public DateOnly? CycleEndDate { get; set; }

    public string CycleName { get; set; }

    public string Status { get; set; }
}
