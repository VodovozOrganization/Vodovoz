namespace DriverApi.Contracts.V5
{
	/// <summary>
	/// Причина рекламации водителя
	/// </summary>
	public class DriverComplaintReasonDto
	{
		/// <summary>
		/// Номер причины
		/// </summary>
		public int Id { get; set; }

		/// <summary>
		/// Название причины
		/// </summary>
		public string Name { get; set; }
	}
}
