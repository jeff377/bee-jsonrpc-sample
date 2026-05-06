namespace Custom.Contracts
{
    /// <summary>
    /// Hello API 輸入合約介面，client 與 server 共用屬性合約。
    /// </summary>
    public interface IHelloRequest
    {
        /// <summary>
        /// 用戶名稱。
        /// </summary>
        string UserName { get; }
    }
}
