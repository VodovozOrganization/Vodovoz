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
		private readonly IInteractiveService _interactiveService;
		private DelegateCommand _replaceDetailsCommand;
		private CounterpartyRevenueServiceDto _selectedNode;
		private DelegateCommand _exportToExcelCommand;

		public CounterpartyDetailsFromRevenueServiceViewModel(INavigationManager navigationManager, DadataRequestDto request, IRevenueServiceClient revenueServiceClient, 
			IFileDialogService fileDialogService, IInteractiveService interactiveService, CancellationToken cancellationToken)
			: base(navigationManager)
		{
			_revenueServiceClient = revenueServiceClient ?? throw new ArgumentNullException(nameof(revenueServiceClient));
			_fileDialogService = fileDialogService ?? throw new ArgumentNullException(nameof(fileDialogService));
			_interactiveService = interactiveService ?? throw new ArgumentNullException(nameof(interactiveService));
			
			WindowPosition = WindowGravity.None;

			var response = LoadFromRevenueService(request, cancellationToken).Result;

			if(!response.CounterpartyDetailsList.Any()
			   && !string.IsNullOrEmpty(request.Kpp)
			   && interactiveService.Question($"По комбинации ИНН {request.Inn} + КПП {request.Kpp} не найдено результатов. Попробовать загрузить только по ИНН?"))
			{
				request.Kpp = null;
				response = LoadFromRevenueService(request, cancellationToken).Result;
			}

			Nodes = response
				.CounterpartyDetailsList?
				.OrderBy(x => !x.IsActive)
				.ToList();

			var kpp = string.IsNullOrEmpty(request.Kpp) ? "" : $"КПП: {request.Kpp}";
			Message = $"ИНН: {request.Inn} {kpp}";

			if(!string.IsNullOrEmpty(response.ErrorMessage))
			{
				Message += $" Ошибка: {response.ErrorMessage}";
			}
		}

		private async Task<RevenueServiceResponseDto> LoadFromRevenueService(DadataRequestDto request, CancellationToken cancellationToken)
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
