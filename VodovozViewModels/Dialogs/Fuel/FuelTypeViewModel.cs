using System;
using QS.DomainModel.UoW;
using QS.Project.Domain;
using QS.Services;
using QS.ViewModels;
using QS.ViewModels.Extension;
using Vodovoz.Domain.Logistic;
namespace Vodovoz.ViewModels.Dialogs.Fuel
{
	public class FuelTypeViewModel : EntityTabViewModelBase<FuelType>, IAskSaveOnCloseViewModel
	{
		public FuelTypeViewModel(IEntityUoWBuilder uoWBuilder, IUnitOfWorkFactory unitOfWorkFactory, ICommonServices commonServices) : base(uoWBuilder, unitOfWorkFactory, commonServices)
		{
			CanEdit = PermissionResult.CanUpdate 
			          || (PermissionResult.CanCreate && Entity.Id == 0);
		}

		public bool CanEdit { get; }
		
		public bool AskSaveOnClose => CanEdit;
	}
}
