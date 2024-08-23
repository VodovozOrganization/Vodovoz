namespace CustomerAppsApi.Library.Dto.Counterparties
{
	/// <summary>
	/// Id связанных физика и юрика
	/// </summary>
	public class ConnectedLegalCustomerDto
	{
		private ConnectedLegalCustomerDto(int erpLegalCounterpartyId, int erpNaturalCounterpartyId)
		{
			ErpLegalCounterpartyId = erpLegalCounterpartyId;
			ErpNaturalCounterpartyId = erpNaturalCounterpartyId;
		}
		
		/// <summary>
		/// Id юридического лица, от имени которого, физик может делать заказы
		/// </summary>
		public int ErpLegalCounterpartyId { get; }
		/// <summary>
		/// Id физика, который может заказывать от юридического лица 
		/// </summary>
		public int ErpNaturalCounterpartyId { get; }

		/// <summary>
		/// Создание новой Dto
		/// </summary>
		/// <param name="erpLegalCounterpartyId">Id юрика</param>
		/// <param name="erpNaturalCounterpartyId">Id физика</param>
		/// <returns>новая Dto</returns>
		public static ConnectedLegalCustomerDto Create(int erpLegalCounterpartyId, int erpNaturalCounterpartyId)
			=> new ConnectedLegalCustomerDto(erpLegalCounterpartyId, erpNaturalCounterpartyId);
	}
}
