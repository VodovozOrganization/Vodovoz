﻿using NSubstitute;
using NUnit.Framework;
using System;
using Vodovoz.Domain.Logistic;
using VodovozRouteList = Vodovoz.Domain.Logistic.RouteList;
using Vodovoz.Domain.Cash;
using Vodovoz.Domain.Employees;
using QS.DomainModel.UoW;
using System.ComponentModel.DataAnnotations;
using System.Collections;
using NSubstitute.Extensions;
using Vodovoz;
using Vodovoz.Domain.Logistic.Cars;
using Vodovoz.Domain.Organizations;
using Vodovoz.EntityRepositories.Cash;
using Vodovoz.EntityRepositories.Fuel;
using Vodovoz.Parameters;
using Vodovoz.Services;

namespace VodovozBusinessTests.Domain.Fuel
{
	[TestFixture]
	public class FuelDocumentTests
	{
		#region CreateFuelDocumentOperationsTests
		
		private ExpenseCategory expenseCategoryMock;
		private IParametersProvider _parametersProvider = Substitute.For<IParametersProvider>();

		#region CreateFuelOperationTests

		[Test(Description = "Создание операции выдачи топлива, с автомобилем наемника")]
		public void CreateFuelOperationTest_Car_IsNotCompanyHavings()
		{
			// arrange
			var categoryRepositoryMock = Substitute.For<ICategoryRepository>();
			expenseCategoryMock = Substitute.For<ExpenseCategory>();
			categoryRepositoryMock.FuelDocumentExpenseCategory(null).Returns(expenseCategoryMock);

			FuelType fuelTypeMock = Substitute.For<FuelType>();
			fuelTypeMock.Cost.Returns(30);

			Car carMock = Substitute.For<Car>();
			carMock.IsCompanyCar.Returns(false);

			Organization organisationMock = Substitute.For<Organization>();

			IUnitOfWork uowMock = Substitute.For<IUnitOfWork>();

			VodovozRouteList routeListMock = Substitute.For<VodovozRouteList>();
			Subdivision subdivisionMock = Substitute.For<Subdivision>();
			routeListMock.ClosingSubdivision = subdivisionMock;

			IFuelRepository fuelRepositoryMock = Substitute.For<IFuelRepository>();
			fuelRepositoryMock.GetFuelBalanceForSubdivision(uowMock, subdivisionMock, fuelTypeMock).Returns(50);

			OrganizationParametersProvider organisationParametersProviderMock =
				Substitute.For<OrganizationParametersProvider>(_parametersProvider);
			
			CashDistributionCommonOrganisationProvider commonOrganisationProviderMock = 
				Substitute.For<CashDistributionCommonOrganisationProvider>(organisationParametersProviderMock);
			commonOrganisationProviderMock.GetCommonOrganisation(uowMock).Returns(organisationMock);
			
			var fuelDocument = new FuelDocument();
			fuelDocument.Driver = Substitute.For<Employee>();
			fuelDocument.Car = carMock;
			fuelDocument.Date = DateTime.Now;
			fuelDocument.LastEditDate = DateTime.Now;
			fuelDocument.Fuel = fuelTypeMock;
			fuelDocument.RouteList = routeListMock;
			fuelDocument.UoW = uowMock;
			fuelDocument.FuelCoupons = 40;
			fuelDocument.PayedForFuel = null;
			fuelDocument.Subdivision = subdivisionMock;

			// act
			fuelDocument.CreateOperations(fuelRepositoryMock, commonOrganisationProviderMock, categoryRepositoryMock);

			// assert
			AssertsAccumulator.Create
				.Accumulate(() => Assert.That(fuelDocument.FuelOperation.LitersGived, Is.EqualTo(fuelDocument.FuelCoupons), "Количество топлива в операции не совпадает с количеством в документе"))
				.Accumulate(() => Assert.That(fuelDocument.FuelOperation.PayedLiters, Is.EqualTo(fuelDocument.PayedLiters), "Количество топлива оплаченного деньгами не совпадает с количеством топлива оплаченного деньгами в документе"))
				.Accumulate(() => Assert.That(fuelDocument.FuelOperation.Car, Is.Null, "Автомобиль не должен быть заполнен"))
				.Accumulate(() => Assert.That(fuelDocument.FuelOperation.Driver, Is.SameAs(fuelDocument.Driver), "Водитель в операции не совпадает с водителем в документе"))
				.Accumulate(() => Assert.That(fuelDocument.FuelOperation.Fuel, Is.SameAs(fuelDocument.Fuel), "Тип топлива в операции не совпадает с типом топлива в документе"))
				.Release();
		}

