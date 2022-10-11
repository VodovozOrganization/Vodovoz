using NUnit.Framework;
using System;
using NSubstitute;
using Vodovoz.Domain.Fuel;
using Vodovoz.Domain.Employees;
using Vodovoz;
using Vodovoz.Domain.Logistic;
using QS.DomainModel.UoW;
using Vodovoz.Domain.Logistic.Cars;
using Vodovoz.EntityRepositories.Fuel;

namespace VodovozBusinessTests.Domain.Fuel
{
	[TestFixture()]
	public class FuelTransferDocumentTests
	{
		#region SendTests

		private decimal transferedLitersForSend = 50;

		[Test(Description = "Создание операции перемещения топлива при отправке документа перемещения")]
		public void SendTest_CreateFuelTransferOperation()
		{
			// arrange
			var cashier = Substitute.For<Employee>();
			var subdivisionFrom = Substitute.For<Subdivision>();
			subdivisionFrom.Id.Returns(1);

			var subdivisionTo = Substitute.For<Subdivision>();
			subdivisionTo.Id.Returns(2);

			FuelType fuelTypeMock = Substitute.For<FuelType>();
			IUnitOfWork uowMock = Substitute.For<IUnitOfWork>();

			IFuelRepository fuelRepositoryMock = Substitute.For<IFuelRepository>();
			fuelRepositoryMock.GetFuelBalanceForSubdivision(uowMock, subdivisionFrom, fuelTypeMock).Returns(50);

			var document = new FuelTransferDocument();
			document.FuelType = fuelTypeMock;
			document.Author = Substitute.For<Employee>();
			document.Driver = Substitute.For<Employee>();
			document.Car = Substitute.For<Car>();

			document.CashSubdivisionFrom = subdivisionFrom;
			document.CashSubdivisionTo = subdivisionTo;
			document.Status = FuelTransferDocumentStatuses.New;
			document.TransferedLiters = transferedLitersForSend;

			// act
			document.Send(cashier, fuelRepositoryMock);

			// assert
			AssertsAccumulator.Create
				.Accumulate(() => Assert.That(document.FuelTransferOperation.ReceiveTime, Is.Null))
				.Accumulate(() => Assert.That(document.FuelTransferOperation.SendTime, Is.EqualTo(document.SendTime)))
				.Accumulate(() => Assert.That(document.FuelTransferOperation.SubdivisionFrom, Is.SameAs(document.CashSubdivisionFrom)))
				.Accumulate(() => Assert.That(document.FuelTransferOperation.SubdivisionTo, Is.SameAs(document.CashSubdivisionTo)))
				.Accumulate(() => Assert.That(document.FuelTransferOperation.TransferedLiters, Is.EqualTo(document.TransferedLiters)))
				.Release();
		}

		[Test(Description = "Если не указан кассир, то должно выбрасыватся исключение ArgumentNullException")]
		public void SendTest__TransferDocument_Without_Cashier__Thrown_ArgumentNullException()
		{
			// arrange
			var subdivisionFrom = Substitute.For<Subdivision>();
			subdivisionFrom.Id.Returns(1);

			var subdivisionTo = Substitute.For<Subdivision>();
			subdivisionTo.Id.Returns(2);

			FuelType fuelTypeMock = Substitute.For<FuelType>();
			IUnitOfWork uowMock = Substitute.For<IUnitOfWork>();

			IFuelRepository fuelRepositoryMock = Substitute.For<IFuelRepository>();
			fuelRepositoryMock.GetFuelBalanceForSubdivision(uowMock, subdivisionFrom, fuelTypeMock).Returns(50);

			var document = new FuelTransferDocument();
			document.FuelType = fuelTypeMock;
			document.Author = Substitute.For<Employee>();
			document.Driver = Substitute.For<Employee>();
			document.Car = Substitute.For<Car>();

			document.CashSubdivisionFrom = subdivisionFrom;
			document.CashSubdivisionTo = subdivisionTo;
			document.Status = FuelTransferDocumentStatuses.New;
			document.TransferedLiters = transferedLitersForSend;
			var parameterName = document.GetType().GetMethod(nameof(document.Send)).GetParameters()[0].Name;

			// act
			var exception = Assert.Throws<ArgumentNullException>(() => document.Send(null, fuelRepositoryMock));

			// assert
			Assert.That(exception.ParamName, Is.EqualTo(parameterName));
		}

