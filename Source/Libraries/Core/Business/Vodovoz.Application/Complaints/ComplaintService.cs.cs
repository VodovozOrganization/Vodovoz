using QS.Dialog;
using QS.DomainModel.UoW;
using QS.Services;
using System;
using System.Linq;
using Vodovoz.Core.Domain.Repositories;
using Vodovoz.Domain.Complaints;

namespace Vodovoz.Application.Complaints
{
	public class ComplaintService : IComplaintService
	{
		private IGenericRepository<Complaint> _complaintRepository;
		private readonly ICurrentPermissionService _currentPermissionService;
		private readonly IInteractiveService _interactiveService;

		public ComplaintService(
			IGenericRepository<Complaint> complaintRepository,
			ICurrentPermissionService currentPermissionService,
			IInteractiveService interactiveService
			)
		{
			_complaintRepository = complaintRepository ?? throw new ArgumentNullException(nameof(complaintRepository));
			_currentPermissionService = currentPermissionService ?? throw new ArgumentNullException(nameof(currentPermissionService));
			_interactiveService = interactiveService ?? throw new ArgumentNullException(nameof(interactiveService));
		}

		public bool CheckForDuplicateComplaint(IUnitOfWork uow, Complaint complaint, DateTime checkDuplicatesFromDate, DateTime checkDuplicatesToDate)
		{
			var canCreateDuplicateComplaints = _currentPermissionService.ValidatePresetPermission(Vodovoz.Core.Domain.Permissions.ComplaintPermissions.CanCreateDuplicateComplaints);
			var hasСounterpartyDuplicateToday = HasOrderDuplicatesBetweenDates(uow, complaint, checkDuplicatesFromDate, checkDuplicatesToDate);

			if(hasСounterpartyDuplicateToday && !canCreateDuplicateComplaints)
			{
				_interactiveService.ShowMessage(ImportanceLevel.Warning,
					"Рекламация с данным заказом уже создавалась сегодня, у вас нет прав на создание дубликатов рекламаций.");

				return false;
			}

			var canSaveDuplicate = !hasСounterpartyDuplicateToday
				|| (canCreateDuplicateComplaints && _interactiveService.Question(
					"Рекламация с данным заказом уже создавалась сегодня, создать ещё одну?"));

			return canSaveDuplicate;
		}

		private bool HasOrderDuplicatesBetweenDates(IUnitOfWork uow, Complaint complaint, DateTime checkDuplicatesFromDate, DateTime checkDuplicatesToDate)
		{
			if(complaint.Order is null)
			{
				return false;
			}

			var existsComplaint = _complaintRepository
		   .Get(uow,
				c => c.Order.Id == complaint.Order.Id
				&& c.CreationDate >= checkDuplicatesFromDate
				&& c.CreationDate <= checkDuplicatesToDate
				&& c.Id != complaint.Id
			).FirstOrDefault();

			return existsComplaint != null;
		}
	}
}