		[Test(Description = "Создание операции выдачи топлива, с автомобилем компании")]
		public void CreateFuelOperationTest_Car_IsCompanyHavings()
		{
			// arrange
			var categoryRepositoryMock = Substitute.For<ICategoryRepository>();
			expenseCategoryMock = Substitute.For<ExpenseCategory>();
			categoryRepositoryMock.FuelDocumentExpenseCategory(null).Returns(expenseCategoryMock);

			FuelType fuelTypeMock = Substitute.For<FuelType>();
			fuelTypeMock.Cost.Returns(30);

			Car carMock = Substitute.For<Car>();
			carMock.IsCompanyCar.Returns(true);
			
			Organization organisationMock = Substitute.For<Organization>();

			IUnitOfWork uowMock = Substitute.For<IUnitOfWork>();

			VodovozRouteList routeListMock = Substitute.For<VodovozRouteList>();
			Subdivision subdivisionMock = Substitute.For<Subdivision>();
			routeListMock.ClosingSubdivision = subdivisionMock;

			IFuelRepository fuelRepositoryMock = Substitute.For<IFuelRepository>();
			fuelRepositoryMock.GetFuelBalanceForSubdivision(uowMock, subdivisionMock, fuelTypeMock).Returns(50);
			
			OrganizationParametersProvider organisationParametersProviderMock =
				Substitute.For<OrganizationParametersProvider>(_parametersProvider);
			
			CashDistributionCommonOrganisationProvider commonOrganisationProviderMock = 
				Substitute.For<CashDistributionCommonOrganisationProvider>(organisationParametersProviderMock);
			commonOrganisationProviderMock.GetCommonOrganisation(uowMock).Returns(organisationMock);
			
			var fuelDocument = new FuelDocument();
			fuelDocument.Driver = Substitute.For<Employee>();
			fuelDocument.Car = carMock;
			fuelDocument.Date = DateTime.Now;
			fuelDocument.LastEditDate = DateTime.Now;
			fuelDocument.Fuel = fuelTypeMock;
			fuelDocument.RouteList = routeListMock;
			fuelDocument.UoW = uowMock;
			fuelDocument.FuelCoupons = 40;
			fuelDocument.PayedForFuel = null;
			fuelDocument.Subdivision = subdivisionMock;

			// act
			fuelDocument.CreateOperations(fuelRepositoryMock, commonOrganisationProviderMock, categoryRepositoryMock);

			// assert
			AssertsAccumulator.Create
				.Accumulate(() => Assert.That(fuelDocument.FuelOperation.LitersGived, Is.EqualTo(fuelDocument.FuelCoupons), "Количество топлива в операции не совпадает с количеством в документе"))
				.Accumulate(() => Assert.That(fuelDocument.FuelOperation.PayedLiters, Is.EqualTo(fuelDocument.PayedLiters), "Количество топлива оплаченного деньгами не совпадает с количеством топлива оплаченного деньгами в документе"))
				.Accumulate(() => Assert.That(fuelDocument.FuelOperation.Car, Is.SameAs(fuelDocument.Car), "Автомобиль в операции не совпадает с автомобилем в документе"))
				.Accumulate(() => Assert.That(fuelDocument.FuelOperation.Driver, Is.Null, "Водитель не должен быть заполнен"))
				.Accumulate(() => Assert.That(fuelDocument.FuelOperation.Fuel, Is.SameAs(fuelDocument.Fuel), "Тип топлива в операции не совпадает с типом топлива в документе"))
				.Release();
		}

		[Test(Description = "При отсутствии статьи расхода должно выбрасываться исключение InvalidProgrammException и не должна создаваться операция выдачи топлива")]
		public void CreateFuelOperationTest__Without_ExpenseCategory__Thrown_InvalidProgrammException_and_FuelOperation_must_be_null()
		{
			// arrange
			// имитация того что нужной статьи не было найдено
			// arrange
			var categoryRepositoryMock = Substitute.For<ICategoryRepository>();
			expenseCategoryMock = null;
			categoryRepositoryMock.FuelDocumentExpenseCategory(null).Returns(expenseCategoryMock);
			var fuelDocument = new FuelDocument();
			FuelType fuelTypeMock = Substitute.For<FuelType>();
			IUnitOfWork uowMock = Substitute.For<IUnitOfWork>();
			Subdivision subdivisionMock = Substitute.For<Subdivision>();
			Organization organisationMock = Substitute.For<Organization>();
			IFuelRepository fuelRepositoryMock = Substitute.For<IFuelRepository>();
			
			OrganizationParametersProvider organisationParametersProviderMock =
				Substitute.For<OrganizationParametersProvider>(_parametersProvider);
			
			CashDistributionCommonOrganisationProvider commonOrganisationProviderMock = 
				Substitute.For<CashDistributionCommonOrganisationProvider>(organisationParametersProviderMock);
			commonOrganisationProviderMock.GetCommonOrganisation(uowMock).Returns(organisationMock);

			// act, assert
			Assert.Throws(typeof(InvalidProgramException),
				() => fuelDocument.CreateOperations(fuelRepositoryMock, commonOrganisationProviderMock, categoryRepositoryMock));

			// additional assert
			Assert.IsNull(fuelDocument.FuelOperation, "При исключении в момент создания операций, операции выдачи топлива не должно быть создано");
		}

