using QS.DomainModel.Entity;
using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Domain.Logistic
{
	public class LogisticsRequirementsBase : PropertyChangedBase, IDomainObject
	{
		public virtual int Id {get; set; }

		private bool _forwarderRequired;
		[Display(Name = "Требуется экспедитор на адресе: \"Э\" на карте")]
		public virtual bool ForwarderRequired
		{
			get => _forwarderRequired;
			set => SetField(ref _forwarderRequired, value);
		}

		private bool _documentsRequired;
		[Display(Name = "Требуется наличие паспорта/документов у водителя: \"Д\" на карте")]
		public virtual bool DocumentsRequired
		{
			get => _documentsRequired;
			set => SetField(ref _documentsRequired, value);
		}

		private bool _russianDriverRequired;
		[Display(Name = "Требуется русский водитель: \"Р\" на карте")]
		public virtual bool RussianDriverRequired
		{
			get => _russianDriverRequired;
			set => SetField(ref _russianDriverRequired, value);
		}

		private bool _passRequired;
		[Display(Name = "Требуется пропуск: \"П\" на карте")]
		public virtual bool PassRequired
		{
			get => _passRequired;
			set => SetField(ref _passRequired, value);
		}

		private bool _lagrusRequired;
		[Display(Name = "Требуется Ларгус (газель не проедет): \"Л\" на карте")]
		public virtual bool LagrusRequired
		{
			get => _lagrusRequired;
			set => SetField(ref _lagrusRequired, value);
		}

		public abstract string Title { get; }
	}
}
