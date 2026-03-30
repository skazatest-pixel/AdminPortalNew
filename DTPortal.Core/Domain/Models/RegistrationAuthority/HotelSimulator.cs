using System;
using System.Collections.Generic;

namespace DTPortal.Core.Domain.Models.RegistrationAuthority;

public partial class HotelSimulator
{
    public int Id { get; set; }

    public string Name { get; set; }

    public string RoomAllocated { get; set; }

    public string DocumentNumber { get; set; }

    public string DateOfBirth { get; set; }

    public int? AgeInYears { get; set; }

    public string Gender { get; set; }

    public string Photo { get; set; }

    public DateTime? CreationDate { get; set; }

    public bool? AgeOver18 { get; set; }
}
