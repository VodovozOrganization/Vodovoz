namespace Vodovoz.Journals.Nodes.Rent
{
    public class PaidRentPackagesJournalNode
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string EquipmentKindName { get; set; }
        public decimal PriceDaily { get; set; }
        public decimal PriceMonthly { get; set; }
        public string PriceDailyString => $"{PriceDaily:N2} ₽";
        public string PriceMonthlyString => $"{PriceMonthly:N2} ₽";
    }
}