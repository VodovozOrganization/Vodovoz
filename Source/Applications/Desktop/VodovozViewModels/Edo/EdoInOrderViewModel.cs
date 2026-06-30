using Gamma.Utilities;
using QS.Commands;
using QS.DomainModel.UoW;
using QS.ViewModels;
using QS.ViewModels.Widgets.Pipeline;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Input;
using Vodovoz.Core.Data.Repositories;
using Vodovoz.Core.Domain.Edo;
using Vodovoz.ViewModels.TrueMark;

namespace Vodovoz.ViewModels.Edo
{
	public class EdoInOrderViewModel : WidgetViewModelBase
	{
		private readonly IEdoRepository _edoRepository;
		private int _orderId;
		private IUnitOfWork _uow;
		private bool _loaded;
		private IList<EdoInOrderDocumentTypeViewModel> _documentGroupTypes;
		private EdoInOrderDocumentTypeViewModel _selectedDocumentType;
		private IList<EdoInOrderDocumentHistoryRowViewModel> _allDocuments;
		private IList<EdoInOrderDocumentHistoryRowViewModel> _documents;
		private EdoInOrderDocumentHistoryRowViewModel _selectedDocument;
		private EdoInOrderDocumentViewModel _documentViewModel;
		private PipelineViewModel _pipelineViewModel;
		private IList<EdoInOrderProblemViewModel> _problems;
		private EdoInOrderProblemViewModel _selectedProblem;
		private IEnumerable<EdoInOrderProblemNode> _allProblems;
		private IEnumerable<EdoInOrderTransferNode> _allTransfers;
		private bool _hasProblems;
		private string _problemDescription;
		private string _problemRecommendation;
		private IList<string> _problemItems;

		public EdoInOrderViewModel(
			IEdoRepository edoRepository,
			OrderCodesViewModel orderCodesViewModel
			)
		{
			_documents = new List<EdoInOrderDocumentHistoryRowViewModel>();
			_edoRepository = edoRepository ?? throw new System.ArgumentNullException(nameof(edoRepository));
			OrderCodesViewModel = orderCodesViewModel ?? throw new ArgumentNullException(nameof(orderCodesViewModel));

			_allProblems = new List<EdoInOrderProblemNode>();

			RefreshCommnand = new DelegateCommand(Refresh);
		}

		public ICommand RefreshCommnand { get; }
		public OrderCodesViewModel OrderCodesViewModel { get; set; }

		public virtual IList<EdoInOrderDocumentTypeViewModel> DocumentGroupTypes
		{
			get => _documentGroupTypes;
			set => SetField(ref _documentGroupTypes, value);
		}

		public virtual EdoInOrderDocumentTypeViewModel SelectedDocumentGroupType
		{
			get => _selectedDocumentType;
			set
			{
				if(SetField(ref _selectedDocumentType, value))
				{
					SelectDocumentsByGroup();
				}
			}
		}

		public virtual IList<EdoInOrderDocumentHistoryRowViewModel> Documents
		{
			get => _documents;
			set => SetField(ref _documents, value);
		}

		public virtual EdoInOrderDocumentHistoryRowViewModel SelectedDocument
		{
			get => _selectedDocument;
			set
			{
				if(SetField(ref _selectedDocument, value))
				{
					SelectDocument();
					SelectProblemsByDocument();
				}
			}
		}

		public virtual EdoInOrderDocumentViewModel DocumentViewModel
		{
			get => _documentViewModel;
			private set => SetField(ref _documentViewModel, value);
		}

		public virtual PipelineViewModel PipelineViewModel
		{
			get => _pipelineViewModel;
			private set => SetField(ref _pipelineViewModel, value);
		}

		public virtual IList<EdoInOrderProblemViewModel> Problems
		{
			get => _problems;
			set => SetField(ref _problems, value);
		}

		public virtual EdoInOrderProblemViewModel SelectedProblem
		{
			get => _selectedProblem;
			set
			{
				if(SetField(ref _selectedProblem, value))
				{
					ShowProblemDetails();
				}
			}
		}

