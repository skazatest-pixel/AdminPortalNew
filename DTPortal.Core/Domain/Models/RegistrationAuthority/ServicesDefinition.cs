using System;
using System.Collections.Generic;

namespace DTPortal.Core.Domain.Models.RegistrationAuthority;

public partial class ServicesDefinition
{
    public int Id { get; set; }

    public string ServiceName { get; set; }

    public string ServiceDisplayName { get; set; }

    public string Status { get; set; }

    public short PricingSlabApplicable { get; set; }
}
