namespace TaxcomEdo.Contracts.Documents
{
	/// <summary>
	/// Информация об участнике для ЭДО
	/// </summary>
	public class EdoParticipantInfo
	{
		public string Inn { get; set; }
		public string Kpp { get; set; }
		public string TaxcomEdoAccountId { get; set; }
		public string OrganizationFullName { get; set; }
	}
}

