using System.Threading.Tasks;
using System.Threading;
using QS.Project.Journal.Search.Criterion;

namespace Vodovoz.SearchViewModels
{
	public class DelayedMultipleEntryCriterionSearchViewModel : MultipleEntryCriterionSearchViewModel
	{
		private CancellationTokenSource cts;
		private readonly int delay;

		public DelayedMultipleEntryCriterionSearchViewModel(CriterionSearchModel searchModel, int delay = 1000) : base(searchModel)
		{
			this.delay = delay < 0 ? 0 : delay;
		}

		private void StartUpdateTask()
		{
			if(cts != null) {
				cts.Cancel();
				cts.Dispose();
			}
			cts = new CancellationTokenSource();

			Task delayTask = Task.Delay(delay, cts.Token);
			delayTask.ContinueWith((task) => UpdateValue(), cts.Token, TaskContinuationOptions.OnlyOnRanToCompletion, TaskScheduler.Current);
		}

		private void UpdateValue()
		{
			if(cts == null || cts.IsCancellationRequested) {
				return;
			}
			base.OnSearchValueUpdated();
		}

		public override void ManualSearchUpdate()
		{
			base.OnSearchValueUpdated();
		}

		protected override void OnSearchValueUpdated()
		{
			StartUpdateTask();
		}
	}
}