		[Test(Description = "При не правильном количестве литров топлива должно выбрасываться исключение ValidationException и не должна создаваться операция выдачи топлива")]
		public void CreateFuelOperationTest__Incorrect_FuelCoupons_and_PayedForFuel__Thrown_ValidationException_and_FuelOperation_must_be_null()
		{
			// arrange
			var categoryRepositoryMock = Substitute.For<ICategoryRepository>();
			expenseCategoryMock = Substitute.For<ExpenseCategory>();
			categoryRepositoryMock.FuelDocumentExpenseCategory(null).Returns(expenseCategoryMock);

			FuelType fuelTypeMock = Substitute.For<FuelType>();
			fuelTypeMock.Cost.Returns(30);

			Car carMock = Substitute.For<Car>();
			carMock.IsCompanyCar.Returns(true);
			
			Organization organisationMock = Substitute.For<Organization>();

			IUnitOfWork uowMock = Substitute.For<IUnitOfWork>();

			VodovozRouteList routeListMock = Substitute.For<VodovozRouteList>();
			Subdivision subdivisionMock = Substitute.For<Subdivision>();
			routeListMock.ClosingSubdivision = subdivisionMock;

			IFuelRepository fuelRepositoryMock = Substitute.For<IFuelRepository>();
			fuelRepositoryMock.GetFuelBalanceForSubdivision(uowMock, subdivisionMock, fuelTypeMock).Returns(50);
			
			OrganizationParametersProvider organisationParametersProviderMock =
				Substitute.For<OrganizationParametersProvider>(_parametersProvider);
			
			CashDistributionCommonOrganisationProvider commonOrganisationProviderMock = 
				Substitute.For<CashDistributionCommonOrganisationProvider>(organisationParametersProviderMock);
			commonOrganisationProviderMock.GetCommonOrganisation(uowMock).Returns(organisationMock);

			var fuelDocument = new FuelDocument();
			fuelDocument.Driver = Substitute.For<Employee>();
			fuelDocument.Car = carMock;
			fuelDocument.Date = DateTime.Now;
			fuelDocument.LastEditDate = DateTime.Now;
			fuelDocument.Fuel = fuelTypeMock;
			fuelDocument.RouteList = routeListMock;
			fuelDocument.UoW = uowMock;
			fuelDocument.FuelCoupons = 0;
			fuelDocument.PayedForFuel = 0;

			// act, assert
			Assert.Throws(typeof(ValidationException),
				() => fuelDocument.CreateOperations(fuelRepositoryMock, commonOrganisationProviderMock, categoryRepositoryMock));

			// additional assert
			Assert.That(fuelDocument.FuelOperation, Is.Null, "При исключении в момент создания операций, операции выдачи топлива не должно быть создано");
		}

		#endregion CreateFuelOperationTests

		#region CreateFuelExpenseOperationTests

		[Test(Description = "Создание операции списания топлива")]
		public void CreateFuelExpenseOperationTest()
		{
			// arrange
			var categoryRepositoryMock = Substitute.For<ICategoryRepository>();
			expenseCategoryMock = Substitute.For<ExpenseCategory>();
			categoryRepositoryMock.FuelDocumentExpenseCategory(null).Returns(expenseCategoryMock);

			FuelType fuelTypeMock = Substitute.For<FuelType>();
			fuelTypeMock.Cost.Returns(30);

			Car carMock = Substitute.For<Car>();
			carMock.IsCompanyCar.Returns(false);
			
			Organization organisationMock = Substitute.For<Organization>();

			IUnitOfWork uowMock = Substitute.For<IUnitOfWork>();

			VodovozRouteList routeListMock = Substitute.For<VodovozRouteList>();
			Subdivision subdivisionMock = Substitute.For<Subdivision>();
			routeListMock.ClosingSubdivision = subdivisionMock;

			IFuelRepository fuelRepositoryMock = Substitute.For<IFuelRepository>();
			fuelRepositoryMock.GetFuelBalanceForSubdivision(uowMock, subdivisionMock, fuelTypeMock).Returns(50);
			
			OrganizationParametersProvider organisationParametersProviderMock =
				Substitute.For<OrganizationParametersProvider>(_parametersProvider);
			
			CashDistributionCommonOrganisationProvider commonOrganisationProviderMock = 
				Substitute.For<CashDistributionCommonOrganisationProvider>(organisationParametersProviderMock);
			commonOrganisationProviderMock.GetCommonOrganisation(uowMock).Returns(organisationMock);

			var fuelDocument = new FuelDocument();
			fuelDocument.Driver = Substitute.For<Employee>();
			fuelDocument.Car = carMock;
			fuelDocument.Date = DateTime.Now;
			fuelDocument.LastEditDate = DateTime.Now;
			fuelDocument.Fuel = fuelTypeMock;
			fuelDocument.RouteList = routeListMock;
			fuelDocument.UoW = uowMock;
			fuelDocument.FuelCoupons = 40;
			fuelDocument.PayedForFuel = null;
			fuelDocument.Subdivision = subdivisionMock;

			// act
			fuelDocument.CreateOperations(fuelRepositoryMock, commonOrganisationProviderMock, categoryRepositoryMock);

			// assert
			AssertsAccumulator.Create
				.Accumulate(() => Assert.That(fuelDocument.FuelExpenseOperation.FuelDocument, Is.SameAs(fuelDocument), "Документ в операции должен совпадать с документом выдачи топлива"))
				.Accumulate(() => Assert.That(fuelDocument.FuelExpenseOperation.FuelTransferDocument, Is.Null, "Документ перемещения топлива не должен быть заполнен"))
				.Accumulate(() => Assert.That(fuelDocument.FuelExpenseOperation.RelatedToSubdivision, Is.SameAs(fuelDocument.RouteList.ClosingSubdivision), "Подразделение в операции должно совпадать с подразделением в маршрутном листе"))
				.Accumulate(() => Assert.That(fuelDocument.FuelExpenseOperation.FuelLiters, Is.EqualTo(fuelDocument.FuelCoupons), "Списанное топливо должно совпадать с топливом выданным талонами в документе выдачи"))
				.Release();
		}

