using EdoService.Library;
using Microsoft.Extensions.DependencyInjection;
using QS.Dialog;
using QS.ViewModels;
using QS.ViewModels.Widgets.Pipeline;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Input;
using Vodovoz.Core.Data.Repositories;
using Vodovoz.Core.Domain.Edo;

namespace Vodovoz.ViewModels.Edo
{
	public class EdoInOrderDocumentViewModel : WidgetViewModelBase, IDisposable
	{
		private readonly EdoInOrderDocumentHistoryRowViewModel _documentViewModel;
		private readonly PipelineViewModel _pipelineViewModel;
		private readonly IEnumerable<EdoInOrderTransferNode> _allTransfers;
		private readonly IEnumerable<EdoInOrderReceiptNode> _allReceipts;
		private readonly IEnumerable<EdoInOrderTaxcomDocflowNode> _allDocflows;
		private readonly IEdoService _edoService;
		private readonly IInteractiveService _interactiveService;
		private WidgetViewModelBase _stageViewModel;

		public EdoInOrderDocumentViewModel(
			EdoInOrderDocumentHistoryRowViewModel documentRowViewModel,
			PipelineViewModel pipelineViewModel,
			IEnumerable<EdoInOrderTransferNode> allTransfers,
			IEnumerable<EdoInOrderReceiptNode> allReceipts,
			IEnumerable<EdoInOrderTaxcomDocflowNode> allDocflows,
			IEdoService edoService,
			IInteractiveService interactiveService)
		{
			_documentViewModel = documentRowViewModel ?? throw new ArgumentNullException(nameof(documentRowViewModel));
			_pipelineViewModel = pipelineViewModel ?? throw new ArgumentNullException(nameof(pipelineViewModel));
			_allTransfers = allTransfers ?? throw new ArgumentNullException(nameof(allTransfers));
			_allReceipts = allReceipts ?? throw new ArgumentNullException(nameof(allReceipts));
			_allDocflows = allDocflows ?? throw new ArgumentNullException(nameof(allDocflows));
			_edoService = edoService ?? throw new ArgumentNullException(nameof(edoService));

			_pipelineViewModel.PropertyChanged += PipelineOnPropertyChanged;
			_interactiveService = interactiveService ?? throw new ArgumentNullException(nameof(interactiveService));
			
			if(_pipelineViewModel.CurrentStage != null)
			{
				StageChanged();
			}
		}

		public ICommand EdoInOrderRefreshCommand { get; set; }

		public virtual WidgetViewModelBase StageViewModel
		{
			get => _stageViewModel;
			set => SetField(ref _stageViewModel, value);
		}

		private void PipelineOnPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if(e.PropertyName == nameof(PipelineViewModel.CurrentStage))
			{
				StageChanged();
			}
		}

		private void StageChanged()
		{
			var enumStage = _pipelineViewModel.CurrentStage as EnumPipelineStageViewModel;
			if(enumStage == null)
			{
				throw new NotSupportedException($"Поддерживается работа только с {nameof(EnumPipelineStageViewModel)}");
			}

			var distributionStages = new Enum[] {
				DocumentEdoTaskStage.New,
				EdoReceiptStatus.New,
				TenderEdoTaskStage.New
			};
			var isDistributionStages = distributionStages.Any(x => x.Equals(enumStage.Content));
			if(isDistributionStages)
			{
				StageViewModel = new EdoInOrderDistributionStageViewModel(_documentViewModel.DocumentType);
				return;
			}

			if(EdoReceiptStatus.SavedToPool.Equals(enumStage.Content))
			{
				StageViewModel = new EdoInOrderSavedToPoolStageViewModel();
				return;
			}

			var transferStages = new Enum[] {
				DocumentEdoTaskStage.Transfering,
				EdoReceiptStatus.Transfering,
				TenderEdoTaskStage.Transfering
			};
			var isAnyTransfer = transferStages.Any(x => x.Equals(enumStage.Content));
			if(isAnyTransfer)
			{
				var transferStageViewModel = new EdoInOrderTransferStageViewModel(
					_allDocflows,
					_edoService,
					_interactiveService,
					enumStage.Status)
				{
					Transfers = _allTransfers
						.Where(x => x.OrderTaskId == _documentViewModel.Document.TaskId)
						.Select(x => new EdoInOrderTransferRowViewModel(x))
						.ToList()
				};
				StageViewModel = transferStageViewModel;
				return;
			}

			var receiptSentStages = new Enum[] {
				EdoReceiptStatus.Sending,
				EdoReceiptStatus.Sent,
				EdoReceiptStatus.Completed
			};
			var isReceiptSentStages = receiptSentStages.Any(x => x.Equals(enumStage.Content));
			if(isReceiptSentStages)
			{
				var receiptStageViewModel = new EdoInOrderReceiptSendStageViewModel(_allReceipts);
				StageViewModel = receiptStageViewModel;
				return;
			}

			var docflowsStages = new Enum[] {
				DocumentEdoTaskStage.Sending,
				DocumentEdoTaskStage.Sent,
				DocumentEdoTaskStage.Completed
			};
			var isDocflowsStages = docflowsStages.Any(x => x.Equals(enumStage.Content));
			if(isDocflowsStages)
			{
				var docflowsByTask = _allDocflows
					.Where(x => x.TaskId == _documentViewModel.Document.TaskId)
					.ToList();

				var docflowsStageViewModel = new EdoInOrderDocflowsStageViewModel(
					docflowsByTask,
					_edoService,
					_interactiveService)
				{
					EdoInOrderRefreshCommand = EdoInOrderRefreshCommand
				};

				StageViewModel = docflowsStageViewModel;
				return;
			}

			if(TenderEdoTaskStage.Sending.Equals(enumStage.Content))
			{
				StageViewModel = new EdoInOrderTenderSendingStageViewModel();
				return;
			}

			if(TenderEdoTaskStage.ManualUploaded.Equals(enumStage.Content))
			{
				StageViewModel = new EdoInOrderTenderManualUploadedStageViewModel();
				return;
			}

			StageViewModel = null;
		}

		public void Dispose()
		{
			_pipelineViewModel.PropertyChanged -= PipelineOnPropertyChanged;
		}
	}
}
