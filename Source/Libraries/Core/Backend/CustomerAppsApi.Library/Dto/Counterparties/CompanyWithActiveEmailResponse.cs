namespace CustomerAppsApi.Library.Dto.Counterparties
{
	/// <summary>
	/// Ответ по эндпойнту получения активных связок по электронной почте
	/// </summary>
	public class CompanyWithActiveEmailResponse
	{
		protected CompanyWithActiveEmailResponse(int erpCounterpartyId)
		{
			ErpCounterpartyId = erpCounterpartyId;
		}
		
		/// <summary>
		/// Идентификатор юр лица
		/// </summary>
		public int ErpCounterpartyId { get; }
		
		public static CompanyWithActiveEmailResponse Create(int erpCounterpartyId) => new CompanyWithActiveEmailResponse(erpCounterpartyId);
	}
}
