using QS.Services;
using QS.ViewModels;
using Vodovoz.Domain.Logistic;

namespace Vodovoz.ViewModels.ViewModels.Logistic
{
	public class LogisticsRequirementsViewModel : EntityWidgetViewModelBase<LogisticsRequirements>
	{
		public LogisticsRequirementsViewModel(
			LogisticsRequirements entity,
			ICommonServices commonServices) : base(entity, commonServices) 
		{ 
		}
	}
}
