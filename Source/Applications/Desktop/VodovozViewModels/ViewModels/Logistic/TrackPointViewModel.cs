using QS.DomainModel.UoW;
using QS.Project.Domain;
using QS.Services;
using QS.ViewModels;
using Vodovoz.Domain.Logistic;

namespace Vodovoz.ViewModels.ViewModels.Logistic
{
	public class TrackPointViewModel : EntityTabViewModelBase<TrackPoint>
	{
		public TrackPointViewModel(IEntityUoWBuilder uowBuilder, IUnitOfWorkFactory unitOfWorkFactory, ICommonServices commonServices)
			: base(uowBuilder, unitOfWorkFactory, commonServices) { }
	}
}
