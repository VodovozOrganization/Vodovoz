using System;
using QS.DomainModel.UoW;
using QS.Project.Domain;
using QS.Services;
using QS.ViewModels;
using Vodovoz.Domain.Logistic;
namespace Vodovoz.ViewModels.Dialogs.Fuel
{
	public class FuelTypeViewModel : EntityTabViewModelBase<FuelType>
	{
		public FuelTypeViewModel(IEntityUoWBuilder uoWBuilder, IUnitOfWorkFactory unitOfWorkFactory, ICommonServices commonServices) : base(uoWBuilder, unitOfWorkFactory, commonServices)
		{
		}
	}
}
