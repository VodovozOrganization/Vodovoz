using QS.Utilities.Numeric;
using RoboAtsService.Contracts.Requests;
using System;

namespace RoboatsService.Handlers
{
	public abstract class RequestHandlerBase
	{
		private static NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

		protected RequestDto RequestDto { get; }

		public virtual string ClientPhone { get; }
		public abstract string ErrorMessage { get; }

		public abstract string Execute();

		public RequestHandlerBase(RequestDto requestDto)
		{
			RequestDto = requestDto ?? throw new ArgumentNullException(nameof(requestDto));

			if(string.IsNullOrWhiteSpace(requestDto.ClientPhone))
			{
				throw new ArgumentException($"'{nameof(requestDto.ClientPhone)}' cannot be null or whitespace.", nameof(requestDto.ClientPhone));
			}

			PhoneFormatter phoneFormatter = new PhoneFormatter(PhoneFormat.DigitsTen);


			ClientPhone = phoneFormatter.FormatString(requestDto.ClientPhone);
			_logger.Info($"Поступил запрос по номеру {ClientPhone} ({requestDto.ClientPhone})");
		}
	}
}
