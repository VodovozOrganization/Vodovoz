using System;
using QS.Services;
using QS.ViewModels;
using System.Collections.Generic;
using System.Linq;
using QS.Commands;
using QS.DomainModel.UoW;
using Vodovoz.Domain.WageCalculation;
using Vodovoz.EntityRepositories.WageCalculation;
using System.Data.Bindings.Collections.Generic;
using QS.DomainModel.Entity;

namespace Vodovoz.ViewModels.WageCalculation
{
	public class CarsWageParametersViewModel : UoWTabViewModelBase
	{
		private readonly IWageCalculationRepository wageCalculationRepository;
		private readonly ICommonServices commonServices;

		public event EventHandler OnParameterNodesUpdated;

		public CarsWageParametersViewModel(IWageCalculationRepository wageCalculationRepository, ICommonServices commonServices) : base(commonServices.InteractiveService)
		{
			TabName = "Ставки для автомобилей компании";

			this.wageCalculationRepository = wageCalculationRepository ?? throw new ArgumentNullException(nameof(wageCalculationRepository));
			this.commonServices = commonServices ?? throw new ArgumentNullException(nameof(commonServices));

			UoW = UnitOfWorkFactory.CreateWithoutRoot();

			ObservableWageParameters.ElementAdded += (aList, aIdx) => WageParametersUpdated();
			ObservableWageParameters.ElementRemoved += (aList, aIdx, aObject) => WageParametersUpdated();
			CreateCommands();
			LoadData();
		}

		private DateTime? startDate;
		[PropertyChangedAlso(nameof(CanChangeWageCalculation))]
		public virtual DateTime? StartDate {
			get => startDate;
			set => SetField(ref startDate, value, () => StartDate);
		}

		IList<WageParameter> wageParameters = new List<WageParameter>();
		public virtual IList<WageParameter> WageParameters {
			get => wageParameters;
			set => SetField(ref wageParameters, value, () => WageParameters);
		}

		GenericObservableList<WageParameter> observableWageParameters;
		//FIXME Кослыль пока не разберемся как научить hibernate работать с обновляемыми списками.
		public virtual GenericObservableList<WageParameter> ObservableWageParameters {
			get {
				if(observableWageParameters == null)
					observableWageParameters = new GenericObservableList<WageParameter>(WageParameters);
				return observableWageParameters;
			}
		}

		public virtual IList<WageParameterNode> WageParameterNodes => ObservableWageParameters.Select(x => new WageParameterNode(x)).ToList();

		private void WageParametersUpdated()
		{
			OnParameterNodesUpdated?.Invoke(this, EventArgs.Empty);
		}

		private void LoadData()
		{
			var items = wageCalculationRepository.GetWageParameters(UoW, WageParameterTargets.ForOurCars);
			ObservableWageParameters.Clear();
			foreach(var item in items) {
				ObservableWageParameters.Add(item);
			}
		}

		public virtual bool CheckStartDateForNewWageParameter(DateTime newStartDate)
		{
			WageParameter oldWageParameter = ObservableWageParameters.FirstOrDefault(x => x.EndDate == null);
			if(oldWageParameter == null) {
				return true;
			}

			return oldWageParameter.StartDate < newStartDate;
		}

		public virtual void ChangeWageParameter(WageParameter wageParameter)
		{
			if(wageParameter == null) {
				throw new ArgumentNullException(nameof(wageParameter));
			}

			if(StartDate == null) {
				ShowErrorMessage("Необходимо выбрать время");
				return;
			}

			wageParameter.StartDate = StartDate.Value.AddTicks(1);
			WageParameter oldWageParameter = ObservableWageParameters.FirstOrDefault(x => x.EndDate == null);
			if(oldWageParameter != null) {
				if(oldWageParameter.StartDate > startDate) {
					throw new InvalidOperationException("Нельзя создать новую запись с датой более ранней уже существующей записи. Неверно выбрана дата");
				}
				oldWageParameter.EndDate = StartDate;
			}
			ObservableWageParameters.Add(wageParameter);
		}

