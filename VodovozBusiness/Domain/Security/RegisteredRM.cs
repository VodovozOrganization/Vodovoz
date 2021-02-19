using QS.DomainModel.Entity;
using QS.DomainModel.Entity.EntityPermissions;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Bindings.Collections.Generic;
using Vodovoz.Domain.Employees;

namespace Vodovoz.Domain.Security
{
    [Appellative(Gender = GrammaticalGender.Masculine,
        NominativePlural = "зарегистрированные RM",
        Nominative = "зарегистрированный RM")]
    [EntityPermission]
    public class RegisteredRM : PropertyChangedBase, IDomainObject, IValidatableObject
    {
        /// <summary>
        /// Идентификатор
        /// </summary>
        [Display(Name = "Идентификатор")]
        public virtual int Id { get; set; }
        
        private string username;
        /// <summary>
        /// Имя пользователя в системе
        /// </summary>
        [Display(Name = "Имя пользователя в системе")]
        public virtual string Username
        {
            get => username;
            set { SetField(ref username, value, () => Username); }
        }

        private string domain;
        /// <summary>
        /// Имя домена
        /// </summary>
        [Display(Name = "Имя домена")]
        public virtual string Domain
        {
            get => domain;
            set { SetField(ref domain, value, () => Domain); }
        }

        private string sID;
        /// <summary>
        /// SID пользователя
        /// </summary>
        [Display(Name = "SID пользователя")]
        public virtual string SID
        {
            get => sID;
            set { SetField(ref sID, value, () => SID); }
        }

        private IList<User> users = new List<User>();
        /// <summary>
        /// Пользователи программы
        /// </summary>
        [Display (Name = "Пользователи программы доступные для подключения")]
        public virtual IList<User> Users
        {
            get => users;
            set { SetField(ref users, value, () => Users); }
        }

        GenericObservableList<User> observableUsers;
        //FIXME Кослыль пока не разберемся как научить hibernate работать с обновляемыми списками.
        public virtual GenericObservableList<User> ObservableUsers
        {
            get
            {
                if (observableUsers == null)
                {
                    observableUsers = new GenericObservableList<User>(users);
                    observableUsers.ListContentChanged += ObservableUsers_ListContentChanged;
                }

                return observableUsers;
            }
        }

        private void ObservableUsers_ListContentChanged(object sender, EventArgs e)
        {
            OnPropertyChanged(nameof(ObservableUsers));
        }

        private bool isActive;
        /// <summary>
        /// Запись активна
        /// </summary>
        [Display(Name = "Запись активна")]
        public virtual bool IsActive
        {
            get => isActive;
            set { SetField(ref isActive, value, () => IsActive); }
        }

        public virtual IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (string.IsNullOrWhiteSpace(Username))
                yield return new ValidationResult("Имя пользователя не может быть пустым",
                    new[] { GetPropertyName(() => Username)});

            if (string.IsNullOrWhiteSpace(Domain))
                yield return new ValidationResult("Домен не может быть пустым",
                    new[] { GetPropertyName(() => Domain) });

            if (string.IsNullOrWhiteSpace(SID))
                yield return new ValidationResult("SID пользователя не может быть пустым",
                    new[] { GetPropertyName(() => SID) });
        }
    }
}
