// <copyright company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

using System;
using Fabric.Rti.workload.Backend.Services;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;

namespace Fabric.Rti.workload.Backend.Exceptions
{
    public class HttpResponseExceptionFilter : IActionFilter, IOrderedFilter
    {
        private readonly ILogger<HttpResponseExceptionFilter> _logger;

        public int Order => int.MaxValue - 10;

        public HttpResponseExceptionFilter(ILogger<HttpResponseExceptionFilter> logger)
        {
            _logger = logger;
        }

        public void OnActionExecuting(ActionExecutingContext context) { }

        public void OnActionExecuted(ActionExecutedContext context)
        {
            switch (context.Exception)
            {
                case AuthenticationUIRequiredException authenticationUIRequiredException:
                    _logger.LogError("Failed to acquire a token, user interaction is required, returning '401 Unauthorized' with WWW-Authenticate header");
                    AuthenticationService.AddBearerClaimToResponse(authenticationUIRequiredException, context.HttpContext.Response);
                    context.Result = authenticationUIRequiredException.ToHttpActionResult();
                    context.ExceptionHandled = true;
                    break;

                case WorkloadExceptionBase workloadException:
                    _logger.LogError("Workload exception: {0}\r\n{1}", workloadException, workloadException.ToTelemetryString());
                    context.Result = workloadException.ToHttpActionResult();
                    context.ExceptionHandled = true;
                    break;

                case Exception e:
                    _logger.LogError("Unknown exception: {0}", e);
                    context.Result = new InternalErrorException("Unexpected error").ToHttpActionResult();
                    context.ExceptionHandled = true;
                    break;
            }
        }
    }
}
