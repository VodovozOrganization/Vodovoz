using System;
using System.Collections;
using NUnit.Framework;
using Vodovoz.Domain.Payments;

namespace VodovozBusinessTests.Domain.Payments
{
	[TestFixture]
	public class PaymentFromTinkoffTests
	{
		static IEnumerable StringArraysAndResultPayment()
		{
			yield return new object[] {
				new string[] { "NEW", "2018-11-20 23:15:15", "22512", "630.0", "jenetajardon@gmail.com", "79998298933", "vodovoz-spb" },
				new PaymentFromTinkoff{
					PaymentStatus = PaymentStatus.NEW,
					DateAndTime = new DateTime(2018,11,20,23,15,15),
					PaymentNr = 22512,
					PaymentRUR = 630,
					Email = "jenetajardon@gmail.com",
					Phone = "79998298933",
					Shop = "vodovoz-spb"
				}
			};
			yield return new object[] {
				new string[] { "CANCELED", "2019-01-20 03:15:15", "29512", "1130.0", "jeon@gmail.com", "70008298933", "vodovoz-ololo" },
				new PaymentFromTinkoff{
					PaymentStatus = PaymentStatus.CANCELED,
					DateAndTime = new DateTime(2019,01,20,03,15,15),
					PaymentNr = 29512,
					PaymentRUR = 1130,
					Email = "jeon@gmail.com",
					Phone = "70008298933",
					Shop = "vodovoz-ololo"
				}
			};
			yield return new object[] {
				new string[] { "FORMSHOWED", "2020-12-22 22:22:15", "22222", "220.0", "j2n@gmail.com", "79998222933", "vodovoz-spb" },
				new PaymentFromTinkoff{
					PaymentStatus = PaymentStatus.FORMSHOWED,
					DateAndTime = new DateTime(2020,12,22,22,22,15),
					PaymentNr = 22222,
					PaymentRUR = 220,
					Email = "j2n@gmail.com",
					Phone = "79998222933",
					Shop = "vodovoz-spb"
				}
			};
			yield return new object[] {
				new string[] { "DEADLINE_EXPIRED", "2118-11-20 23:15:15", "11512", "631.1", "111@gmail.com", "79998298913", "vodovoz-spb1" },
				new PaymentFromTinkoff{
					PaymentStatus = PaymentStatus.DEADLINE_EXPIRED,
					DateAndTime = new DateTime(2118,11,20,23,15,15),
					PaymentNr = 11512,
					PaymentRUR = 631.1M,
					Email = "111@gmail.com",
					Phone = "79998298913",
					Shop = "vodovoz-spb1"
				}
			};
			yield return new object[] {
				new string[] { "AUTHORIZING", "2011-11-20 23:15:15", "12512", "130.0", "jenetajardon@1.com", "19998298933", "1-spb" },
				new PaymentFromTinkoff{
					PaymentStatus = PaymentStatus.AUTHORIZING,
					DateAndTime = new DateTime(2011,11,20,23,15,15),
					PaymentNr = 12512,
					PaymentRUR = 130,
					Email = "jenetajardon@1.com",
					Phone = "19998298933",
					Shop = "1-spb"
				}
			};
			yield return new object[] {
				new string[] { "3DS_CHECKING", "2038-11-23 23:33:15", "333", "633.0", "sd@gmail.com", "73338298933", "3vodovoz-spb" },
				new PaymentFromTinkoff{
					PaymentStatus = PaymentStatus.CHECKING,
					DateAndTime = new DateTime(2038,11,23,23,33,15),
					PaymentNr = 333,
					PaymentRUR = 633,
					Email = "sd@gmail.com",
					Phone = "73338298933",
					Shop = "3vodovoz-spb"
				}
			};
			yield return new object[] {
				new string[] { "3DS_CHECKED", "2218-11-20 23:15:22", "221512", "130.0", "", "719998298933", "vodovoz-m" },
				new PaymentFromTinkoff{
					PaymentStatus = PaymentStatus.CHECKED,
					DateAndTime = new DateTime(2218,11,20,23,15,22),
					PaymentNr = 221512,
					PaymentRUR = 130,
					Email = "",
					Phone = "719998298933",
					Shop = "vodovoz-m"
				}
			};
			yield return new object[] {
				new string[] { "AUTH_FAIL", "2018-11-20 11:15:15", "55512", "5555.5", "555@gmail.com", "", "5vodovoz-spb" },
				new PaymentFromTinkoff{
					PaymentStatus = PaymentStatus.AUTH_FAIL,
					DateAndTime = new DateTime(2018,11,20,11,15,15),
					PaymentNr = 55512,
					PaymentRUR = 5555.5M,
					Email = "555@gmail.com",
					Phone = "",
					Shop = "5vodovoz-spb"
				}
			};
			yield return new object[] {
				new string[] { "AUTHORIZED", "2008-11-20 03:10:05", "00512", "030.0", "jenetajardon11@gmail.com", "71118298933", "" },
				new PaymentFromTinkoff{
					PaymentStatus = PaymentStatus.AUTHORIZED,
					DateAndTime = new DateTime(2008,11,20,3,10,5),
					PaymentNr = 512,
					PaymentRUR = 30,
					Email = "jenetajardon11@gmail.com",
					Phone = "71118298933",
					Shop = ""
				}
			};
			yield return new object[] {
				new string[] { "REVERSING", "2011-11-20 11:15:15", "1", "1.0", "1@gmail.com", "11111", "vodovoz-1" },
				new PaymentFromTinkoff{
					PaymentStatus = PaymentStatus.REVERSING,
					DateAndTime = new DateTime(2011,11,20,11,15,15),
					PaymentNr = 1,
					PaymentRUR = 1,
					Email = "1@gmail.com",
					Phone = "11111",
					Shop = "vodovoz-1"
				}
			};
			yield return new object[] {
				new string[] { "REVERSED", "2012-02-20 23:22:22", "22", "630.2", "jenetajardon@2.com", "79998298922", "222-spb" },
				new PaymentFromTinkoff{
					PaymentStatus = PaymentStatus.REVERSED,
					DateAndTime = new DateTime(2012,2,20,23,22,22),
					PaymentNr = 22,
					PaymentRUR = 630.2M,
					Email = "jenetajardon@2.com",
					Phone = "79998298922",
					Shop = "222-spb"
				}
			};
			yield return new object[] {
				new string[] { "CONFIRMING", "2218-11-20 23:15:15", "22212", "6322.0", "jenetajardon@2gmail.com", "", "" },
				new PaymentFromTinkoff{
					PaymentStatus = PaymentStatus.CONFIRMING,
					DateAndTime = new DateTime(2218,11,20,23,15,15),
					PaymentNr = 22212,
					PaymentRUR = 6322,
					Email = "jenetajardon@2gmail.com",
					Phone = "",
					Shop = ""
				}
			};
			yield return new object[] {
				new string[] { "CONFIRMED", "2018-01-20 03:15:15", "225121", "6310.0", "1jenetajardon@gmail.com", "71998298933", "vodovoz111spb" },
				new PaymentFromTinkoff{
					PaymentStatus = PaymentStatus.CONFIRMED,
					DateAndTime = new DateTime(2018,1,20,3,15,15),
					PaymentNr = 225121,
					PaymentRUR = 6310,
					Email = "1jenetajardon@gmail.com",
					Phone = "71998298933",
					Shop = "vodovoz111spb"
				}
			};
			yield return new object[] {
				new string[] { "REFUNDING", "2010-11-20 23:15:15", "202512", "0630.0", "0@gmail.com", "719998298933", "1111" },
				new PaymentFromTinkoff{
					PaymentStatus = PaymentStatus.REFUNDING,
					DateAndTime = new DateTime(2010,11,20,23,15,15),
					PaymentNr = 202512,
					PaymentRUR = 630,
					Email = "0@gmail.com",
					Phone = "719998298933",
					Shop = "1111"
				}
			};
			yield return new object[] {
				new string[] { "PARTIAL_REFUNDED", "2018-08-20 23:15:15", "225112", "111.0", "111@gmail.com", "11111", "11-spb" },
				new PaymentFromTinkoff{
					PaymentStatus = PaymentStatus.PARTIAL_REFUNDED,
					DateAndTime = new DateTime(2018,8,20,23,15,15),
					PaymentNr = 225112,
					PaymentRUR = 111,
					Email = "111@gmail.com",
					Phone = "11111",
					Shop = "11-spb"
				}
			};
			yield return new object[] {
				new string[] { "REFUNDED", "2018-12-12 12:15:15", "12", "12.0", "12@gmail.com", "12", "12-12" },
				new PaymentFromTinkoff{
					PaymentStatus = PaymentStatus.REFUNDED,
					DateAndTime = new DateTime(2018,12,12,12,15,15),
					PaymentNr = 12,
					PaymentRUR = 12,
					Email = "12@gmail.com",
					Phone = "12",
					Shop = "12-12"
				}
			};
			yield return new object[] {
				new string[] { "REJECTED", "2000-11-20 23:15:15", "221512", "6310.0", "jenetajardon@1gmail.com", "79998298911", "vodovoz111spb" },
				new PaymentFromTinkoff{
					PaymentStatus = PaymentStatus.REJECTED,
					DateAndTime = new DateTime(2000,11,20,23,15,15),
					PaymentNr = 221512,
					PaymentRUR = 6310,
					Email = "jenetajardon@1gmail.com",
					Phone = "79998298911",
					Shop = "vodovoz111spb"
				}
			};
			yield return new object[] {
				new string[] { "WRONG_STATUS", "2018-01-20 23:15:15", "225172", "630.7", "jenetajardon@11.com", "79998298933", "vodovoz-spb" },
				new PaymentFromTinkoff{
					PaymentStatus = PaymentStatus.Unacceptable,
					DateAndTime = new DateTime(2018,1,20,23,15,15),
					PaymentNr = 225172,
					PaymentRUR = 630.7M,
					Email = "jenetajardon@11.com",
					Phone = "79998298933",
					Shop = "vodovoz-spb"
				}
			};
		}

		[Test(Description = "Создание экземпляра сущьности через конструктор массивом строк")]
		[TestCaseSource(nameof(StringArraysAndResultPayment))]
		public void PaymentFromTinkoff_CreatingOfNewInstanceUseingCtorWithStringParameter_SuccessfullCreation(string[] parameter, PaymentFromTinkoff result)
		{
			// arrange and act
			PaymentFromTinkoff testPayment = new PaymentFromTinkoff(parameter);

			// assert
			Assert.That(testPayment.PaymentStatus, Is.EqualTo(result.PaymentStatus));
			Assert.That(testPayment.DateAndTime, Is.EqualTo(result.DateAndTime));
			Assert.That(testPayment.PaymentNr, Is.EqualTo(result.PaymentNr));
			Assert.That(testPayment.PaymentRUR, Is.EqualTo(result.PaymentRUR));
			Assert.That(testPayment.Email, Is.EqualTo(result.Email));
			Assert.That(testPayment.Phone, Is.EqualTo(result.Phone));
			Assert.That(testPayment.Shop, Is.EqualTo(result.Shop));
		}
	}
}