		[Test(Description = "При отсутствии статьи расхода должно выбрасываться исключение InvalidProgrammException и не должна создаваться операция списания топлива")]
		public void CreateFuelExpenseOperationTest__Without_ExpenseCategory__Thrown_InvalidProgrammException_and_FuelExpenseOperation_must_be_null()
		{
			// arrange
			// имитация того что нужной статьи не было найдено
			// arrange
			var categoryRepositoryMock = Substitute.For<ICategoryRepository>();
			expenseCategoryMock = null;
			categoryRepositoryMock.FuelDocumentExpenseCategory(null).Returns(expenseCategoryMock);
			var fuelDocument = new FuelDocument();
			FuelType fuelTypeMock = Substitute.For<FuelType>();
			IUnitOfWork uowMock = Substitute.For<IUnitOfWork>();
			Subdivision subdivisionMock = Substitute.For<Subdivision>();
			Organization organisationMock = Substitute.For<Organization>();
			IFuelRepository fuelRepositoryMock = Substitute.For<IFuelRepository>();
			
			OrganizationParametersProvider organisationParametersProviderMock =
				Substitute.For<OrganizationParametersProvider>(_parametersProvider);
			
			CashDistributionCommonOrganisationProvider commonOrganisationProviderMock = 
				Substitute.For<CashDistributionCommonOrganisationProvider>(organisationParametersProviderMock);
			commonOrganisationProviderMock.GetCommonOrganisation(uowMock).Returns(organisationMock);

			// act, assert
			Assert.Throws(typeof(InvalidProgramException),
				() => fuelDocument.CreateOperations(fuelRepositoryMock, commonOrganisationProviderMock, categoryRepositoryMock));

			// additional assert
			Assert.That(fuelDocument.FuelExpenseOperation, Is.Null, "При исключении в момент создания операций, операции списания топлива не должно быть создано");
		}

		[Test(Description = "При не правильном количестве литров топлива должно выбрасываться исключение ValidationException и не должна создаваться операция списания топлива")]
		public void CreateFuelExpenseOperationTest__FuelCoupons_and_PayedForFuel__Thrown_ValidationException_and_FuelExpenseOperation_must_be_null()
		{
			// arrange
			var categoryRepositoryMock = Substitute.For<ICategoryRepository>();
			expenseCategoryMock = Substitute.For<ExpenseCategory>();
			categoryRepositoryMock.FuelDocumentExpenseCategory(null).Returns(expenseCategoryMock);

			FuelType fuelTypeMock = Substitute.For<FuelType>();
			fuelTypeMock.Cost.Returns(30);

			Car carMock = Substitute.For<Car>();
			carMock.IsCompanyCar.Returns(true);
			
			Organization organisationMock = Substitute.For<Organization>();

			IUnitOfWork uowMock = Substitute.For<IUnitOfWork>();

			VodovozRouteList routeListMock = Substitute.For<VodovozRouteList>();
			Subdivision subdivisionMock = Substitute.For<Subdivision>();
			routeListMock.ClosingSubdivision = subdivisionMock;

			IFuelRepository fuelRepositoryMock = Substitute.For<IFuelRepository>();
			fuelRepositoryMock.GetFuelBalanceForSubdivision(uowMock, subdivisionMock, fuelTypeMock).Returns(50);
			
			OrganizationParametersProvider organisationParametersProviderMock =
				Substitute.For<OrganizationParametersProvider>(_parametersProvider);
			
			CashDistributionCommonOrganisationProvider commonOrganisationProviderMock = 
				Substitute.For<CashDistributionCommonOrganisationProvider>(organisationParametersProviderMock);
			commonOrganisationProviderMock.GetCommonOrganisation(uowMock).Returns(organisationMock);

			var fuelDocument = new FuelDocument();
			fuelDocument.Driver = Substitute.For<Employee>();
			fuelDocument.Car = carMock;
			fuelDocument.Date = DateTime.Now;
			fuelDocument.LastEditDate = DateTime.Now;
			fuelDocument.Fuel = fuelTypeMock;
			fuelDocument.RouteList = routeListMock;
			fuelDocument.UoW = uowMock;
			fuelDocument.FuelCoupons = 0;
			fuelDocument.PayedForFuel = null;

			// act, assert
			Assert.Throws(typeof(ValidationException),
				() => fuelDocument.CreateOperations(fuelRepositoryMock, commonOrganisationProviderMock, categoryRepositoryMock));

			// additional assert
			Assert.That(fuelDocument.FuelExpenseOperation, Is.Null, "При исключении в момент создания операций, операции списания топлива не должно быть создано");
		}

