using QS.Commands;
using QS.Dialog;
using QS.Navigation;
using QS.Project.Services.FileDialog;
using QS.ViewModels.Dialog;
using RevenueService.Client;
using RevenueService.Client.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.ViewModels.ViewModels.Reports.Counterparty;

namespace Vodovoz.ViewModels.ViewModels.Counterparty
{
	public class CounterpartyDetailsFromRevenueServiceViewModel : WindowDialogViewModelBase
	{
		private readonly IRevenueServiceClient _revenueServiceClient;
		private readonly IFileDialogService _fileDialogService;
		private DelegateCommand _replaceDetailsCommand;
		private CounterpartyRevenueServiceDto _selectedNode;
		private DelegateCommand _exportToExcelCommand;

		public CounterpartyDetailsFromRevenueServiceViewModel(INavigationManager navigationManager, DadataRequestDto request, IRevenueServiceClient revenueServiceClient, 
			IFileDialogService fileDialogService, CancellationToken cancellationToken)
			: base(navigationManager)
		{
			_revenueServiceClient = revenueServiceClient ?? throw new ArgumentNullException(nameof(revenueServiceClient));
			_fileDialogService = fileDialogService ?? throw new ArgumentNullException(nameof(fileDialogService)); ;
			WindowPosition = WindowGravity.None;

			Nodes = LoadFromRevenueService(request, cancellationToken).Result
				.OrderBy(x => !x.IsActive)
				.ToList();
		}

		private async Task<IList<CounterpartyRevenueServiceDto>> LoadFromRevenueService(DadataRequestDto request, CancellationToken cancellationToken)
		{
			var counterpartyDetails = await _revenueServiceClient.GetCounterpartyInfoAsync(request, cancellationToken);

			return counterpartyDetails;
		}

		public IList<CounterpartyRevenueServiceDto> Nodes { get; set; }

		public CounterpartyRevenueServiceDto SelectedNode
		{
			get => _selectedNode;
			set => SetField(ref _selectedNode, value);
		}

		public event EventHandler<CounterpartyRevenueServiceDto> OnSelectResult;

		public string Message { get; set; }

		public DelegateCommand ReplaceDetailsCommand =>
			_replaceDetailsCommand ?? (_replaceDetailsCommand = new DelegateCommand(() =>
				{
					OnSelectResult?.Invoke(this, SelectedNode);
					Close(false, CloseSource.ClosePage);
				}
			));

		public DelegateCommand ExportToExcelCommand =>
			_exportToExcelCommand ?? (_exportToExcelCommand = new DelegateCommand(() =>
				{
					var report = new CounterpartyRevenueServiceReport(Nodes, _fileDialogService);
					report.Export();
					Close(false, CloseSource.ClosePage);
				}
			));
	}
}
