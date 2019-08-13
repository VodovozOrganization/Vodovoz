using QS.Project.Domain;
using QS.Services;
using QS.ViewModels;
using Vodovoz.Domain.Complaints;

namespace Vodovoz.ViewModels.Complaints
{
	public class ComplaintSourceViewModel : EntityTabViewModelBase<ComplaintSource>
	{
		public ComplaintSourceViewModel(IEntityConstructorParam ctorParam, ICommonServices commonServices) : base(ctorParam, commonServices)
		{
			TabName = "Источники жалоб";
		}
	}
}
