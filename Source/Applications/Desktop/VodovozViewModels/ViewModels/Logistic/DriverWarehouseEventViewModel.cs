using System;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Domain;
using QS.Services;
using QS.ViewModels;
using Autofac;
using QS.Commands;
using QS.ViewModels.Extension;
using Vodovoz.Core.Domain.Interfaces.Logistics;
using Vodovoz.Core.Domain.Logistics.Drivers;
using Vodovoz.EntityRepositories.Logistic;
using Vodovoz.Settings.Employee;
using VodovozInfrastructure.Services;

namespace Vodovoz.ViewModels.ViewModels.Logistic
{
	public class DriverWarehouseEventViewModel : EntityTabViewModelBase<DriverWarehouseEvent>, IAskSaveOnCloseViewModel
	{
		private readonly ICoordinatesParser _coordinatesParser;
		private readonly IDriverWarehouseEventRepository _driverWarehouseEventRepository;
		private readonly ICompletedDriverWarehouseEventRepository _completedDriverWarehouseEventRepository;

		private ILifetimeScope _scope;
		private bool _hasCompletedEvents;

		public DriverWarehouseEventViewModel(
			IEntityUoWBuilder uowBuilder,
			IUnitOfWorkFactory unitOfWorkFactory,
			ICommonServices commonServices,
			INavigationManager navigation,
			ICoordinatesParser coordinatesParser,
			IDriverWarehouseEventRepository driverWarehouseEventRepository,
			ICompletedDriverWarehouseEventRepository completedDriverWarehouseEventRepository,
			IDriverWarehouseEventSettings driverWarehouseEventSettings,
			ILifetimeScope scope) : base(uowBuilder, unitOfWorkFactory, commonServices, navigation)
		{
			_coordinatesParser = coordinatesParser ?? throw new ArgumentNullException(nameof(coordinatesParser));
			_driverWarehouseEventRepository =
				driverWarehouseEventRepository ?? throw new ArgumentNullException(nameof(driverWarehouseEventRepository));
			_completedDriverWarehouseEventRepository =
				completedDriverWarehouseEventRepository ?? throw new ArgumentNullException(nameof(completedDriverWarehouseEventRepository));
			DriverWarehouseEventSettings =
				driverWarehouseEventSettings ?? throw new ArgumentNullException(nameof(driverWarehouseEventSettings));
			_scope = scope ?? throw new ArgumentNullException(nameof(scope));
			
			Configure();
		}

		public bool IdGtZero => Entity.Id > 0;

		public bool CanEditByPermission => (PermissionResult.CanCreate && Entity.Id == 0) || PermissionResult.CanUpdate;

		public bool CanEdit => CanEditByPermission && !_hasCompletedEvents;
		public bool AskSaveOnClose => CanEditByPermission;

		public bool IsCoordinatesVisible => Entity.Type == DriverWarehouseEventType.OnLocation;
		public bool CanPrintQrCode => Entity.Type == DriverWarehouseEventType.OnLocation;
		public bool IsDocumentQrParametersVisible => Entity.Type == DriverWarehouseEventType.OnDocuments;
		public IDriverWarehouseEventSettings DriverWarehouseEventSettings { get; }

		public DriverWarehouseEventType EventType
		{
			get => Entity.Type;
			set
			{
				Entity.Type = value;

				Entity.ResetEventParameters();
				OnPropertyChanged();
			}
		}

		public DelegateCommand<string> SetCoordinatesFromBufferCommand { get; private set; }

		public bool AskUserQuestion(string question, string title = null)
		{
			return base.AskQuestion(question, title);
		}
		
		private void Configure()
		{
			CreateCommands();
			ConfigureEntityChangingRelations();
			CheckCompletedEventsByEntity();
			ConfigureValidationContext();
		}

		private void CreateCommands()
		{
			CreateSetCoordinatesFromBufferCommand();
		}

		private void CreateSetCoordinatesFromBufferCommand()
		{
			SetCoordinatesFromBufferCommand =
				new DelegateCommand<string>(
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
				);
		}

		private void ConfigureValidationContext()
		{
			ValidationContext.ServiceContainer.AddService(typeof(IDriverWarehouseEventRepository), _driverWarehouseEventRepository);
		}

		private void CheckCompletedEventsByEntity()
		{
			_hasCompletedEvents = Entity.Id != 0 && _completedDriverWarehouseEventRepository.HasCompletedEventsByEventId(UoW, Entity.Id);
		}

		private void ConfigureEntityChangingRelations()
		{
			SetPropertyChangeRelation(
				e => e.Id,
				() => IdGtZero,
				() => CanEdit);
			
			SetPropertyChangeRelation(
				e => e.Type,
				() => IsCoordinatesVisible,
				() => IsDocumentQrParametersVisible);
		}

		public override void Dispose()
		{
			_scope = null;
			base.Dispose();
		}
	}
}
