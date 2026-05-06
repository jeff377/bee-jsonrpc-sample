using Bee.Business;
using Custom.Api.Contracts;

namespace Custom.Business
{
    /// <summary>
    /// Hello BO 輸入型別（純 POCO）。實作 <see cref="IHelloRequest"/>，
    /// 可在此擴充 BO 內部專屬屬性而不影響 API 合約。
    /// </summary>
    public class HelloArgs : BusinessArgs, IHelloRequest
    {
        /// <summary>
        /// 用戶名稱。
        /// </summary>
        public string UserName { get; set; } = string.Empty;
    }
}
