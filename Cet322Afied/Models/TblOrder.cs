using System;
using System.Collections.Generic;

// Code scaffolded by EF Core assumes nullable reference types (NRTs) are not used or disabled.
// If you have enabled NRTs for your project, then un-comment the following line:
// #nullable disable

namespace Cet322Afied.Models
{
    public partial class TblOrder
    {
        public TblOrder()
        {
            TblProductOrder = new HashSet<TblProductOrder>();
        }

        public long OrderId { get; set; }
        public int OrderCustomerId { get; set; }
        public DateTime? OrderDate { get; set; }

        public virtual TblCustomerUser OrderCustomer { get; set; }
        public virtual ICollection<TblProductOrder> TblProductOrder { get; set; }
    }
}
