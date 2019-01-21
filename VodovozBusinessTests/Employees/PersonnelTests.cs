using NUnit.Framework;
using System;
using NSubstitute;
using Vodovoz.Domain.Employees;
using QS.DomainModel.UoW;
using System.Collections.Generic;
using System.Linq;

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
			traineeMock.DrivingNumber.Returns("TestPassportSeria");
			traineeMock.AddressRegistration.Returns("TestPassportSeria");
			traineeMock.AddressCurrent.Returns("TestPassportSeria");
			traineeMock.INN.Returns("TestPassportSeria");
			var phones = new List<QSContacts.Phone>();
			//TODO возможно здесь нужно сделать мок для Phone
			phones.Add(new QSContacts.Phone() {
				Name = "TestPhoneName"
			});
			traineeMock.Phones.Returns(phones);
			//TODO возможно здесь нужно сделать мок для Nationality
			traineeMock.Nationality.Returns(new Nationality { Name = "TestNationality" });
			traineeMock.Photo.Returns(new byte[] { 1, 2, 3 });
		}

		void CreateEmployeeMock()
		{
			employeeMock = Substitute.For(
				new[] { typeof(IEmployee), typeof(Employee) },
				new object[0]
			) as Employee;
			employeeMock.Phones.Returns(new List<QSContacts.Phone>());
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
			Personnel.ChangeTraineeToEmployee(uowEmployeeMock, traineeMock);

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
			if(traineeMock.DrivingNumber != employeeMock.DrivingNumber) {
				result = false;
				output += nameof(traineeMock.DrivingNumber) + ", ";
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
