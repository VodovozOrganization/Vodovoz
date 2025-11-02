using DateTimeHelpers;
using Microsoft.Extensions.Logging;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.Dialect.Function;
using NHibernate.Transform;
using QS.Dialog;
using QS.Dialog.Gtk;
using QS.Dialog.GtkUI;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Domain;
using QS.Project.Journal;
using QS.Services;
using QSProjectsLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Core.Domain.Documents;
using Vodovoz.Core.Domain.Goods;
using Vodovoz.Core.Domain.Repositories;
using Vodovoz.Domain.Cash;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Orders;
using Vodovoz.EntityRepositories.Cash;
using Vodovoz.EntityRepositories.Orders;
using Vodovoz.Extensions;
using Vodovoz.Filters.ViewModels;
using Vodovoz.Infrastructure;
using Vodovoz.JournalNodes;
using Vodovoz.ViewModels.Cash;
using Vodovoz.ViewModels.Logistic;
using VodovozOrder = Vodovoz.Domain.Orders.Order;

namespace Vodovoz.Representations
{
	public class SelfDeliveriesJournalViewModel : FilterableSingleEntityJournalViewModelBase<VodovozOrder, OrderDlg, SelfDeliveryJournalNode, OrderJournalFilterViewModel>
	{
		private readonly ILogger<SelfDeliveriesJournalViewModel> _logger;
		private readonly ICommonServices _commonServices;
		private readonly ICashRepository _cashRepository;
		private readonly IGenericRepository<Income> _incomeRepository;
		private readonly IOrderRepository _orderRepository;
		private readonly IGuiDispatcher _guiDispatcher;
		private readonly bool _userCanChangePayTypeToByCard;

		private readonly string _dataIsLoadingString = "Идёт загрузка данных... ";
		private string _footerInfo;
		private CancellationTokenSource _cancellationTokenSource;

		public SelfDeliveriesJournalViewModel(
			OrderJournalFilterViewModel filterViewModel,
			ILogger<SelfDeliveriesJournalViewModel> logger,
			IUnitOfWorkFactory unitOfWorkFactory,
			ICommonServices commonServices,
			ICashRepository cashRepository,
			IOrderRepository orderRepository,
			IGenericRepository<Income> incomeRepository,
			INavigationManager navigationManager,
			IGuiDispatcher guiDispatcher,
			Action<OrderJournalFilterViewModel> filterConfig = null)
			: base(filterViewModel, unitOfWorkFactory, commonServices, navigation: navigationManager)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_commonServices = commonServices ?? throw new ArgumentNullException(nameof(commonServices));
			_cashRepository = cashRepository ?? throw new ArgumentNullException(nameof(cashRepository));
			_orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
			_incomeRepository = incomeRepository ?? throw new ArgumentNullException(nameof(incomeRepository));
			NavigationManager = navigationManager ?? throw new ArgumentNullException(nameof(navigationManager));
			_guiDispatcher = guiDispatcher ?? throw new ArgumentNullException(nameof(guiDispatcher));

			TabName = "Журнал самовывозов";

			filterViewModel.Journal = this;

			if(filterConfig != null)
			{
				filterViewModel.ConfigureWithoutFiltering(filterConfig);
			}

			SetOrder(x => x.Date, true);

			UpdateOnChanges(
				typeof(VodovozOrder),
				typeof(OrderItem));

			DataLoader.ItemsListUpdated += OnDataLoaderItemsListUpdated;

			_userCanChangePayTypeToByCard = commonServices.CurrentPermissionService.ValidatePresetPermission(Vodovoz.Core.Domain.Permissions.StorePermissions.Documents.CanLoadSelfDeliveryDocument);
		}

