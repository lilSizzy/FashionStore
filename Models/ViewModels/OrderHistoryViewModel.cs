using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FashionStore.Models.ViewModels
{
    public class OrderHistoryViewModel
    {
        public int Id { get; set; }
        public string OrderCode { get; set; }
        public DateTime CreatedDate { get; set; }
        public decimal TotalAmount { get; set; }
        public string Status { get; set; }
        public string PaymentMethod { get; set; }
        public int ItemCount { get; set; } // Tổng số món trong đơn       

    }
}


