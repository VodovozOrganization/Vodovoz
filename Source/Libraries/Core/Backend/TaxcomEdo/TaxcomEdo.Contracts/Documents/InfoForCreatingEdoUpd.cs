using System;
using System.Collections.Generic;
using TaxcomEdo.Contracts.Orders;
using TaxcomEdo.Contracts.Payments;

namespace TaxcomEdo.Contracts.Documents
{
	/// <summary>
	/// Необходимая информация для отправки УПД по ЭДО
	/// </summary>
	public class InfoForCreatingEdoUpd : InfoForCreatingDocumentEdo
	{
		public static readonly string ExchangeAndQueueName = "info-for-create-upd";
		
		/// <summary>
		/// Конструктор, нужен для десериализации из Json
		/// </summary>
		public InfoForCreatingEdoUpd() { }

		protected InfoForCreatingEdoUpd(OrderInfoForEdo orderInfoForEdo, IEnumerable<PaymentInfoForEdo> paymentsInfoForEdo, int edoTaskId)
		{
			OrderInfoForEdo = orderInfoForEdo;
			PaymentsInfoForEdo = paymentsInfoForEdo;
			MainDocumentId = Guid.NewGuid();
			EdoTaskId = edoTaskId;
		}
		
		/// <summary>
		/// Информация о заказе <see cref="OrderInfoForEdo"/>
		/// </summary>
		public OrderInfoForEdo OrderInfoForEdo { get; set; }
		/// <summary>
		/// Информация об оплате <see cref="PaymentInfoForEdo"/>
		/// </summary>
		public IEnumerable<PaymentInfoForEdo> PaymentsInfoForEdo { get; set; }

		public static InfoForCreatingEdoUpd Create(OrderInfoForEdo orderInfoForEdo, IEnumerable<PaymentInfoForEdo> paymentsInfoForEdo, int edoTaskId) =>
			new InfoForCreatingEdoUpd(orderInfoForEdo, paymentsInfoForEdo, edoTaskId);
		
		/// <summary>
		/// Id задачи по ЭДО, которая инициировала отправку документа
		/// </summary>
		public int EdoTaskId { get; set; }
	}
}
