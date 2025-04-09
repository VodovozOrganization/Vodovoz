using QS.DomainModel.Entity;

namespace VodovozBusiness.Domain.Goods.NomenclaturesOnlineParameters
{
	public class SlangWord : PropertyChangedBase, IDomainObject
	{
		private int _id;
		private int? _robotMiaParametersId;
		private string _word;

		public virtual int Id
		{
			get => _id;
			set => SetField(ref _id, value);
		}

		public virtual int? RobotMiaParametersId
		{
			get => _robotMiaParametersId;
			set => SetField(ref _robotMiaParametersId, value);
		}

		public virtual string Word
		{
			get => _word;
			set => SetField(ref _word, value);
		}
	}
}
