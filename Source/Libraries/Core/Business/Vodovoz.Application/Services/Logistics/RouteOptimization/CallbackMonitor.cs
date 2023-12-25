﻿using System;
using Google.OrTools.ConstraintSolver;

namespace Vodovoz.Application.Services.Logistics.RouteOptimization
{
	/// <summary>
	/// Класс позволяющий мониторить состояние процесс оптимизации
	/// </summary>
	public class CallbackMonitor : SearchMonitor
	{
		private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
		private SolutionCollector bestSol;
		private readonly Action<string> statisticsTxtFunc;

		public CallbackMonitor(Solver s, Action<string> statisticsTxtAction, SolutionCollector best) : base(s)
		{
			statisticsTxtFunc = statisticsTxtAction;
			bestSol = best;
		}

		public override int ProgressPercent()
		{
			var val = base.ProgressPercent();
			logger.Info(val);
			return val;
		}

		/// <summary>
		/// Нужен только для отображения текущего состояния поиска решений.
		/// </summary>
		public override void PeriodicCheck()
		{
			if(statisticsTxtFunc != null)
			{
				statisticsTxtFunc.Invoke(
					string.Format(
						"Branches={0}\nFailures={1}\nFailStamp={2}\nSolutions={3}\nWallTime={4}\nCost={5}",
						solver().Branches(),
						solver().Failures(),
						solver().FailStamp(),
						solver().Solutions(),
						solver().WallTime(),
						bestSol.SolutionCount() > 0 ? bestSol.ObjectiveValue(0) : -1
					)
				);
				//QSMain.WaitRedraw(200);
			}

			base.PeriodicCheck();
		}

		public override void EnterSearch()
		{
			logger.Debug("EnterSearch");
			base.EnterSearch();
		}

		/// <summary>
		/// Логируем стоимость нового решения.
		/// </summary>
		public override bool AcceptSolution()
		{
			var val = base.AcceptSolution();
			if(bestSol.SolutionCount() > 0)
			{
				logger.Debug("New Cost = {0}",
							 bestSol.ObjectiveValue(0)
			   );
			}
			return val;
		}

		public override bool LocalOptimum()
		{
			var val = base.LocalOptimum();
			logger.Debug("LocalOptimum={0}", val);
			return val;
		}
	}
}
