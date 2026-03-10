using Vodovoz.Domain.Client;

namespace VodovozBusiness.Nodes
{
	/// <summary>
	/// Информация о пользователе физ лица
	/// </summary>
	public class PersonalCounterpartyExternalUserInfo
	{
		/// <summary>
		/// Идентификатор из БД
		/// </summary>
		public int Id { get; set; }
		/// <summary>
		/// Идентификатор из ИПЗ
		/// </summary>
		public string ExternalId { get; set; }
		/// <summary>
		/// Номер телефона
		/// </summary>
		public string Phone { get; set; }
		/// <summary>
		/// Откуда пользователь
		/// </summary>
		public CounterpartyFrom CounterpartyFrom { get; set; }
	}
}
