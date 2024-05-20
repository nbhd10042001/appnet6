using System.ComponentModel.DataAnnotations.Schema;

namespace App.Models.Blog;

public class PostCategory
    {
        public int PostID {set; get;}

        public int CategoryID {set; get;}

        [ForeignKey("PostID")]
        public Post Post {set; get;}

        [ForeignKey("CategoryID")]
        public CategoryModel CategoryModel {set; get;}
        
    }