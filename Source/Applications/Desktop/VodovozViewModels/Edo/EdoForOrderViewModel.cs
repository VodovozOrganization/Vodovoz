using Gamma.Utilities;
using QS.Commands;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QS.Extensions.Observable.Collections.List;
using QS.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using Vodovoz.Core.Data.Repositories;
using Vodovoz.Domain.Orders;

namespace Vodovoz.ViewModels.Edo
{

	public class EdoForOrderViewModel : WidgetViewModelBase
	{
		private readonly IEdoRepository _edoRepository;
		private Order _order;
		private IUnitOfWork _uow;
		private IObservableList<EdoTaskInOrderViewModel> _edoTasks;
		private EdoTaskInOrderViewModel _selectedTask;
		private IObservableList<TransferEdoTaskInOrderViewModel> _transferEdoTasks;
		private TransferEdoTaskInOrderViewModel _selectedTransferTask;
		private IEnumerable<TransferEdoTaskNode> _allTransferEdoTasks;
		private IObservableList<EdoProblemInOrderViewModel> _problems;
		private EdoProblemInOrderViewModel _selectedProblem;
		private IEnumerable<EdoProblemNode> _allProblems;
		private string _problemDescription;
		private string _problemRecommendation;

		public EdoForOrderViewModel(
			IEdoRepository edoRepository,
			EdoTaskInOrderResolveViewModel edoTaskInOrderResolveViewModel
		)
		{
			_edoRepository = edoRepository ?? throw new ArgumentNullException(nameof(edoRepository));
			EdoTaskInOrderResolveViewModel = edoTaskInOrderResolveViewModel ?? throw new ArgumentNullException(nameof(edoTaskInOrderResolveViewModel));
			_edoTasks = new ObservableList<EdoTaskInOrderViewModel>();
			_transferEdoTasks = new ObservableList<TransferEdoTaskInOrderViewModel>();
			_allTransferEdoTasks = new List<TransferEdoTaskNode>();
			_allProblems = new List<EdoProblemNode>();

			RefreshCommand = new DelegateCommand(Refresh);
		}

		public EdoTaskInOrderResolveViewModel EdoTaskInOrderResolveViewModel { get; }

		public ICommand RefreshCommand { get; }

		public void Setup(IUnitOfWork uow, Order order)
		{
			_uow = uow ?? throw new ArgumentNullException(nameof(uow));
			_order = order ?? throw new ArgumentNullException(nameof(order));
			Refresh();
		}

		private void Refresh()
		{
			//SelectedProblem = null;
			//SelectedTransferTask = null;
			//SelectedTask = null;
			var edoTasks = _edoRepository.GetEdoTasksForOrder(_uow, _order.Id);
			var edoTaskViewModels = edoTasks.Select(x => new EdoTaskInOrderViewModel(x));
			EdoTasks = new ObservableList<EdoTaskInOrderViewModel>(edoTaskViewModels);

			_allTransferEdoTasks = _edoRepository.GetTransferEdoTasksForOrder(_uow, _order.Id);
			_allProblems = _edoRepository.GetEdoProblemsForOrder(_uow, _order.Id);

			EdoTaskInOrderResolveViewModel.Reload(_uow, _order.Id);
		}

		public virtual IObservableList<EdoTaskInOrderViewModel> EdoTasks
		{
			get => _edoTasks;
			private set => SetField(ref _edoTasks, value);
		}

		public virtual EdoTaskInOrderViewModel SelectedTask
		{
			get => _selectedTask;
			set
			{
				if(SetField(ref _selectedTask, value))
				{
					MatchTransferTasks();
					FilterByOrderTask();
				}
			}
		}

		public virtual IObservableList<TransferEdoTaskInOrderViewModel> TransferEdoTasks
		{
			get => _transferEdoTasks;
			set => SetField(ref _transferEdoTasks, value);
		}

		public virtual TransferEdoTaskInOrderViewModel SelectedTransferTask
		{
			get => _selectedTransferTask;
			set
			{
				if(SetField(ref _selectedTransferTask, value))
				{
					FilterByTransferTask();
				}
			}
		}

		private void MatchTransferTasks()
		{
			if(SelectedTask == null)
			{
				TransferEdoTasks = new ObservableList<TransferEdoTaskInOrderViewModel>();
				return;
			}

			var viewModels = _allTransferEdoTasks
				.Where(x => x.OrderTaskId == SelectedTask.TaskId)
				.Select(x => new TransferEdoTaskInOrderViewModel(x))
				;

			TransferEdoTasks = new ObservableList<TransferEdoTaskInOrderViewModel>(viewModels);
		}

