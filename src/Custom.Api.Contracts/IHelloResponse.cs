namespace Custom.Api.Contracts
{
    /// <summary>
    /// Hello API 輸出合約介面，client / API / BO 三層共用屬性合約。
    /// </summary>
    public interface IHelloResponse
    {
        /// <summary>
        /// 回傳訊息。
        /// </summary>
        string Message { get; }
    }
}
