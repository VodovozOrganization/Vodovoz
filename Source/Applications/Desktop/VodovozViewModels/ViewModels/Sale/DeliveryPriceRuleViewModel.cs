using Microsoft.Extensions.Logging;
using QS.DomainModel.UoW;
using QS.Project.Domain;
using QS.Services;
using QS.ViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Vodovoz.Domain.Sale;
using Vodovoz.EntityRepositories.Sale;

namespace Vodovoz.ViewModels.ViewModels.Sale
{
	public class DeliveryPriceRuleViewModel : EntityTabViewModelBase<DeliveryPriceRule>
	{
		private readonly ILogger<DeliveryPriceRuleViewModel> _logger;
		private readonly IDistrictRuleRepository _districtRuleRepository;

		public DeliveryPriceRuleViewModel(
			ILogger<DeliveryPriceRuleViewModel> logger,
			IEntityUoWBuilder uowBuilder,
			IUnitOfWorkFactory unitOfWorkFactory,
			ICommonServices commonServices,
			IDistrictRuleRepository districtRuleRepository)
			: base(uowBuilder, unitOfWorkFactory, commonServices)
		{
			_logger = logger
				?? throw new ArgumentNullException(nameof(logger));
			_districtRuleRepository = districtRuleRepository
				?? throw new ArgumentNullException(nameof(districtRuleRepository));

			ValidationContext.ServiceContainer.AddService(typeof(IDistrictRuleRepository), districtRuleRepository);

			if(!CanRead)
			{
				AbortOpening("У вас недостаточно прав для просмотра");
			}

			TabName = (CanCreateOrUpdate) ? "Редактирование правила цен доставки" : "Просмотр правила цен доставки";
		}

		public List<DistrictAndDistrictSet> DistrictsHavingCurrentRule =>
			_districtRuleRepository.GetDistrictNameDistrictSetNameAndCreationDateByDeliveryPriceRule(UoW, Entity);

		public override bool Save(bool close)
		{
			_logger.LogInformation("Сохраняем правило для цены доставки...");
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
