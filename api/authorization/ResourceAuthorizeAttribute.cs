using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Reservae.Authorization;

// Require login through the standard authorization middleware, then evaluate ownership after model binding.
[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
public sealed class ResourceAuthorizeAttribute(
    ResourceKind kind,
    string argument,
    string resourcePolicy = ResourcePolicies.SpaceOwner) : AuthorizeAttribute, IFilterFactory
{
    public bool IsReusable => false;
    public ResourceKind Kind { get; } = kind;
    public string Argument { get; } = argument;
    public string ResourcePolicy { get; } = resourcePolicy;

    public IFilterMetadata CreateInstance(IServiceProvider services)
        => new ResourceAuthorizationFilter(
            services.GetRequiredService<IResourceOwnershipResolver>(),
            services.GetRequiredService<IAuthorizationService>(),
            Kind, Argument, ResourcePolicy);
}
