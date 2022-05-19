using System;
using System.Collections.Generic;

// Code scaffolded by EF Core assumes nullable reference types (NRTs) are not used or disabled.
// If you have enabled NRTs for your project, then un-comment the following line:
// #nullable disable

namespace Cet322Afied.Models
{
    public partial class TblUser
    {
        public int UserId { get; set; }
        public string UserName { get; set; }
        public string UserPhone { get; set; }
        public string UserEmail { get; set; }
        public string UserPasswordHash { get; set; }

        public virtual TblCustomerUser TblCustomerUser { get; set; }
        public virtual TblManagerUser TblManagerUser { get; set; }
    }
}
