using ClosedXML.Excel;
using Gamma.Binding.Core.RecursiveTreeConfig;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.Transform;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Journal;
using QS.Project.Journal.DataLoader;
using QS.Project.Journal.DataLoader.Hierarchy;
using QS.Project.Services.FileDialog;
using QS.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Timers;
using Vodovoz.Domain.Roboats;
using Vodovoz.Settings.Roboats;
using Vodovoz.ViewModels.Journals.FilterViewModels.Roboats;
using Vodovoz.ViewModels.Journals.JournalNodes.Roboats;

namespace Vodovoz.ViewModels.Journals.JournalViewModels.Roboats
{
	public class RoboatsCallsRegistryJournalViewModel : JournalViewModelBase
	{
		private readonly IFileDialogService _fileDialogService;
		private int _autoRefreshInterval;
		private Timer _autoRefreshTimer;

		public RoboatsCallsRegistryJournalViewModel(
			IUnitOfWorkFactory unitOfWorkFactory,
			RoboatsCallsFilterViewModel filterViewModel,
			ICommonServices commonServices,
			IFileDialogService fileDialogService,
			IRoboatsSettings roboatsSettings,
			INavigationManager navigation = null)
			: base(unitOfWorkFactory, commonServices.InteractiveService, navigation)
		{
			if(roboatsSettings is null)
			{
				throw new ArgumentNullException(nameof(roboatsSettings));
			}

			_fileDialogService = fileDialogService ?? throw new System.ArgumentNullException(nameof(fileDialogService));
			Filter = filterViewModel ?? throw new System.ArgumentNullException(nameof(filterViewModel));

			Title = "Реестр Roboats звонков";

			var levelDataLoader = new HierarchicalQueryLoader<RoboatsCall, RoboatsCallJournalNode>(unitOfWorkFactory, GetCount);
			
			levelDataLoader.SetLevelingModel(GetQuery)
				.AddNextLevelSource(GetDetails);

			RecuresiveConfig = levelDataLoader.TreeConfig;

			var threadDataLoader = new ThreadDataLoader<RoboatsCallJournalNode>(unitOfWorkFactory);
			threadDataLoader.DynamicLoadingEnabled = true;
			threadDataLoader.QueryLoaders.Add(levelDataLoader);
			DataLoader = threadDataLoader;
			_autoRefreshInterval = roboatsSettings.CallRegistryAutoRefreshInterval;

			CreateNodeActions();
			StartAutoRefresh();
		}

		public IRecursiveConfig RecuresiveConfig { get; }

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

		public override string FooterInfo
		{
			get
			{
				var loadedCount = DataLoader.TotalCount.HasValue ? $" | Загружено: { DataLoader.TotalCount.Value }" : "";
				return $"{GetAutoRefreshInfo()}{loadedCount}";
			}

			set => base.FooterInfo = value;
		}

		public override JournalSelectionMode SelectionMode => JournalSelectionMode.Single;

		protected override void CreateNodeActions()
		{
			NodeActionsList.Clear();
			CreateExportAction();
			CreateAutorefreshAction();
		}

		#region Queries

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
				.Select(Projections.Constant(RoboatsCallNodeType.RoboatsCall)).WithAlias(() => resultAlias.NodeType)
				.Select(Projections.Property(() => callAlias.CallTime)).WithAlias(() => resultAlias.Time)
				.Select(Projections.Property(() => callAlias.Phone)).WithAlias(() => resultAlias.Phone)
				.Select(Projections.Property(() => callAlias.Status)).WithAlias(() => resultAlias.Status)
				.Select(Projections.Property(() => callAlias.Result)).WithAlias(() => resultAlias.Result)
				.Select(Projections.Count(() => callDetailAlias.Id)).WithAlias(() => resultAlias.ProblemsCount)			
			)
			.OrderByAlias(() => callAlias.Id).Desc()
			.TransformUsing(Transformers.AliasToBean<RoboatsCallJournalNode>());

