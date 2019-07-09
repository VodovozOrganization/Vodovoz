using System;
using QS.Project.Domain;
using QS.Services;
using QS.ViewModels;
using Vodovoz.Domain.Logistic;
namespace Vodovoz.Dialogs.Fuel
{
	public class FuelTypeViewModel : EntityTabViewModelBase<FuelType>
	{
		public FuelTypeViewModel(IEntityConstructorParam ctorParam, ICommonServices commonServices) : base(ctorParam, commonServices)
		{
		}
	}
}
