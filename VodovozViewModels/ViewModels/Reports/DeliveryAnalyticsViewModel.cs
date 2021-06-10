using System;
using System.ComponentModel.DataAnnotations;
using System.Data.Bindings.Collections.Generic;
using System.Linq;
using QS.Commands;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Journal.EntitySelector;
using QS.Project.Services;
using QS.Services;
using QS.Tdi;
using QS.ViewModels;
using Vodovoz.Domain.Sale;
using Vodovoz.Domain.WageCalculation;
using Vodovoz.Journals.FilterViewModels;
using Vodovoz.Journals.JournalViewModels;
using Vodovoz.ViewModels.Logistic;
using Vodovoz.ViewModels.ViewModels.Reports.DeliveryAnalytics;

namespace Vodovoz.ViewModels.ViewModels.Reports
{
	public class DeliveryAnalyticsViewModel : DialogTabViewModelBase, ITDICloseControlTab
	{
		private readonly IEntityAutocompleteSelectorFactory _districtSelectorFactory;
		private DateTime? startDeliveryDate = DateTime.Today;
		private DateTime? endDeliveryDate = DateTime.Today;
		private DateTime? startCreationDate;
		private DelegateCommand exportCommand = null;
		public string LoadingData = "Идет загрузка данных...";
		private bool isLoadingData;
		
		public DeliveryAnalyticsViewModel(
			IUnitOfWorkFactory unitOfWorkFactory,
			IInteractiveService interactiveService,
			INavigationManager navigation)
			: base(unitOfWorkFactory, interactiveService, navigation)
		{
			//this.interactiveService = interactiveService ?? throw new ArgumentNullException(nameof(interactiveService));

			if(unitOfWorkFactory == null)
			{
				throw new ArgumentNullException(nameof(unitOfWorkFactory));
			}
			UoW = unitOfWorkFactory.CreateWithoutRoot();
			Title = "Аналитика объёмов доставки";
			
			_districtSelectorFactory =
				new DefaultEntityAutocompleteSelectorFactory<District, DistrictJournalViewModel,
					DistrictJournalFilterViewModel>(ServicesConfig.CommonServices);
			WaveList = new GenericObservableList<WaveNode>();
			GeographicGroupNodes = new GenericObservableList<GeographicGroupNode>(UoW.GetAll<GeographicGroup>().Select(x => new GeographicGroupNode(x)).ToList());
			WageDistrictNodes = new GenericObservableList<WageDistrictNode>(UoW.GetAll<WageDistrict>().Select(x => new WageDistrictNode(x)).ToList());
			
			foreach(var wave in Enum.GetValues(typeof(WaveNodes)))
			{
				var waveNode = new WaveNode { WaveNodes = (WaveNodes) wave, Selected = false};
				WaveList.Add(waveNode);
			}
		}
		
		#region Свойства
		private District _district;

		public District District
		{
			get => _district;
			set => SetField(ref _district, value);
		}

		public DateTime? StartDeliveryDate
		{
			get => startDeliveryDate;
			set 
			{
				if (SetField(ref startDeliveryDate, value)) 
				{
					OnPropertyChanged(nameof(HasRunReport));
				}
			}
		}
        
		public DateTime? EndDeliveryDate
		{
			get => endDeliveryDate;
			set => SetField(ref endDeliveryDate, value);
		}
        
		public DateTime? DayOfWeek
		{
			get => startCreationDate;
			set 
			{
				if (SetField(ref startCreationDate, value)) 
				{
					OnPropertyChanged(nameof(HasRunReport));
				}
			} 
		}
		
		public bool IsLoadingData
		{
			get => isLoadingData;
			set
			{
				if (isLoadingData != value)
				{
					isLoadingData = value;
					OnPropertyChanged(nameof(HasRunReport));
				}
			}
		}

		public bool HasRunReport => StartDeliveryDate.HasValue && !IsLoadingData;

		public GenericObservableList<GeographicGroupNode> GeographicGroupNodes { get; private set; }
		
		public GenericObservableList<WageDistrictNode> WageDistrictNodes { get; private set; }

		public GenericObservableList<WaveNode> WaveList { get; private set; }
		
		public string FileName { get; set; }
		#endregion


		public bool CanClose()
		{
			throw new NotImplementedException();
		}public DelegateCommand ExportCommand => exportCommand ?? (exportCommand = new DelegateCommand(
			() => 
			{
				try
				{
					
				}
				catch (Exception e) {
				}
			},
			() => !string.IsNullOrEmpty(FileName)
		));
	}

	public enum WaveNodes
	{
		[Display(Name = "1 Волна")]
		FirstWave,
		[Display(Name = "2 Волна")]
		SecondWave,
		[Display(Name = "3 Волна")]
		ThirdWave
	}
}
