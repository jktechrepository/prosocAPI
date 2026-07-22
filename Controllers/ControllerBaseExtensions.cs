using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ProsocAPI.Exceptions;
using ProsocAPI.Models.DTOs.Core;
using ProsocAPI.Services;

namespace ProsocAPI.Controllers
{
    /// <summary>
    /// Extensions pour les contrôleurs qui n'héritent pas de <see cref="BaseApiController"/>.
    /// </summary>
    public static class ControllerBaseExtensions
    {
        public static ObjectResult TechnicalErrorResponse(this ControllerBase controller, string message, Exception ex)
        {
            if (ex is SuperviseurSansCommuneTitulaireException scopeEx)
            {
                var loggerFactory = controller.HttpContext.RequestServices.GetRequiredService<ILoggerFactory>();
                loggerFactory.CreateLogger(controller.GetType())
                    .LogWarning(scopeEx, "Superviseur {SuperviseurAgentId} sans commune titulaire", scopeEx.SuperviseurAgentId);

                return controller.StatusCode(422, new
                {
                    message = scopeEx.Message,
                    codeErreur = ErrorCodes.BUSINESS_SUPERVISEUR_SANS_COMMUNE_TITULAIRE,
                    superviseurAgentId = scopeEx.SuperviseurAgentId
                });
            }

            return controller.TechnicalErrorResponse(ErrorCodes.TECHNICAL_INTERNAL_ERROR, message, ex);
        }

        public static ObjectResult TechnicalErrorResponse(
            this ControllerBase controller,
            string code,
            string message,
            Exception ex)
        {
            var errorService = controller.HttpContext.RequestServices.GetRequiredService<ErrorService>();
            var errorResponse = errorService.CreateTechnicalError(code, message, ex);

            var loggerFactory = controller.HttpContext.RequestServices.GetRequiredService<ILoggerFactory>();
            loggerFactory.CreateLogger(controller.GetType()).LogError(ex, "{Message}", message);

            return controller.StatusCode(500, errorResponse);
        }
    }
}
