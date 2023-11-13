using System;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Domain;
using QS.Services;
using QS.ViewModels;
using Vodovoz.Domain.Logistic.Drivers;
using Autofac;
using QS.Commands;
using VodovozInfrastructure.Services;

namespace Vodovoz.ViewModels.ViewModels.Logistic
{
	public class DriverWarehouseEventViewModel : EntityTabViewModelBase<DriverWarehouseEvent>
	{
		private readonly ICoordinatesParser _coordinatesParser;
		private readonly ILifetimeScope _scope;

		private DelegateCommand<string> _setCoordinatesFromBufferCommand;

		public DriverWarehouseEventViewModel(
			IEntityUoWBuilder uowBuilder,
			IUnitOfWorkFactory unitOfWorkFactory,
			ICommonServices commonServices,
			INavigationManager navigation,
			ICoordinatesParser coordinatesParser,
			ILifetimeScope scope) : base(uowBuilder, unitOfWorkFactory, commonServices, navigation)
		{
			_coordinatesParser = coordinatesParser ?? throw new ArgumentNullException(nameof(coordinatesParser));
			_scope = scope ?? throw new ArgumentNullException(nameof(scope));
			Configure();
		}

		private void Configure()
		{
			ConfigureEntityChangingRelations();
		}

		public bool IdGtZero => Entity.Id > 0;

		public bool IsCoordinatesVisible => Entity.Type == DriverWarehouseEventType.OnLocation;

		public DriverWarehouseEventType EventType
		{
			get => Entity.Type;
			set
			{
				Entity.Type = value;

				if(Entity.Type == DriverWarehouseEventType.OnDocuments)
				{
					Entity.Latitude = null;
					Entity.Longitude = null;
				}
				
				OnPropertyChanged();
				OnPropertyChanged(nameof(IsCoordinatesVisible));
			}
		}

		public DelegateCommand<string> SetCoordinatesFromBufferCommand =>
			_setCoordinatesFromBufferCommand ?? (_setCoordinatesFromBufferCommand = new DelegateCommand<string>(
				buffer =>
				{
					var result = _coordinatesParser.GetCoordinatesFromBuffer(buffer);

					if(!result.ParsedCoordinates.HasValue)
					{
						ShowWarningMessage(result.ErrorMessage);
					}
					else
					{
						var parsedCoordinates = result.ParsedCoordinates.Value;
						Entity.WriteCoordinates(parsedCoordinates.Latitude, parsedCoordinates.Longitude);
					}
				}
				));

		private void ConfigureEntityChangingRelations()
		{
			SetPropertyChangeRelation(e => e.Id, () => IdGtZero);
		}
	}
}