		#endregion CreateFuelExpenseOperationTests

		#region CreateFuelCashExpenseOperationTests

		[Test(Description = "При отсутствии статьи расхода должно выбрасываться исключение InvalidProgrammException и не должна создаваться операция оплаты топлива")]
		public void CreateFuelCashExpenseTest__Without_ExpenseCategory__Thrown_InvalidProgrammException_and_FuelCashExpense_must_be_null()
		{
			// arrange
			// имитация того что нужной статьи не было найдено
			// arrange
			ICategoryRepository categoryRepositoryMock = Substitute.For<ICategoryRepository>();
			expenseCategoryMock = null;
			categoryRepositoryMock.FuelDocumentExpenseCategory(null).Returns(expenseCategoryMock);
			var fuelDocument = new FuelDocument();
			FuelType fuelTypeMock = Substitute.For<FuelType>();
			IUnitOfWork uowMock = Substitute.For<IUnitOfWork>();
			Subdivision subdivisionMock = Substitute.For<Subdivision>();
			Organization organisationMock = Substitute.For<Organization>();
			IFuelRepository fuelRepositoryMock = Substitute.For<IFuelRepository>();
			
			OrganizationParametersProvider organisationParametersProviderMock =
				Substitute.For<OrganizationParametersProvider>(_parametersProvider);
			
			CashDistributionCommonOrganisationProvider commonOrganisationProviderMock = 
				Substitute.For<CashDistributionCommonOrganisationProvider>(organisationParametersProviderMock);
			commonOrganisationProviderMock.GetCommonOrganisation(uowMock).Returns(organisationMock);

			// act, assert
			Assert.Throws(typeof(InvalidProgramException),
				() => fuelDocument.CreateOperations(fuelRepositoryMock, commonOrganisationProviderMock, categoryRepositoryMock));

			// additional assert
			Assert.That(fuelDocument.FuelCashExpense, Is.Null, "При исключении в момент создания операций, операции оплаты топлива не должно быть создано");
		}

		[Test(Description = "При не правильном количестве литров топлива должно выбрасываться исключение ValidationException и не должна создаваться операция оплаты топлива")]
		public void CreateFuelCashExpenseTest__Incorrect_FuelCoupons_and_PayedForFuel__Thrown_ValidationException_and_FuelCashExpense_must_be_null()
		{
			// arrange
			// arrange
			var categoryRepositoryMock = Substitute.For<ICategoryRepository>();
			expenseCategoryMock = Substitute.For<ExpenseCategory>();
			categoryRepositoryMock.FuelDocumentExpenseCategory(null).Returns(expenseCategoryMock);

			FuelType fuelTypeMock = Substitute.For<FuelType>();
			fuelTypeMock.Cost.Returns(30);

			Car carMock = Substitute.For<Car>();
			carMock.IsCompanyCar.Returns(true);
			
			Organization organisationMock = Substitute.For<Organization>();

			IUnitOfWork uowMock = Substitute.For<IUnitOfWork>();

			VodovozRouteList routeListMock = Substitute.For<VodovozRouteList>();
			Subdivision subdivisionMock = Substitute.For<Subdivision>();
			routeListMock.ClosingSubdivision = subdivisionMock;

			IFuelRepository fuelRepositoryMock = Substitute.For<IFuelRepository>();
			fuelRepositoryMock.GetFuelBalanceForSubdivision(uowMock, subdivisionMock, fuelTypeMock).Returns(50);
			
			OrganizationParametersProvider organisationParametersProviderMock =
				Substitute.For<OrganizationParametersProvider>(_parametersProvider);
			
			CashDistributionCommonOrganisationProvider commonOrganisationProviderMock = 
				Substitute.For<CashDistributionCommonOrganisationProvider>(organisationParametersProviderMock);
			commonOrganisationProviderMock.GetCommonOrganisation(uowMock).Returns(organisationMock);

			var fuelDocument = new FuelDocument();
			fuelDocument.Driver = Substitute.For<Employee>();
			fuelDocument.Car = carMock;
			fuelDocument.Date = DateTime.Now;
			fuelDocument.LastEditDate = DateTime.Now;
			fuelDocument.Fuel = fuelTypeMock;
			fuelDocument.RouteList = routeListMock;
			fuelDocument.UoW = uowMock;
			fuelDocument.PayedForFuel = 0;

			// act, assert
			Assert.Throws(typeof(ValidationException),
				() => fuelDocument.CreateOperations(fuelRepositoryMock, commonOrganisationProviderMock, categoryRepositoryMock));

			// additional assert
			Assert.That(fuelDocument.FuelCashExpense, Is.Null, "При исключении в момент создания операций, операции оплаты топлива не должно быть создано");
		}

