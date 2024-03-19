using BeverageBasket.API.Enum;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BeverageBasket.API.Entities
{
    public class Order
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }

        public DateTime OrderDate { get; set; }

        public Guid BasketId { get; set; }

        public bool HasBeenCompleted { get; set; }

        public decimal TotalPrice { get; set; }

        public PaymentMethodEnum PaymentMethod { get; set; }
    }
}
