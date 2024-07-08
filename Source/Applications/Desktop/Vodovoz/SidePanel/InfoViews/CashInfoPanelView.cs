using Gamma.Binding;
using Gamma.Binding.Core.LevelTreeConfig;
using Gamma.GtkWidgets;
using QS.DomainModel.UoW;
using QS.Project.Services;
using QS.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using Vodovoz.EntityRepositories;
using Vodovoz.EntityRepositories.Cash;
using Vodovoz.EntityRepositories.Subdivisions;
using Vodovoz.SidePanel.InfoProviders;
using Vodovoz.ViewModels.Cash.DocumentsJournal;

namespace Vodovoz.SidePanel.InfoViews
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class CashInfoPanelView : Gtk.Bin, IPanelView
	{
		private readonly IUnitOfWork _uow;
		private readonly ICashRepository _cashRepository;
		private readonly IList<int> _sortedSubdivisionsIds;

		public CashInfoPanelView(IUnitOfWorkFactory uowFactory,
			ICashRepository cashRepository,
			ISubdivisionRepository subdivisionRepository,
			IUserRepository userRepository)
		{
			Build();
			_uow = uowFactory?.CreateWithoutRoot("Боковая панель остатков по кассам") ?? throw new ArgumentNullException(nameof(uowFactory));
			_cashRepository = cashRepository ?? throw new ArgumentNullException(nameof(cashRepository));

			var currentUser = ServicesConfig.CommonServices.UserService.GetCurrentUser();
			var availableSubdivisions = subdivisionRepository.GetCashSubdivisionsAvailableForUser(_uow, currentUser).ToList();
			var settings =
				(userRepository ?? throw new ArgumentNullException(nameof(userRepository)))
				.GetCurrentUserSettings(_uow);
			var needSave = settings.UpdateCashSortingSettings(availableSubdivisions);
			if(needSave)
			{
				_uow.Save(settings);
				_uow.Commit();
			}

			_sortedSubdivisionsIds = settings.CashSubdivisionSortingSettings
				.OrderBy(x => x.SortingIndex)
				.Select(x => x.CashSubdivision.Id)
				.ToList();

			ConfigureWidget();
		}

		#region IPanelView implementation

		public IInfoProvider InfoProvider { get; set; }

		public bool VisibleOnPanel => true;

		public void OnCurrentObjectChanged(object changedObject) => Refresh();

		public void Refresh()
		{
			if(!(InfoProvider is IDocumentsInfoProvider documentsInfoProvider))
			{
				return;
			}

			var filter = documentsInfoProvider.DocumentsFilterViewModel;

			if(filter is null)
			{
				return;
			}

			var selectedSubdivisionsIds = filter.Subdivision is null
				? filter.AvailableSubdivisions?
					.Select(x => x.Id)
					.ToArray()
				: new int[] { filter.Subdivision.Id };

			if(selectedSubdivisionsIds is null)
			{
				return;
			}

			var cashForEmployees = _cashRepository.CurrentCashForGivenSubdivisions(_uow, selectedSubdivisionsIds)
				.OrderBy(x => _sortedSubdivisionsIds.IndexOf(x.SubdivisionId))
				.ToList();

			RefreshMainInfo(cashForEmployees, filter);

			RefreshEmployeesTree(cashForEmployees, filter);

			RefreshDetalizationInfo(cashForEmployees, filter);
		}

		private void RefreshMainInfo(List<EmployeeBalanceNode> cashForEmployees, DocumentsFilterViewModel filter)
		{
			var selectedSubdivisionsIds = filter.Subdivision != null
				? new int[] { filter.Subdivision.Id }
				: filter.AvailableSubdivisions?
					.Select(x => x.Id)
					.ToArray();

			var totalCash = 0m;
			var allCashString = string.Empty;

			foreach(var node in cashForEmployees.GroupBy(c => c.SubdivisionName))
			{
				var balance = node.Sum(n => n.Balance);
				totalCash += balance;
				allCashString += $"\r\n{node.Key}: {CurrencyWorks.GetShortCurrencyString(balance)}";
			}

			var total = $"Денег в кассе: {CurrencyWorks.GetShortCurrencyString(totalCash)}. ";
			var separatedCash = selectedSubdivisionsIds != null && selectedSubdivisionsIds.Any() ? $"\r\n\tИз них: {allCashString}" : "";

			var inTransferring = _cashRepository.GetCashInTransferring(_uow);
			var cashInTransferringMessage = $"\n\nВ сейфе инкассатора: {CurrencyWorks.GetShortCurrencyString(inTransferring)}";

			ylabelMainInfo.Text = total + separatedCash + cashInTransferringMessage;
		}

		private void RefreshDetalizationInfo(List<EmployeeBalanceNode> cashForEmployees, DocumentsFilterViewModel filter)
		{
			var datePeriod = filter.StartDate == filter.EndDate
				? $"{filter.StartDate:d}"
				: $"{filter.StartDate:d}-{filter.EndDate:d}";

			var dateInfo = filter.StartDate is null
				? "Детализация:"
				: $"Дет-я за {datePeriod}";

			ylabelDetalizationTitle.LabelProp = $"<u><b>{dateInfo}</b></u>";

			var totalCash = cashForEmployees
				.Where(c => filter.StartDate is null || (c.Date >= filter.StartDate && c.Date <= filter.EndDate))
				.Sum(x => x.Balance);

			ylabelInfoTop.Text = $"Денег в кассе: {CurrencyWorks.GetShortCurrencyString(totalCash)}.\r\n\tИз них:";

			var inTransferring = _cashRepository.GetCashInTransferring(_uow, filter.StartDate, filter.EndDate);
			ylabelInfoBottom.Text = $"В сейфе инкассатора: {CurrencyWorks.GetShortCurrencyString(inTransferring)}";
		}

		private void RefreshEmployeesTree(List<EmployeeBalanceNode> cashForEmployees, DocumentsFilterViewModel filter)
		{
			var resultBalances = new List<SubdivisionBalanceNode>();

			foreach(var subdivisionGrouped in
				cashForEmployees
				.Where(c => filter.StartDate is null || (c.Date >= filter.StartDate && c.Date <= filter.EndDate))
				.GroupBy(g => g.SubdivisionName))
			{
				var parrentNode = new SubdivisionBalanceNode
				{
					SubdivisionName = subdivisionGrouped.FirstOrDefault().SubdivisionName,
				};

				var childNodes = new List<EmployeeBalanceNode>();

				foreach(var cashierGrouped in
					cashForEmployees
					.Where(c => c.SubdivisionName == parrentNode.SubdivisionName)
					.Where(c => filter.StartDate is null || (c.Date >= filter.StartDate && c.Date <= filter.EndDate))
					.GroupBy(g => g.Cashier))
				{
					var childNode = new EmployeeBalanceNode
					{
						Cashier = cashierGrouped.Key,
						Balance = cashierGrouped.Sum(n => n.Balance),
						ParentBalanceNode = parrentNode
					};

					childNodes.Add(childNode);
				}

				parrentNode.ChildResultBalanceNodes = childNodes;

				resultBalances.Add(parrentNode);
			}

			var levels = LevelConfigFactory
					.FirstLevel<SubdivisionBalanceNode, EmployeeBalanceNode>(x => x.ChildResultBalanceNodes)
					.LastLevel(c => c.ParentBalanceNode).EndConfig();

			yTreeView.YTreeModel = new LevelTreeModel<SubdivisionBalanceNode>(resultBalances, levels);
		}

		#endregion

		private void ConfigureWidget()
		{
			yTreeView.ColumnsConfig = ColumnsConfigFactory.Create<object>()
				.AddColumn("Ответственный")
					.AddTextRenderer(n => GetNodeText(n))
					.AddSetter((c, n) => c.Alignment = n is SubdivisionBalanceNode ? Pango.Alignment.Left : Pango.Alignment.Right)
					.WrapWidth(110).WrapMode(Pango.WrapMode.WordChar)
				.AddColumn("Баланс")
					.AddTextRenderer(n => CurrencyWorks.GetShortCurrencyString(GetBalance(n)))
					.WrapWidth(110).WrapMode(Pango.WrapMode.WordChar)
				.Finish();
		}

		private string GetNodeText(object node)
		{
			if(node is SubdivisionBalanceNode balanceNode)
			{
				return balanceNode.SubdivisionName;
			}

			if(node is EmployeeBalanceNode resultNode)
			{
				return resultNode.Cashier?.GetPersonNameWithInitials() ?? "Не указано";
			}

			return "";
		}

		private decimal GetBalance(object node)
		{
			if(node is SubdivisionBalanceNode balanceNode)
			{
				return balanceNode.ChildResultBalanceNodes.Sum(x => x.Balance);
			}

			if(node is EmployeeBalanceNode resultNode)
			{
				return resultNode.Balance;
			}

			return 0;
		}

		public override void Destroy()
		{
			yTreeView?.Destroy();
			_uow?.Dispose();
			base.Destroy();
		}
	}
}
