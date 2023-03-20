using System;

namespace Vodovoz.Models.CashReceipts.DTO
{
    public class CashboxSetting
    {
        public int Id { get; set; }
        public string RetailPointName { get; set; }
        public Guid UserId { get; set; }
        public string Password { get; set; }
	}
}
