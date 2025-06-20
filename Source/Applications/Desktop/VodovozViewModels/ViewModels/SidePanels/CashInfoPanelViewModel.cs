using Gamma.Binding;
using Gamma.Binding.Core.LevelTreeConfig;
using QS.Commands;
using QS.DomainModel.UoW;
using QS.Services;
using QS.Tdi;
using QS.Utilities;
using QS.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using Vodovoz.EntityRepositories;
using Vodovoz.EntityRepositories.Cash;
using Vodovoz.EntityRepositories.Subdivisions;
using Vodovoz.ViewModels.Cash.DocumentsJournal;
using DateTimeHelpers;


namespace Vodovoz.ViewModels.ViewModels.SidePanels
{
	public class CashInfoPanelViewModel : UoWWidgetViewModelBase
	{
		private readonly IUnitOfWork _unitOfWork;
		private readonly ICashRepository _cashRepository;
		private List<int> _sortedSubdivisionsIds;
		private LevelTreeModel<SubdivisionBalanceNode> _levelTreeModel;
		private string _detalizationInfoBottom;
		private string _detalizationInfoTop;
		private string _detalizationTitle;
		private string _mainInfo;

		public CashInfoPanelViewModel(
			IUnitOfWorkFactory unitOfWorkFactory,
			ICommonServices commonServices,
			ICashRepository cashRepository,
			ISubdivisionRepository subdivisionRepository,
			IUserRepository userRepository)
		{
			if(unitOfWorkFactory is null)
			{
				throw new ArgumentNullException(nameof(unitOfWorkFactory));
			}

			_unitOfWork = unitOfWorkFactory.CreateWithoutRoot("Боковая панель остатков по кассам");

			_cashRepository = cashRepository ?? throw new ArgumentNullException(nameof(cashRepository));

			var currentUser = commonServices.UserService.GetCurrentUser();
			var availableSubdivisions = subdivisionRepository.GetCashSubdivisionsAvailableForUser(_unitOfWork, currentUser).ToList();

			var settings =
				(userRepository ?? throw new ArgumentNullException(nameof(userRepository)))
				.GetCurrentUserSettings(_unitOfWork);

			var needSave = settings.UpdateCashSortingSettings(availableSubdivisions.Select(x => x.Id));
			if(needSave)
			{
				_unitOfWork.Save(settings);
				_unitOfWork.Commit();
			}

			_sortedSubdivisionsIds = settings.CashSubdivisionSortingSettings
				.OrderBy(x => x.SortingIndex)
				.Where(x => x.CashSubdivisionId.HasValue)
				.Select(x => x.CashSubdivisionId.Value)
				.ToList();

			RefreshCommand = new DelegateCommand<DocumentsFilterViewModel>(Refresh);
		}

		private void RefreshMainInfo(List<EmployeeBalanceNode> cashForEmployees, DocumentsFilterViewModel filter)
		{
			var selectedSubdivisionsIds = filter.Subdivision != null
				? new int[] { filter.Subdivision.Id }
				: filter.AvailableSubdivisions?
					.Where(x => !x.IsArchive)
					.Select(x => x.Id)
					.ToArray();

			var totalCash = 0m;
			var allCashString = string.Empty;

			foreach(var node in cashForEmployees.GroupBy(c => c.SubdivisionName))
			{
				var balance = node.Sum(n => n.Balance);
				totalCash += balance;
				allCashString += $"\n{node.Key}: {CurrencyWorks.GetShortCurrencyString(balance)}";
			}

			var total = $"Денег в кассе: {CurrencyWorks.GetShortCurrencyString(totalCash)}. ";
			var separatedCash = selectedSubdivisionsIds != null && selectedSubdivisionsIds.Any() ? $"\n\tИз них: {allCashString}" : "";

			var inTransferring = _cashRepository.GetCashInTransferring(_unitOfWork);
			var cashInTransferringMessage = $"\n\nВ сейфе инкассатора: {CurrencyWorks.GetShortCurrencyString(inTransferring)}";

			MainInfo = total + separatedCash + cashInTransferringMessage;
		}

