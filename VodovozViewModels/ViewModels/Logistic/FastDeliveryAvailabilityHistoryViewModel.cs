using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Domain;
using QS.Services;
using QS.ViewModels;
using Vodovoz.Domain.Logistic.FastDelivery;
using Vodovoz.EntityRepositories.Delivery;
using Vodovoz.Infrastructure.Services;
using Vodovoz.Tools.Orders;
using Vodovoz.ViewModels.Widgets;

namespace Vodovoz.ViewModels.ViewModels.Logistic
{
	public class FastDeliveryAvailabilityHistoryViewModel : EntityTabViewModelBase<FastDeliveryAvailabilityHistory>
	{
		public FastDeliveryAvailabilityHistoryViewModel(
			IEntityUoWBuilder uowBuilder,
			IUnitOfWorkFactory unitOfWorkFactory,
			ICommonServices commonServices,
			IEmployeeService employeeService,
			INavigationManager navigation = null) : base(uowBuilder, unitOfWorkFactory, commonServices, navigation)
		{
			var logistician = employeeService.GetEmployeeForUser(UoW, UserService.CurrentUserId);
			FastDeliveryVerificationViewModel = new FastDeliveryVerificationViewModel(GetFastDeliveryVerification(), UoW, logistician);
		}

		private FastDeliveryVerificationDTO GetFastDeliveryVerification()
		{
			var fastDeliveryHistoryItemConverter = new FastDeliveryHistoryConverter();

			return new FastDeliveryVerificationDTO
			{
				FastDeliveryVerificationDetailsNodes = fastDeliveryHistoryItemConverter.ConvertAvailabilityHistoryItemsToVerificationDetailsNodes(Entity.Items),
				FastDeliveryAvailabilityHistory = Entity
			};
		}

		public override bool HasChanges => false;

		public FastDeliveryVerificationViewModel FastDeliveryVerificationViewModel { get; }
	}
}
