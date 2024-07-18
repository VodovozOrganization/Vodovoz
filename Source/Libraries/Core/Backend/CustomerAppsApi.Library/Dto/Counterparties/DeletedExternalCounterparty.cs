using System;

namespace CustomerAppsApi.Library.Dto.Counterparties
{
	/// <summary>
	/// Информация об удаленном внешнем пользователе
	/// </summary>
	public class DeletedExternalCounterparty
	{
		private DeletedExternalCounterparty(Guid externalCounterpartyId, int erpCounterpartyId)
		{
			ErpCounterpartyId = erpCounterpartyId;
			ExternalCounterpartyId = externalCounterpartyId;
		}

		/// <summary>
		/// Код клиента в ДВ
		/// </summary>
		public int ErpCounterpartyId { get; }
		/// <summary>
		/// Код пользователя из ИПЗ
		/// </summary>
		public Guid ExternalCounterpartyId { get; }

		public static DeletedExternalCounterparty Create(Guid externalCounterpartyId, int erpCounterpartyId)
		{
			return new DeletedExternalCounterparty(externalCounterpartyId, erpCounterpartyId);
		}
	}
}
