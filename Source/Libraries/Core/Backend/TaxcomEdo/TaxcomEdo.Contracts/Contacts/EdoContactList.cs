using System;

namespace TaxcomEdo.Contracts.Contacts
{
	/// <summary>
	/// Список контактов по ЭДО
	/// </summary>
	public class EdoContactList
	{
		/// <summary>
		/// Информация о контакте по ЭДО
		/// </summary>
		public EdoContactInfo[] Contacts { get; set; }
		/// <summary>
		/// 
		/// </summary>
		public DateTime Asof { get; set; }
		/// <summary>
		/// 
		/// </summary>
		public Guid TemplateId { get; set; }
	}
}
