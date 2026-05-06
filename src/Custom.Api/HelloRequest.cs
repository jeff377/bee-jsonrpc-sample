using Bee.Api.Core.Messages;
using Custom.Api.Contracts;
using MessagePack;

namespace Custom.Api
{
    /// <summary>
    /// Hello API 輸入型別，承載 API 傳輸所需的序列化標記。
    /// </summary>
    [MessagePackObject]
    public class HelloRequest : ApiRequest, IHelloRequest
    {
        /// <summary>
        /// 用戶名稱。
        /// </summary>
        [Key(100)]
        public string UserName { get; set; } = string.Empty;
    }
}
