using System;
using System.Linq;
using NSubstitute;
using NUnit.Framework;
using QS.Dialog;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.WageCalculation;
using Vodovoz.EntityRepositories.WageCalculation;
using Vodovoz.Services;
using QS.Services;
using Vodovoz.Core.Domain.Employees;

namespace VodovozBusinessTests.Employees
{
	[TestFixture(TestOf = typeof(Employee))]
	public class EmployeeTests
	{
		#region Test cases

		[Test(Description = "Если до установки даты не было зарплат в списке сотрудника, то успех")]
		public void CheckStartDateForNewWageParameter_IfWereNoAnyWagesBefore_ThenSuccess()
		{
			//arrange
			var employee = new Employee();
			//act
			var res = employee.CheckStartDateForNewWageParameter(new DateTime(2019, 01, 01));
			//assert
			Assert.That(res, Is.True);
		}

		[Test(Description = "Если до установки даты дата существующей ЗП в списке сотрудника раньше новой даты, то успех")]
		public void CheckStartDateForNewWageParameter_IfNewDateLaterThenExisting_ThenSuccess()
		{
			//arrange
			var newDate = new DateTime(2019, 01, 01);
			var existingWage = new EmployeeWageParameter(){
				StartDate = new DateTime(2000, 02, 11),
				WageParameterItem = new ManualWageParameterItem()
			};

			var employee = new Employee();
			employee.WageParameters.Add(existingWage);
			//act
			var res = employee.CheckStartDateForNewWageParameter(newDate);
			//assert
			Assert.That(res, Is.True);
		}

		[Test(Description = "Если до установки даты дата существующей ЗП в списке сотрудника позже новой даты, то провал")]
		public void CheckStartDateForNewWageParameter_IfNewDateEarlierThenExisting_ThenFail()
		{
			//arrange
			var newDate = new DateTime(2019, 01, 01);
			var existingWage = new EmployeeWageParameter(){
				StartDate = new DateTime(2019, 11, 01),
				WageParameterItem = new ManualWageParameterItem()
			};

			var employee = new Employee();
			employee.WageParameters.Add(existingWage);
			//act
			var res = employee.CheckStartDateForNewWageParameter(newDate);
			//assert
			Assert.That(res, Is.False);
		}

		[Test(Description = "Если до добавления ЗП в списке ЗП не было ничего, то добавляем новую ЗП с установкой в неё даты поздней на момент и сотрудника")]
		public void ChangeWageParameter_IfThereWereNoAnyWagesInList_ThenJustAddNewWageAndSetNewDateWithOneTickAndSetCurrentEmployee()
		{
			//arrange
			var newDate = new DateTime(2019, 01, 01);
			var employee = new Employee();
			var wageParameter = new EmployeeWageParameter
			{
				WageParameterItem = new ManualWageParameterItem()
			};

			//act
			employee.ChangeWageParameter(wageParameter, newDate);
			//assert
			Assert.That(employee.ObservableWageParameters.Count, Is.EqualTo(1));
			Assert.That(employee.ObservableWageParameters.First().Employee, Is.EqualTo(employee));
			Assert.That(employee.ObservableWageParameters.First().StartDate, Is.EqualTo(newDate));
		}

		[Test(Description = "Если до добавления ЗП в списке была ЗП с датой начала раньшей чем новая дата, то добавляем новую ЗП с установкой в неё даты поздней на момент и сотрудника. Выставляем у старой ЗП дату окончания.")]
		public void
			ChangeWageParameter_IfThereWereWageInList_ThenAddNewWageAndSetNewDateWithOneTickAndSetCurrentEmployeeAndSetEndDateForExistingWage()
		{
			//arrange
			var newDate = new DateTime(2019, 01, 01);
			var existingWage = new EmployeeWageParameter
			{
				StartDate = new DateTime(2018, 01, 01),
				WageParameterItem = new FixedWageParameterItem()
			};

			var employee = new Employee();
			employee.WageParameters.Add(existingWage);
			var wageParameter = new EmployeeWageParameter
			{
				WageParameterItem = new ManualWageParameterItem()
			};

			//act
			employee.ChangeWageParameter(wageParameter, newDate);
			//assert
			Assert.That(employee.ObservableWageParameters.Count, Is.EqualTo(2));
			Assert.That(
				employee.ObservableWageParameters
					.First(w => w.WageParameterItem.WageParameterItemType == WageParameterItemTypes.Manual)
					.Employee,
				Is.EqualTo(employee)
			);
			Assert.That(
				employee.ObservableWageParameters
					.First(w => w.WageParameterItem.WageParameterItemType == WageParameterItemTypes.Manual)
					.StartDate,
				Is.EqualTo(newDate)
			);
			Assert.That(
				employee.ObservableWageParameters
					.First(w => w.WageParameterItem.WageParameterItemType == WageParameterItemTypes.Fixed)
					.EndDate,
				Is.EqualTo(newDate.AddMilliseconds(-1))
			);
		}

