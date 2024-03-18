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
using QS.Tdi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Timers;
using Vodovoz.Domain.Contacts;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Operations;
using Vodovoz.TempAdapters;
using Vodovoz.ViewModels.Journals.FilterViewModels;
using Vodovoz.ViewModels.Journals.JournalNodes.Orders;
using VodovozOrder = Vodovoz.Domain.Orders.Order;

namespace Vodovoz.ViewModels.Journals.JournalViewModels.Orders
{
	public class DriverTareMessagesJournalViewModel : FilterableMultipleEntityJournalViewModelBase<DriverMessageJournalNode, DriverMessageFilterViewModel>
	{
		private readonly IGtkTabsOpener _gtkTabsOpener;
		private Timer _autoRefreshTimer;
		private const string _xlsxFileFilter = "XLSX File (*.xlsx)";
		private readonly IFileDialogService _fileDialogService;

		public DriverTareMessagesJournalViewModel(
			DriverMessageFilterViewModel filterViewModel,
			IGtkTabsOpener gtkTabsOpener,
			IUnitOfWorkFactory unitOfWorkFactory,
			ICommonServices commonServices,
			IFileDialogService fileDialogService,
			INavigationManager navigation = null)
			: base(filterViewModel, unitOfWorkFactory, commonServices)
		{
			_gtkTabsOpener = gtkTabsOpener ?? throw new ArgumentNullException(nameof(gtkTabsOpener));
			_fileDialogService = fileDialogService ?? throw new ArgumentNullException(nameof(fileDialogService));

			Title = "Сообщения водителей по таре";

			var threadDataLoader = new ThreadDataLoader<DriverMessageJournalNode>(unitOfWorkFactory)
			{
				DynamicLoadingEnabled = true
			};

			threadDataLoader.AddQuery(GetData, GetDataCount);
			DataLoader = threadDataLoader;

			CreateNodeActions();

			InitAutoRefreshTimer();
		}

		private void InitAutoRefreshTimer()
		{
			var interval = TimeSpan.FromMinutes(10).TotalMilliseconds;
			_autoRefreshTimer = new Timer(interval);
			_autoRefreshTimer.Elapsed += (s, e) => Refresh();
			_autoRefreshTimer.Start();
		}

		public override JournalSelectionMode SelectionMode => JournalSelectionMode.Single;

		protected override void CreateNodeActions()
		{
			NodeActionsList.Clear();
			CreateEditAction();
			CreateSaveAction();
		}

		private void CreateSaveAction()
		{
			var saveAction = new JournalAction("Сохранить",
				(selected) => true,
				(selected) => true,
				(selected) => ExportReport());
			NodeActionsList.Add(saveAction);
		}

		private void CreateEditAction()
		{
			var editAction = new JournalAction("Открыть заказ",
				(selected) =>
				{
					var selectedNodes = selected.OfType<DriverMessageJournalNode>();
					return selectedNodes.Count() == 1;
				},
				(selected) => true,
				(selected) =>
				{
					var selectedNodes = selected.OfType<DriverMessageJournalNode>();
					if(selectedNodes.Count() != 1)
					{
						return;
					}
					var selectedNode = selectedNodes.First();
					_gtkTabsOpener.OpenOrderDlg(this, selectedNode.OrderId);
				}
			);
			RowActivatedAction = editAction;
			NodeActionsList.Add(editAction);
		}