		public virtual bool HasProblems
		{
			get => _hasProblems;
			set => SetField(ref _hasProblems, value);
		}

		public virtual string ProblemDescription
		{
			get => _problemDescription;
			set => SetField(ref _problemDescription, value);
		}

		public virtual string ProblemRecommendation
		{
			get => _problemRecommendation;
			set => SetField(ref _problemRecommendation, value);
		}

		public virtual IList<string> ProblemItems
		{
			get => _problemItems;
			set => SetField(ref _problemItems, value);
		}


		public virtual void Setup(IUnitOfWork uow, int orderId)
		{
			_orderId = orderId;
			_uow = uow ?? throw new System.ArgumentNullException(nameof(uow));
		}

		public virtual void Load()
		{
			if(_loaded)
			{
				return;
			}

			Refresh();

			_loaded = true;
		}

		private void Refresh()
		{
			var documents = _edoRepository.GetEdoInOrderDocuments(_uow, _orderId);
			_allDocuments = documents.Select(x => new EdoInOrderDocumentHistoryRowViewModel(x)).ToList();


			var documentGroupTypes = Enum.GetValues(typeof(EdoInOrderDocumentGroupType))
				.Cast<EdoInOrderDocumentGroupType>()
				.Select(x => new EdoInOrderDocumentTypeViewModel(x))
				.ToList();

			var documentsByGroupType = _allDocuments.GroupBy(x => x.DocumentGroupType)
				.ToDictionary(key => key.Key, value => value.Count());
			foreach(var documentGroupType in documentGroupTypes)
			{
				if(documentsByGroupType.TryGetValue(documentGroupType.DocumentGroupType, out int docsQuantity))
				{
					documentGroupType.Quantity = docsQuantity;
				}
			}

			DocumentGroupTypes = documentGroupTypes
				.OrderByDescending(x => x.Quantity)
				.ThenBy(x => (int)x.DocumentGroupType)
				.ToList();

			OrderCodesViewModel.OrderId = _orderId;
			OrderCodesViewModel.RefreshCommand.Execute(null);

			_allProblems = _edoRepository.GetEdoProblemsForOrder(_uow, _orderId);
			_allTransfers = _edoRepository.GetTransferEdoTasksForOrder(_uow, _orderId);
		}

		private void SelectDocumentsByGroup()
		{
			if(SelectedDocumentGroupType == null)
			{
				Documents = new List<EdoInOrderDocumentHistoryRowViewModel>();
				return;
			}

			Documents = _allDocuments
				.Where(x => x.DocumentGroupType == SelectedDocumentGroupType.DocumentGroupType)
				.ToList();
		}

		private void SelectDocument()
		{
			if(SelectedDocument == null)
			{
				PipelineViewModel = new PipelineViewModel();
				DocumentViewModel = null;
				return;
			}

			PipelineViewModel = CreateStages(SelectedDocument);
			DocumentViewModel = new EdoInOrderDocumentViewModel(
				SelectedDocument,
				PipelineViewModel,
				_allTransfers
			);
		}

		private PipelineViewModel CreateStages(EdoInOrderDocumentHistoryRowViewModel historyRow)
		{
			var pipelineViewModel = new PipelineViewModel();

			switch(historyRow.DocumentType)
			{
				case EdoInOrderDocumentType.Upd:
					CreateUpdStages(historyRow.Document, pipelineViewModel);
					break;
				case EdoInOrderDocumentType.Receipt:
					CreateReceiptStages(historyRow.Document, pipelineViewModel);
					break;
				case EdoInOrderDocumentType.Tender:
					CreateTenderStages(historyRow.Document, pipelineViewModel);
					break;
				case EdoInOrderDocumentType.Withdrawal:
				case EdoInOrderDocumentType.SaveCode:
				case EdoInOrderDocumentType.Bill:
				default:
					// нет стадий
					return pipelineViewModel;
			}

			return pipelineViewModel;
		}

