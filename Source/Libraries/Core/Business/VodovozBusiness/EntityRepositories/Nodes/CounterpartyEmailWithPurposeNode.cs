using Vodovoz.Core.Domain.Contacts;

namespace VodovozBusiness.EntityRepositories.Nodes
{
	/// <summary>
	/// Email контрагента с назначением адреса
	/// </summary>
	public class CounterpartyEmailWithPurposeNode
	{
		/// <summary>
		/// Id контрагента
		/// </summary>
		public int CounterpartyId { get; set; }

		/// <summary>
		/// Адрес электронной почты
		/// </summary>
		public string Address { get; set; }

		/// <summary>
		/// Назначение адреса, null - если тип адреса не указан
		/// </summary>
		public EmailPurpose? EmailPurpose { get; set; }
	}
}
