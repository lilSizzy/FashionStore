using System;
using System.Collections.Generic;
using System.Linq;

namespace FashionStore.Models
{
    public class CartItemModel
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public string Image { get; set; }

        public decimal TotalPrice
        {
            get { return Price * Quantity; }
        }
    }

    public class CartModel
    {
        public List<CartItemModel> Items { get; set; } = new List<CartItemModel>();

        public decimal GrandTotal
        {
            get { return Items.Sum(x => x.TotalPrice); }
        }

        public int TotalItems
        {
            get { return Items.Sum(x => x.Quantity); }
        }
    }
}