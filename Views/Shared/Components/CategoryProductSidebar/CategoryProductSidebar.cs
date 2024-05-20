using App.Models.Product;
using Microsoft.AspNetCore.Mvc;

namespace App.Views.Shared.Components;

[ViewComponent]
public class CategoryProductSidebar : ViewComponent
{
    public class CategoryProductSidebarData
    {
        public List<CategoryProductModel> Categories {set; get;}
        public int level {set; get;}
        public string categorySlug {set; get;}
    }

    public IViewComponentResult Invoke(CategoryProductSidebarData data)
    {
        return View(data); //default view
    }
}