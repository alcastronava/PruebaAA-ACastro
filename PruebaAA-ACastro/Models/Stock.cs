using System;
using System.Collections.Generic;
using System.Text;

namespace PruebaAA_ACastro.Models
{
    class Stock
    {
        public int Id { get; set; }

        public string PointOfSale { get; set; }

        public string Product { get; set; }

        public string /*DateTime*/ Date { get; set; }

        public int StockQty { get; set; }
    }
}
