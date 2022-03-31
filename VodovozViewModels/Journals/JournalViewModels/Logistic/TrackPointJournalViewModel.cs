using NHibernate;
using NHibernate.Transform;
using QS.DomainModel.UoW;
using QS.Project.Journal;
using QS.Services;
using System;
using System.Timers;
using Vodovoz.Domain.Logistic;
using Vodovoz.ViewModels.Journals.FilterViewModels.Logistic;
using Vodovoz.ViewModels.Journals.JournalNodes.Logistic;
using Vodovoz.ViewModels.ViewModels.Logistic;

namespace Vodovoz.ViewModels.Journals.JournalViewModels.Logistic
{
	public class TrackPointJournalViewModel : FilterableSingleEntityJournalViewModelBase<TrackPoint, TrackPointViewModel, TrackPointJournalNode,
		TrackPointJournalFilterViewModel>
	{
		private readonly Timer _timer;
		private const double _interval = 30 * 1000; //30 секунд
		public TrackPointJournalViewModel(TrackPointJournalFilterViewModel filterViewModel, IUnitOfWorkFactory unitOfWorkFactory, ICommonServices commonServices)
			: base(filterViewModel, unitOfWorkFactory, commonServices)
		{
			TabName = "Журнал координат";

			_timer = new Timer(_interval);
			_timer.Elapsed += TimerOnElapsed;
			_timer.Start();
		}

		protected override void CreateNodeActions() { }

		private void TimerOnElapsed(object sender, ElapsedEventArgs e)
		{
			_timer.Interval = _interval;
			Refresh();
		}

		protected override Func<IUnitOfWork, IQueryOver<TrackPoint>> ItemsSourceQueryFunction => (uow) =>
		{
			if(FilterViewModel.RouteListId == null)
			{
				return null;
			}

			TrackPoint trackPointAlias = null;
			Track trackAlias = null;
			TrackPointJournalNode resultAlias = null;

			var itemsQuery = uow.Session.QueryOver(() => trackPointAlias)
				.JoinAlias(tp => tp.Track, () => trackAlias);

			if(FilterViewModel.RouteListId != null)
			{
				itemsQuery.Where(() => trackAlias.RouteList.Id == FilterViewModel.RouteListId);
			}

			itemsQuery.Where(GetSearchCriterion(
				() => trackPointAlias.Latitude,
				() => trackPointAlias.Longitude,
				() => trackPointAlias.TimeStamp,
				() => trackPointAlias.ReceiveTimeStamp,
				() => trackAlias.RouteList.Id)
			);

			itemsQuery
				.SelectList(list => list
					.Select(() => trackPointAlias.TimeStamp).WithAlias(() => resultAlias.Time)
					.Select(() => trackPointAlias.ReceiveTimeStamp).WithAlias(() => resultAlias.ReceiveTime)
					.Select(() => trackPointAlias.Longitude).WithAlias(() => resultAlias.Longitude)
					.Select(() => trackPointAlias.Latitude).WithAlias(() => resultAlias.Latitude)
					.Select(() => trackAlias.RouteList.Id).WithAlias(() => resultAlias.RouteListId)
				)
				.OrderBy(() => trackPointAlias.ReceiveTimeStamp).Desc
				.TransformUsing(Transformers.AliasToBean<TrackPointJournalNode>());

			return itemsQuery;
		};

		protected override Func<TrackPointViewModel> CreateDialogFunction =>
			() => throw new InvalidOperationException("Нельзя создавать координаты из данного журнала");

		protected override Func<TrackPointJournalNode, TrackPointViewModel> OpenDialogFunction =>
			(node) => throw new InvalidOperationException("Нельзя изменять координаты из данного журнала");

		public override void Dispose()
		{
			_timer?.Dispose();
			base.Dispose();
		}
	}
}