		[Test(Description = "Если не указано топливо оплаченное деньгами не должна создаваться операция оплаты топлива")]
		public void CreateFuelCashExpenseTest__Without_PayedFor_Fuel__FuelCashExpense_must_be_null()
		{
			// arrange
			var categoryRepositoryMock = Substitute.For<ICategoryRepository>();
			expenseCategoryMock = Substitute.For<ExpenseCategory>();
			categoryRepositoryMock.FuelDocumentExpenseCategory(null).Returns(expenseCategoryMock);

			FuelType fuelTypeMock = Substitute.For<FuelType>();
			fuelTypeMock.Cost.Returns(30);

			Car carMock = Substitute.For<Car>();
			carMock.IsCompanyCar.Returns(true);
			
			Organization organisationMock = Substitute.For<Organization>();

			IUnitOfWork uowMock = Substitute.For<IUnitOfWork>();

			VodovozRouteList routeListMock = Substitute.For<VodovozRouteList>();
			Subdivision subdivisionMock = Substitute.For<Subdivision>();
			routeListMock.ClosingSubdivision = subdivisionMock;

			IFuelRepository fuelRepositoryMock = Substitute.For<IFuelRepository>();
			fuelRepositoryMock.GetFuelBalanceForSubdivision(uowMock, subdivisionMock, fuelTypeMock).Returns(50);
			
			OrganizationParametersProvider organisationParametersProviderMock =
				Substitute.For<OrganizationParametersProvider>(_parametersProvider);
			
			CashDistributionCommonOrganisationProvider commonOrganisationProviderMock = 
				Substitute.For<CashDistributionCommonOrganisationProvider>(organisationParametersProviderMock);
			commonOrganisationProviderMock.GetCommonOrganisation(uowMock).Returns(organisationMock);

			var fuelDocument = new FuelDocument();
			fuelDocument.Driver = Substitute.For<Employee>();
			fuelDocument.Car = carMock;
			fuelDocument.Date = DateTime.Now;
			fuelDocument.LastEditDate = DateTime.Now;
			fuelDocument.Fuel = fuelTypeMock;
			fuelDocument.RouteList = routeListMock;
			fuelDocument.UoW = uowMock;
			fuelDocument.FuelCoupons = 40;
			fuelDocument.PayedForFuel = null;
			fuelDocument.Subdivision = subdivisionMock;

			// act
			fuelDocument.CreateOperations(fuelRepositoryMock, commonOrganisationProviderMock, categoryRepositoryMock);

			// assert
			Assert.That(fuelDocument.FuelCashExpense, Is.Null);
		}

		[Test(Description = "Если указано нулевое топливо оплаченное деньгами не должна создаваться операция оплаты топлива")]
		public void CreateFuelCashExpenseTest__With_Zero_PayedFor_Fuel__FuelCashExpense_must_be_null()
		{
			// arrange
			var categoryRepositoryMock = Substitute.For<ICategoryRepository>();
			expenseCategoryMock = Substitute.For<ExpenseCategory>();
			categoryRepositoryMock.FuelDocumentExpenseCategory(null).Returns(expenseCategoryMock);

			FuelType fuelTypeMock = Substitute.For<FuelType>();
			fuelTypeMock.Cost.Returns(30);

			Car carMock = Substitute.For<Car>();
			carMock.IsCompanyCar.Returns(true);
			
			Organization organisationMock = Substitute.For<Organization>();

			IUnitOfWork uowMock = Substitute.For<IUnitOfWork>();

			VodovozRouteList routeListMock = Substitute.For<VodovozRouteList>();
			Subdivision subdivisionMock = Substitute.For<Subdivision>();
			routeListMock.ClosingSubdivision = subdivisionMock;

			IFuelRepository fuelRepositoryMock = Substitute.For<IFuelRepository>();
			fuelRepositoryMock.GetFuelBalanceForSubdivision(uowMock, subdivisionMock, fuelTypeMock).Returns(50);
			
			OrganizationParametersProvider organisationParametersProviderMock =
				Substitute.For<OrganizationParametersProvider>(_parametersProvider);
			
			CashDistributionCommonOrganisationProvider commonOrganisationProviderMock = 
				Substitute.For<CashDistributionCommonOrganisationProvider>(organisationParametersProviderMock);
			commonOrganisationProviderMock.GetCommonOrganisation(uowMock).Returns(organisationMock);

			var fuelDocument = new FuelDocument();
			fuelDocument.Driver = Substitute.For<Employee>();
			fuelDocument.Car = carMock;
			fuelDocument.Date = DateTime.Now;
			fuelDocument.LastEditDate = DateTime.Now;
			fuelDocument.Fuel = fuelTypeMock;
			fuelDocument.RouteList = routeListMock;
			fuelDocument.UoW = uowMock;
			fuelDocument.FuelCoupons = 40;
			fuelDocument.PayedForFuel = 0;
			fuelDocument.Subdivision = subdivisionMock;

			// act
			fuelDocument.CreateOperations(fuelRepositoryMock, commonOrganisationProviderMock, categoryRepositoryMock);

			// assert
			Assert.That(fuelDocument.FuelCashExpense, Is.Null);
		}

