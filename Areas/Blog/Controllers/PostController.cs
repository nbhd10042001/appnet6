using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using App.Models;
using App.Models.Blog;
using Microsoft.AspNetCore.Authorization;
using App.Data;
using App.Areas.Blog.Models;
using Microsoft.AspNetCore.Identity;
using App.Utilities;

namespace App.Areas.Blog.Controllers
{
    [Area("Blog")]
    [Route("admin/blog/post/{action}/{id?}")]
    [Authorize(Roles = RoleName.Administrator + "," + RoleName.Member)]
    public class PostController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<AppUser> _userManager;

        public PostController(AppDbContext context, UserManager<AppUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [TempData]
        public string StatusMessage {set; get;}
        [TempData]
        public string TypeStatusMessage {set; get;}

        // GET: Blog/Post
        public async Task<IActionResult> Index([FromQuery(Name = "p")] int currentPage) // du lieu URL co query la p se binding vao bien currentPage
        {
            // var appDbContext = _context.Posts.Include(p => p.Author);
            // return View(await appDbContext.ToListAsync());

            var posts = _context.Posts
                                    .Include(p => p.Author)
                                    .Include(p => p.PostCategories)
                                    .ThenInclude(pc => pc.CategoryModel)
                                    .OrderByDescending(p => p.DateUpdated);

            const int ITEMS_PER_PAGE = 5;

            int totalPosts = await posts.CountAsync();
            int countPages = (int)Math.Ceiling((double)totalPosts / ITEMS_PER_PAGE);

            if (currentPage > countPages)  currentPage = countPages;
            if (currentPage < 1) currentPage = 1;

            var pagingModel = new PagingModel()
            {
                countpages = countPages,
                currentpage = currentPage,
                generateUrl = (pageNumber) => Url.Action("Index", new {p = pageNumber})
            };

            ViewBag.pagingModel = pagingModel;
            ViewBag.totalPosts = totalPosts;
            ViewBag.postIndex = (currentPage - 1) * ITEMS_PER_PAGE; // danh STT cac posts

            var postsInPage = await posts.Skip((currentPage - 1) * ITEMS_PER_PAGE)
                                        .Take(ITEMS_PER_PAGE)
                                        .ToListAsync();

            return View(postsInPage);
        }

        // GET: Blog/Post/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var post = await _context.Posts
                .Include(p => p.Author)
                .FirstOrDefaultAsync(m => m.PostId == id);

            if (post == null) return NotFound();

            return View(post);
        }

        // GET: Blog/Post/Create
        public async Task<IActionResult> Create()
        {
            // ViewData["AuthorId"] = new SelectList(_context.Users, "Id", "Id");

            var categories = await _context.Categories.ToListAsync();
            ViewData["categoriesSelect"] = new MultiSelectList(categories, "Id", "Title");

            return View();
        }

        // POST: Blog/Post/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Title,Description,Slug,Content,Published, CategoryIDs")] CreatePostModel post)
        {
            var categories = await _context.Categories.ToListAsync();
            ViewData["categoriesSelect"] = new MultiSelectList(categories, "Id", "Title");

            if (await _context.Posts.AnyAsync(p => p.Slug == post.Slug))
            {
                ModelState.AddModelError("Slug", "Nhap chuoi URL khac");
                return View(post);
            }

            if (post.Slug == null)
                post.Slug = AppUtilities.GenerateSlug(post.Title);

            if (ModelState.IsValid)
            {
                var user = await _userManager.GetUserAsync(this.User);
                post.DateCreated = post.DateUpdated = DateTime.Now;
                post.AuthorId = user.Id;
                _context.Add(post);

                if (post.CategoryIDs != null)
                {
                    foreach (var CateId in post.CategoryIDs)
                    {
                        _context.Add(new PostCategory(){
                            CategoryID = CateId,
                            Post = post
                        });
                    }
                }

                await _context.SaveChangesAsync();

                StatusMessage = @$"Ban vua tao bai viet moi: ""{post.Title}""";
                TypeStatusMessage = TypeName.Success;

                return RedirectToAction(nameof(Index));
            }
            return View(post);
        }

