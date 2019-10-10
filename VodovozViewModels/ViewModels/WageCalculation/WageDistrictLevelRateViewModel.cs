using System;
using System.Linq;
using QS.Commands;
using QS.Services;
using QS.ViewModels;
using Vodovoz.Domain.WageCalculation;
using QS.DomainModel.UoW;

namespace Vodovoz.ViewModels.WageCalculation
{
	public class WageDistrictLevelRateViewModel : EntityWidgetViewModelBase<WageDistrictLevelRate>
	{
		public WageDistrictLevelRateViewModel(WageDistrictLevelRate entity, ICommonServices commonServices, IUnitOfWork uow) : base(entity, commonServices)
		{
			UoW = uow;
			ConfigureViewModel();
			CreateCreateAndFillNewRatesCommand();
		}

		void ConfigureViewModel()
		{
			CanFillRates = Entity.Id <= 0 && !Entity.WageRates.Any();
		}

		bool canFillRates;
		public virtual bool CanFillRates {
			get => canFillRates;
			set => SetField(ref canFillRates, value);
		}

		#region CreateAndFillNewRatesCommand

		public DelegateCommand CreateAndFillNewRatesCommand { get; private set; }

		void CreateCreateAndFillNewRatesCommand()
		{
			CreateAndFillNewRatesCommand = new DelegateCommand(
				() => {
					foreach(WageRateTypes enumValue in Enum.GetValues(typeof(WageRateTypes))) {
						if(!Entity.ObservableWageRates.Any(r => r.WageRateType == enumValue))
							Entity.ObservableWageRates.Add(
								new WageRate {
									WageRateType = enumValue,
									ForDriverWithForwarder = 0,
									ForDriverWithoutForwarder = 0,
									ForForwarder = 0,
									WageDistrictLevelRate = Entity
								}
							);
					}
					CanFillRates = false;
				},
				() => CanFillRates
			);
		}

		#endregion CreateAndFillNewRatesCommand
	}
}
