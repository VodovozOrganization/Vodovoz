using EdoService.Library;
using Gamma.Utilities;
using QS.Commands;
using QS.Dialog;
using QS.ViewModels;
using System.Collections.Generic;
using System.Linq;
using Vodovoz.Core.Data.Repositories;
using Vodovoz.Core.Domain.Results;

namespace Vodovoz.ViewModels.Edo
{
	public class EdoInOrderDocflowsStageViewModel : WidgetViewModelBase
	{
		private readonly IEnumerable<EdoInOrderTaxcomDocflowNode> _docflows;
		private readonly IEdoService _edoService;
		private readonly IInteractiveService _interactiveService;
		private IList<EdoInOrderTaxcomDocflowViewModel> _taxcomDocflows;
		private EdoInOrderTaxcomDocflowViewModel _selectedTaxcomDocflow;

		public EdoInOrderDocflowsStageViewModel(
			IEnumerable<EdoInOrderTaxcomDocflowNode> docflows, 
			IEdoService edoService,
			IInteractiveService interactiveService)
		{
			_docflows = docflows ?? throw new System.ArgumentNullException(nameof(docflows));
			_edoService = edoService ?? throw new System.ArgumentNullException(nameof(edoService));
			_interactiveService = interactiveService ?? throw new System.ArgumentNullException(nameof(interactiveService));

			var docflowNode = _docflows.FirstOrDefault();
			RefreshCommand = new DelegateCommand(() => UpdateDocflow(docflowNode));
			if(docflowNode is null)
			{
				RefreshButtonSensetive = false;
				return;
			}

			CreationTime = docflowNode.DocflowCreationTime.ToString("dd.MM.yyyy HH:mm");
			Status = docflowNode.DocflowStatus.GetEnumTitle();

			TaxcomDocflows = _docflows
				.Where(x => x.TaxcomDocflowSendTime != null)
				.Select(x => new EdoInOrderTaxcomDocflowViewModel(x)).ToList();

		}

		public string CreationTime { get; }

		public string Status { get; }

		public bool RefreshButtonSensetive { get; }

		public virtual IList<EdoInOrderTaxcomDocflowViewModel> TaxcomDocflows
		{
			get => _taxcomDocflows;
			set => SetField(ref _taxcomDocflows, value);
		}

		public virtual EdoInOrderTaxcomDocflowViewModel SelectedTaxcomDocflow
		{
			get => _selectedTaxcomDocflow;
			set => SetField(ref _selectedTaxcomDocflow, value);
		}

		public DelegateCommand RefreshCommand { get; set; }

		public void UpdateDocflow(EdoInOrderTaxcomDocflowNode node)
		{
			ShowResult(_edoService.UpdateDocflowStatus(node.TaskId, node.TaxcomDocflowId));
		}

		private void ShowResult(Result<string> result)
		{
			if(result.IsFailure)
			{
				_interactiveService.ShowMessage(
					ImportanceLevel.Warning,
					result.Errors.First().Message
				);
			}
			else
			{
				_interactiveService.ShowMessage(
					ImportanceLevel.Info,
					result.Value
				);
			}
		}
	}
}