		[Test(Description = "Если в списке ЗП несколько параметров, то вернётся первый с конца у которой дата меньше переданной в параметр")]
		public void GetActualWageParameter_IfThereAreSomeWagesInListWithDifferentDates_ThenReturnsTheLatestWageWhoseDateLessThenInParameter()
		{
			//arrange
			var date = new DateTime(2018, 11, 01);
			var wage1 = new EmployeeWageParameter(){
				StartDate = new DateTime(2017, 01, 01),
				WageParameterItem = new FixedWageParameterItem()
			};
			var wage2 = new EmployeeWageParameter(){
				StartDate = new DateTime(2018, 01, 01),
				WageParameterItem = new FixedWageParameterItem()
			};
			var wage3 = new EmployeeWageParameter(){
				StartDate = new DateTime(2019, 01, 01),
				WageParameterItem = new FixedWageParameterItem()
			};
			var employee = new Employee();
			employee.WageParameters.Add(wage1);
			employee.WageParameters.Add(wage2);
			employee.WageParameters.Add(wage3);

			//act
			var wage = employee.GetActualWageParameter(date);

			//assert
			Assert.That(wage, Is.EqualTo(wage2));
		}

		[Test(Description = "Если в списке ЗП нет параметров, то вернётся null")]
		public void GetActualWageParameter_IfThereAreNoAnyWages_ThenReturnsNull()
		{
			//arrange
			var date = new DateTime(2018, 11, 01);
			var employee = new Employee();

			//act
			var wage = employee.GetActualWageParameter(date);

			//assert
			Assert.That(wage, Is.Null);
		}

		[Test(Description = "Если в сущность сотрудника не новая, то ничего не делаем")]
		public void CreateDefaultWageParameter_IfInstanceOfEmployeeIsNotNew_ThenDoNothing()
		{
			//arrange
			IWageSettings wageSettings = Substitute.For<IWageSettings>();
			IWageCalculationRepository wageCalculationRepository = Substitute.For<IWageCalculationRepository>();
			var employee = new Employee { Id = 1 };
			employee.WageParameters.Add(
				new EmployeeWageParameter(){
					StartDate = new DateTime(2000, 01, 01),
					WageParameterItem = new FixedWageParameterItem()
				}
			);
			IInteractiveService interactiveService = Substitute.For<IInteractiveService>();

			//act
			employee.CreateDefaultWageParameter(wageCalculationRepository, wageSettings, interactiveService);

			//assert
			Assert.That(employee.ObservableWageParameters.Count(), Is.EqualTo(1));
			Assert.That(
				employee.ObservableWageParameters
						.FirstOrDefault()
						.StartDate,
				Is.EqualTo(new DateTime(2000, 01, 01))
			);
		}

		[Test(Description = "Если сущность сотрудника новая и сотрудник будет выездным мастером, но не одноразовым, то создаём ЗП как процент от сервиса")]
		public void CreateDefaultWageParameter_IfInstanceOfEmployeeIsNewAndCategoryOfEmployeeIsVisitingMasterAndIsNotDriverForOneDay_ThenCreatePercentWageParameter()
		{
			//arrange
			IWageSettings wageSettings = Substitute.For<IWageSettings>();
			IWageCalculationRepository wageCalculationRepository = Substitute.For<IWageCalculationRepository>();
			var employee = new Employee {
				WageCalculationRepository = wageCalculationRepository,
				Category = EmployeeCategory.driver,
				VisitingMaster = true,
			};
			WageDistrictLevelRates levelRates = Substitute.For<WageDistrictLevelRates>();
			wageCalculationRepository.DefaultLevelForNewEmployees(null).ReturnsForAnyArgs(levelRates);
			IInteractiveService interactiveService = Substitute.For<IInteractiveService>();

			//act
			employee.CreateDefaultWageParameter(wageCalculationRepository, wageSettings, interactiveService);

			//assert
			Assert.That(employee.ObservableWageParameters.Count(), Is.EqualTo(1));
			Assert.That(
				employee.ObservableWageParameters
						.FirstOrDefault()
						.WageParameterItem
						.WageParameterItemType,
				Is.EqualTo(WageParameterItemTypes.Percent)
			);
		}

