using ClosedXML.Excel;
using Gamma.Binding.Core.RecursiveTreeConfig;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.Dialect.Function;
using NHibernate.SqlCommand;
using NHibernate.Transform;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Journal;
using QS.Project.Journal.DataLoader;
using QS.Project.Services.FileDialog;
using QS.Services;
using System;
using System.Linq;
using System.Timers;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Orders;
using Vodovoz.Domain.TrueMark;
using Vodovoz.EntityRepositories.TrueMark;
using Vodovoz.Models.TrueMark;
using Vodovoz.ViewModels.Journals.FilterViewModels.TrueMark;
using Vodovoz.ViewModels.Journals.JournalNodes.Roboats;
using VodovozOrder = Vodovoz.Domain.Orders.Order;

namespace Vodovoz.ViewModels.Journals.JournalViewModels.Roboats
{
	public class ReceiptsJournalViewModel : JournalViewModelBase
	{
		private readonly TrueMarkReceiptOrderJournalFilterViewModel _filter;
		private readonly TrueMarkCodesPool _trueMarkCodesPool;
		private readonly ITrueMarkRepository _trueMarkRepository;
		private readonly IFileDialogService _fileDialogService;
		private Timer _autoRefreshTimer;
		private int _autoRefreshInterval;

		public ReceiptsJournalViewModel(
			TrueMarkReceiptOrderJournalFilterViewModel filter,
			IUnitOfWorkFactory unitOfWorkFactory,
			ICommonServices commonServices,
			TrueMarkCodesPool trueMarkCodesPool,
			ITrueMarkRepository trueMarkRepository,
			IFileDialogService fileDialogService,
			INavigationManager navigation = null)
			: base(unitOfWorkFactory, commonServices.InteractiveService, navigation)
		{
			_filter = filter ?? throw new ArgumentNullException(nameof(filter));
			_trueMarkCodesPool = trueMarkCodesPool ?? throw new ArgumentNullException(nameof(trueMarkCodesPool));
			_trueMarkRepository = trueMarkRepository ?? throw new ArgumentNullException(nameof(trueMarkRepository));
			_fileDialogService = fileDialogService ?? throw new ArgumentNullException(nameof(fileDialogService));
			_autoRefreshInterval = 30;

			Title = "Журнал чеков";
			Filter = filter;

			var loader = new ThreadDataLoader<CashReceiptNode>(unitOfWorkFactory);
			loader.AddQuery(GetQuery);
			loader.MergeInOrderBy(x => x.OrderId, true);
			DataLoader = loader;

			CreateNodeActions();
			StartAutoRefresh();
		}

		private IJournalFilter filter;
		public IJournalFilter Filter
		{
			get => filter;
			protected set
			{
				if(filter != null)
					filter.OnFiltered -= FilterViewModel_OnFiltered;
				filter = value;
				if(filter != null)
					filter.OnFiltered += FilterViewModel_OnFiltered;
			}
		}

		public IRecursiveConfig RecuresiveConfig { get; }

		void FilterViewModel_OnFiltered(object sender, EventArgs e)
		{
			Refresh();
		}

		public override string FooterInfo
		{
			get
			{
				var poolCount = _trueMarkCodesPool.GetTotalCount();
				var defectivePoolCount = _trueMarkCodesPool.GetDefectiveTotalCount();
				var autorefreshInfo = GetAutoRefreshInfo();
				var codeErrorsOrdersCount = _trueMarkRepository.GetCodeErrorsOrdersCount(UoW);
				return $"Заказов с ошибками кодов: {codeErrorsOrdersCount} | Кодов в пуле: {poolCount}, бракованных: {defectivePoolCount} | {autorefreshInfo} | {base.FooterInfo}";
			}
		}

		public override JournalSelectionMode SelectionMode => JournalSelectionMode.Single;

		protected override void CreateNodeActions()
		{
			NodeActionsList.Clear();
			CreateAutorefreshAction();
			CreateExportAction();
		}

		#region Queries

