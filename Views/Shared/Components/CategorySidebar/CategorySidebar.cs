using App.Models.Blog;
using Microsoft.AspNetCore.Mvc;

namespace App.Views.Shared.Components;

[ViewComponent]
public class CategorySidebar : ViewComponent
{
    public class CategorySidebarData
    {
        public List<CategoryModel> Categories {set; get;}
        public int level {set; get;}
        public string categorySlug {set; get;}
    }

    public IViewComponentResult Invoke(CategorySidebarData data)
    {
        return View(data); //default view
    }
}