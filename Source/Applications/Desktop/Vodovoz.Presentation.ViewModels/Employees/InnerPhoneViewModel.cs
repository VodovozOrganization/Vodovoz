using QS.Navigation;
using QS.Validation;
using ReactiveUI;
using System;
using System.Reactive.Disposables;
using Vodovoz.Core.Application.Entity;
using Vodovoz.Core.Domain.Employees;
using Vodovoz.Presentation.ViewModels.Common;

namespace Vodovoz.Presentation.ViewModels.Employees
{

	public class InnerPhoneViewModel : EntityReactiveDialog<InnerPhone>
	{
		private readonly IValidator _validator;
		private string _number;
		private string _description;

		public InnerPhoneViewModel(
			IEntityIdentifier entityId, 
			EntityModelFactory entityModelFactory,
			IValidator validator,
			INavigationManager navigation
			) : base(entityId, entityModelFactory, navigation)
		{
			_validator = validator ?? throw new ArgumentNullException(nameof(validator));

			Title = "Внутренний телефон";

			this.WhenAnyValue(x => x.Entity.PhoneNumber)
				.BindTo(this, x => x.Number)
				.DisposeWith(Subscriptions);
			this.WhenAnyValue(x => x.Number)
				.BindTo(Entity, x => x.PhoneNumber)
				.DisposeWith(Subscriptions);

			this.WhenAnyValue(x => x.Entity.Description)
				.BindTo(this, x => x.Description)
				.DisposeWith(Subscriptions);
			this.WhenAnyValue(x => x.Description)
				.BindTo(Entity, x => x.Description)
				.DisposeWith(Subscriptions);
		}

		public bool CanChangeNumber => Model.IsNewEntity;

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
