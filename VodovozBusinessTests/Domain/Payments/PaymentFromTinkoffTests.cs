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
				new string[] { "22512", "2018-11-20 23:15:15", "vodovoz-spb",  "630.0", "NEW", "", "jenetajardon@gmail.com", "79998298933" },
				new PaymentByCardOnline{
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
				new string[] { "29512",  "2019-01-20 03:15:15", "vodovoz-ololo",  "1130.0", "CANCELED", "", "jeon@gmail.com", "70008298933" },
				new PaymentByCardOnline{
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
				new string[] { "22222", "2020-12-22 22:22:15", "vodovoz-spb",  "220.0", "FORMSHOWED", "", "j2n@gmail.com", "79998222933" },
				new PaymentByCardOnline{
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
				new string[] { "11512",  "2118-11-20 23:15:15", "vodovoz-spb1", "631.1", "DEADLINE_EXPIRED", "", "111@gmail.com", "79998298913" },
				new PaymentByCardOnline{
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
				new string[] { "12512", "2011-11-20 23:15:15", "1-spb",  "130.0", "AUTHORIZING", "", "jenetajardon@1.com", "19998298933" },
				new PaymentByCardOnline{
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
				new string[] { "333", "2038-11-23 23:33:15", "3vodovoz-spb", "633.0", "3DS_CHECKING", "", "sd@gmail.com", "73338298933" },
				new PaymentByCardOnline{
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
				new string[] { "221512", "2218-11-20 23:15:22", "vodovoz-m", "130.0", "3DS_CHECKED", "", "", "719998298933" },
				new PaymentByCardOnline{
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
				new string[] { "55512", "2018-11-20 11:15:15", "5vodovoz-spb", "5555.5", "AUTH_FAIL", "", "555@gmail.com", "" },
				new PaymentByCardOnline{
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
				new string[] { "00512", "2008-11-20 03:10:05", "",  "030.0", "AUTHORIZED", "", "jenetajardon11@gmail.com", "71118298933" },
				new PaymentByCardOnline{
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
				new string[] { "1", "2011-11-20 11:15:15", "vodovoz-1", "1.0", "REVERSING", "", "1@gmail.com", "11111" },
				new PaymentByCardOnline{
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
				new string[] { "22", "2012-02-20 23:22:22", "222-spb", "630.2", "REVERSED", "", "jenetajardon@2.com", "79998298922" },
				new PaymentByCardOnline{
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
				new string[] { "22212", "2218-11-20 23:15:15", "", "6322.0", "CONFIRMING", "", "jenetajardon@2gmail.com", "" },
				new PaymentByCardOnline{
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
				new string[] { "225121", "2018-01-20 03:15:15", "vodovoz111spb",  "6310.0", "CONFIRMED", "", "1jenetajardon@gmail.com", "71998298933" },
				new PaymentByCardOnline{
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
				new string[] { "202512", "2010-11-20 23:15:15", "1111", "0630.0", "REFUNDING", "", "0@gmail.com", "719998298933" },
				new PaymentByCardOnline{
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
				new string[] { "225112", "2018-08-20 23:15:15", "11-spb", "111.0", "PARTIAL_REFUNDED", "", "111@gmail.com", "11111" },
				new PaymentByCardOnline{
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
				new string[] { "12", "2018-12-12 12:15:15", "12-12",  "12.0", "REFUNDED", "", "12@gmail.com", "12" },
				new PaymentByCardOnline{
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
				new string[] { "221512", "2000-11-20 23:15:15", "vodovoz111spb", "6310.0", "REJECTED", "", "jenetajardon@1gmail.com", "79998298911" },
				new PaymentByCardOnline{
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
				new string[] { "225172", "2018-01-20 23:15:15", "vodovoz-spb", "630.7", "WRONG_STATUS", "", "jenetajardon@11.com", "79998298933" },
				new PaymentByCardOnline{
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
		public void PaymentFromTinkoff_CreatingOfNewInstanceUseingCtorWithStringParameter_SuccessfullCreation(string[] parameter, PaymentByCardOnline result)
		{
			// arrange and act
			PaymentByCardOnline testPayment = new PaymentByCardOnline(parameter);

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
