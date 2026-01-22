using QS.DomainModel.UoW;
using Vodovoz.Core.Domain.Clients.Accounts;
using Vodovoz.Core.Domain.Results;

namespace VodovozBusiness.Services.Clients
{
	/// <summary>
	/// Обработчик аккаунта юр лица
	/// </summary>
	public interface ILegalCounterpartyAccountHandler
	{
		/// <summary>
		/// Удаление аккаунта юр лица из ДВ
		/// </summary>
		/// <param name="uow">UnitOfWork</param>
		/// <param name="account">Удаляемый аккаунт</param>
		/// <returns></returns>
		Result DeleteAccountFromDesktop(IUnitOfWork uow, ExternalLegalCounterpartyAccount account);
		/// <summary>
		/// Удаление аккаунта юр лица
		/// </summary>
		/// <param name="uow">UnitOfWork</param>
		/// <param name="account">Удаляемый аккаунт</param>
		/// <returns></returns>
		Result DeleteAccount(IUnitOfWork uow, ExternalLegalCounterpartyAccount account);
	}
}