		private IQueryOver<VodovozOrder> GetQuery(IUnitOfWork uow, bool isItemsCountFunction)
		{
			CashReceiptNode resultAlias = null;
			VodovozOrder orderAlias = null;
			TrueMarkCashReceiptOrder trueMarkOrderAlias = null;
			RouteList routeListAlias = null;
			RouteListItem routeListItemAlias = null;
			Employee driverAlias = null;
			OrderItem orderItemAlias = null;

			var query = uow.Session.QueryOver(() => orderAlias);
			query.JoinEntityQueryOver(() => trueMarkOrderAlias,
				Restrictions.Where(() => trueMarkOrderAlias.Order.Id == orderAlias.Id),
				JoinType.LeftOuterJoin);

			if(!isItemsCountFunction)
			{
				query.JoinEntityQueryOver(() => routeListItemAlias,
					Restrictions.Where(() => routeListItemAlias.Order.Id == orderAlias.Id),
					JoinType.LeftOuterJoin);
				query.Left.JoinAlias(() => routeListItemAlias.RouteList, () => routeListAlias);
				query.Left.JoinAlias(() => routeListAlias.Driver, () => driverAlias);
				query.Left.JoinAlias(() => orderAlias.OrderItems, () => orderItemAlias);
			}

			if(_filter.Status.HasValue)
			{
				var disjunction = Restrictions.Disjunction();
				disjunction.Add(Restrictions.Eq(Projections.Property(() => trueMarkOrderAlias.Status), _filter.Status.Value));

				if(_filter.Status == TrueMarkCashReceiptOrderStatus.ReceiptNotNeeded)
				{
					disjunction.Add(Restrictions.IsNull(Projections.Property(() => trueMarkOrderAlias.Id)));
				}

				query.Where(disjunction);
			}

			if(_filter.StartDate.HasValue)
			{
				query.Where(
					Restrictions.Disjunction()
						.Add(Restrictions.Conjunction()
							.Add(Restrictions.Ge(Projections.Property(() => trueMarkOrderAlias.Date), _filter.StartDate.Value))
							.Add(Restrictions.IsNotNull(Projections.Property(() => trueMarkOrderAlias.Id))))
						.Add(Restrictions.Conjunction()
							.Add(Restrictions.Ge(Projections.Property(() => orderAlias.DeliveryDate), _filter.StartDate.Value))
							.Add(Restrictions.IsNull(Projections.Property(() => trueMarkOrderAlias.Id))))
				);
			}

			if(_filter.EndDate.HasValue)
			{
				query.Where(
					Restrictions.Disjunction()
						.Add(Restrictions.Conjunction()
							.Add(Restrictions.Le(Projections.Property(() => trueMarkOrderAlias.Date), _filter.EndDate.Value))
							.Add(Restrictions.IsNotNull(Projections.Property(() => trueMarkOrderAlias.Id))))
						.Add(Restrictions.Conjunction()
							.Add(Restrictions.Le(Projections.Property(() => orderAlias.DeliveryDate), _filter.EndDate.Value))
							.Add(Restrictions.IsNull(Projections.Property(() => trueMarkOrderAlias.Id))))
				);
			}

			if(_filter.HasUnscannedReason)
			{
				query.Where(Restrictions.Eq(Projections.SqlFunction("IS_NULL_OR_WHITESPACE", NHibernateUtil.Boolean, Projections.Property(() => trueMarkOrderAlias.UnscannedCodesReason)), false));
			}

			query.Where(
				GetSearchCriterion(
					() => trueMarkOrderAlias.Id,
					() => trueMarkOrderAlias.Order.Id,
					() => trueMarkOrderAlias.UnscannedCodesReason
				)
			);

			var orderSumProjection = Projections.Sum(
				Projections.SqlFunction(
					new SQLFunctionTemplate(NHibernateUtil.Decimal, "ROUND(IFNULL(?1, ?2) * ?3 - ?4, 2)"),
					NHibernateUtil.Decimal,
					Projections.Property(() => orderItemAlias.ActualCount),
					Projections.Property(() => orderItemAlias.Count),
					Projections.Property(() => orderItemAlias.Price),
					Projections.Property(() => orderItemAlias.DiscountMoney)
				)
			);

			if(isItemsCountFunction)
			{
				return query;
			}

			query.SelectList(list => list
					.SelectGroup(() => orderAlias.Id).WithAlias(() => resultAlias.OrderId)
					.Select(Projections.Property(() => orderAlias.DeliveryDate)).WithAlias(() => resultAlias.DeliveryDate)
					.Select(Projections.Property(() => trueMarkOrderAlias.Id)).WithAlias(() => resultAlias.ReceiptId)
					.Select(Projections.Property(() => trueMarkOrderAlias.Date)).WithAlias(() => resultAlias.ReceiptTime)
					.Select(orderSumProjection).WithAlias(() => resultAlias.OrderSum)
					.Select(Projections.Property(() => orderAlias.PaymentType)).WithAlias(() => resultAlias.OrderPaymentType)
					.Select(Projections.Property(() => orderAlias.SelfDelivery)).WithAlias(() => resultAlias.IsSelfdelivery)
					.Select(Projections.Property(() => routeListAlias.Id)).WithAlias(() => resultAlias.RouteListId)
					.Select(Projections.Property(() => driverAlias.Name)).WithAlias(() => resultAlias.DriverName)
					.Select(Projections.Property(() => driverAlias.LastName)).WithAlias(() => resultAlias.DriverLastName)
					.Select(Projections.Property(() => driverAlias.Patronymic)).WithAlias(() => resultAlias.DriverPatronimyc)
					.Select(Projections.Property(() => trueMarkOrderAlias.Status)).WithAlias(() => resultAlias.ReceiptStatus)
					.Select(Projections.Property(() => trueMarkOrderAlias.UnscannedCodesReason)).WithAlias(() => resultAlias.UnscannedReason)
					.Select(Projections.Property(() => trueMarkOrderAlias.ErrorDescription)).WithAlias(() => resultAlias.ErrorDescription)
				);
			query.OrderByAlias(() => orderAlias.Id).Desc();
			query.TransformUsing(Transformers.AliasToBean<CashReceiptNode>());

			return query;
		}

