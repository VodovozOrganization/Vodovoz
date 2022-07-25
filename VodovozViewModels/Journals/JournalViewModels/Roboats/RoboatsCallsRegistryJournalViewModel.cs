using ClosedXML.Excel;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.Dialect.Function;
using NHibernate.Transform;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Journal;
using QS.Project.Journal.DataLoader;
using QS.Project.Services.FileDialog;
using QS.Services;
using System;
using System.Timers;
using Vodovoz.Domain.Roboats;
using Vodovoz.ViewModels.Journals.FilterViewModels.Roboats;
using Vodovoz.ViewModels.Journals.JournalNodes.Roboats;

namespace Vodovoz.ViewModels.Journals.JournalViewModels.Roboats
{
	public class RoboatsCallsRegistryJournalViewModel : JournalViewModelBase
	{
		private readonly IFileDialogService _fileDialogService;
		private Timer _autoRefreshTimer;

		public RoboatsCallsRegistryJournalViewModel(
			IUnitOfWorkFactory unitOfWorkFactory,
			RoboatsCallsFilterViewModel filterViewModel,
			ICommonServices commonServices,
			IFileDialogService fileDialogService,
			INavigationManager navigation = null)
			: base(unitOfWorkFactory, commonServices.InteractiveService, navigation)
		{
			_fileDialogService = fileDialogService ?? throw new System.ArgumentNullException(nameof(fileDialogService));
			Filter = filterViewModel ?? throw new System.ArgumentNullException(nameof(filterViewModel));

			Title = "Реестр Roboats звонков";

			var threadDataLoader = new ThreadDataLoader<RoboatsCallJournalNode>(unitOfWorkFactory);
			threadDataLoader.DynamicLoadingEnabled = true;
			threadDataLoader.AddQuery(GetQuery);
			DataLoader = threadDataLoader;

			CreateNodeActions();

			StartAutoRefresh();
		}

		private RoboatsCallsFilterViewModel _filter;

		public virtual RoboatsCallsFilterViewModel Filter
		{
			get => _filter;
			set
			{
				if(_filter != null)
				{
					_filter.OnFiltered -= FilterViewModel_OnFiltered;
				}

				if(SetField(ref _filter, value))
				{
					if(_filter != null)
					{
						_filter.OnFiltered += FilterViewModel_OnFiltered;
					}
				}
			}
		}

		void FilterViewModel_OnFiltered(object sender, EventArgs e)
		{
			Refresh();
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
			CreateExportAction();
		}

		private void CreateExportAction()
		{
			var exportAction = new JournalAction("Выгрузить в Excel",
				(selected) => true,
				(selected) => true,
				(selected) => ExportToExcel()
			);
			NodeActionsList.Add(exportAction);
		}

		private void ExportToExcel()
		{
			var dialogSettings = new DialogSettings();
			dialogSettings.DefaultFileExtention = ".xlsx";
			dialogSettings.FileFilters.Clear();
			dialogSettings.FileFilters.Add(new DialogFileFilter("Excel", ".xlsx"));

			var result = _fileDialogService.RunSaveFileDialog(dialogSettings);
			if(!result.Successful)
			{
				return;
			}

			using(var workbook = new XLWorkbook())
			{
				var worksheet = workbook.Worksheets.Add("Реестр звонков");
				worksheet.Cell(1, 1).Value = "Код";
				worksheet.Cell(1, 2).Value = "Время звонка";
				worksheet.Cell(1, 3).Value = "Телефон";
				worksheet.Cell(1, 4).Value = "Статус";
				worksheet.Cell(1, 5).Value = "Результат";
				worksheet.Cell(1, 6).Value = "Детали звонка";

				var exportedList = Items;
				for(int i = 0; i < Items.Count; i++)
				{
					var currentItem = exportedList[i] as RoboatsCallJournalNode;
					if(currentItem == null)
					{
						continue;
					}

					worksheet.Cell(i + 2, 1).Value = currentItem.Id;
					worksheet.Cell(i + 2, 2).Value = currentItem.Time;
					worksheet.Cell(i + 2, 3).Value = currentItem.Phone;
					worksheet.Cell(i + 2, 4).Value = currentItem.CallStatus;
					worksheet.Cell(i + 2, 5).Value = currentItem.CallResult;
					worksheet.Cell(i + 2, 6).Value = currentItem.Details;
				}
				workbook.SaveAs(result.Path);
			}
		}

		private IQueryOver<RoboatsCall> GetQuery(IUnitOfWork uow)
		{
			RoboatsCall callAlias = null;
			RoboatsCallDetail callDetailAlias = null;
			RoboatsCallJournalNode resultAlias = null;

			var query = uow.Session.QueryOver(() => callAlias)
				.Left.JoinAlias(() => callAlias.CallDetails, () => callDetailAlias);

			if(Filter != null)
			{
				if(Filter.StartDate.HasValue)
				{
					query.Where(() => callAlias.CallTime >= Filter.StartDate);
				}

				if(Filter.EndDate.HasValue)
				{
					query.Where(() => callAlias.CallTime <= Filter.EndDate);
				}

				if(Filter.Status.HasValue)
				{
					query.Where(() => callAlias.Status == Filter.Status);
				}
			}

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
			.OrderByAlias(() => callAlias.Id).Desc()
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
