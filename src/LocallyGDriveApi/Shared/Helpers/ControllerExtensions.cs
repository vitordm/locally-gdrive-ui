using System;
using Microsoft.AspNetCore.Mvc;

namespace LocallyGDriveApi.Shared.Helpers;

public static class ControllerExtensions
{
    public static IActionResult InternalServerError(
        this ControllerBase controller,
        string message = "Internal Server Error")
    {
        return controller.StatusCode(StatusCodes.Status500InternalServerError,
            new { error = message });
    }

    public static IActionResult InternalServerErrorStatusCode(
        this ControllerBase controller,
        string message = "Internal Server Error")
    {
        return InternalServerError(controller, message);
    }
}
