using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using QS.Commands;
using QS.Dialog;
using QS.Navigation;
using QS.ViewModels.Dialog;
using RevenueService.Client;
using RevenueService.Client.Dto;

namespace Vodovoz.ViewModels.ViewModels.Counterparty
{
	public class CounterpartyDetailsFromRevenueServiceViewModel : WindowDialogViewModelBase
	{
		private readonly IRevenueServiceClient _revenueServiceClient;
		private DelegateCommand _replaceDetailsCommand;
		private CounterpartyDto _selectedNode;

		public CounterpartyDetailsFromRevenueServiceViewModel(INavigationManager navigationManager, DadataRequestDto request, IRevenueServiceClient revenueServiceClient, CancellationToken cancellationToken)
			: base(navigationManager)
		{
			_revenueServiceClient = revenueServiceClient ?? throw new ArgumentNullException(nameof(revenueServiceClient));
			WindowPosition = WindowGravity.None;

			Nodes = LoadFromRevenueService(request, cancellationToken).Result
				.OrderBy(x => !x.IsActive).ToList();
		}

		public IList<CounterpartyDto> Nodes { get; set; }

		public CounterpartyDto SelectedNode
		{
			get => _selectedNode;
			set => SetField(ref _selectedNode, value);
		}

		public event EventHandler<CounterpartyDto> OnSelectResult;
		public string Message { get; set; }

		private async Task<IList<CounterpartyDto>> LoadFromRevenueService(DadataRequestDto request, CancellationToken cancellationToken)
		{
			var counterpartyDetails = await _revenueServiceClient.GetCounterpartyInfoAsync(request, cancellationToken);

			return counterpartyDetails;
		}

		public DelegateCommand ReplaceDetailsCommand =>
			_replaceDetailsCommand ?? (_replaceDetailsCommand = new DelegateCommand(() =>
				{
					OnSelectResult?.Invoke(this, SelectedNode);
					Close(false, CloseSource.ClosePage);
				}
			));
	}
}