		#endregion Queries

		private int GetCount(IUnitOfWork uow)
		{
			var query = GetQuery(uow, true);
			var count = query.List<CashReceiptNode>().Count();
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
				var colNumber = 1;
				var worksheet = workbook.Worksheets.Add("Реестр заказов для чека");
				worksheet.Cell(1, colNumber++).Value = "Код заказа";
				worksheet.Cell(1, colNumber++).Value = "Дата доставки";
				worksheet.Cell(1, colNumber++).Value = "Сумма";
				worksheet.Cell(1, colNumber++).Value = "Тип оплаты";
				worksheet.Cell(1, colNumber++).Value = "Самовывоз";
				worksheet.Cell(1, colNumber++).Value = "Код чека";
				worksheet.Cell(1, colNumber++).Value = "Время чека";
				worksheet.Cell(1, colNumber++).Value = "Статус";
				worksheet.Cell(1, colNumber++).Value = "МЛ";
				worksheet.Cell(1, colNumber++).Value = "Водитель";
				worksheet.Cell(1, colNumber++).Value = "Причина не отсканированных бутылей";
				worksheet.Cell(1, colNumber++).Value = "Описание ошибки";

				var excelRowCounter = 2;
				foreach(var call in Items.Cast<CashReceiptNode>())
				{
					colNumber = 1;
					worksheet.Cell(excelRowCounter, colNumber++).Value = call.OrderId;
					worksheet.Cell(excelRowCounter, colNumber++).Value = call.DeliveryDate;
					worksheet.Cell(excelRowCounter, colNumber++).Value = call.OrderSum;
					worksheet.Cell(excelRowCounter, colNumber++).Value = call.PaymentType;
					worksheet.Cell(excelRowCounter, colNumber++).Value = call.IsSelfdelivery ? "Да" : "Нет";
					worksheet.Cell(excelRowCounter, colNumber++).Value = call.ReceiptId;
					worksheet.Cell(excelRowCounter, colNumber++).Value = call.ReceiptTime;
					worksheet.Cell(excelRowCounter, colNumber++).Value = call.Status;
					worksheet.Cell(excelRowCounter, colNumber++).Value = call.RouteListId;
					worksheet.Cell(excelRowCounter, colNumber++).Value = call.DriverFIO;
					worksheet.Cell(excelRowCounter, colNumber++).Value = call.UnscannedReason;
					worksheet.Cell(excelRowCounter, colNumber++).Value = call.ErrorDescription;
					excelRowCounter++;
				}
				workbook.SaveAs(result.Path);
			}
		}

		#endregion Export
	}
}
