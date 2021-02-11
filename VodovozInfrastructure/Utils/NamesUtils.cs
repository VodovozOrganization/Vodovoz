using System;
using System.Collections.Generic;
using System.Runtime.Remoting.Metadata.W3cXsd2001;
using System.Text.RegularExpressions;

namespace VodovozInfrastructure.Utils {
    public class NamesUtils {
        
        public static string GetSecondNameFromFullName(string fullName)
        {
            var a = fullName.Split(' ');
            if (a.Length == 3){
                return a[1].Trim();
            }
            else{
                throw new ArgumentException("The full name must have 2 spaces(3 names)");
            }
        }
        
        /// <summary>
        /// Достает тип организации из названия, вроде ООО, ОАО, ИП и т.д.
        /// </summary>
        /// <param name="name">Название организации</param>
        /// <returns>Тип организации</returns>
        public static string TryGetOrganizationType (string name)
        {
            foreach (var pair in OrganizationTypes) {
                string pattern = string.Format (@".*(^|\(|\s|\W|['""]){0}($|\)|\s|\W|['""]).*", pair.Key);
                string fullPattern = string.Format (@".*(^|\(|\s|\W|['""]){0}($|\)|\s|\W|['""]).*", pair.Value);
                Regex regex = new Regex (pattern, RegexOptions.IgnoreCase);

                if (regex.IsMatch (name))
                    return pair.Key;

                regex = new Regex (fullPattern, RegexOptions.IgnoreCase);

                if (regex.IsMatch (name))
                    return pair.Key;
            }
            return null;
        }
        
        public static Dictionary<string, string> OrganizationTypes = new Dictionary<string, string> {
            { "ООО", "Общество с ограниченной ответственностью" },
            { "ЗАО", "Закрытое акционерное общество" },
            { "ОАО", "Открытое акционерное общество" },
            { "ГорПО", "Городское потребительское общество" },
            { "РайПО", "Районное потребительское общество" },
            { "СельПО", "Сельское потребительское общество" },
            { "НКО", "Некоммерческая организация" },
            { "ИП", "Индивидуальный предприниматель" },
            { "ОДО", "Общество с дополнительной ответственностью" },
            { "ПТ", "Полное товарищество" },
            { "АО", "Акционерное общество" },
            { "АК", "Акционерная компания" },
            { "КТ", "Коммандитное товарищество" },
            { "ХП", "Хозяйственное партнерство" },
            { "КФХ", "Крестьянское (фермерское) хозяйство" },
            { "НП", "Некоммерческое партнерство" },
            { "ПК", "Потребительский кооператив" },
            { "ТСН", "Товарищество собственников недвижимости" },
            { "АНО", "Автономная некоммерческая организация" },
            { "ФГУП", "Федеральное государственное унитарное предприятие" },
            { "ГУП", "Государственное унитарное предприятие" },
            { "МУП", "Муниципальное унитарное предприятие" },
            { "ГП", "Государственное предприятие" },
            { "СНТ", "Садоводческое некоммерческое товарищество" },
            { "ДНТ", "Дачное некоммерческое товарищество" },
            { "ОНТ", "Огородническое некоммерческое товарищество" },
            { "СНП", "Садоводческое некоммерческое партнерство" },
            { "ДНП", "Дачное некоммерческое партнерство" },
            { "ОНП", "Огородническое некоммерческое партнерство" },
            { "СПК", "Садоводческий потребительский кооператив" },
            { "ДПК", "Дачный потребительский кооператив" },
            { "ОПК", "Огороднический потребительский кооператив" }
        };
        
    }
}