using System.ComponentModel.DataAnnotations;

namespace GoodHamburger.Shared.Enums
{
    public enum ProductType
    {
        [Display(Name = "Sanduíche")]
        Sandwich = 1,
        [Display(Name = "Batata Frita")]
        Fries = 2,
        [Display(Name = "Bebida")]
        Drink = 3,
    }
}
