using System;
using System.Collections.Generic;

// Code scaffolded by EF Core assumes nullable reference types (NRTs) are not used or disabled.
// If you have enabled NRTs for your project, then un-comment the following line:
// #nullable disable

namespace Cet322Afied.Models
{
    public partial class TblCustomerUser
    {
        public TblCustomerUser()
        {
            TblOrder = new HashSet<TblOrder>();
        }

        public int CustomerId { get; set; }
        public string CustomerAddress { get; set; }

        public virtual TblUser Customer { get; set; }
        public virtual ICollection<TblOrder> TblOrder { get; set; }
    }
}
