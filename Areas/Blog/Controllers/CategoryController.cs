using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using App.Models;
using App.Models.Blog;
using Microsoft.AspNetCore.Authorization;
using App.Data;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Microsoft.CodeAnalysis;
using App.Utilities;

namespace App.Areas.Blog.Controllers
{
    [Area("Blog")]
    [Route("admin/blog/category/[action]/{id?}/{oldparentid?}")]
    [Authorize(Roles = RoleName.Administrator)]

    public class CategoryController : Controller
    {
        private readonly AppDbContext _context;

        public CategoryController(AppDbContext context)
        {
            _context = context;
        }

        [TempData]
        public string StatusMessage {set; get;}
        [TempData]
        public string TypeStatusMessage {set; get;}

        // GET: Blog/Category
        public async Task<IActionResult> Index()
        {
            // var appDbContext = _context.Categories.Include(c => c.ParentCategory);
            // return View(await appDbContext.ToListAsync());

            var  qr = (from c in _context.Categories select c)
                        .Include(c => c.ParentCategory)
                        .Include(c => c.CategoryChildren);
            
            var categories = (await qr.ToListAsync())
                            .Where(c => c.ParentCategory == null)
                            .ToList();

            return View(categories);
        }

        // GET: Blog/Category/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var categoryModel = await _context.Categories
                .Include(c => c.ParentCategory)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (categoryModel == null) return NotFound();

            return View(categoryModel);
        }

        // tuy bien cay thu muc select category
        private void CreateSelectItems(List<CategoryModel> source, List<CategoryModel> newCate, int level)
        {
            string prefix = string.Concat(Enumerable.Repeat("----", level));

            foreach (var cate in source)
            {
                // cate.Title = prefix + " " + cate.Title;
                newCate.Add(new CategoryModel(){
                    Id = cate.Id,
                    Title = prefix + " " + cate.Title
                });

                if (cate.CategoryChildren?.Count > 0)
                {
                    CreateSelectItems(cate.CategoryChildren.ToList(), newCate, level + 1);
                }
            }
        }

        private List<CategoryModel> SelectCategories ()
        {
            var  qr = (from c in _context.Categories select c)
                        .Include(c => c.ParentCategory)
                        .Include(c => c.CategoryChildren);
            
            var categories = qr.ToList()
                            .Where(c => c.ParentCategory == null)
                            .ToList();

            categories.Insert(0, new CategoryModel(){
                Id = -1,
                Title = "Khong co danh muc cha"
            });

            var newCate = new List<CategoryModel>();
            CreateSelectItems(categories, newCate, 0);
            return newCate;
        }

        // GET: Blog/Category/Create
        public IActionResult Create()
        {
            // ViewData["ParentCategoryId"] = new SelectList(_context.Categories, "Id", "Slug");

            var items = SelectCategories();
            var selectList = new SelectList(items, "Id", "Title");
            ViewData["ParentCategoryId"] = selectList;

            return View();
        }

        // POST: Blog/Category/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,ParentCategoryId,Title,Content,Slug")] CategoryModel categoryModel)
        {
            if (ModelState.IsValid)
            {
                if (await _context.Categories.AnyAsync(c => c.Slug == categoryModel.Slug))
                {
                    ModelState.AddModelError("Slug", "Nhap chuoi URL khac");
                    return View(categoryModel);
                }

                if (categoryModel.Slug == null)
                    categoryModel.Slug = AppUtilities.GenerateSlug(categoryModel.Title);

                if(categoryModel.ParentCategoryId == -1)
                    categoryModel.ParentCategoryId = null;

                _context.Add(categoryModel);
                await _context.SaveChangesAsync();

                StatusMessage = @$"Ban vua tao chuyen muc moi ""{categoryModel.Title}""";
                TypeStatusMessage = TypeName.Success;
                return RedirectToAction(nameof(Index));
            }

            var items = SelectCategories();
            var selectList = new SelectList(items, "Id", "Title");
            ViewData["ParentCategoryId"] = selectList;

            // if false
            return View(categoryModel);
        }

        // GET: Blog/Category/Edit/5
        public async Task<IActionResult> Edit(int? id, int? parentid)
        {
            if (id == null) return NotFound();

            var categoryModel = await _context.Categories.FindAsync(id);
            if (categoryModel == null) return NotFound();

            var items = SelectCategories();
            var selectList = new SelectList(items, "Id", "Title");
            ViewData["ParentCategoryId"] = selectList;

            return View(categoryModel);
        }