		[Test(Description = "Если статус документа не равен New, то должно выбрасыватся исключение InvalidOperationException")]
		public void SendTest__TransferDocument_Send_From_Not_New_Status__Thrown_InvalidOperationException()
		{
			// arrange
			var cashier = Substitute.For<Employee>();
			var subdivisionFrom = Substitute.For<Subdivision>();
			subdivisionFrom.Id.Returns(1);

			var subdivisionTo = Substitute.For<Subdivision>();
			subdivisionTo.Id.Returns(2);

			FuelType fuelTypeMock = Substitute.For<FuelType>();
			IUnitOfWork uowMock = Substitute.For<IUnitOfWork>();

			IFuelRepository fuelRepositoryMock = Substitute.For<IFuelRepository>();
			fuelRepositoryMock.GetFuelBalanceForSubdivision(uowMock, subdivisionFrom, fuelTypeMock).Returns(50);

			var document = new FuelTransferDocument();
			document.FuelType = fuelTypeMock;
			document.Author = Substitute.For<Employee>();
			document.Driver = Substitute.For<Employee>();
			document.Car = Substitute.For<Car>();

			document.CashSubdivisionFrom = subdivisionFrom;
			document.CashSubdivisionTo = subdivisionTo;
			document.TransferedLiters = transferedLitersForSend;
			document.Status = FuelTransferDocumentStatuses.Sent;

			// act, assert
			Assert.Throws<InvalidOperationException>(() => document.Send(cashier, fuelRepositoryMock));
		}

		[Test(Description = "Если операция перемещения топлива уже была создана, то должно выбрасыватся исключение InvalidOperationException")]
		public void SendTest__TransferDocument_FuelTransferOperation_already_exists__Thrown_InvalidOperationException()
		{
			// arrange
			var cashier = Substitute.For<Employee>();
			var subdivisionFrom = Substitute.For<Subdivision>();
			subdivisionFrom.Id.Returns(1);

			var subdivisionTo = Substitute.For<Subdivision>();
			subdivisionTo.Id.Returns(2);

			FuelType fuelTypeMock = Substitute.For<FuelType>();
			IUnitOfWork uowMock = Substitute.For<IUnitOfWork>();

			IFuelRepository fuelRepositoryMock = Substitute.For<IFuelRepository>();
			fuelRepositoryMock.GetFuelBalanceForSubdivision(uowMock, subdivisionFrom, fuelTypeMock).Returns(50);

			var document = new FuelTransferDocument();
			document.FuelType = fuelTypeMock;
			document.Author = Substitute.For<Employee>();
			document.Driver = Substitute.For<Employee>();
			document.Car = Substitute.For<Car>();

			document.CashSubdivisionFrom = subdivisionFrom;
			document.CashSubdivisionTo = subdivisionTo;
			document.Status = FuelTransferDocumentStatuses.New;
			document.TransferedLiters = transferedLitersForSend;
			document.FuelTransferOperation = Substitute.For<FuelTransferOperation>();

			// act, assert
			Assert.Throws<InvalidOperationException>(() => document.Send(cashier, fuelRepositoryMock));
		}

