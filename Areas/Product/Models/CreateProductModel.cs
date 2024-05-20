using System.ComponentModel.DataAnnotations;
using App.Models.Product;

namespace App.Areas.Product.Models;

public class CreateProductModel : ProductModel
{
    [Display(Name = "Chuyen muc")]
    public int[] CategoryIDs {set; get;}
}