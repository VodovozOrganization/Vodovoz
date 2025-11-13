using System;

namespace VodovozBusiness.Models.CashReceipts.DTO
{
	public class TrueMarkOrganizationClientSetting 
	{
		/// <summary>
		/// Организация ДВ
		/// </summary>
		public int OrganizationId { get; set; }

		/// <summary>
		/// Токен в заголовок
		/// </summary>
		public Guid HeaderTokenApiKey { get; set; }
	}
}