		private IQueryOver<RouteListItem> GetData(IUnitOfWork uow)
		{
			BottlesMovementOperation debtBottlesOperationAlias = null;
			VodovozOrder orderAlias = null;
			RouteListItem routeListItemAlias = null;
			RouteList routeListAlias = null;
			Employee driverAlias = null;
			Employee lastOpManager = null;
			Phone phoneAlias = null;
			DriverMessageJournalNode resultAlias = null;

			var bottlesDebtSubquery = QueryOver.Of(() => debtBottlesOperationAlias)
				.Where(() => debtBottlesOperationAlias.DeliveryPoint.Id == orderAlias.DeliveryPoint.Id)
				.Select(
					Projections.SqlFunction(
						new SQLFunctionTemplate(NHibernateUtil.Int32, "?1 - ?2"),
						NHibernateUtil.Int32,
						Projections.Sum(Projections.Property(() => debtBottlesOperationAlias.Delivered)),
						Projections.Sum(Projections.Property(() => debtBottlesOperationAlias.Returned))
					)
				);

			var query = uow.Session.QueryOver(() => routeListItemAlias)
				.Left.JoinAlias(() => routeListItemAlias.Order, () => orderAlias)
				.Left.JoinAlias(() => routeListItemAlias.RouteList, () => routeListAlias)
				.Left.JoinAlias(() => routeListAlias.Driver, () => driverAlias)
				.Left.JoinAlias(() => driverAlias.Phones, () => phoneAlias, () => !phoneAlias.IsArchive)
				.Left.JoinAlias(() => orderAlias.CommentOPManagerChangedBy, () => lastOpManager)
				.Where(Restrictions.IsNotNull(Projections.Property(() => orderAlias.DriverMobileAppCommentTime)))
				.Where(() => routeListItemAlias.Status != RouteListItemStatus.Transfered);

			query.Where(
				GetSearchCriterion(
					() => orderAlias.Id,
					() => routeListAlias.Id,
					() => driverAlias.LastName,
					() => driverAlias.Name,
					() => driverAlias.Patronymic,
					() => orderAlias.DriverMobileAppComment,
					() => phoneAlias.DigitsNumber
				)
			);

			if(FilterViewModel != null)
			{
				if(FilterViewModel.StartDate.HasValue)
				{
					query.Where(() => orderAlias.DriverMobileAppCommentTime >= FilterViewModel.StartDate.Value.Date);
				}

				if(FilterViewModel.EndDate.HasValue)
				{
					query.Where(() => orderAlias.DriverMobileAppCommentTime < FilterViewModel.EndDate.Value.Date.AddDays(1));
				}
			}

			query.SelectList(list => list
				.SelectGroup(() => orderAlias.Id).WithAlias(() => resultAlias.OrderId)
				.Select(Projections.Property(() => orderAlias.DriverMobileAppCommentTime)).WithAlias(() => resultAlias.CommentDate)
				.Select(Projections.SqlFunction(
						new SQLFunctionTemplate(NHibernateUtil.String, "GET_PERSON_NAME_WITH_INITIALS(?1, ?2, ?3)"),
						NHibernateUtil.String,
						Projections.Property(() => driverAlias.LastName),
						Projections.Property(() => driverAlias.Name),
						Projections.Property(() => driverAlias.Patronymic)
					))
					.WithAlias(() => resultAlias.DriverName)
				.Select(Projections.Property(() => phoneAlias.Number)).WithAlias(() => resultAlias.DriverPhone)
				.Select(Projections.Property(() => routeListAlias.Id)).WithAlias(() => resultAlias.RouteListId)
				.Select(Projections.Property(() => orderAlias.BottlesReturn)).WithAlias(() => resultAlias.BottlesReturn)
				.Select(Projections.Property(() => routeListItemAlias.DriverBottlesReturned)).WithAlias(() => resultAlias.ActualBottlesReturn)
				.Select(Projections.SubQuery(bottlesDebtSubquery)).WithAlias(() => resultAlias.AddressBottlesDebt)
				.Select(Projections.Property(() => orderAlias.DriverMobileAppComment)).WithAlias(() => resultAlias.DriverComment)
				.Select(Projections.Property(() => orderAlias.OPComment)).WithAlias(() => resultAlias.OPComment)
				.Select(Projections.SqlFunction(
						new SQLFunctionTemplate(NHibernateUtil.String, "GET_PERSON_NAME_WITH_INITIALS(?1, ?2, ?3)"),
						NHibernateUtil.String,
						Projections.Property(() => lastOpManager.LastName),
						Projections.Property(() => lastOpManager.Name),
						Projections.Property(() => lastOpManager.Patronymic)
					))
					.WithAlias(() => resultAlias.CommentOPManagerChangedBy)
				.Select(Projections.Property(() => orderAlias.CommentOPManagerUpdatedAt).WithAlias(() => resultAlias.CommentOPManagerUpdatedAt))
			)
			.OrderBy(() => orderAlias.DriverMobileAppCommentTime).Desc()
			.TransformUsing(Transformers.AliasToBean<DriverMessageJournalNode<RouteListItem>>());

			return query;
		}
		
