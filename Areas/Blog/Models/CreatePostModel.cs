using System.ComponentModel.DataAnnotations;
using App.Models.Blog;

namespace App.Areas.Blog.Models;

public class CreatePostModel : Post
{
    [Display(Name = "Chuyen muc")]
    public int[] CategoryIDs {set; get;}
}