		[Test(Description = "Создание операции списания топлива при отправке документа перемещения")]
		public void SendTest_CreateFuelExpenseOperation()
		{
			// arrange
			var cashier = Substitute.For<Employee>();
			var subdivisionFrom = Substitute.For<Subdivision>();
			subdivisionFrom.Id.Returns(1);

			var subdivisionTo = Substitute.For<Subdivision>();
			subdivisionTo.Id.Returns(2);

			FuelType fuelTypeMock = Substitute.For<FuelType>();
			IUnitOfWork uowMock = Substitute.For<IUnitOfWork>();

			IFuelRepository fuelRepositoryMock = Substitute.For<IFuelRepository>();
			fuelRepositoryMock.GetFuelBalanceForSubdivision(uowMock, subdivisionFrom, fuelTypeMock).Returns(50);

			var document = new FuelTransferDocument();
			document.FuelType = fuelTypeMock;
			document.Author = Substitute.For<Employee>();
			document.Driver = Substitute.For<Employee>();
			document.Car = Substitute.For<Car>();

			document.CashSubdivisionFrom = subdivisionFrom;
			document.CashSubdivisionTo = subdivisionTo;
			document.Status = FuelTransferDocumentStatuses.New;
			document.TransferedLiters = transferedLitersForSend;

			// act
			document.Send(cashier, fuelRepositoryMock);

			// assert
			AssertsAccumulator.Create
				.Accumulate(() => Assert.That(document.FuelExpenseOperation.FuelTransferDocument, Is.EqualTo(document)))
				.Accumulate(() => Assert.That(document.FuelExpenseOperation.FuelDocument, Is.Null))
				.Accumulate(() => Assert.That(document.FuelExpenseOperation.RelatedToSubdivision, Is.SameAs(document.CashSubdivisionFrom)))
				.Accumulate(() => Assert.That(document.FuelExpenseOperation.FuelLiters, Is.EqualTo(document.TransferedLiters)))
				.Release();
		}

		[Test(Description = "Если операция списания топлива уже была создана, то должно выбрасыватся исключение InvalidOperationException")]
		public void SendTest__TransferDocument_FuelExpenseOperation_already_exists__Thrown_InvalidOperationException()
		{
			// arrange
			var cashier = Substitute.For<Employee>();
			var subdivisionFrom = Substitute.For<Subdivision>();
			subdivisionFrom.Id.Returns(1);

			var subdivisionTo = Substitute.For<Subdivision>();
			subdivisionTo.Id.Returns(2);

			FuelType fuelTypeMock = Substitute.For<FuelType>();
			IUnitOfWork uowMock = Substitute.For<IUnitOfWork>();
			IFuelRepository fuelRepositoryMock = Substitute.For<IFuelRepository>();

			fuelRepositoryMock.GetFuelBalanceForSubdivision(uowMock, subdivisionFrom, fuelTypeMock).Returns(50);

			var document = new FuelTransferDocument();
			document.FuelType = fuelTypeMock;
			document.Author = Substitute.For<Employee>();
			document.Driver = Substitute.For<Employee>();
			document.Car = Substitute.For<Car>();

			document.CashSubdivisionFrom = subdivisionFrom;
			document.CashSubdivisionTo = subdivisionTo;
			document.Status = FuelTransferDocumentStatuses.New;
			document.TransferedLiters = transferedLitersForSend;
			document.FuelExpenseOperation = Substitute.For<FuelExpenseOperation>();

			// act, assert
			Assert.Throws<InvalidOperationException>(() => document.Send(cashier, fuelRepositoryMock));
		}

		#endregion SendTests

		#region ReceiveTests

		private decimal transferedLitersForReceive = 50;
		private DateTime sendTimeForReceive = DateTime.Now.AddHours(-1);

		[Test(Description = "После получения документа перемещения топлива, время получения в документе перемещения должно быть заполнено")]
		public void ReceiveTest_TransferDocument_ReceiveTime_IsNotNull()
		{
			// arrange
			var receiver = Substitute.For<Employee>();
			var subdivisionFrom = Substitute.For<Subdivision>();
			subdivisionFrom.Id.Returns(1);

			var subdivisionTo = Substitute.For<Subdivision>();
			subdivisionTo.Id.Returns(2);

			FuelType fuelTypeMock = Substitute.For<FuelType>();
			var document = new FuelTransferDocument();
			document.FuelType = fuelTypeMock;

			var transferOperation = Substitute.For<FuelTransferOperation>();
			transferOperation.ReceiveTime = null;
			transferOperation.SendTime = sendTimeForReceive;
			transferOperation.SubdivisionFrom = subdivisionFrom;
			transferOperation.SubdivisionTo = subdivisionTo;
			transferOperation.TransferedLiters = transferedLitersForReceive;

			var expenseOperation = Substitute.For<FuelExpenseOperation>();
			expenseOperation.FuelTransferDocument = document;
			expenseOperation.FuelDocument = null;
			expenseOperation.СreationTime = sendTimeForReceive;
			expenseOperation.RelatedToSubdivision = subdivisionFrom;
			expenseOperation.FuelLiters = transferedLitersForReceive;

			document.Author = Substitute.For<Employee>();
			document.Driver = Substitute.For<Employee>();
			document.Car = Substitute.For<Car>();

			document.SendTime = sendTimeForReceive;
			document.CashSubdivisionFrom = subdivisionFrom;
			document.CashSubdivisionTo = subdivisionTo;
			document.Status = FuelTransferDocumentStatuses.Sent;
			document.TransferedLiters = transferedLitersForReceive;
			document.FuelTransferOperation = transferOperation;
			document.FuelExpenseOperation = expenseOperation;

			// act
			document.Receive(receiver);

			// assert
			Assert.That(document.ReceiveTime, Is.Not.Null);
		}

