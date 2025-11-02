using QS.DomainModel.UoW;
using System;
using System.Collections.Generic;
using System.Linq;
using Vodovoz.Core.Domain.Edo;

namespace Edo.Problems.Validation
{
	public class EdoTaskValidatorsPersister
	{
		private readonly IUnitOfWorkFactory _uowFactory;

		private IEnumerable<IEdoTaskValidator> _persistedValidators;
		private IEnumerable<IEdoTaskValidator> _registeredValidators;

		public EdoTaskValidatorsPersister(
			IUnitOfWorkFactory uowFactory,
			IEnumerable<IEdoTaskValidator> validators
			)
		{
			var sameNameValidators = validators.GroupBy(x => x.Name).Where(x => x.Count() > 1);
			if(sameNameValidators.Count() > 0)
			{
				throw new InvalidOperationException($"Валидаторы не могут иметь одинаковые имена: " +
					$"{string.Join(", ", sameNameValidators.Select(x => x.Key))}");
			}
			_uowFactory = uowFactory ?? throw new ArgumentNullException(nameof(uowFactory));
			_registeredValidators = validators;

			Persist();
		}

		private void Persist()
		{
			using(var uow = _uowFactory.CreateWithoutRoot())
			{
				var changed = false;
				var savedValidators = uow.Session.QueryOver<EdoTaskProblemValidatorSourceEntity>().List();
				foreach(var registeredValidator in _registeredValidators)
				{
					var savedValidator = savedValidators.FirstOrDefault(x => x.Name == registeredValidator.Name);
					if(savedValidator == null)
					{
						savedValidator = new EdoTaskProblemValidatorSourceEntity
						{
							Name = registeredValidator.Name,
							Importance = registeredValidator.Importance,
							Message = registeredValidator.Message,
							Description = registeredValidator.Description,
							Recommendation = registeredValidator.Recommendation
						};
						uow.Save(savedValidator);
						changed = true;
						continue;
					}

					if(savedValidator.Equals(registeredValidator))
					{
						continue;
					}

					savedValidator.Importance = registeredValidator.Importance;
					savedValidator.Message = registeredValidator.Message;
					savedValidator.Description = registeredValidator.Description;
					savedValidator.Recommendation = registeredValidator.Recommendation;

					uow.Save(savedValidator);
					changed = true;
				}
				if(changed)
				{
					uow.Commit();
				}
			}

			_persistedValidators = _registeredValidators;
		}

		public IEnumerable<IEdoTaskValidator> GetEdoTaskValidators()
		{
			return _persistedValidators;
		}
	}
}
