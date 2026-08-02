using System.Collections.Generic;

namespace CustomerAppsApi.Library.V2.Dto
{
	/// <summary>
	/// Данные по ТД клиента
	/// </summary>
	public class DeliveryPointsDto
	{
		/// <summary>
		/// Описание ошибки, если есть
		/// </summary>
		public string ErrorDescription { get; set; }
		/// <summary>
		/// ТД клиента
		/// </summary>
		public IList<CreatedDeliveryPointDto> DeliveryPointsInfo { get; set; }
	}
}
