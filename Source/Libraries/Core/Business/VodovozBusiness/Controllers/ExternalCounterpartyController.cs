using System;
using System.Collections.Generic;
using System.Linq;
using QS.Dialog;
using QS.DomainModel.UoW;
using QS.Project.Services;
using Vodovoz.Domain.Client;
using Vodovoz.EntityRepositories.Counterparties;
using Vodovoz.Nodes;

namespace Vodovoz.Controllers
{
	/// <summary>
	/// Работа с пользователями ИПЗ
	/// </summary>
	public class ExternalCounterpartyController : IExternalCounterpartyController
	{
		private readonly IDeleteEntityService _deleteEntityService;
		private readonly IExternalCounterpartyRepository _externalCounterpartyRepository;
		private readonly IInteractiveService _interactiveService;

		public string PhoneAssignedExternalCounterpartyMessage =>
			"Данный номер телефона привязан к внешнему пользователю сайта/приложения\n" +
			"При удалении/архивации телефона будут также удалены все связанные с пользователем данные и он потеряет доступ к сайту/МП\n";

		public ExternalCounterpartyController(
			IDeleteEntityService deleteEntityService,
			IExternalCounterpartyRepository externalCounterpartyRepository,
			IInteractiveService interactiveService)
		{
			_deleteEntityService = deleteEntityService ?? throw new ArgumentNullException(nameof(deleteEntityService));
			_externalCounterpartyRepository =
				externalCounterpartyRepository ?? throw new ArgumentNullException(nameof(externalCounterpartyRepository));
			_interactiveService = interactiveService ?? throw new ArgumentNullException(nameof(interactiveService));
		}
		
		/// <summary>
		/// Удаление пользователя ИПЗ и всей связанной информации
		/// </summary>
		/// <param name="uow">unit of work</param>
		/// <param name="phoneId">Id телефона</param>
		/// <returns></returns>
		public bool DeleteExternalCounterparties(IUnitOfWork uow, int phoneId)
		{
			if(!HasActiveExternalCounterparties(uow, phoneId, out var externalCounterparties))
			{
				return true;
			}

			if(!_interactiveService.Question(PhoneAssignedExternalCounterpartyMessage + "Вы действительно хотите продолжить?"))
			{
				return false;
			}

			DeleteExternalCounterparties(uow, externalCounterparties);

			return true;
		}
		
		/// <summary>
		/// Удаление пользователя ИПЗ и всей связанной информации
		/// </summary>
		/// <param name="uow">unit of work</param>
		/// <param name="externalCounterparties">Список удаляемых пользователей ИПЗ</param>
		public void DeleteExternalCounterparties(IUnitOfWork uow, IEnumerable<ExternalCounterparty> externalCounterparties)
		{
			foreach(var externalCounterparty in externalCounterparties)
			{
				_deleteEntityService.DeleteEntity<ExternalCounterparty>(externalCounterparty.Id, uow, forceDelete: true);
			}
		}

		/// <summary>
		/// Проверка наличия привязанных пользователей к номеру телефона
		/// </summary>
		/// <param name="uow">unit of work</param>
		/// <param name="phoneId">Id телефона</param>
		/// <param name="externalCounterparties">Возвращаемый список привязанных пользователей ИПЗ</param>
		/// <returns></returns>
		public bool HasActiveExternalCounterparties(
			IUnitOfWork uow,
			int phoneId,
			out IEnumerable<ExternalCounterparty> externalCounterparties)
		{
			externalCounterparties = _externalCounterpartyRepository.GetActiveExternalCounterpartiesByPhone(uow, phoneId);
			return externalCounterparties.Any();
		}
		
		/// <summary>
		/// Получение информации о привязанных пользователях ИПЗ по Id клиента
		/// </summary>
		/// <param name="uow">unit of work</param>
		/// <param name="counterpartyId">Id клиента</param>
		/// <returns></returns>
		public IEnumerable<ExternalCounterpartyNode> GetActiveExternalCounterpartiesByCounterparty(
			IUnitOfWork uow,
			int counterpartyId)
		{
			return _externalCounterpartyRepository.GetActiveExternalCounterpartiesByCounterparty(uow, counterpartyId);
		}
		
		/// <summary>
		/// Получение информации о привязанных пользователях ИПЗ по Id телефонов
		/// </summary>
		/// <param name="uow">unit of work</param>
		/// <param name="phonesIds">Список Id телефонов</param>
		/// <returns></returns>
		public IEnumerable<ExternalCounterpartyNode> GetActiveExternalCounterpartiesByPhones(
			IUnitOfWork uow,
			IEnumerable<int> phonesIds)
		{
			return _externalCounterpartyRepository.GetActiveExternalCounterpartiesByPhones(uow, phonesIds);
		}
	}
}
