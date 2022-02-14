using System.Collections.Generic;
using System.Threading;
using QS.Dialog;
using QS.DomainModel.UoW;
using Vodovoz.Domain.Logistic;

namespace Vodovoz.Controllers
{
	public interface IRouteListsWageController
	{
		void RecalculateRouteListsWage(IUnitOfWork uow, IList<RouteList> routeLists, CancellationToken token);

		IProgressBarDisplayable ProgressBarDisplayable { get; set; }
	}
}
