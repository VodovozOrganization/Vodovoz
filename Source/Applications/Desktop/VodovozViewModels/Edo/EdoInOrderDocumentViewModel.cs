using Microsoft.Extensions.DependencyInjection;
using QS.ViewModels;
using QS.ViewModels.Widgets.Pipeline;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
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
		private WidgetViewModelBase _stageViewModel;

		public EdoInOrderDocumentViewModel(
			EdoInOrderDocumentHistoryRowViewModel documentRowViewModel,
			PipelineViewModel pipelineViewModel,
			IEnumerable<EdoInOrderTransferNode> allTransfers,
			IEnumerable<EdoInOrderReceiptNode> allReceipts,
			IEnumerable<EdoInOrderTaxcomDocflowNode> allDocflows
			)
		{
			_documentViewModel = documentRowViewModel ?? throw new ArgumentNullException(nameof(documentRowViewModel));
			_pipelineViewModel = pipelineViewModel ?? throw new ArgumentNullException(nameof(pipelineViewModel));
			_allTransfers = allTransfers ?? throw new ArgumentNullException(nameof(allTransfers));
			_allReceipts = allReceipts ?? throw new ArgumentNullException(nameof(allReceipts));
			_allDocflows = allDocflows ?? throw new ArgumentNullException(nameof(allDocflows));
			_pipelineViewModel.PropertyChanged += PipelineOnPropertyChanged;
		}

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

			var transferStages = new Enum[] {
				DocumentEdoTaskStage.Transfering,
				EdoReceiptStatus.Transfering,
				TenderEdoTaskStage.Transfering
			};
			var isAnyTransfer = transferStages.Any(x => x.Equals(enumStage.Content));
			if(isAnyTransfer)
			{
				var transferStageViewModel = new EdoInOrderTransferStageViewModel(_allDocflows);
				transferStageViewModel.Transfers = _allTransfers
					.Where(x => x.OrderTaskId == _documentViewModel.Document.TaskId)
					.Select(x => new EdoInOrderTransferRowViewModel(x))
					.ToList();
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
				var docflowsStageViewModel = new EdoInOrderDocflowsStageViewModel(docflowsByTask);
				StageViewModel = docflowsStageViewModel;
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