		[Test(Description = "После получения документа перемещения топлива, время получения в операции перемещения должно быть заполнено")]
		public void ReceiveTest_TransferOperation_ReceiveTime_IsNotNull()
		{
			// arrange
			var receiver = Substitute.For<Employee>();
			var subdivisionFrom = Substitute.For<Subdivision>();
			subdivisionFrom.Id.Returns(1);

			var subdivisionTo = Substitute.For<Subdivision>();
			subdivisionTo.Id.Returns(2);

			FuelType fuelTypeMock = Substitute.For<FuelType>();
			var document = new FuelTransferDocument();
			document.FuelType = fuelTypeMock;

			var transferOperation = Substitute.For<FuelTransferOperation>();
			transferOperation.ReceiveTime = null;
			transferOperation.SendTime = sendTimeForReceive;
			transferOperation.SubdivisionFrom = subdivisionFrom;
			transferOperation.SubdivisionTo = subdivisionTo;
			transferOperation.TransferedLiters = transferedLitersForReceive;

			var expenseOperation = Substitute.For<FuelExpenseOperation>();
			expenseOperation.FuelTransferDocument = document;
			expenseOperation.FuelDocument = null;
			expenseOperation.СreationTime = sendTimeForReceive;
			expenseOperation.RelatedToSubdivision = subdivisionFrom;
			expenseOperation.FuelLiters = transferedLitersForReceive;

			document.Author = Substitute.For<Employee>();
			document.Driver = Substitute.For<Employee>();
			document.Car = Substitute.For<Car>();

			document.SendTime = sendTimeForReceive;
			document.CashSubdivisionFrom = subdivisionFrom;
			document.CashSubdivisionTo = subdivisionTo;
			document.Status = FuelTransferDocumentStatuses.Sent;
			document.TransferedLiters = transferedLitersForReceive;
			document.FuelTransferOperation = transferOperation;
			document.FuelExpenseOperation = expenseOperation;

			// act
			document.Receive(receiver);

			// assert
			Assert.That(document.FuelTransferOperation.ReceiveTime, Is.Not.Null);
		}

		[Test(Description = "После получения документа перемещения топлива, время получения в операции должно быть равно времени получения в документе")]
		public void ReceiveTest__TransferDocument_FilledCorrectly__Are_equals_TransferOperation_ReceiveTime_and_TransferDocument_ReceiveTime()
		{
			// arrange
			var receiver = Substitute.For<Employee>();
			var subdivisionFrom = Substitute.For<Subdivision>();
			subdivisionFrom.Id.Returns(1);

			var subdivisionTo = Substitute.For<Subdivision>();
			subdivisionTo.Id.Returns(2);

			FuelType fuelTypeMock = Substitute.For<FuelType>();
			var document = new FuelTransferDocument();
			document.FuelType = fuelTypeMock;

			var transferOperation = Substitute.For<FuelTransferOperation>();
			transferOperation.ReceiveTime = null;
			transferOperation.SendTime = sendTimeForReceive;
			transferOperation.SubdivisionFrom = subdivisionFrom;
			transferOperation.SubdivisionTo = subdivisionTo;
			transferOperation.TransferedLiters = transferedLitersForReceive;

			var expenseOperation = Substitute.For<FuelExpenseOperation>();
			expenseOperation.FuelTransferDocument = document;
			expenseOperation.FuelDocument = null;
			expenseOperation.СreationTime = sendTimeForReceive;
			expenseOperation.RelatedToSubdivision = subdivisionFrom;
			expenseOperation.FuelLiters = transferedLitersForReceive;

			document.Author = Substitute.For<Employee>();
			document.Driver = Substitute.For<Employee>();
			document.Car = Substitute.For<Car>();

			document.SendTime = sendTimeForReceive;
			document.CashSubdivisionFrom = subdivisionFrom;
			document.CashSubdivisionTo = subdivisionTo;
			document.Status = FuelTransferDocumentStatuses.Sent;
			document.TransferedLiters = transferedLitersForReceive;
			document.FuelTransferOperation = transferOperation;
			document.FuelExpenseOperation = expenseOperation;

			// act
			document.Receive(receiver);

			// assert
			Assert.IsTrue(document.FuelTransferOperation.ReceiveTime.Value == document.ReceiveTime.Value);
		}

