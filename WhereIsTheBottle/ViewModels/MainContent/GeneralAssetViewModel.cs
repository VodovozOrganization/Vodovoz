using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using QS.Dialog;
using Vodovoz.EntityRepositories.Goods.BottleAnalytics;
using WhereIsTheBottle.Commands;
using WhereIsTheBottle.Models.MainContent;

namespace WhereIsTheBottle.ViewModels.MainContent
{
	public class GeneralAssetViewModel : BottleAnalyticsReportViewModelBase<GeneralAssetModel>
	{
		private readonly IInteractiveService _interactiveService;

		private DataTable _dataTable;

		public GeneralAssetViewModel(GeneralAssetModel model, IInteractiveService interactiveService)
			: base(model)
		{
			_interactiveService = interactiveService ?? throw new ArgumentNullException(nameof(interactiveService));
			SubscribeToModelPropertyUpdates();
		}

		public override string HeaderString => "Актив";

		public DataTable DataTable
		{
			get => _dataTable;
			set => SetField(ref _dataTable, value);
		}

		public IList<NomenclatureNode> WaterNomenclatures => Model.WaterNomenclatures;

		public IList<NomenclatureNode> EmptyNomenclatures => Model.EmptyNomenclatures;

		public IList<NomenclatureNode> ShabbyNomenclatures => Model.ShabbyNomenclatures;

		private RelayCommand _loadDataCommand;
		public RelayCommand LoadDataCommand => _loadDataCommand ??= new RelayCommand(
			async () =>
			{
				if(EndDate == null)
				{
					return;
				}
				var endDate = EndDate.Value;

				try
				{
					DataTable?.Clear();
					IsDataLoading = true;

					IList<string> errorMessages = null;
					DataTable dataTable = null;
					var result = await Task.Run(() => Model.TryGetDataTable(endDate, out errorMessages, out dataTable));
					if(!result)
					{
						_interactiveService.ShowMessage(ImportanceLevel.Warning, String.Join('\n', errorMessages));
						return;
					}

					DataTable = dataTable;
					DateFormed = DateTime.Now;
					IsDataLoaded = true;
					OnPropertyChanged(nameof(HeaderString));
				}
				finally
				{
					IsDataLoading = false;
				}
			}, () => !IsDataLoading
		);
	}
}
