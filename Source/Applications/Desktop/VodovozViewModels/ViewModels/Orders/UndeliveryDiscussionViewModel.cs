using QS.Commands;
using QS.Dialog;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QS.Services;
using QS.ViewModels;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Orders;
using Vodovoz.Services;

namespace Vodovoz.ViewModels.Orders
{
	public class UndeliveryDiscussionViewModel : EntityWidgetViewModelBase<UndeliveryDiscussion>
	{
		private readonly bool _canCompleteUndeliveryDiscussionPermission;
		private readonly IPermissionResult _undeliveryPermissionResult;
		private string _newCommentText;

		public UndeliveryDiscussionViewModel(
			UndeliveryDiscussion undeliveryDiscussion,
			IEmployeeService employeeService,
			ICommonServices commonServices,
			IUnitOfWork uow)
			: base(undeliveryDiscussion, commonServices)
		{
			_undeliveryPermissionResult = commonServices.CurrentPermissionService.ValidateEntityPermission(typeof(UndeliveredOrder));
			_canCompleteUndeliveryDiscussionPermission = CommonServices.CurrentPermissionService.ValidatePresetPermission(
				Vodovoz.Permissions.Order.UndeliveredOrder.CanCompleteUndeliveryDiscussion);

			UoW = uow;
			CurrentEmployee = employeeService.GetEmployeeForUser(UoW, CommonServices.UserService.CurrentUserId);
			CreateCommands();
			ConfigureEntityPropertyChanges();
		}

		private void ConfigureEntityPropertyChanges()
		{
			SetPropertyChangeRelation(e => e.Status, () => CanEditStatus);
		}

		private void CreateCommands()
		{
			CreateAddCommentCommand();
		}

		private void CreateAddCommentCommand()
		{
			AddCommentCommand = new DelegateCommand(
				() =>
				{
					var newComment = new UndeliveryDiscussionComment();
					if(CurrentEmployee == null)
					{
						CommonServices.InteractiveService.ShowMessage(ImportanceLevel.Warning, "Невозможно добавить комментарий так как к вашему пользователю не привязан сотрудник");
						return;
					}

					newComment.Author = CurrentEmployee;
					newComment.Comment = NewCommentText;
					newComment.UndeliveryDiscussion = Entity;
					Entity.ObservableComments.Add(newComment);
					NewCommentText = string.Empty;
				},
				() => CanAddComment
			);

			AddCommentCommand.CanExecuteChangedWith(this, x => x.CanAddComment);
		}

		public Employee CurrentEmployee { get; }

		[PropertyChangedAlso(nameof(CanEditDate), nameof(CanEditStatus))]
		public bool CanEdit => PermissionResult.CanUpdate && _undeliveryPermissionResult.CanUpdate;

		public bool CanEditDate => CanEdit && CanCompleteDiscussion;

		public string SubdivisionShortName => string.IsNullOrWhiteSpace(Entity.Subdivision.ShortName) ? "?" : Entity.Subdivision.ShortName;

		#region Status

		public virtual UndeliveryDiscussionStatus[] HiddenDiscussionStatuses => new[] { UndeliveryDiscussionStatus.Closed };

		public bool CanEditStatus => CanEdit && Entity.Status != UndeliveryDiscussionStatus.Closed || (CanEdit && _canCompleteUndeliveryDiscussionPermission);

		public bool CanCompleteDiscussion => CanEditStatus && _canCompleteUndeliveryDiscussionPermission;

		#endregion Status

		#region Comment

		[PropertyChangedAlso(nameof(CanAddComment))]
		public virtual string NewCommentText
		{
			get => _newCommentText;
			set => SetField(ref _newCommentText, value, () => NewCommentText);
		}

		public bool CanAddComment => !string.IsNullOrWhiteSpace(NewCommentText);

		public DelegateCommand AddCommentCommand { get; private set; }

		#endregion

	}
}