		protected override Func<IUnitOfWork, IQueryOver<VodovozOrder>> ItemsSourceQueryFunction => (uow) =>
		{
			SelfDeliveryJournalNode resultAlias = null;
			VodovozOrder orderAlias = null;
			Nomenclature nomenclatureAlias = null;
			OrderItem orderItemAlias = null;
			OrderDepositItem orderDepositItemAlias = null;
			Income incomeAlias = null;
			Expense expenseAlias = null;
			Counterparty counterpartyAlias = null;
			DeliveryPoint deliveryPointAlias = null;
			Employee authorAlias = null;
			CounterpartyContract contractAlias = null;

			var depositReturnQuery = QueryOver.Of(() => orderDepositItemAlias)
				.Select(Projections.Sum(
						Projections.SqlFunction(
							new SQLFunctionTemplate(NHibernateUtil.Decimal, "IFNULL(?2, ?1) * ?3"),
							NHibernateUtil.Decimal,
							Projections.Property(() => orderDepositItemAlias.Count),
							Projections.Property(() => orderDepositItemAlias.ActualCount),
							Projections.Property(() => orderDepositItemAlias.Deposit)
						   )
					))
				.Where(() => orderDepositItemAlias.Order.Id == orderAlias.Id);

			var query = uow.Session.QueryOver<VodovozOrder>(() => orderAlias)
								   .Where(() => orderAlias.SelfDelivery)
								   .Where(() => orderAlias.OrderAddressType != OrderAddressType.Service);

			var bottleCountSubquery = QueryOver.Of(() => orderItemAlias)
				.Where(() => orderAlias.Id == orderItemAlias.Order.Id)
				.JoinAlias(() => orderItemAlias.Nomenclature, () => nomenclatureAlias)
				.Where(() => nomenclatureAlias.Category == NomenclatureCategory.water && nomenclatureAlias.TareVolume == TareVolume.Vol19L)
				.Select(Projections.Sum(() => orderItemAlias.Count));

			var orderSumSubquery = QueryOver.Of(() => orderItemAlias)
				.Where(() => orderItemAlias.Order.Id == orderAlias.Id)
				.Select(
					Projections.Sum(
						Projections.SqlFunction(
							new SQLFunctionTemplate(NHibernateUtil.Decimal, "ROUND(IFNULL(?1, ?2) * ?3 - ?4, 2)"),
							NHibernateUtil.Decimal,
							Projections.Property<OrderItem>(x => x.ActualCount),
							Projections.Property<OrderItem>(x => x.Count),
							Projections.Property<OrderItem>(x => x.Price),
							Projections.Property<OrderItem>(x => x.DiscountMoney)
						)
					)
				);

			var incomeSumSubquery = QueryOver.Of(() => incomeAlias)
				.Where(() => incomeAlias.Order.Id == orderAlias.Id)
				.Select(Projections.Sum(() => incomeAlias.Money));

			var expenseSumSubquery = QueryOver.Of(() => expenseAlias)
				.Where(() => expenseAlias.Order.Id == orderAlias.Id)
				.Select(Projections.Sum(() => expenseAlias.Money));

			if(FilterViewModel.RestrictStatus != null)
			{
				query.Where(o => o.OrderStatus == FilterViewModel.RestrictStatus);
			}
			else if(FilterViewModel.AllowStatuses != null && FilterViewModel.AllowStatuses.Any())
			{
				query.WhereRestrictionOn(o => o.OrderStatus).IsIn(FilterViewModel.AllowStatuses);
			}

			if(FilterViewModel.RestrictPaymentType != null)
			{
				query.Where(o => o.PaymentType == FilterViewModel.RestrictPaymentType);
			}
			else if(FilterViewModel.AllowPaymentTypes != null && FilterViewModel.AllowPaymentTypes.Any())
			{
				query.WhereRestrictionOn(o => o.PaymentType).IsIn(FilterViewModel.AllowPaymentTypes);
			}

			if(FilterViewModel.RestrictCounterparty != null)
			{
				query.Where(o => o.Client == FilterViewModel.RestrictCounterparty);
			}

			if(FilterViewModel.DeliveryPoint != null)
			{
				query.Where(o => o.DeliveryPoint == FilterViewModel.DeliveryPoint);
			}

			if(FilterViewModel.StartDate != null)
			{
				query.Where(o => o.DeliveryDate >= FilterViewModel.StartDate);
			}

			var endDate = FilterViewModel.EndDate;
			if(endDate != null)
			{
				query.Where(o => o.DeliveryDate <= endDate.Value.LatestDayTime());
			}

			if(FilterViewModel.PaymentOrder != null)
			{
				bool paymentAfterShipment = false || FilterViewModel.PaymentOrder == PaymentOrder.AfterShipment;
				query.Where(o => o.PayAfterShipment == paymentAfterShipment);
			}

			if(FilterViewModel.Organisation != null)
			{
				query.Where(() => contractAlias.Organization.Id == FilterViewModel.Organisation.Id);
			}

			if(FilterViewModel.PaymentByCardFrom != null)
			{
				query.Where(o => o.PaymentByCardFrom.Id == FilterViewModel.PaymentByCardFrom.Id);
			}

			query
				.Left.JoinAlias(o => o.DeliveryPoint, () => deliveryPointAlias)
				.Left.JoinAlias(o => o.Client, () => counterpartyAlias)
				.Left.JoinAlias(o => o.Author, () => authorAlias)
				.Left.JoinAlias(o => o.Contract, () => contractAlias);

			query.Where(GetSearchCriterion(
				() => counterpartyAlias.Name,
				() => authorAlias.Name,
				() => authorAlias.LastName,
				() => authorAlias.Patronymic,
				() => orderAlias.Id
			));

			var result = query.SelectList(list => list
					.Select(() => orderAlias.Id).WithAlias(() => resultAlias.Id)
					.Select(() => orderAlias.DeliveryDate).WithAlias(() => resultAlias.Date)
					.Select(() => orderAlias.OrderStatus).WithAlias(() => resultAlias.StatusEnum)
					.Select(() => authorAlias.LastName).WithAlias(() => resultAlias.AuthorLastName)
					.Select(() => authorAlias.Name).WithAlias(() => resultAlias.AuthorName)
					.Select(() => authorAlias.Patronymic).WithAlias(() => resultAlias.AuthorPatronymic)
					.Select(() => counterpartyAlias.Name).WithAlias(() => resultAlias.Counterparty)
					.Select(() => orderAlias.PayAfterShipment).WithAlias(() => resultAlias.PayAfterLoad)
					.Select(() => orderAlias.PaymentType).WithAlias(() => resultAlias.PaymentTypeEnum)
					.SelectSubQuery(bottleCountSubquery).WithAlias(() => resultAlias.BottleAmount)
					.SelectSubQuery(orderSumSubquery).WithAlias(() => resultAlias.OrderSum)
					.SelectSubQuery(incomeSumSubquery).WithAlias(() => resultAlias.CashPaid)
					.SelectSubQuery(expenseSumSubquery).WithAlias(() => resultAlias.CashReturn)
					.SelectSubQuery(depositReturnQuery).WithAlias(() => resultAlias.OrderReturnSum)
				)
				.OrderBy(x => x.DeliveryDate).Desc.ThenBy(x => x.Id).Desc
				.TransformUsing(Transformers.AliasToBean<SelfDeliveryJournalNode>());

			return result;
		};

