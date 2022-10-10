using System;

namespace VodovozSalesReceiptsService.DTO
{
    public class CashBox
    {
        public int Id { get; set; }
        public string RetailPointName { get; set; }
        public Guid UserName { get; set; }
        public string Password { get; set; }
    }
}
