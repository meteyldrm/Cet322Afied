using System;
using System.Collections.Generic;

// Code scaffolded by EF Core assumes nullable reference types (NRTs) are not used or disabled.
// If you have enabled NRTs for your project, then un-comment the following line:
// #nullable disable

namespace Cet322Afied.Models
{
    public partial class TblManagerUser
    {
        public int ManagerId { get; set; }
        public int ManagerAuthorizationLevel { get; set; }

        public virtual TblUser Manager { get; set; }
    }
}
