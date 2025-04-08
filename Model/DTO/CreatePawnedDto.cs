namespace GoldNest.Model.DTO
{
    public class CreatePawnedItemDto
    {
        public required int ItemID { get; set; }
        public string? ItemType { get; set; }
        public decimal GrossWeight { get; set; }
        public decimal NetWeight { get; set; }
        public decimal Amount { get; set; }
    }

}
