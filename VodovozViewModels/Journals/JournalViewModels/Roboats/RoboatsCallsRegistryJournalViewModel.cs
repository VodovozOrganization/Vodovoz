using NHibernate;
using NHibernate.Criterion;
using NHibernate.Dialect.Function;
using NHibernate.Transform;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Journal;
using QS.Project.Journal.DataLoader;
using QS.Services;
using System.Timers;
using Vodovoz.Domain.Roboats;
using Vodovoz.ViewModels.Journals.JournalNodes.Roboats;

namespace Vodovoz.ViewModels.Journals.JournalViewModels.Roboats
{
	public class RoboatsCallsRegistryJournalViewModel : JournalViewModelBase
	{
		private Timer _autoRefreshTimer;

		public RoboatsCallsRegistryJournalViewModel(
			IUnitOfWorkFactory unitOfWorkFactory,
			ICommonServices commonServices,
			INavigationManager navigation = null)
			: base(unitOfWorkFactory, commonServices.InteractiveService, navigation)
		{
			Title = "Реестр Roboats звонков";

			var threadDataLoader = new ThreadDataLoader<RoboatsCallJournalNode>(unitOfWorkFactory);
			threadDataLoader.DynamicLoadingEnabled = true;
			threadDataLoader.AddQuery(GetQuery);
			DataLoader = threadDataLoader;

			CreateNodeActions();

			StartAutoRefresh();
		}

		private void StartAutoRefresh()
		{
			_autoRefreshTimer = new Timer(15000);
			_autoRefreshTimer.Elapsed += (s, e) => Refresh();
			_autoRefreshTimer.Start();
		}

		public override JournalSelectionMode SelectionMode => JournalSelectionMode.Single;

		protected override void CreateNodeActions()
		{
			NodeActionsList.Clear();
		}

		private IQueryOver<RoboatsCall> GetQuery(IUnitOfWork uow)
		{
			RoboatsCall callAlias = null;
			RoboatsCallDetail callDetailAlias = null;
			RoboatsCallJournalNode resultAlias = null;

			var query = uow.Session.QueryOver(() => callAlias)
				.Left.JoinAlias(() => callAlias.CallDetails, () => callDetailAlias);

			query.Where(
				GetSearchCriterion(
					() => callAlias.Id,
					() => callAlias.Phone
				)
			);

			query.SelectList(list => list
				.SelectGroup(() => callAlias.Id).WithAlias(() => resultAlias.Id)
				.Select(Projections.Property(() => callAlias.CallTime)).WithAlias(() => resultAlias.Time)
				.Select(Projections.Property(() => callAlias.Phone)).WithAlias(() => resultAlias.Phone)
				.Select(Projections.Property(() => callAlias.Status)).WithAlias(() => resultAlias.Status)
				.Select(Projections.Property(() => callAlias.Result)).WithAlias(() => resultAlias.Result)
				.Select(
					Projections.SqlFunction(
						"GROUP_CONCAT",
						NHibernateUtil.String,
						Projections.SqlFunction(
							"CONCAT_WS",
							NHibernateUtil.String,
							Projections.Constant(", "),
							Projections.SqlFunction(
								new SQLFunctionTemplate(NHibernateUtil.String, "DATE_FORMAT(?1, '%d.%m.%y %H:%i:%s')"),
								NHibernateUtil.String,
								Projections.Property(() => callDetailAlias.OperationTime)
							),
							Projections.Property(() => callDetailAlias.Description)
						),
						Projections.Constant("\n")
					)
				).WithAlias(() => resultAlias.Details)
			)
			.OrderByAlias(() => callAlias.CallTime).Desc()
			.TransformUsing(Transformers.AliasToBean<RoboatsCallJournalNode>());

			return query;
		}

		public override void Dispose()
		{
			_autoRefreshTimer?.Dispose();
			base.Dispose();
		}
	}
}//  
