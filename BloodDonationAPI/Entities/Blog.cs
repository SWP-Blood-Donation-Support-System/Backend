using System;
using System.Collections.Generic;

namespace BloodDonationAPI.Entities;

public partial class Blog
{
    public int BlogId { get; set; }

    public string? BlogTitle { get; set; }

    public string? BlogContent { get; set; }

    public string? BlogImage { get; set; }

    public string? Username { get; set; }

    public virtual User? UsernameNavigation { get; set; }
}
