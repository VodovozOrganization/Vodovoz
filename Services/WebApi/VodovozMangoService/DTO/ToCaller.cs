namespace VodovozMangoService.DTO
{
    public class ToCaller
    {
        public string extension { get; set; }
        public string number { get; set; }
        public string line_number { get; set; }
        public string acd_group { get; set; }
        
        #region Calculated
        public uint? Extension => uint.TryParse (extension, out var i) ? (uint?) i : null;
        #endregion
    }
}