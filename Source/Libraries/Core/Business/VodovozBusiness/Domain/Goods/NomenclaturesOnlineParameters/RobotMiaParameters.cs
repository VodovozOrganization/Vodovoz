using NHibernate.Util;
using QS.DomainModel.Entity;
using QS.Extensions.Observable.Collections.List;
using System;
using System.Linq;
using Vodovoz.Domain.Goods.NomenclaturesOnlineParameters;

namespace VodovozBusiness.Domain.Goods.NomenclaturesOnlineParameters
{
	public class RobotMiaParameters : PropertyChangedBase, IDomainObject
	{
		private int _id;
		private int? _nomenclatureId;
		private IObservableList<SlangWord> _slangWords = new ObservableList<SlangWord>();
		private GoodsOnlineAvailability? _goodsOnlineAvailability;

		public virtual int Id
		{
			get => _id;
			set => SetField(ref _id, value);
		}

		public virtual int? NomenclatureId
		{
			get => _nomenclatureId;
			set => SetField(ref _nomenclatureId, value);
		}

		public virtual GoodsOnlineAvailability? GoodsOnlineAvailability
		{
			get => _goodsOnlineAvailability;
			set => SetField(ref _goodsOnlineAvailability, value);
		}

		public virtual IObservableList<SlangWord> SlangWords
		{
			get => _slangWords;
			set => SetField(ref _slangWords, value);
		}

		public virtual void AddSlangWord(string word)
		{
			if(NomenclatureId is null)
			{
				throw new InvalidOperationException("Нельзя добавить слово, не указана номенклатура");
			}

			if(!SlangWords.Any(sw => sw.Word == word))
			{
				SlangWords.Add(new SlangWord
				{
					Word = word
				});
			}
		}

		public virtual void ChangeSlangWord(string wordOld, string wordNew)
		{
			if(NomenclatureId is null)
			{
				throw new InvalidOperationException("Нельзя изменить слово, не указана номенклатура");
			}

			var wordToChange = SlangWords.FirstOrDefault(sw => sw.Word == wordOld);

			if(wordToChange is null)
			{
				wordToChange.Word = wordNew;
			}
		}

		public virtual void RemoveSlangWord(string word)
		{
			if(NomenclatureId is null)
			{
				throw new InvalidOperationException("Нельзя добавить слово, не указана номенклатура");
			}

			var toRemove = SlangWords.Where(sw => sw.Word == word);

			foreach(var item in toRemove)
			{
				SlangWords.Remove(item);
			}
		}
	}
}
