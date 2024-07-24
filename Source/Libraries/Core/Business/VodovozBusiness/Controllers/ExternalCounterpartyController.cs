using System;
using System.Collections.Generic;
using System.Linq;
using QS.Dialog;
using QS.DomainModel.UoW;
using QS.Project.Services;
using QS.Services;
using Vodovoz.Domain.Client;
using Vodovoz.EntityRepositories.Counterparties;
using Vodovoz.EntityRepositories.Nodes;

namespace Vodovoz.Controllers
{
	public class ExternalCounterpartyController : IExternalCounterpartyController
	{
		private readonly IDeleteEntityService _deleteEntityService;
		private readonly IExternalCounterpartyRepository _externalCounterpartyRepository;
		private readonly IInteractiveService _interactiveService;
		private readonly ICurrentPermissionService _currentPermissionService;
		private readonly IList<ExternalCounterparty> _deletedExternalCounterparties;

		public string PhoneAssignedExternalCounterpartyMessage =>
			"Данный номер телефона привязан к внешнему пользователю сайта/приложения\n" +
			"При удалении/архивации телефона будут также удалены все связанные с пользователем данные и он потеряет доступ к сайту/МП\n";

		public ExternalCounterpartyController(
			IDeleteEntityService deleteEntityService,
			IExternalCounterpartyRepository externalCounterpartyRepository,
			IInteractiveService interactiveService,
			ICurrentPermissionService currentPermissionService)
		{
			_deleteEntityService = deleteEntityService ?? throw new ArgumentNullException(nameof(deleteEntityService));
			_externalCounterpartyRepository =
				externalCounterpartyRepository ?? throw new ArgumentNullException(nameof(externalCounterpartyRepository));
			_interactiveService = interactiveService ?? throw new ArgumentNullException(nameof(interactiveService));
			_currentPermissionService = currentPermissionService ?? throw new ArgumentNullException(nameof(currentPermissionService));
			_deletedExternalCounterparties = new List<ExternalCounterparty>();
		}

		public bool TryDeleteExternalCounterparties(IUnitOfWork uow, IEnumerable<int> externalCounterpartiesIds, bool ask = false)
		{
			if(!HasActiveExternalCounterparties(externalCounterpartiesIds))
			{
				return true;
			}

			if(ask
				&& !_interactiveService.Question(PhoneAssignedExternalCounterpartyMessage + "Вы действительно хотите продолжить?"))
			{
				return false;
			}

			foreach(var externalCounterpartyId in externalCounterpartiesIds)
			{
				var externalCounterparty = uow.GetById<ExternalCounterparty>(externalCounterpartyId);
				//_deleteEntityService.DeleteEntity<ExternalCounterparty>(externalCounterpartyId, uow, forceDelete: true);
				_deletedExternalCounterparties.Add(externalCounterparty);
				//uow.Delete(externalCounterparty);
			}

			return true;
		}
		
		public bool CanArchiveOrDeletePhone(IEnumerable<int> externalCounterpartiesIds)
		{
			if(!HasActiveExternalCounterparties(externalCounterpartiesIds))
			{
				return true;
			}

			var canArchiveOrDeletePhone =
				_currentPermissionService.ValidatePresetPermission(Vodovoz.Permissions.Phone.CanArchiveOrDeleteExternalUserPhone);
			
			return canArchiveOrDeletePhone;
		}

		public bool HasActiveExternalCounterparties(IEnumerable<int> externalCounterpartiesIds)
		{
			return externalCounterpartiesIds.Any();
		}
		
		public IEnumerable<ExternalCounterpartyNode> GetActiveExternalCounterpartiesByCounterparty(
			IUnitOfWork uow,
			int counterpartyId)
		{
			return _externalCounterpartyRepository.GetActiveExternalCounterpartiesByCounterparty(uow, counterpartyId);
		}
		
		public IEnumerable<ExternalCounterpartyNode> GetActiveExternalCounterpartiesByPhones(
			IUnitOfWork uow,
			IEnumerable<int> phonesIds)
		{
			return _externalCounterpartyRepository.GetActiveExternalCounterpartiesByPhones(uow, phonesIds);
		}

		public void TryCreateNotifications(IUnitOfWork uow)
		{
			if(!_deletedExternalCounterparties.Any())
			{
				return;
			}
			
			foreach(var deletedExternalCounterparty in _deletedExternalCounterparties)
			{
				var notification =
					DeletedExternalCounterpartyNotification.Create(
						deletedExternalCounterparty.ExternalCounterpartyId,
						deletedExternalCounterparty.Phone.Counterparty.Id,
						deletedExternalCounterparty.CounterpartyFrom);
				
				uow.Save(notification);
			}

			_deletedExternalCounterparties.Clear();
		}
	}
}
