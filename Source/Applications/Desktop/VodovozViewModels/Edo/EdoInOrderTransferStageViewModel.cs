using QS.ViewModels;
using QS.ViewModels.Widgets.Pipeline;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Vodovoz.Core.Data.Repositories;
using Vodovoz.Core.Domain.Edo;

namespace Vodovoz.ViewModels.Edo
{
	public class EdoInOrderTransferStageViewModel : WidgetViewModelBase 
	{
		private IList<EdoInOrderTransferRowViewModel> _transfers;
		private EdoInOrderTransferRowViewModel _selectedTransfer;
		private PipelineViewModel _pipelineViewModel;
		private IList<string> _transferedCodes;

		public EdoInOrderTransferStageViewModel()
		{
		}

		public virtual IList<EdoInOrderTransferRowViewModel> Transfers
		{
			get => _transfers;
			set => SetField(ref _transfers, value);
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

		private void SelectTransfer()
		{
			PipelineViewModel = new PipelineViewModel();
			
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

				if(transferNode.TransferStage == EdoTransferTaskStage.Completed)
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
	}
}
