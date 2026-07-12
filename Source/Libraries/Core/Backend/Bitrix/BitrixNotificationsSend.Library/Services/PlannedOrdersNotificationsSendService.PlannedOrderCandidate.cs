using System;
using VodovozBusiness.EntityRepositories.Nodes;

namespace BitrixNotificationsSend.Library.Services
{
	public partial class PlannedOrdersNotificationsSendService
	{
		/// <summary>
		/// Класс, представляющий кандидата на уведомление по плановому заказу
		/// </summary>
		public class PlannedOrderCandidate
		{
			/// <summary>
			/// Агрегированные данные по заказам для точки доставки или самовывоза
			/// </summary>
			public PlannedOrdersAggregatedNode Aggregate { get; set; }

			/// <summary>
			/// Дата планируемого заказа, рассчитанная на основе данных по заказам
			/// </summary>
			public DateTime PlannedOrderDate { get; set; }

			/// <summary>
			/// Данные по последнему выполненному заказу для точки доставки или самовывоза
			/// </summary>
			public PlannedOrderLastOrderNode LastOrder { get; set; }
		}
	}
}
