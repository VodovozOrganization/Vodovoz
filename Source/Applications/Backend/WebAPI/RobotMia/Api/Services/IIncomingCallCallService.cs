using QS.DomainModel.UoW;
using System;
using System.Threading.Tasks;
using Vodovoz.Core.Domain.RobotMia;

namespace Vodovoz.RobotMia.Api.Services
{
	/// <summary>
	/// Сервис входящих звонков Api робота Мия
	/// </summary>
	public interface IIncomingCallCallService
	{
		/// <summary>
		/// Получение зарегистрарованного звонка по уникальному идентификатору
		/// </summary>
		/// <param name="callId">Идентификатор звонка</param>
		/// <param name="uow"></param>
		/// <returns></returns>
		Task<RobotMiaCall> GetCallByIdAsync(Guid callId, IUnitOfWork uow = null);

		/// <summary>
		/// Регистрация входящего звонка робота Мия
		/// </summary>
		/// <param name="callId">Идентификатор звонка</param>
		/// <param name="phoneNumber">Номер телефона</param>
		/// <param name="counterpartyId">Идентификатор контрагента</param>
		/// <param name="uow"></param>
		/// <returns></returns>
		Task RegisterCallAsync(Guid callId, string phoneNumber, int? counterpartyId, IUnitOfWork uow = null);
	}
}
