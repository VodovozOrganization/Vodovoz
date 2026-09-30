using System;

namespace Edo.Contracts.Messages.Dto
{
	/// <summary>
	/// Запрос статуса документооборота из Taxcom
	/// </summary>
	public class GetDocflowStatusRequest
	{
		/// <summary>
		/// ID документооборота в Taxcom
		/// </summary>
		public Guid? DocflowId { get; set; }

		/// <summary>
		/// ЭДО аккаунт организации
		/// </summary>
		public string EdoAccount { get; set; }
	}
}
