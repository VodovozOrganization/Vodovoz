using QS.DomainModel.UoW;
using QS.Project.Domain;
using QS.Services;
using QS.ViewModels;
using QS.ViewModels.Extension;
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Vodovoz.Domain.Orders;
using Vodovoz.Domain.Organizations;
using Vodovoz.EntityRepositories.Settings;
using Vodovoz.ViewModels.Widgets;
using VodovozBusiness.Domain.Settings;

namespace Vodovoz.ViewModels.Orders
{
	public class PaymentFromViewModel : EntityTabViewModelBase<PaymentFrom>, IAskSaveOnCloseViewModel
	{
		private string _organizationsCriterion;
		private OnlinePaymentTypeOrganizationSettings _onlinePaymentTypeOrganizationSettings;

		public PaymentFromViewModel(
			IEntityUoWBuilder uoWBuilder,
			IUnitOfWorkFactory uowFactory,
			ICommonServices commonServices,
			IOrderOrganizationSettingsRepository orderOrganizationSettingsRepository,
			AddOrRemoveIDomainObjectViewModel addOrRemoveIDomainObjectViewModel)
			: base(uoWBuilder, uowFactory, commonServices)
		{
			OrganizationsViewModel =
				addOrRemoveIDomainObjectViewModel ?? throw new ArgumentNullException(nameof(addOrRemoveIDomainObjectViewModel));

			Configure();
		}

		public bool CanEdit => PermissionResult.CanUpdate || (PermissionResult.CanCreate && Entity.Id == 0);
		public bool AskSaveOnClose => CanEdit;
		public bool CanShowOrganizations { get; private set; }
		public AddOrRemoveIDomainObjectViewModel OrganizationsViewModel { get; }

		public string OrganizationsCriterion
		{
			get => _organizationsCriterion;
			set => SetField(ref _organizationsCriterion, value);
		}

		protected override bool BeforeValidation()
		{
			if(Entity.Id != 0)
			{
				return true;
			}

			return CommonServices.ValidationService.Validate(_onlinePaymentTypeOrganizationSettings,
				new ValidationContext(_onlinePaymentTypeOrganizationSettings));
		}

		protected override bool BeforeSave()
		{
			if(Entity.Id != 0)
			{
				return true;
			}

			_onlinePaymentTypeOrganizationSettings.CriterionForOrganization = OrganizationsCriterion;
			return true;
		}

		private void InitializePaymentTypeOrganizationSettings()
		{
			_onlinePaymentTypeOrganizationSettings =
				UoW.GetAll<OnlinePaymentTypeOrganizationSettings>()
					.FirstOrDefault(s =>
						s.PaymentFrom.Id == Entity.Id);

			if(_onlinePaymentTypeOrganizationSettings is null)
			{
				_onlinePaymentTypeOrganizationSettings = new OnlinePaymentTypeOrganizationSettings
				{
					PaymentFrom = Entity
				};
				
				UoW.Save(_onlinePaymentTypeOrganizationSettings);
			}
			
			OrganizationsViewModel.Configure(
				typeof(Organization),
				CanEdit && Entity.Id == 0,
				"Организации для подбора в заказе: ",
				UoW,
				parentViewModel: this,
				_onlinePaymentTypeOrganizationSettings.Organizations);
		}

		private void Configure()
		{
			if(Entity.Id == 0)
			{
				CanShowOrganizations = true;
				SetDefaultOrganizationCriterion();
			}
			else
			{
				OrganizationsCriterion = Entity.OrganizationSettingsCriterion;
			}
			
			InitializePaymentTypeOrganizationSettings();
		}

		private void SetDefaultOrganizationCriterion()
		{
			OrganizationsCriterion = "Должны быть сохранены настройки для обмена с Модуль-кассой";
		}
	}
}
