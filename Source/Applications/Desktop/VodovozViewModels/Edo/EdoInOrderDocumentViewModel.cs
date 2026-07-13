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
		private WidgetViewModelBase _stageViewModel;

		public EdoInOrderDocumentViewModel(
			EdoInOrderDocumentHistoryRowViewModel documentRowViewModel,
			PipelineViewModel pipelineViewModel,
			IEnumerable<EdoInOrderTransferNode> allTransfers
			)
		{
			_documentViewModel = documentRowViewModel ?? throw new ArgumentNullException(nameof(documentRowViewModel));
			_pipelineViewModel = pipelineViewModel ?? throw new ArgumentNullException(nameof(pipelineViewModel));
			_allTransfers = allTransfers ?? throw new ArgumentNullException(nameof(allTransfers));
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

			var isUpdTransfer = enumStage.Content.Equals(DocumentEdoTaskStage.Transfering);
			var isReceiptTransfer = enumStage.Content.Equals(EdoReceiptStatus.Transfering);
			var isTenderTransfer = enumStage.Content.Equals(TenderEdoTaskStage.Transfering);
			if(isUpdTransfer || isReceiptTransfer || isTenderTransfer)
			{
				var transferStageViewModel = new EdoInOrderTransferStageViewModel();
				transferStageViewModel.Transfers = _allTransfers
					.Where(x => x.OrderTaskId == _documentViewModel.Document.TaskId)
					.Select(x => new EdoInOrderTransferRowViewModel(x))
					.ToList();
				StageViewModel = transferStageViewModel;
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