        // kiem tra parentid select co thuoc id children cua category do
        public bool CheckIfParentIdSelectSameChildren (List<CategoryModel> cateChilds, int? parentid_select)
        {
            foreach (var cateChild in cateChilds)
            {
                if (cateChild.Id == parentid_select)
                    return true;

                var cateChilds_1 = (from c in _context.Categories select c)
                                .Where(c => c.ParentCategoryId == cateChild.Id).ToList();
                                
                if (cateChilds_1.Count > 0)
                    // thuc hien de quy neu phat hien childrens trong childCate
                    return CheckIfParentIdSelectSameChildren(cateChilds_1, parentid_select);
            }
            return false;
        }

        // thay doi parent id cua cac category children
        public void SwitchParent (List<CategoryModel> cateChilds, int? parentid)
        {
            // lap qua cac child category
            foreach(var cateChild in cateChilds)
            {
                // gan idparent cua cateChild == idparent categoryEdit
                cateChild.ParentCategoryId = parentid;
                _context.Update(cateChild);
            }
            _context.SaveChanges();
        }

        // POST: Blog/Category/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,ParentCategoryId,Title,Content,Slug")] CategoryModel categoryModel, int? oldparentid)
        {
            if (id != categoryModel.Id) return NotFound();

            if (categoryModel.ParentCategoryId == categoryModel.Id)
            {
                ModelState.AddModelError(string.Empty, "Phai chon thu muc cha khac!");
            }

            if (ModelState.IsValid && (categoryModel.ParentCategoryId != categoryModel.Id))
            {
                try
                {
                    var isError = false;
                    // lay danh sach cac categoryChild cua categoryModel
                    var cateChilds = (from c in _context.Categories select c)
                                    .Where(c => c.ParentCategoryId == categoryModel.Id).ToList();

                    if (cateChilds.Count > 0)
                    {
                        isError = CheckIfParentIdSelectSameChildren(cateChilds, categoryModel.ParentCategoryId);
                        if (isError == true)
                            // ModelState.AddModelError(string.Empty, "Phai chon thu muc cha khac!");
                            SwitchParent(cateChilds, oldparentid);
                    }


                    if (categoryModel.ParentCategoryId == -1)
                        categoryModel.ParentCategoryId = null;

                    _context.Update(categoryModel);
                    await _context.SaveChangesAsync();
                }

                catch (DbUpdateConcurrencyException)
                {
                    if (!CategoryModelExists(categoryModel.Id)) return NotFound();
                    else throw;
                }

                StatusMessage = @$"Ban vua cap nhat chuyen muc ""{categoryModel.Title}""";
                TypeStatusMessage = TypeName.Success;

                return RedirectToAction(nameof(Index));
            }

            var items = SelectCategories();
            var selectList = new SelectList(items, "Id", "Title");
            ViewData["ParentCategoryId"] = selectList;

            // if have error
            return View(categoryModel);
        }

        // GET: Blog/Category/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var categoryModel = await _context.Categories
                .Include(c => c.ParentCategory)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (categoryModel == null) return NotFound();

            return View(categoryModel);
        }

        // POST: Blog/Category/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            // var categoryModel = await _context.Categories.FindAsync(id);

            var categoryModel = await _context.Categories
                                .Include(c => c.CategoryChildren) // lay ra cac categories children cua category bi xoa
                                .FirstOrDefaultAsync(c => c.Id == id); // lay ra category co Id == id can xoa

            if (categoryModel == null) return NotFound();

            // duyet cac cate child va thiet lap parent cua cate child la parent cua cate bi xoa
            foreach(var childrenCate in categoryModel.CategoryChildren)
            {
                childrenCate.ParentCategoryId = categoryModel.ParentCategoryId;
            }

            _context.Categories.Remove(categoryModel);

            await _context.SaveChangesAsync();

            StatusMessage = @$"Ban da xoa chuyen muc ""{categoryModel.Title}""";
            TypeStatusMessage = TypeName.Danger;

            return RedirectToAction(nameof(Index));
        }

        private bool CategoryModelExists(int id)
        {
            return _context.Categories.Any(e => e.Id == id);
        }
    }
}
