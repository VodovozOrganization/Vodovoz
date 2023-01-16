using System.Net;
using Vodovoz.Domain.Orders;
using Vodovoz.Domain.Organizations;
using Vodovoz.Domain.TrueMark;

namespace VodovozSalesReceiptsService.DTO
{
    public class PreparedReceiptNode
    {
        /// <summary>
        /// Чек на отправку
        /// </summary>
        public CashReceipt CashReceipt { get; set; }

		/// <summary>
		/// Данные кодов честного знака заказа
		/// </summary>
		public TrueMarkCashReceiptOrder TrueMarkCashReceiptOrder { get; set; }

		/// <summary>
		/// Валидный документ на отправку
		/// </summary>
		public SalesDocumentDTO SalesDocumentDTO { get; set; }
        
        /// <summary>
        /// Кассовый аппарат, на который будет отправлен документ
        /// </summary>
        public CashBox CashBox { get; set; }
        
        /// <summary>
        /// Результат отправки
        /// </summary>
        public HttpStatusCode SendResultCode { get; set; }
    }
}
