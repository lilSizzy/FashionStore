using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FashionStore.Models.ViewModels
{
    public class ProductSearchVM
    {
        // Thuộc tính tìm kiếm
        public string SearchTerm { get; set; }
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }

        // Thuộc tính sắp xếp
        public string SortBy { get; set; }
        public string SortOrder { get; set; }

        // Thuộc tính phân trang
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;

        // Kết quả
        public List<Product> Products { get; set; }
        public int TotalCount { get; set; }
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);

        public ProductSearchVM()
        {
            Products = new List<Product>();
        }
    }
}