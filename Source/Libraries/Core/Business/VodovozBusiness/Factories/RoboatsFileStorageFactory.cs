using QS.Dialog;
using System;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Roboats;
using Vodovoz.Models;
using Vodovoz.Settings.Roboats;
using Vodovoz.Tools;

namespace Vodovoz.Factories
{
	public class RoboatsFileStorageFactory : IRoboatsFileStorageFactory
	{
		private readonly IRoboatsSettings _roboatsSettings;
		private readonly IInteractiveService _interactiveService;
		private readonly IErrorReporter _errorReporter;

		public RoboatsFileStorageFactory(IRoboatsSettings roboatsSettings, IInteractiveService interactiveService, IErrorReporter errorReporter)
		{
			_roboatsSettings = roboatsSettings ?? throw new ArgumentNullException(nameof(roboatsSettings));
			_interactiveService = interactiveService ?? throw new ArgumentNullException(nameof(interactiveService));
			_errorReporter = errorReporter ?? throw new ArgumentNullException(nameof(errorReporter));
		}

		public RoboatsFileStorage CreateDeliveryScheduleStorage()
		{
			return new RoboatsFileStorage(_roboatsSettings.DeliverySchedulesAudiofilesFolder, _interactiveService, _errorReporter);
		}

		public RoboatsFileStorage CreateAddressStorage()
		{
			return new RoboatsFileStorage(_roboatsSettings.AddressesAudiofilesFolder, _interactiveService, _errorReporter);
		}

		public RoboatsFileStorage CreateWaterTypeStorage()
		{
			return new RoboatsFileStorage(_roboatsSettings.WaterTypesAudiofilesFolder, _interactiveService, _errorReporter);
		}

		public RoboatsFileStorage CreateStorage(RoboatsEntityType roboatsEntityType)
		{
			string storagePath;
			switch(roboatsEntityType)
			{
				case RoboatsEntityType.DeliverySchedules:
					storagePath = _roboatsSettings.DeliverySchedulesAudiofilesFolder;
					break;
				case RoboatsEntityType.Streets:
					storagePath = _roboatsSettings.AddressesAudiofilesFolder;
					break;
				case RoboatsEntityType.WaterTypes:
					storagePath = _roboatsSettings.WaterTypesAudiofilesFolder;
					break;
				case RoboatsEntityType.CounterpartyName:
					storagePath = _roboatsSettings.CounterpartyNameAudiofilesFolder;
					break;
				case RoboatsEntityType.CounterpartyPatronymic:
					storagePath = _roboatsSettings.CounterpartyPatronymicAudiofilesFolder;
					break;
				default:
					throw new NotSupportedException($"Тип {roboatsEntityType} не поддерживается.");
			}
			return new RoboatsFileStorage(storagePath, _interactiveService, _errorReporter);
		}
	}
}
