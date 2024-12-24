using QS.DomainModel.UoW;
using System;
using System.Collections.Generic;
using System.Linq;
using Vodovoz.Core.Domain.Edo;

namespace Edo.TaskValidation
{
	public class EdoTaskValidatorsPersister
	{
		private IEnumerable<IEdoTaskValidator> _persistedValidators;
		private IEnumerable<IEdoTaskValidator> _registeredValidators;

		public EdoTaskValidatorsPersister(IEnumerable<IEdoTaskValidator> validators)
		{
			var sameNameValidators = validators.GroupBy(x => x.Name).Where(x => x.Count() > 1);
			if(sameNameValidators.Count() > 0)
			{
				throw new InvalidOperationException($"Валидаторы не могут иметь одинаковые имена: " +
					$"{string.Join(", ", sameNameValidators.Select(x => x.Key))}");
			}

			_registeredValidators = validators;
		}

		public IEnumerable<IEdoTaskValidator> GetEdoTaskValidators(IUnitOfWorkFactory uowFactory)
		{
			if(_persistedValidators != null)
			{
				return _persistedValidators;
			}

			using(var uow = uowFactory.CreateWithoutRoot())
			{
				var savedValidators = uow.Session.QueryOver<EdoTaskValidatorEntity>().List();
				foreach(var registeredValidator in _registeredValidators)
				{
					var savedValidator = savedValidators.FirstOrDefault(x => x.Name == registeredValidator.Name);
					if(savedValidator == null)
					{
						savedValidator = new EdoTaskValidatorEntity
						{
							Name = registeredValidator.Name,
							Importance = registeredValidator.Importance,
							Message = registeredValidator.Message,
							Description = registeredValidator.Description,
							Recommendation = registeredValidator.Recommendation
						};
					}

					if(savedValidator != registeredValidator)
					{
						savedValidator.Importance = registeredValidator.Importance;
						savedValidator.Message = registeredValidator.Message;
						savedValidator.Description = registeredValidator.Description;
						savedValidator.Recommendation = registeredValidator.Recommendation;
					}

					uow.Save(savedValidator);
				}
				uow.Commit();
			}

			_persistedValidators = _registeredValidators;
			return _persistedValidators;
		}
	}
}
