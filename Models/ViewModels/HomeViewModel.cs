using System.Collections.Generic;

namespace FashionStore.Models.ViewModels
{
    public class HomeViewModel
    {
        public List<Banner> Banners { get; set; }
        public List<Product> FeaturedProducts { get; set; }
        public List<Product> NewProducts { get; set; }

        public HomeViewModel()
        {
            Banners = new List<Banner>();
            FeaturedProducts = new List<Product>();
            NewProducts = new List<Product>();
        }
    }
}