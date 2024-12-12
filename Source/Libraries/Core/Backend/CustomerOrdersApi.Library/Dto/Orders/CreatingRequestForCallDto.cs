using Vodovoz.Core.Domain.Clients;

namespace CustomerOrdersApi.Library.Dto.Orders
{
	public class CreatingRequestForCallDto
	{
		/// <summary>
		/// Источник запроса
		/// </summary>
		public Source Source { get; set; }
		
		/// <summary>
		/// Контрольная сумма, для проверки валидности отправителя
		/// </summary>
		public string Signature { get; set; }
		
		/// <summary>
		/// Номер телефона для связи
		/// </summary>
		public string PhoneNumber { get; set; }
		
		/// <summary>
		/// ФИО для связи
		/// </summary>
		public string ContactName { get; set; }
		
		/// <summary>
		/// Id товара в ERP по которому вопрос
		/// </summary>
		public int? NomenclatureErpId { get; set; }
		
		/// <summary>
		/// Id контрагента в ERP
		/// </summary>
		public int? CounterpartyErpId { get; set; }
	}
}
