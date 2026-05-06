using Bee.Business;
using Custom.Api.Contracts;

namespace Custom.Business
{
    /// <summary>
    /// Hello BO 輸出型別（純 POCO）。實作 <see cref="IHelloResponse"/>，
    /// 序列化前由 <c>ApiContractRegistry</c> 自動轉為 API 型別 HelloResponse。
    /// </summary>
    public class HelloResult : BusinessResult, IHelloResponse
    {
        /// <summary>
        /// 回傳訊息。
        /// </summary>
        public string Message { get; set; } = string.Empty;
    }
}