		private void CreateCommands()
		{
			CreateChangeWageParameterCommand();
			CreateOpenWageParameterCommand();
			CreateChangeWageStartDateCommand();
		}

		#region ChangeWageParameterCommand

		public DelegateCommand ChangeWageParameterCommand { get; private set; }

		public virtual bool CanChangeWageCalculation => StartDate.HasValue && CheckStartDateForNewWageParameter(StartDate.Value);

		private void CreateChangeWageParameterCommand()
		{
			ChangeWageParameterCommand = new DelegateCommand(
				() => {
					WageParameterViewModel newWageParameterViewModel = new WageParameterViewModel(UoW, WageParameterTargets.ForOurCars, commonServices);
					newWageParameterViewModel.OnWageParameterCreated += (sender, wageParameter) => {
						ChangeWageParameter(wageParameter);
					};
					TabParent.AddSlaveTab(this, newWageParameterViewModel);
				},
				() => CanChangeWageCalculation
			);

			ChangeWageParameterCommand.CanExecuteChangedWith(this, x => x.CanChangeWageCalculation);
		}

		#endregion ChangeWageParameterCommand

		#region ChangeWageStartDateCommand

		public DelegateCommand<WageParameterNode> ChangeWageStartDateCommand { get; private set; }

		private void CreateChangeWageStartDateCommand()
		{
			ChangeWageStartDateCommand = new DelegateCommand<WageParameterNode>(
				(node) => {
					if(!AskQuestion(
						"Внимание! Будет произведено изменение даты в уже имеющемся расчете зарплаты, " +
						"документы попадающие в этот интервал будут пересчитываться по другому расчету! " +
						"Продолжить?", "Внимание!")) {
						return;
					}

					var previousParameter = GetPreviousParameter(node.WageParameter.StartDate);
					if(previousParameter != null) {
						previousParameter.EndDate = StartDate.Value.AddTicks(-1);
					}
					node.WageParameter.StartDate = StartDate.Value;
					WageParametersUpdated();

				},
				(node) => {
					if(node == null || !StartDate.HasValue) {
						return false;
					}
					var previousParameterByDate = GetPreviousParameter(StartDate.Value);
					var previousParameterBySelectedParameter = GetPreviousParameter(node.WageParameter.StartDate);

					bool noConflictWithEndDate = !node.WageParameter.EndDate.HasValue || node.WageParameter.EndDate.Value > StartDate;
					bool noConflictWithPreviousStartDate = (previousParameterByDate == null && previousParameterBySelectedParameter == null) || (previousParameterBySelectedParameter != null && previousParameterBySelectedParameter.StartDate < StartDate);

					return StartDate.HasValue && noConflictWithEndDate && noConflictWithPreviousStartDate;
				}
			);
			ChangeWageStartDateCommand.CanExecuteChangedWith(this, x => x.StartDate);
		}

		private WageParameter GetPreviousParameter(DateTime date)
		{
			return ObservableWageParameters
						.Where(x => x.EndDate != null)
						.Where(x => x.EndDate <= date)
						.OrderByDescending(x => x.EndDate)
						.FirstOrDefault();
		}

		#endregion ChangeWageStartDateCommand

		#region OpenWageParameterCommand

		public DelegateCommand<WageParameterNode> OpenWageParameterCommand { get; private set; }	

		private void CreateOpenWageParameterCommand()
		{
			OpenWageParameterCommand = new DelegateCommand<WageParameterNode>(
				(node) => {
					WageParameterViewModel wageParameterViewModel = new WageParameterViewModel(node.WageParameter, UoW, commonServices);
					TabParent.AddTab(wageParameterViewModel, this);
				},
				(node) => node != null
			);
		}

		#endregion OpenWageParameterCommand

		public override bool Save(bool close)
		{
			foreach(var wagePararmeter in WageParameters) {
				UoW.Save(wagePararmeter);
			}
			UoW.Commit();
			Close(false);
			return true;
		}
	}
}
