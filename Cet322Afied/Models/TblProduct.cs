using System;
using System.Collections.Generic;

// Code scaffolded by EF Core assumes nullable reference types (NRTs) are not used or disabled.
// If you have enabled NRTs for your project, then un-comment the following line:
// #nullable disable

namespace Cet322Afied.Models
{
    public partial class TblProduct
    {
        public TblProduct()
        {
            TblProductOrder = new HashSet<TblProductOrder>();
        }

        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public int ProductCategory { get; set; }
        public decimal ProductPrice { get; set; }
        public string ProductMeasurementUnit { get; set; }

        public virtual TblProductCategory ProductCategoryNavigation { get; set; }
        public virtual ICollection<TblProductOrder> TblProductOrder { get; set; }
    }
}
