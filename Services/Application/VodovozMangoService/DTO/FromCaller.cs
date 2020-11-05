namespace VodovozMangoService.DTO
{
    public class FromCaller
    {
        public string extension { get; set; }
        public string number { get; set; }
        public string taken_from_call_id { get; set; }
        
        #region Calculated
        public uint? Extension => uint.TryParse (extension, out var i) ? (uint?) i : null;
        #endregion
    }
}