using QS.ViewModels;
using QS.ViewModels.Widgets.Pipeline;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using Vodovoz.Core.Data.Repositories;
using Vodovoz.Core.Domain.Edo;
using Vodovoz.ViewModels.Counterparties;

namespace Vodovoz.ViewModels.Edo
{
	public class EdoInOrderTransferStageViewModel : WidgetViewModelBase 
	{
		private IList<EdoInOrderTransferRowViewModel> _transfers;
		private EdoInOrderTransferRowViewModel _selectedTransfer;
		private PipelineViewModel _pipelineViewModel;
		private IList<string> _transferedCodes;
		private WidgetViewModelBase _transferStageViewModel;
		private readonly IEnumerable<EdoInOrderTaxcomDocflowNode> _allDocflows;

		public EdoInOrderTransferStageViewModel(
			IEnumerable<EdoInOrderTaxcomDocflowNode> allDocflows
			)
		{
			_allDocflows = allDocflows ?? throw new ArgumentNullException(nameof(allDocflows));
		}

		public virtual IList<EdoInOrderTransferRowViewModel> Transfers
		{
			get => _transfers;
			set
			{
				if(SetField(ref _transfers, value))
				{
					SelectedTransfer = _transfers?.FirstOrDefault();
				}
			}
		}

		public virtual EdoInOrderTransferRowViewModel SelectedTransfer
		{
			get => _selectedTransfer;
			set
			{
				if(SetField(ref _selectedTransfer, value))
				{
					SelectTransfer();
				}
			}
		}

		public virtual PipelineViewModel PipelineViewModel
		{
			get => _pipelineViewModel;
			set => SetField(ref _pipelineViewModel, value);
		}

		public virtual IList<string> TransferedCodes
		{
			get => _transferedCodes;
			set => SetField(ref _transferedCodes, value);
		}

		public virtual WidgetViewModelBase TransferStageViewModel
		{
			get => _transferStageViewModel;
			set => SetField(ref _transferStageViewModel, value);
		}


		private void SelectTransfer()
		{
			if(PipelineViewModel != null)
			{
				_pipelineViewModel.PropertyChanged -= PipelineOnPropertyChanged;
			}

			PipelineViewModel = new PipelineViewModel();
			PipelineViewModel.PropertyChanged += PipelineOnPropertyChanged;

			if(SelectedTransfer == null)
			{
				TransferedCodes = new List<string>();
				return;
			}

			CreateStages(SelectedTransfer.Node, PipelineViewModel);
			TransferedCodes = SelectedTransfer.Node.TransferedCodes;
		}

		private void CreateStages(
			EdoInOrderTransferNode transferNode,
			PipelineViewModel pipelineViewModel
			)
		{
			pipelineViewModel.Title = "Стадии отправки УПД для трансфера";
			var stageViewModels = new ObservableCollection<PipelineStageViewModel>();
			var transferValues = Enum.GetValues(typeof(EdoTransferTaskStage))
				.Cast<EdoTransferTaskStage>();

			foreach(var enumValue in transferValues)
			{
				var pipelineStageViewModel = new EnumPipelineStageViewModel(enumValue);

				if(enumValue == EdoTransferTaskStage.Completed &&
					transferNode.TransferStage == EdoTransferTaskStage.Completed)
				{
					pipelineStageViewModel.Status = StageStatus.Completed;
					stageViewModels.Add(pipelineStageViewModel);
					continue;
				}

				if(enumValue == transferNode.TransferStage)
				{
					if(transferNode.Status == EdoTaskStatus.Problem)
					{
						pipelineStageViewModel.UpperTitle = "Проблема";
						pipelineStageViewModel.Status = StageStatus.Failed;
					}
					else
					{
						pipelineStageViewModel.Status = StageStatus.InProgress;
					}
					stageViewModels.Add(pipelineStageViewModel);
					continue;
				}

				if(enumValue < transferNode.TransferStage)
				{
					pipelineStageViewModel.Status = StageStatus.Completed;
				}
				else
				{
					pipelineStageViewModel.Status = StageStatus.NotStarted;
				}
				stageViewModels.Add(pipelineStageViewModel);
			}

			pipelineViewModel.Stages = stageViewModels;
			if(stageViewModels.Any())
			{
				stageViewModels.First().Active = true;
			}
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
			var enumStage = PipelineViewModel.CurrentStage as EnumPipelineStageViewModel;
			if(enumStage == null)
			{
				throw new NotSupportedException($"Поддерживается работа только с {nameof(EnumPipelineStageViewModel)}");
			}

			var docflowsStages = new Enum[] {
				EdoTransferTaskStage.ReadyToSend,
				EdoTransferTaskStage.InProgress,
				EdoTransferTaskStage.Completed
			};
			var isDocflowsStages = docflowsStages.Any(x => x.Equals(enumStage.Content));
			if(isDocflowsStages)
			{
				var docflowsByTask = _allDocflows
					.Where(x => x.TaskId == SelectedTransfer.Node.TransferTaskId)
					.ToList();
				var docflowsStageViewModel = new EdoInOrderDocflowsStageViewModel(docflowsByTask);
				TransferStageViewModel = docflowsStageViewModel;
				return;
			}

			TransferStageViewModel = null;
		}
	}
}
