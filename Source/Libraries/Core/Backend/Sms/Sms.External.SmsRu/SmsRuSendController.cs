using NLog;
using Sms.External.Interface;
using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Gamma.Utilities;
using Microsoft.Extensions.Options;


namespace Sms.External.SmsRu
{
	public partial class SmsRuSendController : ISmsSender, ISmsBalanceNotifier
	{
		private readonly SmsRuConfiguration _configuration;
		private readonly static Logger logger = LogManager.GetCurrentClassLogger();
		private const string _balanceStringPrefix = "balance=";
		private readonly string _baseUrl;

		public SmsRuSendController(IOptions<SmsRuConfiguration> configuration)
		{
			_configuration = configuration?.Value ?? throw new ArgumentNullException(nameof(configuration));
			_baseUrl = configuration.Value.BaseUrl;
		}

		public BalanceResponse GetBalanceResponse
		{
			get
			{
				var balanceResponse = CheckBalance(EnumAuthenticationTypes.StrongApi);

				var lines = balanceResponse.Split('\n');

				var culture = CultureInfo.CreateSpecificCulture("ru-RU");
				culture.NumberFormat.NumberDecimalSeparator = ".";

				BalanceResponse balance = new BalanceResponse()
				{
					BalanceType = BalanceType.CurrencyBalance,
					BalanceValue = decimal.Parse(lines[1], NumberStyles.AllowDecimalPoint, culture.NumberFormat)
				};

				return balance;
			}
		}

		public event EventHandler<SmsBalanceEventArgs> OnBalanceChange;

		public SmsResponseStatus SendSms(ISmsMessage message)
		{
			var response = Send(_configuration.SmsNumberFrom, message.MobilePhoneNumber, message.MessageText, message.ScheduleTime);

			if(!string.IsNullOrEmpty(response))
			{
				var lines = response.Split('\n');

				var enumStatus = (Enum.Parse(typeof(SmsResponseStatus), lines[0])) as SmsResponseStatus?;

				if(enumStatus.HasValue && enumStatus.Value.IsSuccefullStatus())
				{
					var balanceLine = lines.FirstOrDefault(x => x.StartsWith(_balanceStringPrefix));

					var culture = CultureInfo.CreateSpecificCulture("ru-RU");
					culture.NumberFormat.NumberDecimalSeparator = ".";

					var startBalanceIndex = balanceLine.Substring(_balanceStringPrefix.Length);
					var balanceIsParsed = decimal.TryParse(startBalanceIndex, NumberStyles.AllowDecimalPoint | NumberStyles.AllowLeadingSign, culture.NumberFormat, out decimal newBalance);

					if(balanceLine != null && balanceIsParsed)
					{
						OnBalanceChange?.Invoke(this, new SmsBalanceEventArgs(BalanceType.CurrencyBalance, newBalance));
					}
					else
					{
						logger.Warn("Не удалось получить баланс в ответном сообщении");
					}
				}
				
				if(enumStatus == null)
				{
					throw new Exception("Неизвестный ответ от сервера");
				}

				logger.Info($"Ответ сервера: {(int)enumStatus.Value} {enumStatus.Value} {enumStatus.GetEnumTitle()}");

				return enumStatus.Value;
			}
			else
			{
				throw new Exception("Не получен ответ от сервера");
			}
		}

		public Task<SmsResponseStatus> SendSmsAsync(ISmsMessage message)
		{
			throw new NotSupportedException(); // Нет использований в нашем проекте TODO: дописать при рефакторинге библиотеки
		}

		public string CheckBalance(EnumAuthenticationTypes authType)
		{
			string result = string.Empty;
			string args = string.Empty;
			string request = string.Empty;
			string response = string.Empty;
			string token = string.Empty;

			try
			{
				logger.Info($"{DateTime.Now.ToLongDateString()}={DateTime.Now.ToLongTimeString()} Получение состояния баланса");

				token = GetToken();

				string shaStrong = HashCodeHelper.GetSHA512Hash($"{_configuration.Password}{token}").ToLower();
				string shaStrongApi = HashCodeHelper.GetSHA512Hash($"{_configuration.Password}{token}{_configuration.ApiId}").ToLower();

				if(authType == EnumAuthenticationTypes.Simple)
				{
					args = string.Format("{0}?api_id={1}", $"{_baseUrl}/my/balance", _configuration.ApiId);
				}

				if(authType == EnumAuthenticationTypes.Strong)
				{
					args = string.Format("{0}?login={1}&token={2}&sha512={3}", $"{_baseUrl}/my/balance", _configuration.Login, token, shaStrong);
				}

				if(authType == EnumAuthenticationTypes.StrongApi)
				{
					args = string.Format("{0}?login={1}&token={2}&sha512={3}", $"{_baseUrl}/my/balance", _configuration.Login, token, shaStrongApi);
				}

				request = $"{args}";

				logger.Info($"Запрос: {request}");

				using(WebResponse webResponse = WebRequest.Create(request).GetResponse())
				using(Stream stream = webResponse.GetResponseStream())
				{
					if(stream != null)
					{
						using(StreamReader streamReader = new StreamReader(stream))
						{
							response = streamReader.ReadToEnd();
							logger.Info($"Ответ: {response}");
							if(Convert.ToInt32(response.Split(new string[1] { "\n" }, StringSplitOptions.None)[0]) == Convert.ToInt32(SmsResponseStatus.MessageAccepted))
							{
								result = response;
							}
							else
							{
								logger.Info($"{DateTime.Now.ToLongDateString()}={DateTime.Now.ToLongTimeString()} Получение состояния баланса");
								logger.Info($"Ответ: {response}");
								result = string.Empty;
							}
						}
					}
				}
			}
			catch(Exception ex)
			{
				logger.Fatal("Возникла непонятная ошибка. Нужно проверить значения в файле конфигурации и разобраться в коде. Скорее всего введены неверные значения, либо сервер SMS.RU недоступен. " + ex.Message);
				logger.Trace(ex.StackTrace);
			}

			return result;
		}