		public override IEnumerable<IJournalAction> NodeActions => new List<IJournalAction>();

		//Действие при дабл клике
		protected override Func<OrderDlg> CreateDialogFunction => () => throw new ApplicationException();

		//FIXME отделить от GTK
		protected override Func<SelfDeliveryJournalNode, OrderDlg> OpenDialogFunction => node => new OrderDlg(node.Id);

		public override string FooterInfo
		{
			get => _footerInfo;
			set => SetField(ref _footerInfo, value);
		}

		private void OnDataLoaderItemsListUpdated(object sender, EventArgs e)
		{
			UpdateFooterInfo();
		}

		private async Task UpdateFooterInfo()
		{
			FooterInfo = _dataIsLoadingString;

			try
			{
				if(_cancellationTokenSource != null)
				{
					_cancellationTokenSource.Cancel();
					_cancellationTokenSource.Dispose();
					_cancellationTokenSource = null;
				}

				_cancellationTokenSource = new CancellationTokenSource();

				FooterInfo = await GetFooterInfo(_cancellationTokenSource.Token);
			}
			catch(OperationCanceledException)
			{
				return;
			}
			catch(Exception ex)
			{
				var errorMessage = "Ошибка при обновлении суммарной информации в журнале самовывозов";

				_logger.LogError(ex, errorMessage);

				_guiDispatcher.RunInGuiTread(() =>
				{
					_commonServices.InteractiveService.ShowMessage(ImportanceLevel.Error, errorMessage);
				});

				return;
			}
		}

