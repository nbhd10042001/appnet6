using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace App.Models.Blog;

[Table("Category")]
public class CategoryModel
{
    [Key]
    public int Id { get; set; }

    // Category cha (FKey)
    [Display(Name = "Danh mục cha")]
    public int? ParentCategoryId { get; set; }

    // Tiều đề Category
    [Required(ErrorMessage = "Phải có tên danh mục")]
    [StringLength(100, MinimumLength = 3, ErrorMessage = "{0} dài {1} đến {2}")]
    [Display(Name = "Tên danh mục")]
    public string Title { get; set; }

    // Nội dung, thông tin chi tiết về Category
    [DataType(DataType.Text)]
    [Display(Name = "Nội dung danh mục")]
    public string Content { set; get; }

    //chuỗi Url
    // [Required(ErrorMessage = "Phải tạo url")]
    [StringLength(100, MinimumLength = 3, ErrorMessage = "{0} dài {1} đến {2}")]
    [RegularExpression(@"^[a-z0-9-]*$", ErrorMessage = "Chỉ dùng các ký tự [a-z0-9-]")]
    [Display(Name = "Url hiện thị")]
    public string Slug { set; get; }

    // Các Category con
    public ICollection<CategoryModel> CategoryChildren { get; set; }

    [ForeignKey("ParentCategoryId")]
    [Display(Name = "Danh mục cha")]
    public CategoryModel ParentCategory { set; get; }

    public void GetChildCategoryIDs (List<int> listIDs, ICollection<CategoryModel> childCates = null)
    {
        if (childCates == null)
            childCates = this.CategoryChildren;

        foreach(CategoryModel category in childCates)
        {
            listIDs.Add(category.Id);
            GetChildCategoryIDs(listIDs, category.CategoryChildren);
        }
    }

    public List<CategoryModel> GetListParents()
    {
        List<CategoryModel> list = new List<CategoryModel>();
        var parent = this.ParentCategory;
        while (parent != null)
        {
            list.Add(parent);
            parent = parent.ParentCategory;
        }
        list.Reverse();
        list.Add(this);
        return list;
    }



}