using DateTimeHelpers;
using QS.Commands;
using QS.Dialog;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.ViewModels;
using System;
using System.Data.Bindings.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Core.Domain.Clients;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Orders;
using Vodovoz.Services;
using Vodovoz.ViewModelBased;

namespace Vodovoz.ViewModels.AdministrationTools
{
	public class RevenueServiceMassCounterpartyUpdateToolViewModel : DialogTabViewModelBase
	{
		private readonly IUnitOfWork _unitOfWork;
		private readonly IInteractiveService _interactiveService;
		private readonly ICounterpartyService _counterpartyService;
		private readonly CancellationTokenSource _cancellationTokenSource;
		private DateTime? _startDate;
		private DateTime? _endDate;

		private string _lastErrors = string.Empty;

		public RevenueServiceMassCounterpartyUpdateToolViewModel(
			IUnitOfWorkFactory unitOfWorkFactory,
			IInteractiveService interactiveService,
			INavigationManager navigation,
			ICounterpartyService counterpartyService)
			: base(unitOfWorkFactory, interactiveService, navigation)
		{
			_interactiveService = interactiveService ?? throw new ArgumentNullException(nameof(interactiveService));
			_counterpartyService = counterpartyService ?? throw new ArgumentNullException(nameof(counterpartyService));

			if(unitOfWorkFactory is null)
			{
				throw new ArgumentNullException(nameof(unitOfWorkFactory));
			}

			_cancellationTokenSource = new CancellationTokenSource();

			_unitOfWork = unitOfWorkFactory.CreateWithoutRoot(TabName);

			TabName = "Обновление сведений Контрагентов из ФНС";

			StartDate = DateTime.Today.AddDays(-7);
			EndDate = DateTime.Today;

			SearchCounterpartiesCommand = new DelegateCommand(SearchCounterparties);
			UpdateCounterpartiesRevenueServiceInformationCommand = new DelegateCommand(async () => await UpdateCounterpartiesRevenueServiceInformationAsync(), () => CounterpartiesRows.Any(x => x.Selected));
			ShowLastErrorsAndClearCommand = new DelegateCommand(ShowLastErrorsAndClear);
		}

		public DateTime? StartDate
		{
			get => _startDate;
			set => SetField(ref _startDate, value);
		}

		public DateTime? EndDate
		{
			get => _endDate;
			set => SetField(ref _endDate, value);
		}

		public GenericObservableList<CounterpartyUpdateRow> CounterpartiesRows { get; }
			= new GenericObservableList<CounterpartyUpdateRow>();

		public DelegateCommand SearchCounterpartiesCommand { get; }

		public DelegateCommand UpdateCounterpartiesRevenueServiceInformationCommand { get; }

		public DelegateCommand ShowLastErrorsAndClearCommand { get; }

		private void SearchCounterparties()
		{
			CounterpartiesRows.Clear();

			var rows = (from counterparty in _unitOfWork.Session.Query<Counterparty>()
						let latestOrderDate = (from order in _unitOfWork.Session.Query<Order>()
											   where counterparty.Id == order.Client.Id
											   && order.DeliveryDate >= StartDate.Value
											   && order.DeliveryDate <= EndDate.Value.LatestDayTime()
											   orderby order.DeliveryDate descending
											   select order.DeliveryDate)
										  .Take(1)
										  .FirstOrDefault()
						where latestOrderDate != null
							&& counterparty.PersonType == PersonType.legal
							&& !(counterparty.INN == null || counterparty.INN == string.Empty)
						let selected = !counterparty.IsArchive
							&& !counterparty.IsLiquidating
							&& !string.IsNullOrWhiteSpace(counterparty.INN)
						select new CounterpartyUpdateRow
						{
							Id = counterparty.Id,
							Selected = selected,
							Name = counterparty.Name,
							INN = counterparty.INN,
							KPP = counterparty.KPP,
							IsArchive = counterparty.IsArchive,
							IsLiquidating = counterparty.IsLiquidating,
							LastSale = latestOrderDate.Value
						})
						.ToList();

			foreach(var row in rows)
			{
				CounterpartiesRows.Add(row);
			}
		}

		private async Task UpdateCounterpartiesRevenueServiceInformationAsync()
		{
			var selectedCounterpartyRows = CounterpartiesRows.Where(x => x.Selected);

			foreach(var counterparty in selectedCounterpartyRows)
			{
				try
				{
					var information = await _counterpartyService.GetRevenueServiceInformation(
						counterparty.INN,
						counterparty.KPP,
						_cancellationTokenSource.Token);

					var singleInformation = information.Single();

					_counterpartyService.UpdateDetailsFromRevenueServiceInfoIfNeeded(counterparty.Id, singleInformation);
				}
				catch(InvalidOperationException ex) when (ex.Message == "Последовательность содержит более одного элемента"
					&& ex.TargetSite.Name == "Single")
				{
					_lastErrors += $"{counterparty.Name} : найдено несколько записей в ФНС с ИНН:  \"{counterparty.INN}\" и КПП: \"{counterparty.KPP}\" \n";
				}
				catch(InvalidOperationException ex) when(ex.Message == "Последовательность не содержит элементов"
					&& ex.TargetSite.Name == "Single")
				{
					_lastErrors += $"{counterparty.Name} : не найдено ни одной записи в ФНС с ИНН:\"{counterparty.INN}\" и КПП: \"{counterparty.KPP}\" \n";
				}
				catch(Exception ex)
				{
					var type = ex.GetType();
					_lastErrors += counterparty.Name + " : " + ex.Message + '\n';
				}
			}
		}

		private void ShowLastErrorsAndClear()
		{
			if(_lastErrors.Length > 0)
			{
				_interactiveService.ShowMessage(ImportanceLevel.Warning, _lastErrors, "Во время обновлдения произошли ошибки");
				_lastErrors = string.Empty;
			}
		}

		public class CounterpartyUpdateRow
		{
			public int Id { get; set; }

			public bool Selected { get; set; }

			public string Name { get; set; }

			public string Surname { get; set; }

			public string Patronymic { get; set; }

			public string FullName { get; set; }

			public string TypeOfOwnership { get; set; }

			public string SignatoryFIO { get; set; }

			public string INN { get; set; }

			public string KPP { get; set; }

			public string LegalAddress { get; set; }

			public GenericObservableList<string> Phones { get; } = new GenericObservableList<string>();

			public GenericObservableList<string> Emails { get; } = new GenericObservableList<string>();

			public bool IsArchive { get; set; }

			public bool IsLiquidating { get; set; }

			public DateTime LastSale { get; set; }
		}
	}
}
