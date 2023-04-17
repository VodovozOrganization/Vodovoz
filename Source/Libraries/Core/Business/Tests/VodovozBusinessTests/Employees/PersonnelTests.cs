﻿using NUnit.Framework;
using System;
using NSubstitute;
using Vodovoz.Domain.Employees;
using QS.DomainModel.UoW;
using System.Collections.Generic;

namespace VodovozBusinessTests.Employees
{
	[TestFixture(TestOf = typeof(Personnel))]
	public class PersonnelTests
	{
		ITrainee traineeMock;
		IEmployee employeeMock;
		IUnitOfWorkGeneric<Employee> uowEmployeeMock;
		public PersonnelTests()
		{
			CreateTraineeMock();
			CreateEmployeeMock();
			CreateUoWEmployeeMock();
		}

		#region Create mocks

		void CreateTraineeMock()
		{
			traineeMock = Substitute.For(
				new[] { typeof(ITrainee), typeof(Trainee) },
				new object[0]
			) as ITrainee;
			traineeMock.Id.Returns(1);
			traineeMock.CreationDate.Returns(DateTime.Now);
			traineeMock.Name.Returns("TestName");
			traineeMock.LastName.Returns("TestLastName");
			traineeMock.Patronymic.Returns("TestPatronymic");
			traineeMock.DrivingLicense.Returns("TestPassportSeria");
			traineeMock.AddressRegistration.Returns("TestPassportSeria");
			traineeMock.AddressCurrent.Returns("TestPassportSeria");
			traineeMock.INN.Returns("TestPassportSeria");

			var phones = new List<Vodovoz.Domain.Contacts.Phone>();
			//TODO возможно здесь нужно сделать мок для Phone
			phones.Add(new Vodovoz.Domain.Contacts.Phone { Comment = "TestPhoneName" });
			traineeMock.Phones.Returns(phones);

			var documents = new List<EmployeeDocument>();
			documents.Add(new EmployeeDocument { Id = 5, Name = "TestDoc" });
			traineeMock.Documents.Returns(documents);

			//TODO возможно здесь нужно сделать мок для Nationality
			traineeMock.Nationality.Returns(new Nationality { Name = "TestNationality" });
			traineeMock.Citizenship.Returns(new Citizenship { Name = "TestCitizenship" });
			traineeMock.Photo.Returns(new byte[] { 1, 2, 3 });
		}

		void CreateEmployeeMock()
		{
			employeeMock = Substitute.For(
				new[] { typeof(IEmployee), typeof(Employee) },
				new object[0]
			) as Employee;
			employeeMock.Phones.Returns(new List<Vodovoz.Domain.Contacts.Phone>());
			employeeMock.Documents.Returns(new List<EmployeeDocument>());
		}

		void CreateUoWEmployeeMock()
		{
			uowEmployeeMock = Substitute.For<IUnitOfWorkGeneric<Employee>>();
			uowEmployeeMock.Root.Returns(employeeMock);
			uowEmployeeMock.GetById<Trainee>(traineeMock.Id).Returns(traineeMock);
		}

		#endregion

		#region Test cases

		[Test(Description = "Тест перевода стажера в сотрудника")]
		public void ChangeTraineeToEmployeeTestCase()
		{
			//Тестируемое действие
			Personnel.ChangeTraineeToEmployee(uowEmployeeMock.Root, traineeMock);

			//Проверка результата
			string output = "Не верно перенесены следующие свойства: ";
			bool result = true;
			if(traineeMock.CreationDate != employeeMock.CreationDate) {
				result = false;
				output += nameof(traineeMock.CreationDate) + ", ";
			}
			if(traineeMock.Name != employeeMock.Name) {
				result = false;
				output += nameof(traineeMock.Name) + ", ";
			}
			if(traineeMock.LastName != employeeMock.LastName) {
				result = false;
				output += nameof(traineeMock.LastName) + ", ";
			}
			if(traineeMock.Patronymic != employeeMock.Patronymic) {
				result = false;
				output += nameof(traineeMock.Patronymic) + ", ";
			}
			if(traineeMock.DrivingLicense != employeeMock.DrivingLicense) {
				result = false;
				output += nameof(traineeMock.DrivingLicense) + ", ";
			}
			if(traineeMock.AddressRegistration != employeeMock.AddressRegistration) {
				result = false;
				output += nameof(traineeMock.AddressRegistration) + ", ";
			}
			if(traineeMock.AddressCurrent != employeeMock.AddressCurrent) {
				result = false;
				output += nameof(traineeMock.AddressCurrent) + ", ";
			}
			if(traineeMock.INN != employeeMock.INN) {
				result = false;
				output += nameof(traineeMock.INN) + ", ";
			}
			foreach(var item in traineeMock.Phones) {
				if(!employeeMock.Phones.Contains(item)) {
					result = false;
					output += nameof(traineeMock.Phones) + ", ";
					break;
				}
			}
			foreach(var item in traineeMock.Documents) {
				if(!employeeMock.Documents.Contains(item)) {
					result = false;
					output += nameof(traineeMock.Documents) + ", ";
					break;
				}
			}
			if(traineeMock.Citizenship != employeeMock.Citizenship) {
				result = false;
				output += nameof(traineeMock.Citizenship) + ", ";
			}
			if(traineeMock.Nationality != employeeMock.Nationality) {
				result = false;
				output += nameof(traineeMock.Nationality) + ", ";
			}if(traineeMock.Photo != employeeMock.Photo) {
				result = false;
				output += nameof(traineeMock.Photo);
			}

			Assert.IsTrue(result, output);
		}

		#endregion

	}
}
