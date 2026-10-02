using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Reservae.Models.DTOs;

namespace Reservae.Authorization;

public sealed class ResourceAuthorizationFilter(
    IResourceOwnershipResolver resolver,
    IAuthorizationService authorization,
    ResourceKind kind,
    string argument,
    string policy) : IAsyncActionFilter
{
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        if (!context.HttpContext.User.Identities.Any(identity => identity.IsAuthenticated))
        {
            context.Result = new ChallengeResult();
            return;
        }

        var value = GetArgument(context);
        var resourceKind = kind;
        int? resourceId;
        if (kind == ResourceKind.SlotCreation)
        {
            var dto = value as CreateBookableSlotDTO
                ?? throw new InvalidOperationException("SlotCreation exige um CreateBookableSlotDTO.");
            // When a rule is supplied it determines the owner, regardless of a SpaceId supplied by the caller.
            resourceKind = dto.AvailabilityRuleId.HasValue ? ResourceKind.AvailabilityRule : ResourceKind.Space;
            resourceId = dto.AvailabilityRuleId ?? dto.SpaceId;
        }
        else
        {
            resourceId = value as int?;
        }

        if (resourceId is null or <= 0)
        {
            context.Result = new BadRequestObjectResult(new ProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Recurso inválido",
                Detail = "Informe um identificador de recurso válido."
            });
            return;
        }

        var resource = await resolver.ResolveAsync(resourceKind, resourceId.Value, context.HttpContext.RequestAborted);
        if (resource is null)
        {
            context.Result = new NotFoundResult();
            return;
        }

        var result = await authorization.AuthorizeAsync(context.HttpContext.User, resource, policy);
        if (!result.Succeeded)
        {
            context.Result = new ForbidResult();
            return;
        }

        await next();
    }

    private object? GetArgument(ActionExecutingContext context)
    {
        var path = argument.Split('.', 2);
        if (!context.ActionArguments.TryGetValue(path[0], out var value))
            throw new InvalidOperationException($"Argumento '{path[0]}' não encontrado na action protegida.");
        if (path.Length == 1 || value is null) return value;

        var property = value.GetType().GetProperty(path[1])
            ?? throw new InvalidOperationException($"Propriedade '{path[1]}' não encontrada no argumento '{path[0]}'.");
        return property.GetValue(value);
    }
}
