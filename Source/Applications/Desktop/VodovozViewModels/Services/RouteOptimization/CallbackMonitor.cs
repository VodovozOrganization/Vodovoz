using System;
using Google.OrTools.ConstraintSolver;
using Microsoft.Extensions.Logging;

namespace Vodovoz.ViewModels.Services.RouteOptimization
{
	/// <summary>
	/// Класс позволяющий мониторить состояние процесс оптимизации
	/// </summary>
	public class CallbackMonitor : SearchMonitor
	{
		private readonly ILogger<CallbackMonitor> _logger;
		private SolutionCollector _bestSol;
		private readonly Action<string> _statisticsTxtFunc;

		public CallbackMonitor(
			ILogger<CallbackMonitor> logger,
			Solver s,
			Action<string> statisticsTxtAction,
			SolutionCollector best)
			: base(s)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_statisticsTxtFunc = statisticsTxtAction;
			_bestSol = best;
		}

		public override int ProgressPercent()
		{
			var val = base.ProgressPercent();
			_logger.LogInformation(val.ToString());
			return val;
		}

		/// <summary>
		/// Нужен только для отображения текущего состояния поиска решений.
		/// </summary>
		public override void PeriodicCheck()
		{
			if(_statisticsTxtFunc != null)
			{
				_statisticsTxtFunc.Invoke(
					$"Branches={solver().Branches()}\nFailures={solver().Failures()}\nFailStamp={solver().FailStamp()}\nSolutions={solver().Solutions()}\nWallTime={solver().WallTime()}\nCost={(_bestSol.SolutionCount() > 0 ? _bestSol.ObjectiveValue(0) : -1)}");
			}

			base.PeriodicCheck();
		}

		public override void EnterSearch()
		{
			_logger.LogDebug("EnterSearch");
			base.EnterSearch();
		}

		/// <summary>
		/// Логируем стоимость нового решения.
		/// </summary>
		public override bool AcceptSolution()
		{
			var val = base.AcceptSolution();

			if(_bestSol.SolutionCount() > 0)
			{
				_logger.LogDebug("New Cost = {Cost}", _bestSol.ObjectiveValue(0));
			}

			return val;
		}

		public override bool LocalOptimum()
		{
			var val = base.LocalOptimum();
			_logger.LogDebug("LocalOptimum={LocalOptimum}", val);
			return val;
		}
	}
}
