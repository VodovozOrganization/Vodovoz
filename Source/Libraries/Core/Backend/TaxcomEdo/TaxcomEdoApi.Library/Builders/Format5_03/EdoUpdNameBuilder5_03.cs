using System;
using System.Text;

namespace TaxcomEdoApi.Library.Builders.Format5_03
{
	/// <summary>
	/// Класс для формирования имени УПД в ЭДО
	/// в формате R_Т_A_О_GGGGMMDD_N1_N2_N3_N4_N5_N6_N7
	/// где R_Т – префикс, принимающий значение ON_NSCHFDOPPR или ON_NSCHFDOPPOK
	/// А – идентификатор получателя файла обмена
	/// O – уникальный идентификатор отправителя файла обмена
	/// GGGG – год формирования передаваемого файла обмена, MM – месяц, DD – день
	/// N1 – 36 символьный GUID
	/// N2 – формируется значение «1» в случае, если используется в целях контроля за движением товаров,
	/// подлежащих прослеживаемости (при отсутствии показателя принимает однозначное значение «0»)
	/// N3 – формируется значение «1» в случае, если используется в целях контроля за движением товаров,
	/// подлежащих маркировке (при отсутствии показателя принимает однозначное значение «0»)
	/// N4 – формируется значение «1» в случае, если используется в целях контроля за движением алкогольной продукции,
	/// подлежащей маркировке (при отсутствии показателя принимает однозначное значение «0»)
	/// N5 – формируется значение «1» в случае, если используется в целях контроля за движением/оборотом табачной продукции, сырья,
	/// никотинсодержащей продукции и никотинового сырья (при отсутствии показателя принимает однозначное значение «0»)
	/// N6 – формируется значение «1» в случае, если использование настоящего формата предусмотрено в рамках движения нефтепродуктов
	/// (при отсутствии показателя принимает однозначное значение «0»)
	/// N7 – формируется свободное двузначное число, которое принимает значение в соответствии со списком в электронной форме,
	/// размещенный на официальном сайте Федеральной налоговой службы в информационно-телекоммуникационной сети «Интернет»
	/// в виде отдельного файла (при отсутствии показателя принимает однозначное значение «00»)
	/// </summary>
	public abstract class EdoUpdNameBuilder5_03
	{
		private const string _separator = "_";
		private bool _controlMarkGoods;
		private bool _controlTraceabilityGoods;
		private bool _controlAlcoholMarkGoods;
		private bool _controlMovementTobaccoAndNicotineGoods;
		private bool _controlMovementOilProducts;
		private string _freeTwoDigitNumber = "00";
		private string _senderId;
		private string _receiverId;
		private string _date;
		private readonly string _documentId = Guid.NewGuid().ToString("D").ToUpper();

		/// <summary>
		/// R_Т – префикс
		/// </summary>
		protected abstract string DocName { get; }

		/// <summary>
		/// Установка идентификатора отправителя
		/// </summary>
		/// <param name="senderId">Идентификатор отправителя</param>
		public EdoUpdNameBuilder5_03 SenderId(string senderId)
		{
			_senderId = senderId;
			return this;
		}
		
		/// <summary>
		/// Установка идентификатора получателя
		/// </summary>
		/// <param name="receiverId">Идентификатор получателя</param>
		public EdoUpdNameBuilder5_03 ReceiverId(string receiverId)
		{
			_receiverId = receiverId;
			return this;
		}
		
		/// <summary>
		/// Установка даты формирования документа в формате yyyyMMdd
		/// </summary>
		/// <param name="date">Дата</param>
		public EdoUpdNameBuilder5_03 Date(DateTime date)
		{
			_date = $"{date:yyyyMMdd}";
			return this;
		}
		
		/// <summary>
		/// Контроль за движением товаров, подлежащих прослеживаемости (N2)
		/// </summary>
		public EdoUpdNameBuilder5_03 ControlTraceabilityGoods()
		{
			_controlTraceabilityGoods = true;
			return this;
		}
		
		/// <summary>
		/// Контроль за движением товаров, подлежащих маркировке (N3)
		/// </summary>
		public EdoUpdNameBuilder5_03 ControlMarkGoods()
		{
			_controlMarkGoods = true;
			return this;
		}
		
		/// <summary>
		/// Контроль за движением алкогольной продукции, подлежащей маркировке (N4)
		/// </summary>
		public EdoUpdNameBuilder5_03 ControlAlcoholMarkGoods()
		{
			_controlAlcoholMarkGoods = true;
			return this;
		}
		
		/// <summary>
		/// Контроль за движением/оборотом табачной продукции, сырья, никотинсодержащей продукции и никотинового сырья (N5)
		/// </summary>
		public EdoUpdNameBuilder5_03 ControlMovementTobaccoAndNicotineGoods()
		{
			_controlMovementTobaccoAndNicotineGoods = true;
			return this;
		}
		
		/// <summary>
		/// Контроль за движением нефтепродуктов (N6)
		/// </summary>
		public EdoUpdNameBuilder5_03 ControlMovementOilProducts()
		{
			_controlMovementOilProducts = true;
			return this;
		}
		
		/// <summary>
		/// Свободное двузначное число, которое принимает значение в соответствии со списком в электронной форме,
		/// размещенный на официальном сайте Федеральной налоговой службы в информационно-телекоммуникационной сети «Интернет»
		/// в виде отдельного файла (при отсутствии показателя принимает однозначное значение 00 (N7)
		/// </summary>
		/// <returns></returns>
		public EdoUpdNameBuilder5_03 FreeTwoDigitNumber(string freeTwoDigitNumber)
		{
			_freeTwoDigitNumber = freeTwoDigitNumber;
			return this;
		}
		
		public override string ToString()
		{
			var sb = new StringBuilder();
			
			sb.Append(DocName);
			
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

			AppendParameter(sb, _controlTraceabilityGoods);
			AppendParameter(sb, _controlMarkGoods);
			AppendParameter(sb, _controlAlcoholMarkGoods);
			AppendParameter(sb, _controlMovementTobaccoAndNicotineGoods);
			AppendParameter(sb, _controlMovementOilProducts);
			AppendParameter(sb, _freeTwoDigitNumber);

			return sb.ToString();
		}

		private void AppendParameter<T>(StringBuilder sb, T parameter)
		{
			sb.Append(_separator);

			if(parameter is bool boolean)
			{
				sb.Append(boolean ? "1" : "0");
				return;
			}
			
			sb.Append(parameter);
		}
	}
}
