using System;

namespace VodovozSalesReceiptsService.DTO
{
    public class CashBox
    {
        public int Id { get; set; }
        public string RetailPoint { get; set; }
        public Guid UserName { get; set; }
        public string Password { get; set; }
    }
}