		private void CreateUpdStages(
			EdoInOrderDocumentNode document,
			PipelineViewModel pipelineViewModel
			)
		{
			if(document.TaskUpdStage == null)
			{
				return;
			}

			pipelineViewModel.Title = "Стадии отправки УПД";
			var stageViewModels = new ObservableCollection<PipelineStageViewModel>();
			var updValues = Enum.GetValues(typeof(DocumentEdoTaskStage))
				.Cast<DocumentEdoTaskStage>();

			foreach(var enumValue in updValues)
			{
				var pipelineStageViewModel = new EnumPipelineStageViewModel(enumValue);

				if(enumValue == document.TaskUpdStage.Value)
				{
					if(document.TaskStatus == EdoTaskStatus.Problem)
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

				if(enumValue < document.TaskUpdStage.Value)
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

		private void CreateReceiptStages(
			EdoInOrderDocumentNode document,
			PipelineViewModel pipelineViewModel
			)
		{
			if(document.TaskReceiptStage == null)
			{
				return;
			}

			pipelineViewModel.Title = "Стадии отправки чека";
			var stageViewModels = new ObservableCollection<PipelineStageViewModel>();
			var updValues = Enum.GetValues(typeof(EdoReceiptStatus))
				.Cast<EdoReceiptStatus>();

			foreach(var enumValue in updValues)
			{
				var pipelineStageViewModel = new EnumPipelineStageViewModel(enumValue);

				if(enumValue == EdoReceiptStatus.SavedToPool)
				{
					if(document.TaskReceiptStage.Value == EdoReceiptStatus.SavedToPool)
					{
						pipelineStageViewModel.Status = StageStatus.Completed;
						stageViewModels.Add(pipelineStageViewModel);
						break;
					}
					else
					{
						pipelineStageViewModel.Status = StageStatus.NotStarted;
						stageViewModels.Add(pipelineStageViewModel);
						continue;
					}
				}

				if(enumValue == document.TaskReceiptStage.Value)
				{
					if(document.TaskStatus == EdoTaskStatus.Problem)
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

				if(enumValue < document.TaskReceiptStage.Value)
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

		private void CreateTenderStages(
			EdoInOrderDocumentNode document,
			PipelineViewModel pipelineViewModel
			)
		{
			if(document.TaskTenderStage == null)
			{
				return;
			}

			pipelineViewModel.Title = "Стадии отправки тендера";
			var stageViewModels = new ObservableCollection<PipelineStageViewModel>();
			var updValues = Enum.GetValues(typeof(TenderEdoTaskStage))
				.Cast<TenderEdoTaskStage>();

			foreach(var enumValue in updValues)
			{
				var pipelineStageViewModel = new EnumPipelineStageViewModel(enumValue);

				if(enumValue == document.TaskTenderStage.Value)
				{
					if(document.TaskStatus == EdoTaskStatus.Problem)
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

				if(enumValue < document.TaskTenderStage.Value)
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

		private void SelectProblemsByDocument()
		{
			if(_selectedDocument == null)
			{
				HasProblems = false;
				Problems = new List<EdoInOrderProblemViewModel>();
				return;
			}

			var viewModels = _allProblems
				.Where(x => x.OrderTaskId == _selectedDocument.Document.TaskId)
				.Select(x => new EdoInOrderProblemViewModel(x))
				;

			Problems = new List<EdoInOrderProblemViewModel>(viewModels);
			HasProblems = true;
		}

		private void ShowProblemDetails()
		{
			if(SelectedProblem == null)
			{
				ProblemDescription = null;
				ProblemRecommendation = null;
				ProblemItems = new List<string>();
			}
			else
			{
				ProblemDescription = SelectedProblem.Description;
				ProblemRecommendation = SelectedProblem.Recomendation;
				ProblemItems = SelectedProblem.ProblemItems;
			}
		}










	}

	public class EnumPipelineStageViewModel : PipelineStageViewModel
	{
		public EnumPipelineStageViewModel(Enum content)
		{
			Content = content ?? throw new ArgumentNullException(nameof(content));

			Id = content.ToString();
			Name = content.GetEnumTitle();
		}

		public Enum Content { get; }
	}
}
