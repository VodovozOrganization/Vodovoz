using Vodovoz.Core.Domain.Clients;

namespace CustomerAppsApi.Library.V2.Dto
{
	/// <summary>
	/// Обновляемый коммент ТД
	/// </summary>
	public class UpdatingDeliveryPointCommentDto
	{
		/// <summary>
		/// Источник
		/// </summary>
		public Source Source { get; set; }
		/// <summary>
		/// Идентификатор ТД
		/// </summary>
		public int DeliveryPointErpId { get; set; }
		/// <summary>
		/// Комментарий
		/// </summary>
		public string Comment { get; set; }
	}
}
