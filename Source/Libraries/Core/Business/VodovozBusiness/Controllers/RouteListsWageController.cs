using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using NHibernate.Linq;
using QS.Dialog;
using QS.DomainModel.UoW;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.WageCalculation.CalculationServices.RouteList;

namespace Vodovoz.Controllers
{
	public class RouteListsWageController : IRouteListsWageController
	{
		private readonly WageParameterService _wageParameterService;

		public RouteListsWageController(WageParameterService wageParameterService)
		{
			_wageParameterService = wageParameterService ?? throw new ArgumentNullException(nameof(wageParameterService));
		}

		public IProgressBarDisplayable ProgressBarDisplayable { get; set; }

		public void RecalculateRouteListsWage(IUnitOfWork uow, IList<RouteList> routeLists, CancellationToken token)
		{
			if(uow == null)
			{
				throw new ArgumentNullException(nameof(uow));
			}
			if(routeLists == null)
			{
				throw new ArgumentNullException(nameof(routeLists));
			}
			if(!routeLists.Any())
			{
				return;
			}
			if(token.IsCancellationRequested)
			{
				throw new OperationCanceledException("Отмена операции пересчёта зарплаты в МЛ");
			}

			ProgressBarDisplayable?.Start(routeLists.Count, 0, "Подготовка данных...");

			var routeListsIds = routeLists.Select(rl => rl.Id);

			//Предзагрузка данных для ускорения пересчёта
			_ = uow.Session.Query<RouteList>()
				.Where(x => routeListsIds.Contains(x.Id))
				.FetchMany(x => x.Addresses)
				.ThenFetch(a => a.Order)
				.ThenFetch(o => o.DeliveryPoint)
				.ToList();

			var formatString = $"Пересчёт зарплаты в МЛ... ({{0}}/{routeLists.Count})";
			ProgressBarDisplayable?.Update(string.Format(formatString, 0));

			for(var i = 0; i < routeLists.Count; i++)
			{
				if(token.IsCancellationRequested)
				{
					throw new OperationCanceledException("Отмена операции пересчёта зарплаты в МЛ");
				}

				var rl = routeLists[i];
				foreach(var address in rl.Addresses)
				{
					address.DriverWageCalculationMethodic = null;
					address.ForwarderWageCalculationMethodic = null;
				}
				rl.RecalculateAllWages(_wageParameterService);
				rl.UpdateWageOperation();

				ProgressBarDisplayable?.Add(1, string.Format(formatString, i + 1));
			}
		}
	}
}