		[Test(Description = "После получения документа перемещения топлива, статус документа должен быть равен статусу Получен")]
		public void ReceiveTest__TransferDocument_FilledCorrectly__TransferDocument_Status_Is_Received()
		{
			// arrange
			var receiver = Substitute.For<Employee>();
			var subdivisionFrom = Substitute.For<Subdivision>();
			subdivisionFrom.Id.Returns(1);

			var subdivisionTo = Substitute.For<Subdivision>();
			subdivisionTo.Id.Returns(2);

			FuelType fuelTypeMock = Substitute.For<FuelType>();
			var document = new FuelTransferDocument();
			document.FuelType = fuelTypeMock;

			var transferOperation = Substitute.For<FuelTransferOperation>();
			transferOperation.ReceiveTime = null;
			transferOperation.SendTime = sendTimeForReceive;
			transferOperation.SubdivisionFrom = subdivisionFrom;
			transferOperation.SubdivisionTo = subdivisionTo;
			transferOperation.TransferedLiters = transferedLitersForReceive;

			var expenseOperation = Substitute.For<FuelExpenseOperation>();
			expenseOperation.FuelTransferDocument = document;
			expenseOperation.FuelDocument = null;
			expenseOperation.СreationTime = sendTimeForReceive;
			expenseOperation.RelatedToSubdivision = subdivisionFrom;
			expenseOperation.FuelLiters = transferedLitersForReceive;

			document.Author = Substitute.For<Employee>();
			document.Driver = Substitute.For<Employee>();
			document.Car = Substitute.For<Car>();

			document.SendTime = sendTimeForReceive;
			document.CashSubdivisionFrom = subdivisionFrom;
			document.CashSubdivisionTo = subdivisionTo;
			document.Status = FuelTransferDocumentStatuses.Sent;
			document.TransferedLiters = transferedLitersForReceive;
			document.FuelTransferOperation = transferOperation;
			document.FuelExpenseOperation = expenseOperation;

			// act
			document.Receive(receiver);

			// assert
			Assert.IsTrue(document.Status == FuelTransferDocumentStatuses.Received);
		}

