using QS.DomainModel.UoW;
using QS.Project.Domain;
using QS.Services;
using QS.ViewModels;
using QS.ViewModels.Extension;
using System;
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

			CanShowOrganizations = Entity.Id == 0;
			InitializePaymentTypeOrganizationSettings();
			SetDefaultOrganizationCriterion();
		}

		public bool CanEdit => PermissionResult.CanUpdate || (PermissionResult.CanCreate && Entity.Id == 0);
		public bool AskSaveOnClose => CanEdit;
		public bool CanShowOrganizations { get; }
		public AddOrRemoveIDomainObjectViewModel OrganizationsViewModel { get; }

		public string OrganizationsCriterion
		{
			get => _organizationsCriterion;
			set => SetField(ref _organizationsCriterion, value);
		}
		
		private void InitializePaymentTypeOrganizationSettings()
		{
			var onlinePaymentTypeOrganizationSettings =
				UoW.GetAll<OnlinePaymentTypeOrganizationSettings>()
					.FirstOrDefault(s =>
						s.PaymentFrom.Id == Entity.Id);

			if(onlinePaymentTypeOrganizationSettings is null)
			{
				onlinePaymentTypeOrganizationSettings = new OnlinePaymentTypeOrganizationSettings
				{
					PaymentFrom = Entity
				};
				
				UoW.Save(onlinePaymentTypeOrganizationSettings);
			}
			
			OrganizationsViewModel.Configure(
				typeof(Organization),
				CanEdit,
				"Организации для подбора в заказе: ",
				UoW,
				this,
				onlinePaymentTypeOrganizationSettings.Organizations);
		}

		private void SetDefaultOrganizationCriterion()
		{
			OrganizationsCriterion = "Должны быть сохранены настройки для обмена с Модуль-кассой";
		}
	}
}