        // GET: Blog/Post/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            // var post = await _context.Posts.FindAsync(id);
            var post = await _context.Posts.Include(p => p.PostCategories).FirstOrDefaultAsync(p => p.PostId == id);

            if (post == null) return NotFound();

            var postEdit = new CreatePostModel(){
                PostId = post.PostId,
                Title = post.Title,
                Content = post.Content,
                Description = post.Description,
                Slug = post.Slug,
                Published = post.Published,
                CategoryIDs = post.PostCategories.Select(pc => pc.CategoryID).ToArray()
            };

            var categories = await _context.Categories.ToListAsync();
            ViewData["categoriesSelect"] = new MultiSelectList(categories, "Id", "Title");

            return View(postEdit);
        }

        // POST: Blog/Post/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("PostId,Title,Description,Slug,Content,Published, CategoryIDs")] CreatePostModel post)
        {
            if (id != post.PostId) return NotFound();

            if (post.Slug == null)
                post.Slug = AppUtilities.GenerateSlug(post.Title);

            if (await _context.Posts.AnyAsync(p => p.Slug == post.Slug && p.PostId != id))
            {
                ModelState.AddModelError("Slug", "Nhap chuoi URL khac");
                return View(post);
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var postUpdate = await _context.Posts.Include(p => p.PostCategories).FirstOrDefaultAsync(p => p.PostId == id);
                    if (postUpdate == null) return NotFound();

                    postUpdate.Title = post.Title;
                    postUpdate.Description = post.Description;
                    postUpdate.Content = post.Content;
                    postUpdate.Published = post.Published;
                    postUpdate.Slug = post.Slug;
                    postUpdate.DateUpdated = DateTime.Now;

                    // update postcategories
                    if (post.CategoryIDs == null) post.CategoryIDs = new int[] {};

                    var oldCateIDs = postUpdate.PostCategories.Select(c => c.CategoryID).ToArray();
                    var newCateIDs = post.CategoryIDs;

                    var removeCatePosts = from postCate in postUpdate.PostCategories
                                            where (!newCateIDs.Contains(postCate.CategoryID))
                                            select postCate;

                    _context.PostCategories.RemoveRange(removeCatePosts);

                    var addCateIDs = from CateId in newCateIDs
                                        where !oldCateIDs.Contains(CateId)
                                        select CateId;

                    foreach (var CateId in addCateIDs) {
                        _context.PostCategories.Add(new PostCategory(){
                            PostID = id,
                            CategoryID = CateId
                        });
                    }

                    _context.Update(postUpdate);
                    await _context.SaveChangesAsync();

                    StatusMessage = $"Ban vua cap nhat bai viet: {postUpdate.Title}";
                    TypeStatusMessage = TypeName.Success;
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PostExists(post.PostId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }

            var categories = await _context.Categories.ToListAsync();
            ViewData["categoriesSelect"] = new MultiSelectList(categories, "Id", "Title");

            return View(post);
        }

        // GET: Blog/Post/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var post = await _context.Posts
                .Include(p => p.Author)
                .FirstOrDefaultAsync(m => m.PostId == id);
            if (post == null)
            {
                return NotFound();
            }

            return View(post);
        }

        // POST: Blog/Post/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var post = await _context.Posts.FindAsync(id);
            if (post != null)
            {
                _context.Posts.Remove(post);
            }

            await _context.SaveChangesAsync();

            StatusMessage = $"Ban da xoa bai viet : {post.Title}";
            TypeStatusMessage = TypeName.Danger;
            return RedirectToAction(nameof(Index));
        }

        private bool PostExists(int id)
        {
            return _context.Posts.Any(e => e.PostId == id);
        }
    }
}
