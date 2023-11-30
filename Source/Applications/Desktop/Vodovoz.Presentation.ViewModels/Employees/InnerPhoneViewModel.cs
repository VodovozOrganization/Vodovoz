using QS.Commands;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.ViewModels.Dialog;
using ReactiveUI;
using System;
using System.ComponentModel;
using System.Reactive.Linq;
using System.Windows.Input;
using Vodovoz.Core.Domain.Employees;

namespace Vodovoz.Presentation.ViewModels.Employees
{
	public class ReactiveDialogViewModel : DialogViewModelBase, IReactiveObject
	{
		public event PropertyChangingEventHandler PropertyChanging;

		public ReactiveDialogViewModel(INavigationManager navigation) : base(navigation)
		{
		}

		void IReactiveObject.RaisePropertyChanged(PropertyChangedEventArgs args)
		{
			RaisePropertyChanged(args);
		}

		public void RaisePropertyChanging(PropertyChangingEventArgs args)
		{
			PropertyChanging?.Invoke(this, args);
		}
	}

	public class InnerPhoneViewModel : ReactiveDialogViewModel, IDisposable
	{
		private readonly EntityModel<InnerPhone> _model;

		private string _number;
		private string _description;

		public InnerPhoneViewModel(
			IEntityIdentifier entityId, 
			EntityModelFactory entityModelFactory, 
			INavigationManager navigation
			) : base(navigation)
		{
			if(entityId is null)
			{
				throw new ArgumentNullException(nameof(entityId));
			}

			if(entityModelFactory is null)
			{
				throw new ArgumentNullException(nameof(entityModelFactory));
			}

			_model = entityModelFactory.Create<InnerPhone>(entityId);

			SaveCommand = new SaveCommand(_model, this, () => _model.UoW.HasChanges);
			CancelCommand = new CloseDialogCommand(this);

			this.WhenAnyValue(x => x.Number).Do(x => { }).Subscribe(x => _model.Entity.PhoneNumber = x);
			this.WhenAnyValue(x => x._model.Entity.PhoneNumber).ToProperty(this, x => x.Number);

			this.WhenAnyValue(x => x.Description).Subscribe(x => _model.Entity.Description = x);
			this.WhenAnyValue(x => x._model.Entity.Description).ToProperty(this, x => x.Description);
		}

		public string Number
		{
			get => _number;
			set => this.RaiseAndSetIfChanged(ref _number, value);
		}

		public string Description
		{
			get => _description;
			set => this.RaiseAndSetIfChanged(ref _description, value);
		}

		public SaveCommand SaveCommand { get; }
		public CloseDialogCommand CancelCommand { get; }

		public void Dispose()
		{
			_model?.Dispose();
		}
	}

	public class SaveCommand : PropertySubscribedCommandBase
	{
		private readonly ISaveModel _model;
		private readonly DialogViewModelBase _dialog;
		private readonly Func<bool> _canExecute;
		private bool _executing;

		public SaveCommand(ISaveModel model, DialogViewModelBase dialog, Func<bool> canExecute = null)
		{
			_model = model ?? throw new ArgumentNullException(nameof(model));
			_dialog = dialog ?? throw new ArgumentNullException(nameof(dialog));
			if(canExecute == null)
			{
				_canExecute = () => true;
			}
			else
			{
				_canExecute = canExecute;
			}
		}

		public override bool CanExecute(object parameter)
		{
			if(_executing)
			{
				return false;
			}

			return _canExecute();
		}

		public override void Execute(object parameter)
		{
			try
			{
				_executing = true;
				RaiseCanExecuteChanged();
				_model.Save();
			}
			finally 
			{
				_executing = false;
				RaiseCanExecuteChanged();
			}

			_dialog.Close(false, CloseSource.Save);
		}
	}

	public class CloseDialogCommand : PropertySubscribedCommandBase
	{
		private readonly DialogViewModelBase _dialog;

		public CloseDialogCommand(DialogViewModelBase dialog)
		{
			_dialog = dialog ?? throw new ArgumentNullException(nameof(dialog));
		}

		public override bool CanExecute(object parameter)
		{
			throw new NotImplementedException();
		}

		public override void Execute(object parameter)
		{
			_dialog.Close(true, CloseSource.ClosePage);
		}
	}

	public interface IEntityIdentifier
	{
		bool IsNewEntity { get; }
		object Id { get; }
	}

	public class EntityIdentifier : IEntityIdentifier
	{
		public bool IsNewEntity { get; private set; }

		public object Id { get; private set; }

		public static IEntityIdentifier NewEntity()
		{
			return new EntityIdentifier { IsNewEntity = true, Id = null};
		}

		public static IEntityIdentifier OpenEntity(object id)
		{
			return new EntityIdentifier { IsNewEntity = false, Id = id };
		}

		public static IEntityIdentifier OpenByCompositeKey<TEntity>(Action<TEntity> keysSetter)
			where TEntity : class, new()
		{
			var identifier = new TEntity();
			keysSetter(identifier);
			return new EntityIdentifier { IsNewEntity = false, Id = identifier };
		}
	}

	public class EntityModel<TEntity> : IDisposable, ISaveModel 
		where TEntity : class, new()
	{
		private readonly IEntityIdentifier _entityId;

		public TEntity Entity { get; }
		public IUnitOfWork UoW { get; }

		public EntityModel(IEntityIdentifier entityId, IUnitOfWorkFactory uowFactory)
		{
			if(uowFactory is null)
			{
				throw new ArgumentNullException(nameof(uowFactory));
			}
			_entityId = entityId ?? throw new ArgumentNullException(nameof(entityId));

			UoW = uowFactory.CreateWithoutRoot();

			if(_entityId.IsNewEntity)
			{
				Entity = new TEntity();
			}
			else
			{
				Entity = UoW.Session.Get<TEntity>(_entityId.Id);
			}
		}

		public virtual void Save()
		{
			UoW.Save(Entity);
			UoW.Commit();
		}

		public virtual void Dispose()
		{
			UoW?.Dispose();
		}
	}

	public class EntityModelFactory
	{
		private readonly IUnitOfWorkFactory _uowFactory;

		public EntityModelFactory(IUnitOfWorkFactory uowFactory)
		{
			_uowFactory = uowFactory ?? throw new ArgumentNullException(nameof(uowFactory));
		}

		public EntityModel<TEntity> Create<TEntity>(IEntityIdentifier entityId)
			where TEntity : class, new()
		{
			return new EntityModel<TEntity>(entityId, _uowFactory);
		}
	}
}