		[Test(Description = "Создание операции оплаты топлива")]
		public void CreateFuelCashExpenseTest()
		{
			// arrange
			var categoryRepositoryMock = Substitute.For<ICategoryRepository>();
			expenseCategoryMock = Substitute.For<ExpenseCategory>();
			categoryRepositoryMock.FuelDocumentExpenseCategory(null).Returns(expenseCategoryMock);

			FuelType fuelTypeMock = Substitute.For<FuelType>();
			fuelTypeMock.Cost.Returns(30);

			Car carMock = Substitute.For<Car>();
			carMock.IsCompanyCar.Returns(false);
			
			Organization organisationMock = Substitute.For<Organization>();

			VodovozRouteList routeListMock = Substitute.For<VodovozRouteList>();
			Subdivision subdivisionMock = Substitute.For<Subdivision>();
			IUnitOfWork uowMock = Substitute.For<IUnitOfWork>();
			IFuelRepository fuelRepositoryMock = Substitute.For<IFuelRepository>();
			fuelRepositoryMock.GetFuelBalanceForSubdivision(uowMock, subdivisionMock, fuelTypeMock).Returns(50);

			routeListMock.ClosingSubdivision = subdivisionMock;
			
			OrganizationParametersProvider organisationParametersProviderMock =
				Substitute.For<OrganizationParametersProvider>(_parametersProvider);
			
			CashDistributionCommonOrganisationProvider commonOrganisationProviderMock = 
				Substitute.For<CashDistributionCommonOrganisationProvider>(organisationParametersProviderMock);
			commonOrganisationProviderMock.GetCommonOrganisation(uowMock).Returns(organisationMock);

			var fuelDocument = new FuelDocument();
			fuelDocument.Driver = Substitute.For<Employee>();
			fuelDocument.FuelPaymentType = FuelPaymentType.Cash;
			fuelDocument.Car = carMock;
			fuelDocument.Date = DateTime.Now;
			fuelDocument.LastEditDate = DateTime.Now;
			fuelDocument.Fuel = fuelTypeMock;
			fuelDocument.RouteList = routeListMock;
			fuelDocument.UoW = uowMock;
			fuelDocument.FuelCoupons = 40;
			fuelDocument.PayedForFuel = 500;
			fuelDocument.Subdivision = subdivisionMock;

			// act
			fuelDocument.CreateOperations(fuelRepositoryMock, commonOrganisationProviderMock, categoryRepositoryMock);

			// assert
			AssertsAccumulator.Create
				.Accumulate(() => Assert.That(fuelDocument.FuelCashExpense.Casher, Is.SameAs(fuelDocument.Author)))
				.Accumulate(() => Assert.That(fuelDocument.FuelCashExpense.Employee, Is.SameAs(fuelDocument.Driver)))
				.Accumulate(() => Assert.That(fuelDocument.FuelCashExpense.RelatedToSubdivision, Is.SameAs(fuelDocument.RouteList.ClosingSubdivision)))
				.Release();
		}

		public new static IEnumerable PayedForFuelDecimalValuesForRound {
			get {
				yield return new object[] { 80M, 80M };
				yield return new object[] { 80.111M, 80.11M };
				yield return new object[] { 80.115M, 80.12M };
				yield return new object[] { 80.116M, 80.12M };
			}
		}

