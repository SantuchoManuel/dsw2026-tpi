using Dsw2026Tpi.CrossCutting.Models;
using Microsoft.AspNetCore.Mvc;

namespace Dsw2026Tpi.Api.Configurations
{
    public static class ControllersConfigurationExtensions
    {
        public static IServiceCollection AddAppControllers(this IServiceCollection services)
        {
            services.AddControllers()
                .ConfigureApiBehaviorOptions(options =>
                {
                    options.InvalidModelStateResponseFactory = context =>
                    {
                        var errorResponse = new ErrorResponse("VALIDATION_ERROR", "Uno o más errores de validación ocurrieron");
                        foreach (var keyModelStatePair in context.ModelState)
                        {
                            var field = keyModelStatePair.Key; 
                            foreach (var error in keyModelStatePair.Value.Errors)
                            {
                                var camelCaseField = string.IsNullOrEmpty(field) ? field : char.ToLowerInvariant(field[0]) + field.Substring(1);
                                errorResponse.AddDetail(camelCaseField, error.ErrorMessage);
                            }
                        }
                        return new BadRequestObjectResult(errorResponse);
                    };
                });
            return services;
        }
    }
}
