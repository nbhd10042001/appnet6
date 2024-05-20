using App.Data;
using App.Models;
using App.Models.Blog;
using App.Models.Product;
using Bogus;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace App.Areas.Database.Controllers
{
    [Area("Database")]
    [Route("/database-manage/{action}")]
    public class DbManageController : Controller
    {
        private readonly AppDbContext _dbContext;
        private readonly UserManager<AppUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public DbManageController(AppDbContext dbContext, UserManager<AppUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _dbContext = dbContext;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        [TempData]
        public string StatusMessage {set; get;}
        [TempData]
        public string TypeStatusMessage {set; get;}

        // GET: DbManageController
        public ActionResult Index()
        {
            return View();
        }

        [HttpGet]
        [Authorize(Roles = RoleName.Administrator)]
        public IActionResult DeleteDB()
        {
            return View();
        }

        [HttpPost]
        [Authorize(Roles = RoleName.Administrator)]
        public async Task<IActionResult> DeleteDBAsync()
        {
            var result = await _dbContext.Database.EnsureDeletedAsync();
            StatusMessage = result ? "Xoa database thanh cong" : "Khong xoa duoc";
            TypeStatusMessage = result ? "warning" : "danger";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> MigrateAsync()
        {
            await _dbContext.Database.MigrateAsync();
            TypeStatusMessage = "success";
            StatusMessage = "Da tao (cap nhat) database thanh cong";

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> SeedAdminAsync()
        {
            // tao ra cac roles duoc dinh nghia san trong Data\RoleNames.cs
            // var roles = typeof(RoleName).GetFields().ToList();
            // foreach (var role in roles)
            // {
            //     var roleName = (string)role.GetRawConstantValue();
            //     var isHasRole = await _roleManager.FindByNameAsync(roleName);
            //     if(isHasRole == null)
            //     {
            //         await _roleManager.CreateAsync(new IdentityRole(roleName));
            //     }
            // }

            // tao ra user Admin
            var userAdmin = await _userManager.FindByEmailAsync($"admin@example.com");
            if (userAdmin == null)
            {
                userAdmin = new AppUser()
                {
                    UserName = $"admin",
                    Email = $"admin@example.com",
                    EmailConfirmed = true,
                };

                await _userManager.CreateAsync(userAdmin, "123123");
                await _userManager.AddToRoleAsync(userAdmin, RoleName.Administrator);
            }

            return RedirectToAction(nameof(Index));
        }

        [Authorize]
        public async Task<IActionResult> SeedDataAsync()
        {
            var user = await _userManager.GetUserAsync(this.User);
            if (user == null) return this.Forbid();
            var rolesUser = await _userManager.GetRolesAsync(user);
            
            if(!rolesUser.Any(r => r == RoleName.Administrator))
            {
                return this.Forbid();
            }

            SeedPostCategory();
            SeedProductCategory();

            StatusMessage = "Ban vua seed database";
            return RedirectToAction("Index");
        }

        private void SeedProductCategory()
        {
            // de dam bao ko seed them nhieu category du thua, ta thuc hien xoa cac category fake da khoi tao truoc do
            _dbContext.CategoryProducts.RemoveRange(_dbContext.CategoryProducts.Where(c => c.Content.Contains("[fakeData]")));
            _dbContext.Products.RemoveRange(_dbContext.Products.Where(p => p.Content.Contains("[fakeData]")));
            _dbContext.SaveChanges();

            //-----------------Phat sinh Categories Product----------------------------------

            var fakerCategory = new Faker<CategoryProductModel>();
            int cm = 1;
            fakerCategory.RuleFor(c => c.Title, fk => $"Nhom SP {cm++} " + fk.Lorem.Sentence(1,2).Trim('.'));
            fakerCategory.RuleFor(c => c.Content, fk => fk.Lorem.Sentences(5) + "[fakeData]");
            fakerCategory.RuleFor(c => c.Slug, fk => fk.Lorem.Slug());

            var cate_1 = fakerCategory.Generate();
                var cate_1_1 = fakerCategory.Generate();
                var cate_1_2 = fakerCategory.Generate();
            var cate_2 = fakerCategory.Generate();
                var cate_2_1 = fakerCategory.Generate();
                    var cate_2_1_1 = fakerCategory.Generate();

            cate_1_1.ParentCategory = cate_1;
            cate_1_2.ParentCategory = cate_1;
            cate_2_1.ParentCategory = cate_2;
                cate_2_1_1.ParentCategory = cate_2_1;

            var categories = new CategoryProductModel[] { cate_1, cate_1_1, cate_1_2, cate_2, cate_2_1, cate_2_1_1};
            _dbContext.CategoryProducts.AddRange(categories);


            //----------------------Phat sinh Products----------------------------------

            var rCateIndex = new Random();
            int bv = 1;

            var user = _userManager.GetUserAsync(this.User).Result;

            var fakerProduct = new Faker<ProductModel>();
            fakerProduct.RuleFor(p => p.AuthorId, fk => user.Id);
            fakerProduct.RuleFor(p => p.Content, fk => fk.Commerce.ProductDescription() + "[fakeData]");
            fakerProduct.RuleFor(p => p.DateCreated, fk => fk.Date.Between(new DateTime(2021, 1, 1), new DateTime(2023, 12, 30)));
            fakerProduct.RuleFor(p => p.Description, fk => fk.Lorem.Sentences(3));
            fakerProduct.RuleFor(p => p.Published, fk => true);
            fakerProduct.RuleFor(p => p.Slug, fk => fk.Lorem.Slug());
            fakerProduct.RuleFor(p => p.Title, fk => $"SP {bv++} " + fk.Commerce.ProductName());
            fakerProduct.RuleFor(p => p.Price, fk => int.Parse(fk.Commerce.Price(500, 10000, 0)));


            List<ProductModel> products = new List<ProductModel>();
            List<ProductCategoryProduct> product_categories = new List<ProductCategoryProduct>();

            for (int i = 0; i < 40; i++)
            {
                var product = fakerProduct.Generate();
                product.DateUpdated = product.DateCreated;
                products.Add(product);
                
                product_categories.Add(new ProductCategoryProduct(){
                    Product = product,
                    CategoryProduct = categories[rCateIndex.Next(categories.Length - 1)] // random ngau nhien 1 trong cac categories duoc phat sinh
                });
            }

            _dbContext.AddRange(products);
            _dbContext.AddRange(product_categories);

            _dbContext.SaveChanges();
        }

        private void SeedPostCategory()
        {
            // de dam bao ko seed them nhieu category du thua, ta thuc hien xoa cac category fake da khoi tao truoc do
            _dbContext.Categories.RemoveRange(_dbContext.Categories.Where(c => c.Content.Contains("[fakeData]")));
            _dbContext.Posts.RemoveRange(_dbContext.Posts.Where(p => p.Content.Contains("[fakeData]")));
            _dbContext.SaveChanges();

            //-----------------Phat sinh Categories----------------------------------

            var fakerCategory = new Faker<CategoryModel>();
            int cm = 1;
            fakerCategory.RuleFor(c => c.Title, fk => $"CM {cm++} " + fk.Lorem.Sentence(1,2).Trim('.'));
            fakerCategory.RuleFor(c => c.Content, fk => fk.Lorem.Sentences(5) + "[fakeData]");
            fakerCategory.RuleFor(c => c.Slug, fk => fk.Lorem.Slug());

            var cate_1 = fakerCategory.Generate();
                var cate_1_1 = fakerCategory.Generate();
                var cate_1_2 = fakerCategory.Generate();
            var cate_2 = fakerCategory.Generate();
                var cate_2_1 = fakerCategory.Generate();
                    var cate_2_1_1 = fakerCategory.Generate();

            cate_1_1.ParentCategory = cate_1;
            cate_1_2.ParentCategory = cate_1;
            cate_2_1.ParentCategory = cate_2;
                cate_2_1_1.ParentCategory = cate_2_1;

            var categories = new CategoryModel[] { cate_1, cate_1_1, cate_1_2, cate_2, cate_2_1, cate_2_1_1};
            _dbContext.Categories.AddRange(categories);


            //----------------------Phat sinh Posts----------------------------------

            var rCateIndex = new Random();
            int bv = 1;

            var user = _userManager.GetUserAsync(this.User).Result;

            var fakerPost = new Faker<Post>();
            fakerPost.RuleFor(p => p.AuthorId, fk => user.Id);
            fakerPost.RuleFor(p => p.Content, fk => fk.Lorem.Paragraphs(7) + "[fakeData]");
            fakerPost.RuleFor(p => p.DateCreated, fk => fk.Date.Between(new DateTime(2021, 1, 1), new DateTime(2023, 12, 30)));
            fakerPost.RuleFor(p => p.Description, fk => fk.Lorem.Sentences(3));
            fakerPost.RuleFor(p => p.Published, fk => true);
            fakerPost.RuleFor(p => p.Slug, fk => fk.Lorem.Slug());
            fakerPost.RuleFor(p => p.Title, fk => $"Bai {bv++} " + fk.Lorem.Sentence(3, 4).Trim('.'));

            List<Post> posts = new List<Post>();
            List<PostCategory> post_categories = new List<PostCategory>();

            for (int i = 0; i < 40; i++)
            {
                var post = fakerPost.Generate();
                post.DateUpdated = post.DateCreated;
                posts.Add(post);
                post_categories.Add(new PostCategory(){
                    Post = post,
                    CategoryModel = categories[rCateIndex.Next(categories.Length - 1)] // random ngau nhien 1 trong cac categories duoc phat sinh
                });
            }
            _dbContext.AddRange(posts);
            _dbContext.AddRange(post_categories);

            _dbContext.SaveChanges();
        }
    }
}
