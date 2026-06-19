namespace VodovozBusiness.EntityRepositories.Nodes
{
	/// <summary>
	/// Задолженность по МЛ в разрезе организации
	/// </summary>
	public class RouteListDebtByOrganizationNode
	{
		/// <summary>
		/// Id организации
		/// </summary>
		public int OrganizationId { get; set; }

		/// <summary>
		/// Наименование организации
		/// </summary>
		public string OrganizationName { get; set; }

		/// <summary>
		/// Сумма долга
		/// </summary>
		public decimal DebtSum { get; set; }

		public string DebtInfo =>
			$"{OrganizationName}: {DebtSum:#,##0.00 'р.'}";
	}
}
