
using App.Models;
using App.Models.Blog;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace App.Areas.Blog.Controllers
{
    [Area("Blog")]
    public class ViewPostController : Controller
    {
        private readonly ILogger<ViewPostController> _logger;
        private readonly AppDbContext _context;

        public ViewPostController(ILogger<ViewPostController> logger, AppDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        [Route("post/{categoryslug?}")]
        public ActionResult Index(string categoryslug, [FromQuery(Name = "p")]int currentPage)
        {
            var categories = GetCategories();
            ViewBag.categories = categories;
            ViewBag.categoryslug = categoryslug;

            CategoryModel categoryChoosed = null; 
            // kiem tra neu url co query categoryslug thi thuc hien lay category 
            if (!string.IsNullOrEmpty(categoryslug))
            {
                categoryChoosed = _context.Categories.Where(c => c.Slug == categoryslug)
                                            .Include(c => c.CategoryChildren)
                                            .FirstOrDefault();

                if (categoryChoosed == null) return NotFound("Khong tim thay chuyen muc");
            }

            var posts = _context.Posts.Include(p => p.Author)
                                        .Include(p => p.PostCategories)
                                        .ThenInclude(pc => pc.CategoryModel)
                                        .AsQueryable();   

            // lay ra cac post co idcate == id cateSelect
            if (categoryChoosed != null)
            {
                // var ids = new List<int>();
                // categoryChoosed.GetChildCategoryIDs(ids, null);
                // ids.Add(categoryChoosed.Id);
                // posts = posts.Where(p => p.PostCategories.Where(pc => ids.Contains(pc.CategoryID)).Any());
                posts = posts.Where(p => p.PostCategories.Where(pc => pc.CategoryID == categoryChoosed.Id).Any());
            }

            posts = posts.OrderByDescending(p => p.DateUpdated);

            // pagingModel------------------------------------------------------------
            const int ITEMS_PER_PAGE = 6;
            int totalPosts = posts.Count();
            int countPages = (int)Math.Ceiling((double)totalPosts / ITEMS_PER_PAGE);

            if (currentPage > countPages)  currentPage = countPages;
            if (currentPage < 1) currentPage = 1;

            var pagingModel = new PagingModel()
            {
                countpages = countPages,
                currentpage = currentPage,
                generateUrl = (pageNumber) => Url.Action("Index", new {p = pageNumber})
            };

            var postsInPage = posts.Skip((currentPage - 1) * ITEMS_PER_PAGE)
                                    .Take(ITEMS_PER_PAGE);

            ViewBag.pagingModel = pagingModel;
            ViewBag.totalPosts = totalPosts;

            ViewBag.categoryChoosed = categoryChoosed;

            return View(postsInPage.ToList());
        }

        [Route("post/{postslug}.html")]
        public ActionResult Detail(string postslug)
        {
            var categories = GetCategories();
            ViewBag.categories = categories;

            var post = _context.Posts.Where(p => p.Slug == postslug)
                                        .Include(p => p.Author)
                                        .Include(p => p.PostCategories)
                                        .ThenInclude(pc => pc.CategoryModel)
                                        .FirstOrDefault();
            if (post == null) return NotFound("Khong tim thay Post");

            CategoryModel category = post.PostCategories.FirstOrDefault()?.CategoryModel;
            ViewBag.category = category;
            ViewBag.categoryslug = category.Slug;

            // hien thi cac Post moi nhat
            var otherPosts = _context.Posts.Where(p => p.PostCategories.Any(c => c.CategoryID == category.Id))
                                            .Where(p => p.PostId != post.PostId)
                                            .OrderByDescending(p => p.DateUpdated)
                                            .Take(5);
            ViewBag.otherPosts = otherPosts;

            return View(post);
        }


        // other method -------------------------------------------------
        private List<CategoryModel> GetCategories()
        {
            var categories = _context.Categories
                                .Include(c => c.CategoryChildren)
                                .AsEnumerable()
                                .Where(c => c.ParentCategory == null) // chi can lay cate cha vi trong query ta da include cac cate child
                                .ToList();

            return categories;
        }
    }
}