		public virtual IObservableList<EdoProblemInOrderViewModel> Problems
		{
			get => _problems;
			set => SetField(ref _problems, value);
		}

		public virtual EdoProblemInOrderViewModel SelectedProblem
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

		private void ShowProblemDetails()
		{
			if(SelectedProblem == null)
			{
				ProblemDescription = null;
				ProblemRecommendation = null;
			}
			else
			{
				ProblemDescription = SelectedProblem.Description;
				ProblemRecommendation = SelectedProblem.Recomendation;
			}
		}


		private void FilterByOrderTask()
		{
			if(SelectedTask == null)
			{
				if(SelectedTransferTask == null)
				{
					Problems = new ObservableList<EdoProblemInOrderViewModel>();
					EdoTaskInOrderResolveViewModel.Clear();
				}
				return;
			}

			var viewModels = _allProblems
				.Where(x => x.OrderTaskId == SelectedTask.TaskId)
				.Select(x => new EdoProblemInOrderViewModel(x))
				;

			Problems = new ObservableList<EdoProblemInOrderViewModel>(viewModels);

			EdoTaskInOrderResolveViewModel.FilterByOrderTask(SelectedTask.TaskId);
		}

		private void FilterByTransferTask()
		{
			if(SelectedTransferTask == null)
			{
				if(SelectedTask == null)
				{
					Problems = new ObservableList<EdoProblemInOrderViewModel>();
					EdoTaskInOrderResolveViewModel.Clear();
				}
				return;
			}

			var viewModels = _allProblems
				.Where(x => x.TransferTaskId == SelectedTransferTask.TaskId)
				.Select(x => new EdoProblemInOrderViewModel(x))
				;

			Problems = new ObservableList<EdoProblemInOrderViewModel>(viewModels);

			EdoTaskInOrderResolveViewModel.FilterByTransferTask(SelectedTransferTask.TaskId);
		}
	}

	public class EdoTaskInOrderViewModel : ViewModelBase
	{
		public EdoTaskInOrderViewModel(OrderEdoTaskNode edoTaskNode)
		{
			EdoTaskNode = edoTaskNode ?? throw new ArgumentNullException(nameof(edoTaskNode));
			RequestTime = EdoTaskNode.RequestTime.ToString("dd.MM.yyyy HH.mm");
			Initiator = EdoTaskNode.RequestSource.GetEnumTitle();
			TaskId = EdoTaskNode.EdoTaskId;
			TaskType = EdoTaskNode.EdoTaskType.GetEnumTitle();
			Status = EdoTaskNode.EdoTaskStatus.GetEnumTitle();
		}

		public OrderEdoTaskNode EdoTaskNode { get; }

		public string RequestTime { get; }
		public string Initiator { get; }
		public int TaskId { get; }
		public string TaskType { get; }
		public string Status { get; }
	}

	public class TransferEdoTaskInOrderViewModel : ViewModelBase
	{
		public TransferEdoTaskInOrderViewModel(TransferEdoTaskNode transferTaskNode)
		{
			TransferTaskNode = transferTaskNode ?? throw new ArgumentNullException(nameof(transferTaskNode));
			RequestTime = TransferTaskNode.RequestTime.ToString("dd.MM.yyyy HH.mm");
			TaskId = TransferTaskNode.TransferTaskId;
			From = TransferTaskNode.OrganizationFrom;
			To = TransferTaskNode.OrganizationTo;
			Status = TransferTaskNode.Status.GetEnumTitle();
		}

		public TransferEdoTaskNode TransferTaskNode { get; }

		public string RequestTime { get; }
		public int TaskId { get; }
		public string From { get; }
		public string To { get; }
		public string Status { get; }

	}

	public class EdoProblemInOrderViewModel : ViewModelBase
	{
		public EdoProblemInOrderViewModel(EdoProblemNode problemNode)
		{
			ProblemNode = problemNode ?? throw new ArgumentNullException(nameof(problemNode));
			CreationTime = ProblemNode.Time.ToString("dd.MM.yyyy HH.mm");
			State = ProblemNode.State.GetEnumTitle();
			Message = ProblemNode.Message;
			Description = ProblemNode.Description;
			Recomendation = ProblemNode.Recommendation;
		}

		public EdoProblemNode ProblemNode { get; }

		public string CreationTime { get; }
		public string State { get; }
		public string Message { get; }
		public string Description { get; }
		public string Recomendation { get; }
	}

	public class EdoTaskInOrderResolveViewModel : WidgetViewModelBase
	{
		private readonly EdoDocflowsInOrderViewModel _edoDocflowsInOrderViewModel;
		private ViewModelBase _docsContentViewModel;

