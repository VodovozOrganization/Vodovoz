namespace VodovozBusiness.Nodes
{
	/// <summary>
	/// Параметры подбора организации
	/// </summary>
	public struct OrganizationChoiceParametersForOrderOrganizationSettings
	{
		public OrganizationChoiceParametersForOrderOrganizationSettings(
			bool needTaxcomEdoAccountId,
			bool needAvangardShopId,
			bool needCashBoxId)
		{
			NeedTaxcomEdoAccountId = needTaxcomEdoAccountId;
			NeedAvangardShopId = needAvangardShopId;
			NeedCashBoxId = needCashBoxId;
		}
		
		/// <summary>
		/// Нужнен действующий аккаунт в Такском(ЭДО)
		/// </summary>
		public bool NeedTaxcomEdoAccountId { get; }
		/// <summary>
		/// Нужна регистрация в Авангарде(СБП)
		/// </summary>
		public bool NeedAvangardShopId { get; }
		/// <summary>
		/// Нужна регистрация онлайн кассы(Модуль касса)
		/// </summary>
		public bool NeedCashBoxId { get; }
	}
}
