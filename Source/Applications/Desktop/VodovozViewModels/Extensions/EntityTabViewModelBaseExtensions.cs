using QS.DomainModel.Entity;
using QS.ViewModels;
using System;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Vodovoz.ViewModels.Extensions
{
	public static class EntityTabViewModelBaseExtensions
	{
		public delegate bool SetFieldDelegate<T>(ref T field, T value, [CallerMemberName] string propertyName = "");

		public static U GetIdRefField<T, U>(this EntityTabViewModelBase<T> entityTabViewModelBase, ref U field, int? entityFieldId)
			where T : class, IDomainObject, INotifyPropertyChanged, new()
			where U : IDomainObject
		{ 
			if(field?.Id != entityFieldId)
			{
				if(entityFieldId == null)
				{
					field = default;
				}
				else
				{
					field = entityTabViewModelBase.UoW.GetById<U>(entityFieldId.Value);
				}
			}

			return field;
		}

		public static bool SetIdRefField<T, U>(this EntityTabViewModelBase<T> entityTabViewModelBase, SetFieldDelegate<U> setField, ref U targetField, Expression<Func<int?>> targetPropertyExpr, U value, [CallerMemberName] string callerPropertyName = null)
			where T : class, IDomainObject, INotifyPropertyChanged, new()
			where U : IDomainObject
		{
			if(value?.Id == targetField?.Id)
			{
				return false;
			}

			if(targetPropertyExpr.Body is MemberExpression memberSelectorExpression
				&& memberSelectorExpression.Member is PropertyInfo property)
			{
				property.SetValue(entityTabViewModelBase.Entity, value?.Id, null);
			}

			return setField(ref targetField, value, callerPropertyName);
		}

		public static bool SetIdRefField<T, U>(this EntityTabViewModelBase<T> entityTabViewModelBase, ref U targetField, Expression<Func<int?>> targetPropertyExpr, U value, [CallerMemberName] string callerPropertyName = null)
			where T : class, IDomainObject, INotifyPropertyChanged, new()
			where U : IDomainObject
		{
			if(value?.Id == targetField?.Id)
			{
				return false;
			}

			if(targetPropertyExpr.Body is MemberExpression memberSelectorExpression
				&& memberSelectorExpression.Member is PropertyInfo property)
			{
				property.SetValue(entityTabViewModelBase.Entity, value?.Id, null);

				return true;
			}

			return false;
		}
	}
}
