using System;
using Google.OrTools.ConstraintSolver;
using Gtk;
using QSProjectsLib;

namespace Vodovoz.Additions.Logistic.RouteOptimization
{
	public class CallbackMonitor : SearchMonitor
	{
		static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
		ProgressBar progress;
		Gtk.TextBuffer buffer;
		SolutionCollector bestSol;

		public CallbackMonitor(Solver s, ProgressBar bar, TextBuffer buf, SolutionCollector best) : base(s)
		{
			progress = bar;
			//solver = s;
			buffer = buf;
			bestSol = best;
		}

		public override int ProgressPercent()
		{
			var val = base.ProgressPercent();
			logger.Info(val);
			return val;
		}

		public override void PeriodicCheck()
		{
			if(buffer != null) {
				buffer.Text = String.Format("Branches={0}\nFailures={1}\nFailStamp={2}\nSolutions={3}\nWallTime={4}\nCost={5}",
											solver().Branches(),
											solver().Failures(),
											solver().FailStamp(),
											solver().Solutions(),
											solver().WallTime(),
											bestSol.SolutionCount() > 0 ? bestSol.ObjectiveValue(0) : -1);
				QSMain.WaitRedraw(200);
			}

			base.PeriodicCheck();
		}

		public override void EnterSearch()
		{
			logger.Debug("EnterSearch");
			base.EnterSearch();
		}

		public override bool AcceptSolution()
		{
			var val = base.AcceptSolution();
			if(bestSol.SolutionCount() > 0) {
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