			return query;
		}

		private IList<RoboatsCallJournalNode> GetDetails(IEnumerable<RoboatsCallJournalNode> parentNodes)
		{
			using(var uow = UnitOfWorkFactory.CreateWithoutRoot())
			{
				RoboatsCall callAlias = null;
				RoboatsCallDetail callDetailAlias = null;
				RoboatsCallJournalNode resultAlias = null;

				var query = uow.Session.QueryOver(() => callDetailAlias)
					.Where(Restrictions.In(Projections.Property(() => callDetailAlias.Call.Id), parentNodes.Select(x => x.Id).ToArray()));

				query.SelectList(list => list
				.Select(Projections.Constant(RoboatsCallNodeType.RoboatsCallDetail)).WithAlias(() => resultAlias.NodeType)
					.Select(() => callDetailAlias.Call.Id).WithAlias(() => resultAlias.ParentId)
					.Select(() => callDetailAlias.OperationTime).WithAlias(() => resultAlias.Time)
					.Select(() => callDetailAlias.Description).WithAlias(() => resultAlias.Details)
				)
				.OrderByAlias(() => callAlias.Id).Desc()
				.TransformUsing(Transformers.AliasToBean<RoboatsCallJournalNode>());

				return query.List<RoboatsCallJournalNode>();
			}
		}

		#endregion Queries

		private int GetCount(IUnitOfWork uow)
		{
			var query = GetQuery(uow);
			var count = query.List<RoboatsCallJournalNode>().Count();
			return count;
		}

		#region Autorefresh

		private bool autoRefreshEnabled => _autoRefreshTimer != null && _autoRefreshTimer.Enabled;

		private void StartAutoRefresh()
		{
			if(autoRefreshEnabled)
			{
				return;
			}
			_autoRefreshTimer = new Timer(_autoRefreshInterval * 1000);
			_autoRefreshTimer.Elapsed += (s, e) => Refresh();
			_autoRefreshTimer.Start();
		}

		private void StopAutoRefresh()
		{
			_autoRefreshTimer?.Stop();
			_autoRefreshTimer = null;
		}

		private void SwitchAutoRefresh()
		{
			if(autoRefreshEnabled)
			{
				StopAutoRefresh();
			}
			else
			{
				StartAutoRefresh();
			}
			OnPropertyChanged(nameof(FooterInfo));
		}

		private string GetAutoRefreshInfo()
		{
			if(autoRefreshEnabled)
			{
				return $"Автообновление каждые {_autoRefreshInterval} сек.";
			}
			else
			{
				return $"Автообновление выключено";
			}
		}

		private void CreateAutorefreshAction()
		{
			var switchAutorefreshAction = new JournalAction("Вкл/Выкл автообновление",
				(selected) => true,
				(selected) => true,
				(selected) => SwitchAutoRefresh()
			);
			NodeActionsList.Add(switchAutorefreshAction);
		}

		#endregion Autorefresh

		#region Export

		private void CreateExportAction()
		{
			var exportAction = new JournalAction("Выгрузить в Excel",
				(selected) => true,
				(selected) => true,
				(selected) => RunExportToExcel()
			);
			NodeActionsList.Add(exportAction);
		}

		private void RunExportToExcel()
		{
			StopAutoRefresh();
			try
			{
				ExportToExcel();
			}
			finally
			{
				StartAutoRefresh();
			}
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
				worksheet.Cell(1, 6).Value = "Время проверки";
				worksheet.Cell(1, 7).Value = "Детали звонка";

				var excelRowCounter = 2;
				foreach(var call in Items.Cast<RoboatsCallJournalNode>())
				{
					if(!call.Children.Any())
					{
						worksheet.Cell(excelRowCounter, 1).Value = call.Id;
						worksheet.Cell(excelRowCounter, 2).Value = call.Time;
						worksheet.Cell(excelRowCounter, 3).Value = call.Phone;
						worksheet.Cell(excelRowCounter, 4).Value = call.CallStatus;
						worksheet.Cell(excelRowCounter, 5).Value = call.CallResult;
						excelRowCounter++;
						continue;
					}
					foreach(var callDetail in call.Children)
					{
						worksheet.Cell(excelRowCounter, 1).Value = call.Id;
						worksheet.Cell(excelRowCounter, 2).Value = call.Time;
						worksheet.Cell(excelRowCounter, 3).Value = call.Phone;
						worksheet.Cell(excelRowCounter, 4).Value = call.CallStatus;
						worksheet.Cell(excelRowCounter, 5).Value = call.CallResult;
						worksheet.Cell(excelRowCounter, 6).Value = callDetail.Time;
						worksheet.Cell(excelRowCounter, 7).Value = callDetail.Details;
						excelRowCounter++;
					}
				}
				workbook.SaveAs(result.Path);
			}
		}

		#endregion Export

		public override void Dispose()
		{
			_autoRefreshTimer?.Dispose();
			base.Dispose();
		}

	}

	
}