		[Test(Description = "Если сущность сотрудника новая и сотрудник не будет выездным мастером и не одноразовым, то создаём ЗП по ставкам")]
		public void CreateDefaultWageParameter_IfInstanceOfEmployeeIsNewAndCategoryOfEmployeeIsNotVisitingMasterAndIsNotDriverForOneDay_ThenCreateRatesWageParameter()
		{
			//arrange
			IWageSettings wageSettings = Substitute.For<IWageSettings>();
			IWageCalculationRepository wageCalculationRepository = Substitute.For<IWageCalculationRepository>();
			var employee = new Employee {
				WageCalculationRepository = wageCalculationRepository,
				Category = EmployeeCategory.driver,
			};
			WageDistrictLevelRates levelRates = Substitute.For<WageDistrictLevelRates>();
			wageCalculationRepository.DefaultLevelForNewEmployees(null).ReturnsForAnyArgs(levelRates);
			IInteractiveService interactiveService = Substitute.For<IInteractiveService>();

			//act
			employee.CreateDefaultWageParameter(wageCalculationRepository, wageSettings, interactiveService);

			//assert
			Assert.That(employee.ObservableWageParameters.Count(), Is.EqualTo(1));
			Assert.That(
				employee.ObservableWageParameters
						.FirstOrDefault()
						.WageParameterItem
						.WageParameterItemType,
				Is.EqualTo(WageParameterItemTypes.RatesLevel)
			);
		}

		[Test(Description = "Если сущность сотрудника новая и сотрудник одноразовый водитель, то создаём ЗП с ручным расчётом")]
		public void CreateDefaultWageParameter_IfInstanceOfEmployeeIsNewAndCategoryOfEmployeeIsDriverForOneDay_ThenCreateManualWageParameter()
		{
			//arrange
			IWageSettings wageSettings = Substitute.For<IWageSettings>();
			IWageCalculationRepository wageCalculationRepository = Substitute.For<IWageCalculationRepository>();
			var employee = new Employee {
				WageCalculationRepository = wageCalculationRepository,
				Category = EmployeeCategory.driver,
				IsDriverForOneDay = true
			};
			WageDistrictLevelRates levelRates = Substitute.For<WageDistrictLevelRates>();
			wageCalculationRepository.DefaultLevelForNewEmployees(null).ReturnsForAnyArgs(levelRates);
			IInteractiveService interactiveService = Substitute.For<IInteractiveService>();

			//act
			employee.CreateDefaultWageParameter(wageCalculationRepository, wageSettings, interactiveService);

			//assert
			Assert.That(employee.ObservableWageParameters.Count(), Is.EqualTo(1));
			Assert.That(
				employee.ObservableWageParameters
						.FirstOrDefault()
						.WageParameterItem
						.WageParameterItemType,
				Is.EqualTo(WageParameterItemTypes.Manual)
			);
		}

		[Test(Description = "Если сущность сотрудника новая и сотрудник экспедитор, то создаём ЗП с расчётом по ставкам")]
		public void CreateDefaultWageParameter_IfInstanceOfEmployeeIsNewAndCategoryOfEmployeeIsForwarder_ThenCreateRatesWageParameter()
		{
			//arrange
			IWageSettings wageSettings = Substitute.For<IWageSettings>();
			IWageCalculationRepository wageCalculationRepository = Substitute.For<IWageCalculationRepository>();
			var employee = new Employee {
				WageCalculationRepository = wageCalculationRepository,
				Category = EmployeeCategory.forwarder
			};
			WageDistrictLevelRates levelRates = Substitute.For<WageDistrictLevelRates>();
			wageCalculationRepository.DefaultLevelForNewEmployees(null).ReturnsForAnyArgs(levelRates);
			IInteractiveService interactiveService = Substitute.For<IInteractiveService>();

			//act
			employee.CreateDefaultWageParameter(wageCalculationRepository, wageSettings, interactiveService);

			//assert
			Assert.That(employee.ObservableWageParameters.Count(), Is.EqualTo(1));
			Assert.That(
				employee.ObservableWageParameters
						.FirstOrDefault()
						.WageParameterItem
						.WageParameterItemType,
				Is.EqualTo(WageParameterItemTypes.RatesLevel)
			);
		}

		[Test(Description = "Если сущность сотрудника новая и сотрудник офисный, то создаём ЗП с ручным расчётом")]
		public void CreateDefaultWageParameter_IfInstanceOfEmployeeIsNewAndCategoryOfEmployeeIsOffice_ThenCreateManualWageParameter()
		{
			//arrange
			IWageSettings wageSettings = Substitute.For<IWageSettings>();
			IWageCalculationRepository wageCalculationRepository = Substitute.For<IWageCalculationRepository>();
			var employee = new Employee {
				WageCalculationRepository = wageCalculationRepository,
				Category = EmployeeCategory.office
			};
			WageDistrictLevelRates levelRates = Substitute.For<WageDistrictLevelRates>();
			wageCalculationRepository.DefaultLevelForNewEmployees(null).ReturnsForAnyArgs(levelRates);
			IInteractiveService interactiveService = Substitute.For<IInteractiveService>();

			//act
			employee.CreateDefaultWageParameter(wageCalculationRepository, wageSettings, interactiveService);

			//assert
			Assert.That(employee.ObservableWageParameters.Count(), Is.EqualTo(1));
			Assert.That(
				employee.ObservableWageParameters
						.FirstOrDefault()
						.WageParameterItem
						.WageParameterItemType,
				Is.EqualTo(WageParameterItemTypes.Manual)
			);
		}

		#endregion
	}
}
