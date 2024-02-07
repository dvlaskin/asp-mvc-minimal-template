using Microsoft.AspNetCore.Mvc;
using WebApp.Domain.Models;

namespace WebApp.Domain.Extensions;

public static class UrlHelperExtensions
{
    public static string GenerateLinkForAction(this IUrlHelper urlHelper, ActionLinkDescription actionDescription)
    {
        if (actionDescription is null)
            throw new ArgumentNullException(nameof(actionDescription));

        return urlHelper.Action(
            action: actionDescription.Action,
            controller: actionDescription.Controller,
            values: new { userId = actionDescription.UserId.ToString(), code = actionDescription.Code },
            protocol: actionDescription.Scheme
            ) ?? "";
    }
}