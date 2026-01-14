using QS.Dialog;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.ViewModels;
using System;
using System.Collections.ObjectModel;
using Vodovoz.Domain.Logistic.Cars;
using VodovozInfrastructure.StringHandlers;

namespace Vodovoz.ViewModels.ViewModels.Logistic.DriverSchedule
{
	public class DriverScheduleViewModel : DialogTabViewModelBase
	{
		private ObservableCollection<DriverScheduleNode> _drivers;

		public DriverScheduleViewModel(
			IUnitOfWorkFactory unitOfWorkFactory,
			IInteractiveService interactiveService,
			INavigationManager navigation,
			IStringHandler stringHandler
			) : base(unitOfWorkFactory, interactiveService, navigation)
		{
			Title = "График водителей";

			// Инициализируем коллекцию
			Drivers = new ObservableCollection<DriverScheduleNode>();
			StringHandler = stringHandler ?? throw new ArgumentNullException(nameof(stringHandler));

			// Добавляем тестовые данные
			InitializeSampleData();
		}

		public ObservableCollection<DriverScheduleNode> Drivers
		{
			get => _drivers;
			set => SetField(ref _drivers, value);
		}

		public IStringHandler StringHandler { get; }

		private void InitializeSampleData()
		{
			// Тестовые данные
			Drivers.Add(new DriverScheduleNode
			{
				CarTypeOfUse = CarTypeOfUse.Largus,
				CarOwnType = CarOwnType.Company,
				RegNumber = "А123ВС77",
				DriverFullName = "Иванов Иван Иванович",
				DriverCarOwnType = CarOwnType.Company,
				DriverPhone = "+7-999-123-45-67",
				MorningAddress = 15,
				EveningAddress = 12,
				LastModifiedDateTime = DateTime.Now.AddDays(-1)
			});

			Drivers.Add(new DriverScheduleNode
			{
				CarTypeOfUse = CarTypeOfUse.GAZelle,
				CarOwnType = CarOwnType.Company,
				RegNumber = "В456ОР77",
				DriverFullName = "Петров Петр Петрович",
				DriverCarOwnType = CarOwnType.Driver,
				DriverPhone = "+7-999-234-56-78",
				MorningAddress = 20,
				EveningAddress = 18,
				LastModifiedDateTime = DateTime.Now.AddHours(-5)
			});

			Drivers.Add(new DriverScheduleNode
			{
				CarTypeOfUse = CarTypeOfUse.Minivan,
				CarOwnType = CarOwnType.Driver,
				RegNumber = "С789ТУ77",
				DriverFullName = "Сидоров Алексей Владимирович",
				DriverCarOwnType = CarOwnType.Driver,
				DriverPhone = "+7-999-345-67-89",
				MorningAddress = 18,
				EveningAddress = 15,
				LastModifiedDateTime = DateTime.Now
			});

			// Можно добавить ещё тестовых данных
			for(int i = 4; i <= 10; i++)
			{
				Drivers.Add(new DriverScheduleNode
				{
					CarTypeOfUse = CarTypeOfUse.Largus,
					CarOwnType = CarOwnType.Company,
					RegNumber = $"НОВ{i:000}",
					DriverFullName = $"Тестовый водитель {i}",
					DriverCarOwnType = CarOwnType.Company,
					DriverPhone = $"+7-999-000-{i:00}-{i:00}",
					MorningAddress = 10 + i,
					EveningAddress = 8 + i,
					LastModifiedDateTime = DateTime.Now.AddHours(-i)
				});
			}
		}
	}
}
