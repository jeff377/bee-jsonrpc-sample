using Bee.Api.Core.Messages;
using Custom.Api.Contracts;
using MessagePack;

namespace Custom.Api
{
    /// <summary>
    /// Hello API 輸出型別，承載 API 傳輸所需的序列化標記。
    /// </summary>
    [MessagePackObject]
    public class HelloResponse : ApiResponse, IHelloResponse
    {
        /// <summary>
        /// 回傳訊息。
        /// </summary>
        [Key(100)]
        public string Message { get; set; } = string.Empty;
    }
}
