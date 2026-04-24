using System.ComponentModel.DataAnnotations;

namespace GoodHamburger.Shared.Enums
{
    public enum OrderStatus
    {
        [Display(Name = "Pendente")]
        Pending = 0,
        [Display(Name = "Confirmado")]
        Confirmed = 1,
        [Display(Name = "Preparando")]
        Preparing = 2,
        [Display(Name = "Pronto para Entrega")]
        ReadyForDelivery = 3,
        [Display(Name = "A Caminho")]
        InTransit = 4,
        [Display(Name = "Entregue")]
        Delivered = 5,
        [Display(Name = "Cancelado")]
        Cancelled = 6
    }
}
