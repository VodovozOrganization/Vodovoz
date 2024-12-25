namespace Edo.Contracts.Messages.Dto
{
	/// <summary>
	/// Грузополучатель
	/// </summary>
	public class ConsigneeInfo
	{
		/// <summary>
		/// Данные организации
		/// </summary>
		public OrganizationInfo Organization { get; set; }
		/// <summary>
		/// Особый грузополучатель
		/// </summary>
		public string CargoReceiver { get; set; }
	}
}
