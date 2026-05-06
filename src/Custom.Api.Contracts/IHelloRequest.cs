namespace Custom.Api.Contracts
{
    /// <summary>
    /// Hello API 輸入合約介面，client / API / BO 三層共用屬性合約。
    /// </summary>
    public interface IHelloRequest
    {
        /// <summary>
        /// 用戶名稱。
        /// </summary>
        string UserName { get; }
    }
}
