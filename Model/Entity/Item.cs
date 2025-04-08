using System.ComponentModel.DataAnnotations;

namespace GoldNest.Model.Entity
{
    public class Item
    {
        [Key]
        public int ItemID { get; set; }

        public required string ItemName { get; set; }

        public required string ItemType { get; set; } // Gold, Silver, etc.

        public string? Description { get; set; }


        // Navigation property
        //public ICollection<PawnedItem> PawnedItems { get; set; }
    }
}