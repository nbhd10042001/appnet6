using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace App.Models.Blog;

[Table("Post")]
public class Post : PostBase
{
    [Display(Name = "Tác giả")]
    public string AuthorId {set; get;}
    
    [ForeignKey("AuthorId")]
    [Display(Name = "Tác giả")]
    public AppUser Author {set; get;}
    
    [Display(Name = "Ngày tạo")]
    public DateTime DateCreated {set; get;}

    [Display(Name = "Ngày cập nhật")]
    public DateTime DateUpdated {set; get;}
}