using System;
using System.Text;

namespace TaxcomEdoApi.Library.Builders.Format5_01
{
	/// <summary>
	/// Класс для формирования имени УПД в ЭДО
	/// </summary>
	public abstract class EdoUpdNameBuilder5_01
	{
		private const string _separator = "_";
		private const string _markTag = "MARK";
		private const string _traceabilityTag = "PROS";
		private bool _controlMarkGoods;
		private bool _controlTraceabilityGoods;
		private string _senderId;
		private string _receiverId;
		private string _date;
		private readonly string _documentId = Guid.NewGuid().ToString("D").ToUpper();
		
		protected abstract string DocName { get; }

		/// <summary>
		/// Установка идентификатора отправителя
		/// </summary>
		/// <param name="senderId">Идентификатор отправителя</param>
		public EdoUpdNameBuilder5_01 SenderId(string senderId)
		{
			_senderId = senderId;
			return this;
		}
		
		/// <summary>
		/// Установка идентификатора получателя
		/// </summary>
		/// <param name="receiverId">Идентификатор получателя</param>
		public EdoUpdNameBuilder5_01 ReceiverId(string receiverId)
		{
			_receiverId = receiverId;
			return this;
		}
		
		/// <summary>
		/// Установка даты формирования документа в формате yyyyMMdd
		/// </summary>
		/// <param name="date">Дата</param>
		public EdoUpdNameBuilder5_01 Date(DateTime date)
		{
			_date = $"{date:yyyyMMdd}";
			return this;
		}
		
		/// <summary>
		/// Контроль за движением товаров, подлежащих прослеживаемости
		/// </summary>
		public EdoUpdNameBuilder5_01 ControlTraceabilityGoods()
		{
			_controlTraceabilityGoods = true;
			return this;
		}
		
		/// <summary>
		/// Контроль за движением товаров, подлежащих маркировке
		/// </summary>
		public EdoUpdNameBuilder5_01 ControlMarkGoods()
		{
			_controlMarkGoods = true;
			return this;
		}
		
		public override string ToString()
		{
			var sb = new StringBuilder();
			
			sb.Append(DocName);

			if(_controlTraceabilityGoods)
			{
				sb.Append(_traceabilityTag);
			}

			if(_controlMarkGoods)
			{
				sb.Append(_markTag);
			}
			
			if(!string.IsNullOrWhiteSpace(_receiverId))
			{
				sb.Append(_separator);
				sb.Append(_receiverId);
			}

			if(!string.IsNullOrWhiteSpace(_senderId))
			{
				sb.Append(_separator);
				sb.Append(_senderId);
			}
			
			if(!string.IsNullOrWhiteSpace(_date))
			{
				sb.Append(_separator);
				sb.Append(_date);
			}

			if(!string.IsNullOrWhiteSpace(_documentId))
			{
				sb.Append(_separator);
				sb.Append(_documentId);
			}

			return sb.ToString();
		}
	}
}