		[Test(Description = "Создание операции прихода топлива")]
		public void ReceiveTest_CreateFuelIncomeOperation()
		{
			// arrange
			var receiver = Substitute.For<Employee>();
			var subdivisionFrom = Substitute.For<Subdivision>();
			subdivisionFrom.Id.Returns(1);

			var subdivisionTo = Substitute.For<Subdivision>();
			subdivisionTo.Id.Returns(2);

			FuelType fuelTypeMock = Substitute.For<FuelType>();
			var document = new FuelTransferDocument();
			document.FuelType = fuelTypeMock;

			var transferOperation = Substitute.For<FuelTransferOperation>();
			transferOperation.ReceiveTime = null;
			transferOperation.SendTime = sendTimeForReceive;
			transferOperation.SubdivisionFrom = subdivisionFrom;
			transferOperation.SubdivisionTo = subdivisionTo;
			transferOperation.TransferedLiters = transferedLitersForReceive;

			var expenseOperation = Substitute.For<FuelExpenseOperation>();
			expenseOperation.FuelTransferDocument = document;
			expenseOperation.FuelDocument = null;
			expenseOperation.СreationTime = sendTimeForReceive;
			expenseOperation.RelatedToSubdivision = subdivisionFrom;
			expenseOperation.FuelLiters = transferedLitersForReceive;

			document.Author = Substitute.For<Employee>();
			document.Driver = Substitute.For<Employee>();
			document.Car = Substitute.For<Car>();

			document.SendTime = sendTimeForReceive;
			document.CashSubdivisionFrom = subdivisionFrom;
			document.CashSubdivisionTo = subdivisionTo;
			document.Status = FuelTransferDocumentStatuses.Sent;
			document.TransferedLiters = transferedLitersForReceive;
			document.FuelTransferOperation = transferOperation;
			document.FuelExpenseOperation = expenseOperation;

			// act
			document.Receive(receiver);

			// assert
			AssertsAccumulator.Create
				.Accumulate(() => Assert.That(document.FuelIncomeOperation.FuelTransferDocument, Is.EqualTo(document)))
				.Accumulate(() => Assert.That(document.FuelIncomeOperation.FuelIncomeInvoiceItem, Is.Null))
				.Accumulate(() => Assert.That(document.FuelIncomeOperation.RelatedToSubdivision, Is.SameAs(document.CashSubdivisionTo)))
				.Accumulate(() => Assert.That(document.FuelIncomeOperation.FuelLiters, Is.EqualTo(document.TransferedLiters)))
				.Release();
		}

		[Test(Description = "Если не указан кассир при принятии документа перемещения, то должно выбрасыватся исключение ArgumentNullException")]
		public void ReceiveTest__TransferDocument_without_cashier_receiver__Thrown_ArgumentNullException()
		{
			// arrange
			var subdivisionFrom = Substitute.For<Subdivision>();
			subdivisionFrom.Id.Returns(1);

			var subdivisionTo = Substitute.For<Subdivision>();
			subdivisionTo.Id.Returns(2);

			FuelType fuelTypeMock = Substitute.For<FuelType>();
			var document = new FuelTransferDocument();
			document.FuelType = fuelTypeMock;

			var transferOperation = Substitute.For<FuelTransferOperation>();
			transferOperation.ReceiveTime = null;
			transferOperation.SendTime = sendTimeForReceive;
			transferOperation.SubdivisionFrom = subdivisionFrom;
			transferOperation.SubdivisionTo = subdivisionTo;
			transferOperation.TransferedLiters = transferedLitersForReceive;

			var expenseOperation = Substitute.For<FuelExpenseOperation>();
			expenseOperation.FuelTransferDocument = document;
			expenseOperation.FuelDocument = null;
			expenseOperation.СreationTime = sendTimeForReceive;
			expenseOperation.RelatedToSubdivision = subdivisionFrom;
			expenseOperation.FuelLiters = transferedLitersForReceive;

			document.Author = Substitute.For<Employee>();
			document.Driver = Substitute.For<Employee>();
			document.Car = Substitute.For<Car>();

			document.SendTime = sendTimeForReceive;
			document.CashSubdivisionFrom = subdivisionFrom;
			document.CashSubdivisionTo = subdivisionTo;
			document.Status = FuelTransferDocumentStatuses.Sent;
			document.TransferedLiters = transferedLitersForReceive;
			document.FuelTransferOperation = transferOperation;
			document.FuelExpenseOperation = expenseOperation;
			var parameterName = document.GetType().GetMethod(nameof(document.Receive)).GetParameters()[0].Name;

			// act
			var exception = Assert.Throws<ArgumentNullException>(() => document.Receive(null));

			// assert
			Assert.That(exception.ParamName, Is.EqualTo(parameterName));
		}

