using EdoService.Library;
using QS.Dialog;
using QS.ViewModels;
using QS.ViewModels.Widgets.Pipeline;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using Vodovoz.Core.Data.Repositories;
using Vodovoz.Core.Domain.Edo;

namespace Vodovoz.ViewModels.Edo
{
	public class EdoInOrderTransferStageViewModel : WidgetViewModelBase 
	{
		private const string _transferNotRequiredMessage =
			"Трансфер кодов маркировки не требовался: в заказе нет кодов маркировки, "
			+ "либо все коды уже числились на организации, указанной в заказе.\n"
			+ "Документ перешёл к отправке без передачи кодов между организациями.";

		private const string _transferNotStartedMessage =
			"Необходимость трансфера будет определена на стадии «Распределение».";

		private const string _transferRequestsNotDistributedMessage =
			"Запросы на трансфер кодов сформированы и ожидают обработки. "
			+ "Данные о трансфере появятся после того, как запросы будут распределены.\n"
			+ "Если стадия отмечена проблемой, причину можно посмотреть на вкладке «Проблемы».";

		private readonly IEnumerable<EdoInOrderTaxcomDocflowNode> _allDocflows;
		private readonly IEdoService _edoService;
		private readonly IInteractiveService _interactiveService;
		private readonly StageStatus _transferStageStatus;
		private IList<EdoInOrderTransferRowViewModel> _transfers;
		private EdoInOrderTransferRowViewModel _selectedTransfer;
		private PipelineViewModel _pipelineViewModel;
		private IList<string> _transferedCodes;
		private WidgetViewModelBase _transferStageViewModel;
		private bool _hasTransfers;
		private string _noTransfersMessage;

		public EdoInOrderTransferStageViewModel(
			IEnumerable<EdoInOrderTaxcomDocflowNode> allDocflows, 
			IEdoService edoService,
			IInteractiveService interactiveService,
			StageStatus transferStageStatus)
		{
			_allDocflows = allDocflows ?? throw new ArgumentNullException(nameof(allDocflows));
			_edoService = edoService ?? throw new ArgumentNullException(nameof(edoService));
			_interactiveService = interactiveService ?? throw new ArgumentNullException(nameof(interactiveService));
			_transferStageStatus = transferStageStatus;

			UpdateNoTransfersState();
		}

		public virtual IList<EdoInOrderTransferRowViewModel> Transfers
		{
			get => _transfers;
			set
			{
				if(SetField(ref _transfers, value))
				{
					UpdateNoTransfersState();
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

		/// <summary>
		/// Есть ли по документу хотя бы один трансфер.
		/// Если нет - вместо таблиц трансферов и цепочки стадий трансфера показывается <see cref="NoTransfersMessage"/>
		/// </summary>
		public virtual bool HasTransfers
		{
			get => _hasTransfers;
			private set => SetField(ref _hasTransfers, value);
		}

		/// <summary>
		/// Пояснение, которое показывается вместо таблиц, если трансферов по документу нет.
		/// Зависит от статуса стадии «Трансфер»: не начата, в работе (или с проблемой), пройдена
		/// </summary>
		public virtual string NoTransfersMessage
		{
			get => _noTransfersMessage;
			private set => SetField(ref _noTransfersMessage, value);
		}

		private void UpdateNoTransfersState()
		{
			HasTransfers = _transfers?.Any() ?? false;
			NoTransfersMessage = HasTransfers
				? string.Empty
				: GetNoTransfersMessage(_transferStageStatus);
		}

		private static string GetNoTransfersMessage(StageStatus transferStageStatus)
		{
			switch(transferStageStatus)
			{
				case StageStatus.Completed:
					return _transferNotRequiredMessage;
				case StageStatus.NotStarted:
					return _transferNotStartedMessage;
				case StageStatus.InProgress:
				case StageStatus.Failed:
					return _transferRequestsNotDistributedMessage;
				default:
					throw new NotSupportedException($"Не поддерживаемый статус стадии: {transferStageStatus}");
			}
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

				var docflowsStageViewModel = new EdoInOrderDocflowsStageViewModel(
					docflowsByTask,
					_edoService,
					_interactiveService);

				TransferStageViewModel = docflowsStageViewModel;
				return;
			}

			if(EdoTransferTaskStage.WaitingRequests.Equals(enumStage.Content))
			{
				TransferStageViewModel = new EdoInOrderTransferWaitingRequestsStageViewModel();
				return;
			}

			if(EdoTransferTaskStage.PreparingToSend.Equals(enumStage.Content))
			{
				TransferStageViewModel = new EdoInOrderTransferPreparingStageViewModel();
				return;
			}

			TransferStageViewModel = null;
		}
	}
}
