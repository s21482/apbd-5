using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WarehousesApi.Models
{
    public class ProductWarehouse
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int IdProductWarehouse { get; set; }
        [Required]
        public int IdWarehouse { get; set; }
        [Required]
        public int IdProduct { get; set; }
        public int IdOrder { get; set; }
        [Required]
        public int Amount { get; set; }
        public int Price { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;

    }
}