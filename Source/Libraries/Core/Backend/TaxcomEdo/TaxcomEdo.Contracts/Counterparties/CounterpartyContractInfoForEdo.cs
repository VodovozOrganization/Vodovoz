using System;
using TaxcomEdo.Contracts.Organizations;

namespace TaxcomEdo.Contracts.Counterparties
{
	/// <summary>
	/// Информация о контракте(договоре) клиента для ЭДО(электронного документооборота)
	/// </summary>
	public class CounterpartyContractInfoForEdo
	{
		/// <summary>
		/// Id контракта
		/// </summary>
		public int Id { get; set; }
		/// <summary>
		/// Информация о нашей организации, от которой договор
		/// </summary>
		public OrganizationInfoForEdo OrganizationInfoForEdo { get; set; }
		/// <summary>
		/// Номер контракта
		/// </summary>
		public string Number { get; set; }
		/// <summary>
		/// Дата начала действия
		/// </summary>
		public DateTime IssueDate { get; set; }
	}
}
