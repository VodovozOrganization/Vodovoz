using System.Text.Json.Serialization;

namespace CustomerOrdersApi.Library.V4.Dto.Orders
{
	/// <summary>
	/// Результат создания онлайн заказа
	/// </summary>
	public class CreatedOnlineOrderResult
	{
		public CreatedOnlineOrderResult() { }
		private CreatedOnlineOrderResult((int OnlineOrderId, int Code) data)
		{
			OnlineOrderId = data.OnlineOrderId;
			Code = data.Code;
		}
		/// <summary>
		/// Id онлайн заказа
		/// </summary>
		public int OnlineOrderId { get; set; }
		/// <summary>
		/// Http код
		/// </summary>
		public int Code { get; set; } = 500;
		
		public static CreatedOnlineOrderResult Create((int OnlineOrderId, int Code) data) => new CreatedOnlineOrderResult(data);
	}
}