		public EdoTaskInOrderResolveViewModel(
			EdoDocflowsInOrderViewModel edoDocflowsInOrderViewModel
		)
		{
			_edoDocflowsInOrderViewModel = edoDocflowsInOrderViewModel ?? throw new ArgumentNullException(nameof(edoDocflowsInOrderViewModel));
		}

		public void Reload(IUnitOfWork uow, int orderId)
		{
			_edoDocflowsInOrderViewModel.Reload(uow, orderId);
			Clear();
		}

		public virtual ViewModelBase DocsContentViewModel
		{
			get => _docsContentViewModel;
			set => SetField(ref _docsContentViewModel, value);
		}


		public void FilterByOrderTask(int orderTaskId)
		{
			_edoDocflowsInOrderViewModel.FilterByOrderTask(orderTaskId);
			DocsContentViewModel = _edoDocflowsInOrderViewModel;
		}

		public void FilterByTransferTask(int transferTaskId)
		{
			_edoDocflowsInOrderViewModel.FilterByTransferTask(transferTaskId);
			DocsContentViewModel = _edoDocflowsInOrderViewModel;
		}

		public void Clear()
		{
			DocsContentViewModel = null;
		}
	}

	public class EdoDocflowsInOrderViewModel : WidgetViewModelBase
	{
		private readonly IEdoRepository _edoRepository;
		private IObservableList<EdoDocflowRowInOrderViewModel> _docflows;
		private EdoDocflowRowInOrderViewModel _selectedDocflow;
		private IEnumerable<EdoDocflowForOrderNode> _allDocflows;

		public EdoDocflowsInOrderViewModel(IEdoRepository edoRepository)
		{
			_edoRepository = edoRepository ?? throw new ArgumentNullException(nameof(edoRepository));
			_docflows = new ObservableList<EdoDocflowRowInOrderViewModel>();
			_allDocflows = new List<EdoDocflowForOrderNode>();

			var resendCommand = new DelegateCommand(Resend, () => CanResend);
			resendCommand.CanExecuteChangedWith(this, x => x.CanResend);
			ResendCommand = resendCommand;
		}

		public ICommand ResendCommand { get; }

		public void Reload(IUnitOfWork uow, int orderId)
		{
			_allDocflows = _edoRepository.GetEdoDocflowsForOrder(uow, orderId);
			Docflows = new ObservableList<EdoDocflowRowInOrderViewModel>();
		}

		public virtual IObservableList<EdoDocflowRowInOrderViewModel> Docflows
		{
			get => _docflows;
			set => SetField(ref _docflows, value);
		}

		[PropertyChangedAlso(nameof(CanResend))]
		public virtual EdoDocflowRowInOrderViewModel SelectedDocflow
		{
			get => _selectedDocflow;
			set => SetField(ref _selectedDocflow, value);
		}

		public void FilterByOrderTask(int orderTaskId)
		{
			var viewModels = _allDocflows
					.Where(x => x.OrderTaskId == orderTaskId)
					.Select(x => new EdoDocflowRowInOrderViewModel(x));
			Docflows = new ObservableList<EdoDocflowRowInOrderViewModel>(viewModels);
		}

		public void FilterByTransferTask(int transferTaskId)
		{
			var viewModels = _allDocflows
					.Where(x => x.TransferTaskId == transferTaskId)
					.Select(x => new EdoDocflowRowInOrderViewModel(x));
			Docflows = new ObservableList<EdoDocflowRowInOrderViewModel>(viewModels);
		}

		public bool CanResend => SelectedDocflow != null;

		private void Resend()
		{

		}
	}

	public class EdoDocflowRowInOrderViewModel : ViewModelBase
	{
		public EdoDocflowRowInOrderViewModel(EdoDocflowForOrderNode docflowNode)
		{
			DocflowNode = docflowNode ?? throw new ArgumentNullException(nameof(docflowNode));
			EdoProvider = DocflowNode.EdoType.GetEnumTitle();
			TaxcomDocumentId = DocflowNode.TaxcomDocumentId;
			TaxcomDocflowId = DocflowNode.TaxcomDocflowId;
			TaxcomDocflowStatus = DocflowNode.TaxcomDocflowStatus.GetEnumTitle();
		}

		public EdoDocflowForOrderNode DocflowNode { get; set; }

		public string EdoProvider { get; }
		public int TaxcomDocumentId { get; }
		public string TaxcomDocflowId { get; }
		public string TaxcomDocflowStatus { get; }

	}
}
