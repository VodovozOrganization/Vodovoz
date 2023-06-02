using System;
using System.Collections.Generic;
using System.Linq;
using QS.Project.Journal;
using Vodovoz.Domain.Cash;
using Vodovoz.ViewModels.Journals.FilterViewModels;
using Vodovoz.ViewModels.Journals.FilterViewModels.Enums;

namespace Vodovoz.ViewModels.Journals.JournalNodes
{
    public class IncomeCategoryJournalNode: JournalEntityNodeBase<IncomeCategory>
    {
        private bool isCalculated;
        private string[] Levels;
        private string level5;

		public override string Title =>
			string.IsNullOrWhiteSpace(level5)
				? string.IsNullOrWhiteSpace(level4)
					? string.IsNullOrWhiteSpace(level3)
						? string.IsNullOrWhiteSpace(level2)
							? level1
							: level2
						: level3
					: level4
				: level5;

		public string Level5
        {
            get{
                if (!isCalculated){
                    Calculated();
                    isCalculated = true;
                }
                return level5;
            }
            set { level5 = value; }
        }
        
        private string level4;
        public string Level4
        {
            get{
                if (!isCalculated){
                    Calculated();
                    isCalculated = true;
                }
                return level4;
            }
            set { level4 = value; }
        }
        
        private string level3;
        public string Level3
        {
            get{
                if (!isCalculated){
                    Calculated();
                    isCalculated = true;
                }
                return level3;
            }
            set { level3 = value; }
        }
        
        private string level2;
        public string Level2
        {
            get{
                if (!isCalculated){
                    Calculated();
                    isCalculated = true;
                }
                return level2;
            }
            set { level2 = value; }
        }
        
        private string level1;
        public string Level1 {
            get{
                if (!isCalculated){
                    Calculated();
                    isCalculated = true;
                }
                return level1;
            }
            set { level1 = value; }
        }

        private LevelsFilter levelFilter;
        public LevelsFilter LevelFilter
        {
            get { return levelFilter; }
            set { levelFilter = value; }
        }

        private void Calculated()
        {
            if (Levels == null)
                Levels = new[] {level1, level2, level3, level4, level5};

            var nullCount = Levels.Count(x => x == null);
            if (nullCount > 0)
                MoveLevels(nullCount);
        }
        
        private void MoveLevels(int n)
        {
            for (int i = 0; i < n; i++)
            {
                level1 = level2;
                level2 = level3;
                level3 = level4;
                level4 = level5;
                level5 = "";
            }
        }
        
        public bool IsArchive { get; set; }
        
        public string Subdivision { get; set; }
    }
}
