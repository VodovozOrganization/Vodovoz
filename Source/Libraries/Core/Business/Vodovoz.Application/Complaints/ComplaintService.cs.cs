﻿using QS.Dialog;
using QS.DomainModel.UoW;
using QS.Services;
using System;
using System.Linq;
using Vodovoz.Core.Domain.Common;
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

		public bool CheckForDuplicateComplaint(IUnitOfWork uow, int? orderId)
		{
			var canCreateDuplicateComplaints = _currentPermissionService.ValidatePresetPermission(Permissions.Complaint.CanCreateDuplicateComplaints);
			var hasСounterpartyDuplicateToday = HasСounterpartyDuplicateToday(uow, orderId);

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

		private bool HasСounterpartyDuplicateToday(IUnitOfWork uow, int? orderId)
		{
			if(orderId is null)
			{
				return false;
			}

			var existsComplaint = _complaintRepository
		   .Get(uow,
				c => c.Order.Id == orderId && c.CreationDate >= DateTime.Now.AddDays(-1)
			).FirstOrDefault();

			return existsComplaint != null;
		}
	}
}
