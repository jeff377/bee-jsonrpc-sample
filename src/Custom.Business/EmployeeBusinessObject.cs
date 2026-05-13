using System;
using Bee.Business.Form;
using Bee.Definition;
using Bee.Definition.Attributes;
using Bee.Definition.Security;
using Custom.Api.Contracts;

namespace Custom.Business
{
    /// <summary>
    /// Business logic object for Employee.
    /// </summary>
    public class EmployeeBusinessObject : FormBusinessObject
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="ctx">Per-call context aggregating cross-cutting services.</param>
        /// <param name="accessToken">Access token.</param>
        /// <param name="progId">Program ID.</param>
        /// <param name="isLocalCall">Indicates whether the call is from a local source.</param>
        public EmployeeBusinessObject(IBeeContext ctx, Guid accessToken, string progId, bool isLocalCall = true)
            : base(ctx, accessToken, progId, isLocalCall)
        {
        }

        /// <summary>
        /// Public hello test method (no login, no encoding/encryption).
        /// </summary>
        [ApiAccessControl(ApiProtectionLevel.Public, ApiAccessRequirement.Anonymous)]
        public IHelloResponse Hello(IHelloRequest request)
        {
            System.Threading.Thread.Sleep(500); // Simulate a delay
            return new HelloResult
            {
                Message = $"Hello, {request.UserName}"
            };
        }

        /// <summary>
        /// Encoded request — remote call must be serialized and compressed.
        /// Requires login authentication.
        /// </summary>
        [ApiAccessControl(ApiProtectionLevel.Encoded, ApiAccessRequirement.Authenticated)]
        public IHelloResponse HelloEncoded(IHelloRequest request)
        {
            return new HelloResult
            {
                Message = $"[Encoded & Auth] Hello, {request.UserName}"
            };
        }

        /// <summary>
        /// Encrypted request — remote call must be serialized, compressed, and encrypted.
        /// Requires login authentication.
        /// </summary>
        [ApiAccessControl(ApiProtectionLevel.Encrypted, ApiAccessRequirement.Authenticated)]
        public IHelloResponse HelloEncrypted(IHelloRequest request)
        {
            return new HelloResult
            {
                Message = $"[Encrypted & Auth] Hello, {request.UserName}"
            };
        }

        /// <summary>
        /// Local only — can only be invoked from local server (no remote API access).
        /// </summary>
        [ApiAccessControl(ApiProtectionLevel.LocalOnly, ApiAccessRequirement.Anonymous)]
        public IHelloResponse HelloLocal(IHelloRequest request)
        {
            return new HelloResult
            {
                Message = $"[LocalOnly] Hello, {request.UserName}"
            };
        }
    }
}
