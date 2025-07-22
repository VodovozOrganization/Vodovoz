using QS.Commands;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Domain;
using QS.Services;
using QS.ViewModels;
using Vodovoz.Domain.Orders;

namespace Vodovoz.ViewModels.Logistic
{
	public class SelfDeliveringOrderEditViewModel : EntityTabViewModelBase<Order>
	{
		public SelfDeliveringOrderEditViewModel(
			IEntityUoWBuilder uowBuilder, 
			IUnitOfWorkFactory unitOfWorkFactory, 
			ICommonServices commonServices,
			INavigationManager navigation = null) : base(uowBuilder, unitOfWorkFactory, commonServices, navigation)
		{
			TabName = "Редактирование самовывоза";
			var t = Entity.GetNomenclaturesWithFixPrices;
			var e = Entity;
		
		}
		public DelegateCommand SaveCommand { get; }
		public DelegateCommand CloseCommand { get; }

		protected override bool BeforeSave()
		{

			return base.BeforeSave();
		}
	}
}
