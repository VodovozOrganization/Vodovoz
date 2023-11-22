using QS.DomainModel.UoW;
using QS.Project.Domain;
using QS.Services;
using QS.ViewModels;
using Vodovoz.Domain.Logistic;

namespace Vodovoz.ViewModels.ViewModels.Logistic
{
	public class RegradingOfGoodsReasonViewModel : EntityTabViewModelBase<RegradingOfGoodsReason>
	{
		public RegradingOfGoodsReasonViewModel(IEntityUoWBuilder uowBuilder, IUnitOfWorkFactory unitOfWorkFactory, ICommonServices commonServices)
			: base(uowBuilder, unitOfWorkFactory, commonServices)
		{
			TabName = "Причина пересортицы";
		}
	}
}
