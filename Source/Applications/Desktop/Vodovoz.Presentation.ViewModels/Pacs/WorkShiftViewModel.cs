using QS.Navigation;
using QS.Validation;
using ReactiveUI;
using System;
using System.Reactive.Disposables;
using Vodovoz.Core.Application.Entity;
using Vodovoz.Core.Domain.Pacs;
using Vodovoz.Presentation.ViewModels.Common;

namespace Vodovoz.Presentation.ViewModels.Pacs
{
	public class WorkShiftViewModel : EntityReactiveDialog<WorkShift>
	{
		private readonly IValidator _validator;
		private string _name;
		private TimeSpan _duration;

		public WorkShiftViewModel(
			IEntityIdentifier entityId,
			EntityModelFactory entityModelFactory,
			IValidator validator,
			INavigationManager navigator
		) : base(entityId, entityModelFactory, navigator)
		{
			_validator = validator ?? throw new ArgumentNullException(nameof(validator));

			Title = "Рабочая смена";

			this.WhenAnyValue(x => x.Entity.Name)
				.BindTo(this, x => x.Name)
				.DisposeWith(Subscriptions);
			this.WhenAnyValue(x => x.Name)
				.BindTo(Entity, x => x.Name)
				.DisposeWith(Subscriptions);

			this.WhenAnyValue(x => x.Entity.Duration)
				.BindTo(this, x => x.Duration)
				.DisposeWith(Subscriptions);
			this.WhenAnyValue(x => x.Duration)
				.BindTo(Entity, x => x.Duration)
				.DisposeWith(Subscriptions);
		}

		public virtual string Name
		{
			get => _name;
			set => this.RaiseAndSetIfChanged(ref _name, value);
		}

		public virtual TimeSpan Duration
		{
			get => _duration;
			set => this.RaiseAndSetIfChanged(ref _duration, value);
		}

		public override void Save()
		{
			if(!_validator.Validate(Entity))
			{
				return;
			}
			base.Save();
		}
	}
}