		public string Send(string from, string to, string text, DateTime dateTime)
		{
			return Send(from, new string[1] { to }, text, dateTime, EnumAuthenticationTypes.Strong);
		}

		public string Send(string from, string[] to, string text, DateTime dateTime, EnumAuthenticationTypes authType)
		{
			string result = string.Empty;

			if(to.Length < 1)
			{
				throw new ArgumentNullException("to", "Неверные входные данные - массив пуст.");
			}

			if(to.Length > 100)
			{
				throw new ArgumentOutOfRangeException("to", "Неверные входные данные - слишком много элементов (больше 100) в массиве.");
			}

			if(dateTime == DateTime.MinValue)
			{
				dateTime = DateTime.Now;
			}

			string args = string.Empty;
			string request = string.Empty;
			string response = string.Empty;
			string numbers = string.Empty;
			string token = string.Empty;

			foreach(string number in to)
			{
				numbers = numbers + number + ",";
			}

			numbers = numbers.Substring(0, numbers.Length - 1);

			logger.Info($"{DateTime.Now.ToLongDateString()}={DateTime.Now.ToLongTimeString()}Отправка СМС получателям: {numbers}");

			try
			{
				token = GetToken();

				string shaStrong = HashCodeHelper.GetSHA512Hash($"{_configuration.Password}{token}").ToLower();
				string shaStrongApi = HashCodeHelper.GetSHA512Hash($"{_configuration.Password}{token}{_configuration.ApiId}").ToLower();

				if(authType == EnumAuthenticationTypes.Simple)
				{
					args = $"api_id={_configuration.ApiId}";
				}

				if(authType == EnumAuthenticationTypes.Strong)
				{
					args = $"login={_configuration.Login}&token={token}&sha512={shaStrong}";
				}

				if(authType == EnumAuthenticationTypes.StrongApi)
				{
					args = $"login={_configuration.Login}&token={token}&sha512={shaStrongApi}";
				}

				request = $"{args}&to={numbers}&text={text}&from={from}";

				if(dateTime != DateTime.MinValue)
				{
					request = request + "&time=" + TimeHelper.GetUnixTime(dateTime);
				}

				if(_configuration.PartnerId != string.Empty)
				{
					request = request + "&partner_id=" + _configuration.PartnerId;
				}

				if(_configuration.Translit)
				{
					request += "&translit=1";
				}

				if(_configuration.Test)
				{
					request += "&test=1";
				}

				logger.Info($"Запрос: {request}");

				WebRequest webRequest = WebRequest.Create($"{_baseUrl}/sms/send");
				webRequest.ContentType = "application/x-www-form-urlencoded";
				webRequest.Method = "POST";
				byte[] bytes = Encoding.UTF8.GetBytes(request);
				webRequest.ContentLength = bytes.Length;
				Stream requestStream = webRequest.GetRequestStream();
				requestStream.Write(bytes, 0, bytes.Length);
				requestStream.Close();

				using(WebResponse webResponse = webRequest.GetResponse())
				{
					if(webResponse == null)
					{
						return null;
					}

					using(StreamReader streamReader = new StreamReader(webResponse.GetResponseStream()))
					{
						response = streamReader.ReadToEnd().Trim();
					}
				}

				logger.Info($"Ответ: {response}");

				result = response;
			}
			catch(Exception ex)
			{
				logger.Fatal("Возникла непонятная ошибка. Нужно проверить значения в файле конфигурации и разобраться в коде. Скорее всего введены неверные значения, либо сервер SMS.RU недоступен. " + ex.Message);
				logger.Trace( ex.StackTrace);
			}

			return result;
		}

		public string GetToken()
		{
			string result = string.Empty;
			try
			{
				using(WebResponse webResponse = WebRequest.Create($"{_baseUrl}/auth/get_token").GetResponse())
				using(Stream stream = webResponse.GetResponseStream())
				{
					if(stream != null)
					{
						using(StreamReader streamReader = new StreamReader(stream))
						{
							result = streamReader.ReadToEnd();
						}
					}
				}
			}
			catch(Exception ex)
			{
				logger.Fatal($"Возникла ошибка при получении токена по адресу {_baseUrl}/auth/get_token. " + ex.Message);
				logger.Trace(ex.StackTrace);
			}

			return result;
		}
	}
}
