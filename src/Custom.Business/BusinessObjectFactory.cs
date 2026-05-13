using System;
using Bee.Business.Form;
using Bee.Business.System;
using Bee.Definition;
using Bee.Definition.Identity;
using Bee.Definition.Storage;

namespace Custom.Business
{
    /// <summary>
    /// Business object factory. Dispatches progId to the matching <see cref="FormBusinessObject"/>
    /// subclass and builds the per-call <see cref="IBeeContext"/> for each instance.
    /// </summary>
    public class BusinessObjectFactory : IBusinessObjectFactory
    {
        private readonly IServiceProvider _services;
        private readonly IDefineAccess _defineAccess;
        private readonly ISessionInfoService _sessionInfoService;

        /// <summary>
        /// Constructor. Dependencies are supplied by the host DI container via
        /// <c>ActivatorUtilities.CreateInstance</c>.
        /// </summary>
        public BusinessObjectFactory(
            IServiceProvider services,
            IDefineAccess defineAccess,
            ISessionInfoService sessionInfoService)
        {
            _services = services ?? throw new ArgumentNullException(nameof(services));
            _defineAccess = defineAccess ?? throw new ArgumentNullException(nameof(defineAccess));
            _sessionInfoService = sessionInfoService ?? throw new ArgumentNullException(nameof(sessionInfoService));
        }

        /// <summary>
        /// Creates a system-level business logic object.
        /// </summary>
        public object CreateSystemBusinessObject(Guid accessToken, bool isLocalCall = true)
        {
            return new SystemBusinessObject(BuildContext(), accessToken, isLocalCall);
        }

        /// <summary>
        /// Creates a form-level business logic object.
        /// </summary>
        public object CreateFormBusinessObject(Guid accessToken, string progId, bool isLocalCall = true)
        {
            var ctx = BuildContext();
            return progId switch
            {
                "Employee" => new EmployeeBusinessObject(ctx, accessToken, progId, isLocalCall),
                _ => new FormBusinessObject(ctx, accessToken, progId, isLocalCall),
            };
        }

        private BeeContext BuildContext() => new BeeContext
        {
            DefineAccess = _defineAccess,
            SessionInfoService = _sessionInfoService,
            BoFactory = this,
            Services = _services,
        };
    }
}
