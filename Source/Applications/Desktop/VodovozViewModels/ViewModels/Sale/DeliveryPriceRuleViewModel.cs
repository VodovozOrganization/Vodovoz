using QS.DomainModel.UoW;
using QS.Project.Domain;
using QS.Services;
using QS.ViewModels;
using System;
using System.Collections.Generic;
using Vodovoz.Domain.Sale;
using Vodovoz.EntityRepositories.Sale;
using System.ComponentModel.DataAnnotations;
using Vodovoz.EntityRepositories;

namespace Vodovoz.ViewModels.ViewModels.Sale
{
	public class DeliveryPriceRuleViewModel : EntityTabViewModelBase<DeliveryPriceRule>
	{
		private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
		private readonly IDistrictRuleRepository districtRuleRepository;

		public DeliveryPriceRuleViewModel(
			IEntityUoWBuilder uowBuilder,
			IUnitOfWorkFactory unitOfWorkFactory,
			ICommonServices commonServices,
			IDistrictRuleRepository districtRuleRepository)
		: base(uowBuilder, unitOfWorkFactory, commonServices)
		{
			this.districtRuleRepository = districtRuleRepository ?? throw new ArgumentNullException(nameof(districtRuleRepository));

			ValidationContext.ServiceContainer.AddService(typeof(IDistrictRuleRepository), districtRuleRepository);


			if(!CanRead)
				AbortOpening("У вас недостаточно прав для просмотра");

			TabName = (CanCreateOrUpdate) ? "Редактирование правила цен доставки" : "Просмотр правила цен доставки";
		}

		public List<DistrictAndDistrictSet> DistrictsHavingCurrentRule =>
			districtRuleRepository.GetDistrictNameDistrictSetNameAndCreationDateByDeliveryPriceRule(UoW, this.Entity);
		
		public override bool Save(bool close)
		{
			logger.Info("Сохраняем правило для цены доставки...");
			return base.Save(close);
		}

		#region Permissions

		public bool CanCreate => PermissionResult.CanCreate;
		public bool CanRead => PermissionResult.CanRead;
		public bool CanUpdate => PermissionResult.CanUpdate && DistrictsHavingCurrentRule.Count == 0;
		public bool CanDelete => PermissionResult.CanDelete && DistrictsHavingCurrentRule.Count == 0;

		public bool CanCreateOrUpdate => Entity.Id == 0 ? CanCreate : CanUpdate;

		#endregion
	}
}