		private int GetDataCount(IUnitOfWork uow)
		{
			VodovozOrder orderAlias = null;
			RouteListItem routeListItemAlias = null;
			RouteList routeListAlias = null;
			Employee driverAlias = null;
			Employee lastOpManager = null;
			Phone phoneAlias = null;
			DriverMessageJournalNode resultAlias = null;

			var query = uow.Session.QueryOver(() => routeListItemAlias)
				.Left.JoinAlias(() => routeListItemAlias.Order, () => orderAlias)
				.Left.JoinAlias(() => routeListItemAlias.RouteList, () => routeListAlias)
				.Left.JoinAlias(() => routeListAlias.Driver, () => driverAlias)
				.Left.JoinAlias(() => driverAlias.Phones, () => phoneAlias, () => !phoneAlias.IsArchive)
				.Left.JoinAlias(() => orderAlias.CommentOPManagerChangedBy, () => lastOpManager)
				.Where(Restrictions.IsNotNull(Projections.Property(() => orderAlias.DriverMobileAppCommentTime)))
				.Where(() => routeListItemAlias.Status != RouteListItemStatus.Transfered);

			query.Where(
				GetSearchCriterion(
					() => orderAlias.Id,
					() => routeListAlias.Id,
					() => driverAlias.LastName,
					() => driverAlias.Name,
					() => driverAlias.Patronymic,
					() => orderAlias.DriverMobileAppComment,
					() => phoneAlias.DigitsNumber
				)
			);

			if(FilterViewModel != null)
			{
				if(FilterViewModel.StartDate.HasValue)
				{
					query.Where(() => orderAlias.DriverMobileAppCommentTime >= FilterViewModel.StartDate.Value.Date);
				}

				if(FilterViewModel.EndDate.HasValue)
				{
					query.Where(() => orderAlias.DriverMobileAppCommentTime < FilterViewModel.EndDate.Value.Date.AddDays(1));
				}
			}

			var result = query.SelectList(list => list
					.SelectGroup(() => orderAlias.Id).WithAlias(() => resultAlias.OrderId))
				.List<int>();

			return result.Count;
		}

		public void ExportReport()
		{
			using(var wb = new XLWorkbook())
			{
				var sheetName = $"{DateTime.Now:dd.MM.yyyy}";
				var ws = wb.Worksheets.Add(sheetName);

				InsertValues(ws);
				ws.Columns().AdjustToContents();

				if(TryGetSavePath(out string path))
				{
					wb.SaveAs(path);
				}
			}
		}

		private void InsertValues(IXLWorksheet ws)
		{
			var colNames = new string[] { "Дата", "Время", "ФИО водителя", "Телефон водителя", "№ МЛ", "№ заказа", "План бут.", "Факт бут.", "Долг бут. по адресу", "Комментарий водителя", "Комментарий ОП/ОСК", "Автор комментария", "Время комментария", "Время реакции" };
			List<DriverMessageJournalNode> a = new List<DriverMessageJournalNode>();

			for(int i = 0; i < Items.Count; i++)
			{
				a.Add(Items[i] as DriverMessageJournalNode);
			}

			var rows = from row in a
					   select new
					   {
						   Date = row.CommentDate.ToString("dd.MM.yy"),
						   CommentDate = row.CommentDate.ToString("HH:mm:ss"),
						   row.DriverName,
						   row.DriverPhone,
						   row.RouteListId,
						   row.OrderId,
						   row.BottlesReturn,
						   row.ActualBottlesReturn,
						   row.AddressBottlesDebt,
						   row.DriverComment,
						   CommentManager = row.OPComment,
						   lastEditor = row.CommentOPManagerChangedBy,
						   lastEditTIme = row.CommentOPManagerUpdatedAt != DateTime.MinValue ? row.CommentOPManagerUpdatedAt.ToString("HH:mm dd.MM.yy") : string.Empty,
						   row.ResponseTime
					   };
			int index = 1;
			foreach(var name in colNames)
			{
				ws.Cell(1, index).Value = name;
				index++;
			}
			ws.Cell(2, 1).InsertData(rows);
		}

		private bool TryGetSavePath(out string path)
		{
			var extension = ".xlsx";
			var dialogSettings = new DialogSettings
			{
				Title = "Сохранить",
				FileName = $"{TabName} {DateTime.Now:yyyy-MM-dd-HH-mm}{extension}"
			};

			dialogSettings.FileFilters.Add(new DialogFileFilter(_xlsxFileFilter, $"*{extension}"));
			var result = _fileDialogService.RunSaveFileDialog(dialogSettings);
			path = result.Path;

			return result.Successful;
		}

		public override void Dispose()
		{
			_autoRefreshTimer?.Dispose();
			base.Dispose();
		}
	}
}
