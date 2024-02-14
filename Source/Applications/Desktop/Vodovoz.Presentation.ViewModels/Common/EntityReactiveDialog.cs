using QS.Navigation;
using ReactiveUI;
using System;
using System.Reactive;
using Vodovoz.Core.Application.Entity;

namespace Vodovoz.Presentation.ViewModels.Common
{
	public class EntityReactiveDialog<TEntity> : ReactiveDialogViewModel, IDisposable
		where TEntity : class, new()
	{
		protected EntityModel<TEntity> Model { get; }

		public EntityReactiveDialog(
			IEntityIdentifier entityId,
			EntityModelFactory entityModelFactory,
			INavigationManager navigator
		) : base(navigator)
		{
			if(entityId is null)
			{
				throw new ArgumentNullException(nameof(entityId));
			}

			if(entityModelFactory is null)
			{
				throw new ArgumentNullException(nameof(entityModelFactory));
			}

			Model = entityModelFactory.Create<TEntity>(entityId);
			SaveCommand = ReactiveCommand.Create(() => Save());
			CancelCommand = ReactiveCommand.Create(() => Cancel());
		}

		public TEntity Entity => Model.Entity;
		public ReactiveCommand<Unit, Unit> SaveCommand { get; }
		public ReactiveCommand<Unit, Unit> CancelCommand { get; }

		public virtual void Save()
		{
			Model.Save();
			Close(false, CloseSource.Save);
		}

		public virtual void Cancel()
		{
			Close(false, CloseSource.Cancel);
		}

		public override void Dispose()
		{
			Model.Dispose();
			base.Dispose();
		}
	}
}
