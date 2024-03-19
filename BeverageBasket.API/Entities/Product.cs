using System.ComponentModel.DataAnnotations;

namespace BeverageBasket.API.Entities
{
    public class Product
    {
        [Key]
        public string Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string Name { get; set; }

        [Required]
        public decimal Price { get; set; }

        [Required]
        public int Quantity { get; set; }

        [MaxLength(250)]
        public string? Description { get; set; }

        public Product(string id, string name, decimal price, int quantity)
        {
            Id = id;
            Name = name;
            Price = price;
            Quantity = quantity;
        }
    }
}