		protected async Task<string> GetFooterInfo(CancellationToken token)
		{
			StringBuilder sb = new StringBuilder();

			using(var uow = UnitOfWorkFactory.CreateWithoutRoot())
			{
				var nodes = await ItemsSourceQueryFunction(uow).ListAsync<SelfDeliveryJournalNode>(token);

				sb.Append("Сумма БН: <b>")
					.Append(nodes.Sum(n => n.OrderCashlessSumTotal).ToShortCurrencyString())
					.Append("</b>\t|\t");

				sb.Append("Сумма нал: <b>")
					.Append(nodes.Sum(n => n.OrderCashSumTotal).ToShortCurrencyString())
					.Append("</b>\t|\t");

				sb.Append("Из них возврат: <b>")
					.Append(nodes.Sum(n => n.OrderReturnSum).ToShortCurrencyString())
					.Append("</b>\t|\t");

				sb.Append("Приход: <b>")
					.Append(nodes.Sum(n => n.CashPaid).ToShortCurrencyString())
					.Append("</b>\t|\t");

				sb.Append("Возврат: <b>")
					.Append(nodes.Sum(n => n.CashReturn).ToShortCurrencyString())
					.Append("</b>\t|\t");

				sb.Append("Итог: <b>")
					.Append(nodes.Sum(n => n.CashTotal).ToShortCurrencyString())
					.Append("</b>\t|\t");

				var difference = nodes.Sum(n => n.TotalCashDiff);
				if(difference == 0)
				{
					sb.Append("Расх.нал: <b>")
						.Append(difference.ToShortCurrencyString())
						.Append("</b>\t\t");
				}
				else
				{
					sb.Append($"Расх.нал: <span foreground=\"{GdkColors.DangerText.ToHtmlColor()}\"><b>")
						.Append(difference.ToShortCurrencyString())
						.Append("</b></span>\t\t");
				}

				sb.Append($"<span foreground=\"{GdkColors.InsensitiveText.ToHtmlColor()}\"><b>")
					.Append(base.FooterInfo)
					.Append("</b></span>");
			}
			;

			return sb.ToString();
		}

