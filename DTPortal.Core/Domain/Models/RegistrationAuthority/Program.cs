using System;
using System.Collections.Generic;

namespace DTPortal.Core.Domain.Models.RegistrationAuthority;

public partial class Program
{
    public int Id { get; set; }

    public string ProgramName { get; set; }

    public decimal AmountPerCycle { get; set; }

    public string Recurrence { get; set; }

    public bool? OneTime { get; set; }

    public bool? MatchingRegistrants { get; set; }

    public string CreatedOn { get; set; }

    public string StartDate { get; set; }

    public string Status { get; set; }

    public string Currency { get; set; }
}
