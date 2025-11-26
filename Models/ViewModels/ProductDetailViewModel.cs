using System.Collections.Generic;

namespace FashionStore.Models.ViewModels
{
    public class ProductDetailViewModel
    {
        public Product Product { get; set; }
        public List<Product> SimilarProducts { get; set; }
        public List<Product> TopDeals { get; set; }

        public ProductDetailViewModel()
        {
            SimilarProducts = new List<Product>();
            TopDeals = new List<Product>();
        }
    }
}