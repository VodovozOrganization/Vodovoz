using QS.DomainModel.UoW;
using QS.Project.Domain;
using QS.Services;
using QS.ViewModels;
using QS.ViewModels.Control.EEVM;
using QS.ViewModels.Extension;
using System;
using Vodovoz.Domain.Orders;
using Vodovoz.Domain.Organizations;
using Vodovoz.EntityRepositories.Settings;
using Vodovoz.ViewModels.Organizations;

namespace Vodovoz.ViewModels.Orders
{
	public class PaymentFromViewModel : EntityTabViewModelBase<PaymentFrom>, IAskSaveOnCloseViewModel
	{
		private readonly IOrderOrganizationSettingsRepository _orderOrganizationSettingsRepository;
		private bool _newPaymentFromAddedToSettings;

		public PaymentFromViewModel(
			IEntityUoWBuilder uoWBuilder,
			IUnitOfWorkFactory uowFactory,
			ICommonServices commonServices,
			IOrderOrganizationSettingsRepository orderOrganizationSettingsRepository,
			ViewModelEEVMBuilder<Organization> organizationViewModelEEVMBuilder)
			: base(uoWBuilder, uowFactory, commonServices)
		{
			if(organizationViewModelEEVMBuilder is null)
			{
				throw new ArgumentNullException(nameof(organizationViewModelEEVMBuilder));
			}

			_orderOrganizationSettingsRepository =
				orderOrganizationSettingsRepository ?? throw new ArgumentNullException(nameof(orderOrganizationSettingsRepository));

			CanShowOrganization = true;

			OrganizationViewModel = organizationViewModelEEVMBuilder
				.SetUnitOfWork(UoW)
				.SetViewModel(this)
				.ForProperty(Entity, x => x.OrganizationForOnlinePayments)
				.UseViewModelJournalAndAutocompleter<OrganizationJournalViewModel>()
				.UseViewModelDialog<OrganizationViewModel>()
				.Finish();

			SetDefaultOrganizationCriterion();
		}

		public bool CanEdit => PermissionResult.CanUpdate || (PermissionResult.CanCreate && Entity.Id == 0);
		public bool AskSaveOnClose => CanEdit;
		public bool CanShowOrganization { get; }
		public IEntityEntryViewModel OrganizationViewModel { get; }

		protected override bool BeforeSave()
		{
			if(Entity.Id == 0 && !_newPaymentFromAddedToSettings)
			{
				var onlinePaymentTypeOrganizationSettings =
					_orderOrganizationSettingsRepository.GetOnlinePaymentTypeOrganizationSettings(UoW);

				if(onlinePaymentTypeOrganizationSettings is null)
				{
					ShowErrorMessage(
						"В базе нет настроек для установки организации заказа для типа оплаты Оплачено онлайн. Сохранение не возможно");
					return false;
				}
				
				onlinePaymentTypeOrganizationSettings.PaymentsFrom.Add(Entity);
				_newPaymentFromAddedToSettings = true;
			}

			return true;
		}

		private void SetDefaultOrganizationCriterion()
		{
			if(Entity.Id == 0)
			{
				Entity.OrganizationCriterion = "Должны быть сохранены настройки для обмена с Модуль-кассой";
			}
		}
	}
}
