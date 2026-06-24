namespace VodovozBusiness.EntityRepositories.Nodes
{
	/// <summary>
	/// Задолженность по МЛ в разрезе организации
	/// </summary>
	public class RouteListDebtByOrganizationNode
	{
		/// <summary>
		/// Id организации. <see langword="null"/>, если организация не указана
		/// </summary>
		public int? OrganizationId { get; set; }

		/// <summary>
		/// Наименование организации
		/// </summary>
		public string OrganizationName { get; set; }

		/// <summary>
		/// Сумма наличных по заказам
		/// </summary>
		public decimal OrdersCashSum { get; set; }

		/// <summary>
		/// Сумма наличных по приходным ордерам
		/// </summary>
		public decimal IncomeSum { get; set; }

		/// <summary>
		/// Сумма наличных по расходным ордерам
		/// </summary>

		public decimal ExpenseSum { get; set; }

		/// <summary>
		/// Сумма долга
		/// </summary>
		public decimal DebtSum =>
			OrdersCashSum - IncomeSum + ExpenseSum;

		public string DebtInfo =>
			$"{OrganizationName ?? "Не указано"}: {DebtSum:#,##0.00 'р.'}";
	}
}
