using System;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Services;
using QS.Tdi;
using QS.ViewModels;
using QS.ViewModels.Dialog;

namespace Vodovoz.ViewModels.ViewModels.Reports
{
	public class DeliveryAnalyticsViewModel : DialogTabViewModelBase, ITDICloseControlTab
	{
		public DeliveryAnalyticsViewModel(
			IUnitOfWorkFactory unitOfWorkFactory,
			IInteractiveService interactiveService,
			INavigationManager navigation)
			: base(unitOfWorkFactory, interactiveService, navigation)
		{
			//this.interactiveService = interactiveService ?? throw new ArgumentNullException(nameof(interactiveService));

			if(unitOfWorkFactory == null)
			{
				throw new ArgumentNullException(nameof(unitOfWorkFactory));
			}
			UoW = unitOfWorkFactory.CreateWithoutRoot();
			Title = "Аналитика объёмов доставки";
		}

		public bool CanClose()
		{
			throw new NotImplementedException();
		}
	}
}
