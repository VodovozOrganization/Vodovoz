using QS.Project.Domain;
using QS.Services;
using QS.ViewModels;
using Vodovoz.Domain.Complaints;

namespace Vodovoz.ViewModels.Complaints
{
	public class ComplaintResultViewModel : EntityTabViewModelBase<ComplaintResult>
	{
		public ComplaintResultViewModel(IEntityConstructorParam ctorParam, ICommonServices commonServices) : base(ctorParam, commonServices)
		{
			TabName = "Результат рассмотрения жалобы";
		}
	}
}