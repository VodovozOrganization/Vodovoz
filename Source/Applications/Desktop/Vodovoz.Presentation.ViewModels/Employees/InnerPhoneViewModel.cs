using QS.Commands;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.ViewModels.Dialog;
using ReactiveUI;
using System;
using System.ComponentModel;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
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
		private readonly CompositeDisposable _subscriptions = new CompositeDisposable();
		private readonly EntityModel<InnerPhone> _model;

		protected InnerPhone Entity => _model.Entity;
		private string _number;
		private string _description;

		public SaveCommand2 SaveCommand { get; }
		public CloseDialogCommand CancelCommand { get; }

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

			Title = "Внутренний телефон";

			_model = entityModelFactory.Create<InnerPhone>(entityId);

			SaveCommand = new SaveCommand2(_model, this);
			CancelCommand = new CloseDialogCommand(this);

			this.WhenAnyValue(x => x.Entity.PhoneNumber)
				.BindTo(this, x => x.Number)
				.DisposeWith(_subscriptions);
			this.WhenAnyValue(x => x.Number)
				.BindTo(Entity, x => x.PhoneNumber)
				.DisposeWith(_subscriptions);

			this.WhenAnyValue(x => x.Entity.Description)
				.BindTo(this, x => x.Description)
				.DisposeWith(_subscriptions);
			this.WhenAnyValue(x => x.Description)
				.BindTo(Entity, x => x.Description)
				.DisposeWith(_subscriptions);
		}

		public bool CanChangeNumber => _model.IsNewEntity;

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

		public void Dispose()
		{
			_subscriptions?.Dispose();
			_model?.Dispose();
		}
	}

	public class SaveCommand2 : ReactiveCommandBase
	{
		private readonly ISaveModel _model;
		private readonly DialogViewModelBase _dialog;

		public SaveCommand2(ISaveModel model, DialogViewModelBase dialog, IObservable<bool> canExecute = null) : base(canExecute)
		{
			_model = model ?? throw new ArgumentNullException(nameof(model));
			_dialog = dialog ?? throw new ArgumentNullException(nameof(dialog));
		}

		protected override void InternalExecute(object parameter)
		{
			_model.Save();
			_dialog.Close(false, CloseSource.Save);
		}
	}

	public abstract class ReactiveCommandBase : IReactiveCommand<Unit, Unit>, ICommand
	{
		private ReactiveCommand<Unit, Unit> _reactiveCommand;

		protected ReactiveCommandBase(IObservable<bool> canExecute = null)
		{
			_reactiveCommand = ReactiveCommand.Create(() => InternalExecute(null), canExecute);
		}

		protected abstract void InternalExecute(object parameter);

		public IObservable<bool> IsExecuting => _reactiveCommand.IsExecuting;

		public IObservable<bool> CanExecute => _reactiveCommand.CanExecute;

		public IObservable<Exception> ThrownExceptions => _reactiveCommand.ThrownExceptions;

		public IObservable<Unit> Execute(Unit parameter)
		{
			return _reactiveCommand.Execute(parameter);
		}

		public IObservable<Unit> Execute()
		{
			return _reactiveCommand.Execute();
		}

		public IDisposable Subscribe(IObserver<Unit> observer)
		{
			return _reactiveCommand.Subscribe(observer);
		}

		#region ICommand

		void ICommand.Execute(object parameter)
		{
			(_reactiveCommand as ICommand).Execute(parameter);
		}

		bool ICommand.CanExecute(object parameter)
		{
			return (_reactiveCommand as ICommand).CanExecute(parameter);
		}

		event EventHandler ICommand.CanExecuteChanged
		{
			add { (_reactiveCommand as ICommand).CanExecuteChanged += value; }
			remove { (_reactiveCommand as ICommand).CanExecuteChanged -= value; }
		}

		#endregion ICommand

		public void Dispose()
		{
			_reactiveCommand.Dispose();
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
			return true;
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

		private bool _saved;
		public bool IsNewEntity => _entityId.IsNewEntity && !_saved;
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
			_saved = true;
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