		private void RefreshDetalizationInfo(List<EmployeeBalanceNode> cashForEmployees, DocumentsFilterViewModel filter)
		{
			var datePeriod = filter.StartDate == filter.EndDate
				? $"{filter.StartDate:d}"
				: $"{filter.StartDate:d}-{filter.EndDate:d}";

			var dateInfo = filter.StartDate is null
				? "Детализация:"
				: $"Дет-я за {datePeriod}";

			DetalizationTitle = $"<u><b>{dateInfo}</b></u>";

			var totalCash = cashForEmployees
				.Where(c => filter.StartDate is null || (c.Date >= filter.StartDate && c.Date <= filter.EndDate.Value.LatestDayTime()))
				.Sum(x => x.Balance);

			DetalizationInfoTop = $"Денег в кассе: {CurrencyWorks.GetShortCurrencyString(totalCash)}.\n\tИз них:";

			var inTransferring = _cashRepository.GetCashInTransferring(_unitOfWork, filter.StartDate, filter.EndDate?.LatestDayTime() ?? null);
			DetalizationInfoBottom = $"В сейфе инкассатора: {CurrencyWorks.GetShortCurrencyString(inTransferring)}";
		}

		private void RefreshEmployeesTree(List<EmployeeBalanceNode> cashForEmployees, DocumentsFilterViewModel filter)
		{
			var resultBalances = new List<SubdivisionBalanceNode>();

			foreach(var subdivisionGrouped in
				cashForEmployees
				.Where(c => filter.StartDate is null || (c.Date >= filter.StartDate && c.Date <= filter.EndDate.Value.LatestDayTime()))
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
					.Where(c => filter.StartDate is null || (c.Date >= filter.StartDate && c.Date <= filter.EndDate.Value.LatestDayTime()))
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

			LevelTreeModel = new LevelTreeModel<SubdivisionBalanceNode>(resultBalances, levels);
		}

		private void Refresh(DocumentsFilterViewModel filter)
		{
			var selectedSubdivisionsIds = filter.Subdivision is null
				? filter.AvailableSubdivisions?
					.Where(x => !x.IsArchive)
					.Select(x => x.Id)
					.ToArray()
				: new int[] { filter.Subdivision.Id };

			if(selectedSubdivisionsIds is null)
			{
				return;
			}

			var cashForEmployees = _cashRepository.CurrentCashForGivenSubdivisions(_unitOfWork, selectedSubdivisionsIds)
				.OrderBy(x => _sortedSubdivisionsIds.IndexOf(x.SubdivisionId))
				.ToList();

			RefreshMainInfo(cashForEmployees, filter);

			RefreshEmployeesTree(cashForEmployees, filter);

			RefreshDetalizationInfo(cashForEmployees, filter);
		}

		public LevelTreeModel<SubdivisionBalanceNode> LevelTreeModel
		{
			get => _levelTreeModel;
			set => SetField(ref _levelTreeModel, value);
		}

		public string DetalizationInfoBottom
		{
			get => _detalizationInfoBottom;
			set => SetField(ref _detalizationInfoBottom, value);
		}

		public string DetalizationInfoTop
		{
			get => _detalizationInfoTop;
			set => SetField(ref _detalizationInfoTop, value);
		}

		public string DetalizationTitle
		{
			get => _detalizationTitle;
			set => SetField(ref _detalizationTitle, value);
		}

		public string MainInfo
		{
			get => _mainInfo;
			set => SetField(ref _mainInfo, value);
		}

		public DelegateCommand<DocumentsFilterViewModel> RefreshCommand { get; }

		public string GetNodeText(object node)
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

		public decimal GetBalance(object node)
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
	}
}
