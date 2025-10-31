﻿using Microsoft.Extensions.Logging;
using QS.Commands;
using QS.Dialog;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Domain;
using QS.Services;
using QS.ViewModels;
using QS.ViewModels.Extension;
using System;
using Vodovoz.Domain.Organizations;
using Vodovoz.ViewModels.Factories;
using Vodovoz.ViewModels.Widgets.Organizations;

namespace Vodovoz.ViewModels.Organizations
{
	public class OrganizationViewModel
		: EntityTabViewModelBase<Organization>,
		IAskSaveOnCloseViewModel
	{
		private readonly ILogger<OrganizationViewModel> _logger;
		private readonly IOrganizationVersionsViewModelFactory _organizationVersionsViewModelFactory;

		public OrganizationViewModel(
			ILogger<OrganizationViewModel> logger,
			IOrganizationVersionsViewModelFactory organizationVersionsViewModelFactory,
			IEntityUoWBuilder uowBuilder,
			IUnitOfWorkFactory unitOfWorkFactory,
			ICommonServices commonServices,
			INavigationManager navigation)
			: base(uowBuilder, unitOfWorkFactory, commonServices, navigation)
		{
			_logger = logger
				?? throw new ArgumentNullException(nameof(logger));
			_organizationVersionsViewModelFactory = organizationVersionsViewModelFactory
				?? throw new ArgumentNullException(nameof(organizationVersionsViewModelFactory));

			OrganizationVersionsViewModel = _organizationVersionsViewModelFactory.CreateOrganizationVersionsViewModel(Entity, CanEdit);

			SaveCommand = new DelegateCommand(
				() => Save(true),
				() => CanEdit
			);

			CancelCommand = new DelegateCommand(
				() => Close(CanEdit, CloseSource.Cancel),
				() => CanEdit
			);

			RegexForEmailForMailing = @"^[a-zA-Z0-9.!#$%&'*+/=?^_`{|}~-]+@vodovoz-spb\.ru$";
		}

		public OrganizationVersionsViewModel OrganizationVersionsViewModel { get; }

		public DelegateCommand SaveCommand { get; }
		public DelegateCommand CancelCommand { get; }

		public string RegexForEmailForMailing { get; }

		public bool CanRead => PermissionResult.CanRead;

		public bool CanEdit =>
			PermissionResult.CanUpdate
			|| (Entity.Id == 0 && PermissionResult.CanCreate);

		public bool AskSaveOnClose => CanEdit;

		public override bool Save(bool close)
		{
			_logger.LogInformation("Сохраняем организацию...");

			try
			{
				return base.Save(close);
			}
			catch(Exception ex)
			{
				_logger.LogError(ex, "Ошибка при сохранении организации.");

				CommonServices.InteractiveService.ShowMessage(
					ImportanceLevel.Error,
					"Организация не сохранилась...",
					"Ошибка при сохранении организации.");
				return false;
			}
		}
	}
}