		[Test(Description = "Если не было создано операции перемещения топлива, то должно выбрасыватся исключение InvalidOperationException")]
		public void ReceiveTest__TransferDocument_without_TransferOperation__Thrown_InvalidOperationException()
		{
			// arrange
			var receiver = Substitute.For<Employee>();
			var subdivisionFrom = Substitute.For<Subdivision>();
			subdivisionFrom.Id.Returns(1);

			var subdivisionTo = Substitute.For<Subdivision>();
			subdivisionTo.Id.Returns(2);

			FuelType fuelTypeMock = Substitute.For<FuelType>();
			var document = new FuelTransferDocument();
			document.FuelType = fuelTypeMock;

			var transferOperation = Substitute.For<FuelTransferOperation>();
			transferOperation.ReceiveTime = null;
			transferOperation.SendTime = sendTimeForReceive;
			transferOperation.SubdivisionFrom = subdivisionFrom;
			transferOperation.SubdivisionTo = subdivisionTo;
			transferOperation.TransferedLiters = transferedLitersForReceive;

			var expenseOperation = Substitute.For<FuelExpenseOperation>();
			expenseOperation.FuelTransferDocument = document;
			expenseOperation.FuelDocument = null;
			expenseOperation.СreationTime = sendTimeForReceive;
			expenseOperation.RelatedToSubdivision = subdivisionFrom;
			expenseOperation.FuelLiters = transferedLitersForReceive;

			document.Author = Substitute.For<Employee>();
			document.Driver = Substitute.For<Employee>();
			document.Car = Substitute.For<Car>();

			document.SendTime = sendTimeForReceive;
			document.CashSubdivisionFrom = subdivisionFrom;
			document.CashSubdivisionTo = subdivisionTo;
			document.Status = FuelTransferDocumentStatuses.Sent;
			document.TransferedLiters = transferedLitersForReceive;
			document.FuelTransferOperation = transferOperation;
			document.FuelExpenseOperation = expenseOperation;
			document.FuelTransferOperation = null;

			// act, assert
			var exception = Assert.Throws<InvalidOperationException>(() => document.Receive(receiver));
		}

		[Test(Description = "Если при получении документ находтся не в статусе Отправлен, то должно выбрасыватся исключение InvalidOperationException")]
		public void ReceiveTest__TransferDocument_with_incorrect_status__Thrown_InvalidOperationException()
		{
			// arrange
			var receiver = Substitute.For<Employee>();
			var subdivisionFrom = Substitute.For<Subdivision>();
			subdivisionFrom.Id.Returns(1);

			var subdivisionTo = Substitute.For<Subdivision>();
			subdivisionTo.Id.Returns(2);

			FuelType fuelTypeMock = Substitute.For<FuelType>();
			var document = new FuelTransferDocument();
			document.FuelType = fuelTypeMock;

			var transferOperation = Substitute.For<FuelTransferOperation>();
			transferOperation.ReceiveTime = null;
			transferOperation.SendTime = sendTimeForReceive;
			transferOperation.SubdivisionFrom = subdivisionFrom;
			transferOperation.SubdivisionTo = subdivisionTo;
			transferOperation.TransferedLiters = transferedLitersForReceive;

			var expenseOperation = Substitute.For<FuelExpenseOperation>();
			expenseOperation.FuelTransferDocument = document;
			expenseOperation.FuelDocument = null;
			expenseOperation.СreationTime = sendTimeForReceive;
			expenseOperation.RelatedToSubdivision = subdivisionFrom;
			expenseOperation.FuelLiters = transferedLitersForReceive;

			document.Author = Substitute.For<Employee>();
			document.Driver = Substitute.For<Employee>();
			document.Car = Substitute.For<Car>();

			document.SendTime = sendTimeForReceive;
			document.CashSubdivisionFrom = subdivisionFrom;
			document.CashSubdivisionTo = subdivisionTo;
			document.TransferedLiters = transferedLitersForReceive;
			document.FuelTransferOperation = transferOperation;
			document.FuelExpenseOperation = expenseOperation;
			document.Status = FuelTransferDocumentStatuses.Received;

			// act, assert
			Assert.Throws<InvalidOperationException>(() => document.Receive(receiver));
		}