		protected override void CreatePopupActions()
		{
			PopupActionsList.Add(
				new JournalAction(
					"Открыть заказ",
					selectedItems =>
					{
						var selectedNodes = selectedItems.Cast<SelfDeliveryJournalNode>();
						return selectedNodes.Count() == 1;
					},
					selectedItems => true,
					selectedItems =>
					{
						var selectedNodes = selectedItems.Cast<SelfDeliveryJournalNode>();
						var selectedNode = selectedNodes.FirstOrDefault();
						Startup.MainWin.TdiMain.OpenTab(
							DialogHelper.GenerateDialogHashName<VodovozOrder>(selectedNode.Id),
							() => new OrderDlg(selectedNode.Id)
						);
					}
				)
			);

			PopupActionsList.Add(
				new JournalAction(
					"Создать кассовые ордера",
					selectedItems => true,
					selectedItems =>
					{
						var selectedNodes = selectedItems.Cast<SelfDeliveryJournalNode>();
						return selectedNodes.Count() == 1 && selectedNodes.First().StatusEnum == OrderStatus.WaitForPayment;
					},
					selectedItems =>
					{
						var selectedNodes = selectedItems.Cast<SelfDeliveryJournalNode>();
						var selectedNode = selectedNodes.FirstOrDefault();
						if(selectedNode != null)
						{
							CreateSelfDeliveryCashInvoices(selectedNode.Id);
						}
					}
				)
			);

			PopupActionsList.Add(
				new JournalAction(
					"Оплата по карте",
					selectedItems =>
					{
						var selectedNodes = selectedItems.Cast<SelfDeliveryJournalNode>().ToList();
						var selectedNode = selectedNodes.FirstOrDefault();
						return selectedNodes.Count == 1 && (selectedNode.PaymentTypeEnum == PaymentType.Cash || (selectedNode.PaymentTypeEnum == PaymentType.Terminal && selectedNode.OrderCashSumTotal != 0)) && selectedNode.StatusEnum != OrderStatus.Closed;
					},
					selectedItems => _userCanChangePayTypeToByCard,
					selectedItems =>
					{
						var selectedNodes = selectedItems.Cast<SelfDeliveryJournalNode>();
						var selectedNode = selectedNodes.FirstOrDefault();
						if(selectedNode != null)
						{
							NavigationManager.OpenViewModel<PaymentByCardViewModel, IEntityUoWBuilder>(
								this,
								EntityUoWBuilder.ForOpen(selectedNode.Id),
								OpenPageOptions.AsSlave);
						}
					}

				)
			);

			PopupActionsList.Add(
				new JournalAction(
					"Онлайн оплата",
					selectedItems =>
					{
						var selectedNodes = selectedItems.Cast<SelfDeliveryJournalNode>().ToList();
						var selectedNode = selectedNodes.FirstOrDefault();
						return selectedNodes.Count == 1 && (selectedNode.PaymentTypeEnum == PaymentType.Cash || (selectedNode.PaymentTypeEnum == PaymentType.Terminal && selectedNode.OrderCashSumTotal != 0)) && selectedNode.StatusEnum != OrderStatus.Closed;
					},
					selectedItems => _userCanChangePayTypeToByCard,
					selectedItems =>
					{
						var selectedNodes = selectedItems.Cast<SelfDeliveryJournalNode>();
						var selectedNode = selectedNodes.FirstOrDefault();
						if(selectedNode != null)
						{
							NavigationManager.OpenViewModel<PaymentOnlineViewModel, IEntityUoWBuilder>(
								this,
								EntityUoWBuilder.ForOpen(selectedNode.Id),
								OpenPageOptions.AsSlave);
						}
					}

				)
			);

			PopupActionsList.Add(
				new JournalAction(
					"Изменить самовывоз",
					selectedItems =>
					{
						var selectedNodes = selectedItems.Cast<SelfDeliveryJournalNode>();
						return selectedNodes.Count() == 1;
					},
					selectedItems => true,
					selectedItems =>
					{
						var selectedNodes = selectedItems.Cast<SelfDeliveryJournalNode>();
						var selectedNode = selectedNodes.FirstOrDefault();
						if(selectedNode != null)
						{
							using(var uow = UnitOfWorkFactory.CreateWithoutRoot())
							{
								var order = uow.GetById<VodovozOrder>(selectedNode.Id);

								var incomes = _incomeRepository
									.Get(uow, x => x.Order.Id == order.Id)
									.ToList();

								bool isSentEdoUpd = _orderRepository.OrderHasSentUPD(uow, order.Id);
								bool isSentReceipt = _orderRepository.OrderHasSentReceipt(uow, order.Id);

								if(isSentReceipt)
								{
									_commonServices.InteractiveService.ShowMessage(
										ImportanceLevel.Warning, 
										$"Невозможно изменить самовывоз, т.к. имеется чек по заказу №{order.Id}");

									return;
								}

								if(incomes.Any() || isSentEdoUpd)
								{
									var message = "Для изменения самовывоза необходимо сперва ";
									if(incomes.Any())
									{
										var incomeNumbers = string.Join(", ", incomes.Select(i => $"№{i.Id}"));
										message += $"удалить ПКО {incomeNumbers}";
										_commonServices.InteractiveService.ShowMessage(ImportanceLevel.Warning, message);
									}
									else if (isSentEdoUpd)
									{
										message += $"аннулировать УПД по ЭДО по заказу №{order.Id}";
										_commonServices.InteractiveService.ShowMessage(ImportanceLevel.Warning, message);
									}
									return;
								}
								NavigationManager.OpenViewModel<SelfDeliveringOrderEditViewModel, IEntityUoWBuilder>(
									this,
									EntityUoWBuilder.ForOpen(selectedNode.Id),
									OpenPageOptions.AsSlave);
							}

						}
					}
				)
			);
		}

		//FIXME отделить от GTK
		void CreateSelfDeliveryCashInvoices(int orderId)
		{
			var order = UoW.GetById<VodovozOrder>(orderId);

			if(order.SelfDeliveryIsFullyPaid(_cashRepository))
			{
				MessageDialogHelper.RunInfoDialog("Заказ уже оплачен полностью");
				return;
			}

			if(order.OrderPositiveSum > 0 && !order.SelfDeliveryIsFullyIncomePaid())
			{
				var page = NavigationManager.OpenViewModel<IncomeSelfDeliveryViewModel, IEntityUoWBuilder>(this, EntityUoWBuilder.ForCreate());
				page.ViewModel.SetOrderById(orderId);
			}

			if(order.OrderNegativeSum > 0 && !order.SelfDeliveryIsFullyExpenseReturned())
			{
				var page = NavigationManager.OpenViewModel<ExpenseSelfDeliveryViewModel, IEntityUoWBuilder>(this, EntityUoWBuilder.ForCreate());
				page.ViewModel.SetOrderById(orderId);
			}
		}

		public override void Dispose()
		{
			FilterViewModel.OnFiltered -= OnDataLoaderItemsListUpdated;

			_cancellationTokenSource?.Dispose();
			_cancellationTokenSource = null;

			base.Dispose();
		}
	}
}