		[TestCaseSource(nameof(PayedForFuelDecimalValuesForRound))]
		[Test(Description = "Проверка расчета суммы выданной за топливо")]
		public void CreateFuelCashExpenseTest_Money(decimal payedForFuel, decimal result)
		{
			// arrange
			var categoryRepositoryMock = Substitute.For<ICategoryRepository>();
			expenseCategoryMock = Substitute.For<ExpenseCategory>();
			categoryRepositoryMock.FuelDocumentExpenseCategory(null).Returns(expenseCategoryMock);

			FuelType fuelTypeMock = Substitute.For<FuelType>();
			fuelTypeMock.Cost.Returns(30);

			Car carMock = Substitute.For<Car>();
			carMock.IsCompanyCar.Returns(false);
			
			Organization organisationMock = Substitute.For<Organization>();

			IUnitOfWork uowMock = Substitute.For<IUnitOfWork>();

			VodovozRouteList routeListMock = Substitute.For<VodovozRouteList>();
			Subdivision subdivisionMock = Substitute.For<Subdivision>();
			routeListMock.ClosingSubdivision = subdivisionMock;

			IFuelRepository fuelRepositoryMock = Substitute.For<IFuelRepository>();
			fuelRepositoryMock.GetFuelBalanceForSubdivision(uowMock, subdivisionMock, fuelTypeMock).Returns(50);
			
			var organisationParametersProviderMock = Substitute.For<OrganizationParametersProvider>(_parametersProvider);
			organisationParametersProviderMock.CommonCashDistributionOrganisationId.Returns(2);
			
			CashDistributionCommonOrganisationProvider commonOrganisationProviderMock = 
				Substitute.For<CashDistributionCommonOrganisationProvider>(organisationParametersProviderMock);
			commonOrganisationProviderMock.GetCommonOrganisation(uowMock).Returns(organisationMock);

			var fuelDocument = new FuelDocument();
			fuelDocument.Driver = Substitute.For<Employee>();
			fuelDocument.Car = carMock;
			fuelDocument.FuelPaymentType = FuelPaymentType.Cash;
			fuelDocument.Date = DateTime.Now;
			fuelDocument.LastEditDate = DateTime.Now;
			fuelDocument.Fuel = fuelTypeMock;
			fuelDocument.RouteList = routeListMock;
			fuelDocument.UoW = uowMock;
			fuelDocument.FuelCoupons = 40;
			fuelDocument.PayedForFuel = payedForFuel;
			fuelDocument.Subdivision = subdivisionMock;

			// act
			fuelDocument.CreateOperations(fuelRepositoryMock, commonOrganisationProviderMock, categoryRepositoryMock);

			// assert
			Assert.That(fuelDocument.FuelCashExpense.Money, Is.EqualTo(result));
		}

		#endregion CreateFuelCashExpenseOperationTests

		#endregion CreateFuelDocumentOperationsTests
		
		#region PayedLitersTests

		public new static IEnumerable PayedForFuelDecimalValues {
			get {
				yield return new object[] { 70M, 30M, 2.33M };
				yield return new object[] { 75M, 30M, 2.5M };
				yield return new object[] { 76.65M, 30M, 2.56M };
				yield return new object[] { 80M, 30M, 2.67M };
			}
		}
		[TestCaseSource(nameof(PayedForFuelDecimalValues))]
		[Test(Description = "Расчет количества оплачиваемного топлива, исходя из суммы выдаваемой на оплату топлива")]
		public void PayedLitersTest(decimal payedForFuel, decimal fuelCost, decimal fuelLitersExpectedResult)
		{
			// arrange
			FuelType fuelTypeMock = Substitute.For<FuelType>();
			fuelTypeMock.Cost.Returns(fuelCost);

			FuelDocument fuelDocument = new FuelDocument();
			fuelDocument.Fuel = fuelTypeMock;
			fuelDocument.PayedForFuel = payedForFuel;

			// act, assert
			string message = $"Оплачено: {payedForFuel}, стоимость: {fuelCost}, ожидаемое количество топлива: {fuelLitersExpectedResult}, расчитано: {fuelDocument.PayedLiters}";
			Assert.IsTrue(fuelDocument.PayedLiters == fuelLitersExpectedResult, message);
		}

		[Test(Description = "Если не указано сколько топлива оплачено деньгами то количество оплаченного топлива должно быть 0")]
		public void PayedLitersTest_PayedForFuel_IsNull()
		{
			// arrange
			FuelType fuelTypeMock = Substitute.For<FuelType>();
			fuelTypeMock.Cost.Returns(30);

			FuelDocument fuelDocument = new FuelDocument();
			fuelDocument.Fuel = fuelTypeMock;
			fuelDocument.PayedForFuel = null;

			// act, assert
			Assert.IsTrue(fuelDocument.PayedLiters == 0);
		}

		[Test(Description = "Если не указан тип топлива то количество оплаченного топлива должно быть 0")]
		public void PayedLitersTest_Fuel_IsNull()
		{
			// arrange
			FuelDocument fuelDocument = new FuelDocument();
			fuelDocument.Fuel = null;
			fuelDocument.PayedForFuel = 500;

			// act, assert
			Assert.IsTrue(fuelDocument.PayedLiters == 0);
		}

		#endregion
	}
}