		[Test(Description = "Если при получении уже была создана операция прихода топлива, то должно выбрасыватся исключение InvalidOperationException")]
		public void ReceiveTest__TransferDocument_FuelIncomeOperation_already_exists__Thrown_InvalidOperationException()
		{
			// arrange
			var receiver = Substitute.For<Employee>();
			var subdivisionFrom = Substitute.For<Subdivision>();
			subdivisionFrom.Id.Returns(1);

			var subdivisionTo = Substitute.For<Subdivision>();
			subdivisionTo.Id.Returns(2);

			FuelType fuelTypeMock = Substitute.For<FuelType>();
			var document = new FuelTransferDocument();
			document.FuelType = fuelTypeMock;

			var transferOperation = Substitute.For<FuelTransferOperation>();
			transferOperation.ReceiveTime = null;
			transferOperation.SendTime = sendTimeForReceive;
			transferOperation.SubdivisionFrom = subdivisionFrom;
			transferOperation.SubdivisionTo = subdivisionTo;
			transferOperation.TransferedLiters = transferedLitersForReceive;

			var expenseOperation = Substitute.For<FuelExpenseOperation>();
			expenseOperation.FuelTransferDocument = document;
			expenseOperation.FuelDocument = null;
			expenseOperation.СreationTime = sendTimeForReceive;
			expenseOperation.RelatedToSubdivision = subdivisionFrom;
			expenseOperation.FuelLiters = transferedLitersForReceive;

			document.Author = Substitute.For<Employee>();
			document.Driver = Substitute.For<Employee>();
			document.Car = Substitute.For<Car>();

			document.SendTime = sendTimeForReceive;
			document.CashSubdivisionFrom = subdivisionFrom;
			document.CashSubdivisionTo = subdivisionTo;
			document.Status = FuelTransferDocumentStatuses.Sent;
			document.TransferedLiters = transferedLitersForReceive;
			document.FuelTransferOperation = transferOperation;
			document.FuelExpenseOperation = expenseOperation;
			document.FuelIncomeOperation = Substitute.For<FuelIncomeOperation>();

			// act, assert
			var exception = Assert.Throws<InvalidOperationException>(() => document.Receive(receiver));
		}

		[Test(Description = "Если при получении уже было указано время получения топлива, то должно выбрасыватся исключение InvalidOperationException")]
		public void ReceiveTest__TransferDocument_FuelTransferOperation_ReceiveTime_already_exists__Thrown_InvalidOperationException()
		{
			// arrange
			var receiver = Substitute.For<Employee>();
			var subdivisionFrom = Substitute.For<Subdivision>();
			subdivisionFrom.Id.Returns(1);

			var subdivisionTo = Substitute.For<Subdivision>();
			subdivisionTo.Id.Returns(2);

			FuelType fuelTypeMock = Substitute.For<FuelType>();
			var document = new FuelTransferDocument();
			document.FuelType = fuelTypeMock;

			var transferOperation = Substitute.For<FuelTransferOperation>();
			transferOperation.ReceiveTime = null;
			transferOperation.SendTime = sendTimeForReceive;
			transferOperation.SubdivisionFrom = subdivisionFrom;
			transferOperation.SubdivisionTo = subdivisionTo;
			transferOperation.TransferedLiters = transferedLitersForReceive;

			var expenseOperation = Substitute.For<FuelExpenseOperation>();
			expenseOperation.FuelTransferDocument = document;
			expenseOperation.FuelDocument = null;
			expenseOperation.СreationTime = sendTimeForReceive;
			expenseOperation.RelatedToSubdivision = subdivisionFrom;
			expenseOperation.FuelLiters = transferedLitersForReceive;

			document.Author = Substitute.For<Employee>();
			document.Driver = Substitute.For<Employee>();
			document.Car = Substitute.For<Car>();

			document.SendTime = sendTimeForReceive;
			document.CashSubdivisionFrom = subdivisionFrom;
			document.CashSubdivisionTo = subdivisionTo;
			document.Status = FuelTransferDocumentStatuses.Sent;
			document.TransferedLiters = transferedLitersForReceive;
			document.FuelTransferOperation = transferOperation;
			document.FuelExpenseOperation = expenseOperation;
			document.FuelTransferOperation.ReceiveTime = DateTime.Now;

			// act, assert
			var exception = Assert.Throws<InvalidOperationException>(() => document.Receive(receiver));
		}

		#endregion ReceiveTests
	}
}
