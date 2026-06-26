using System;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;
using WMS.SharedKernel;

namespace WMS.Application.Common.Behaviors;

public class ExceptionBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly ILogger<ExceptionBehavior<TRequest, TResponse>> _logger;

    public ExceptionBehavior(ILogger<ExceptionBehavior<TRequest, TResponse>> logger)
    {
        _logger = logger;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        try
        {
            return await next();
        }
        catch (Exception ex)
        {
            var requestName = typeof(TRequest).Name;
            _logger.LogError(ex, "[WMS Application] Unhandled exception occurred for request {RequestName}", requestName);

            // Handle validation errors or general failures and project them to a failed Result response if expected
            if (typeof(TResponse) == typeof(Result))
            {
                var errorMessage = ex is ValidationException valEx 
                    ? string.Join("; ", valEx.Errors.Select(e => e.ErrorMessage)) 
                    : ex.Message;
                return (TResponse)(object)Result.Failure(errorMessage);
            }

            if (typeof(TResponse).IsGenericType && typeof(TResponse).GetGenericTypeDefinition() == typeof(Result<>))
            {
                var errorMessage = ex is ValidationException valEx 
                    ? string.Join("; ", valEx.Errors.Select(e => e.ErrorMessage)) 
                    : ex.Message;
                
                var failureMethod = typeof(TResponse).GetMethod("Failure", BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy);
                if (failureMethod != null)
                {
                    var failedResult = failureMethod.Invoke(null, new object[] { errorMessage });
                    return (TResponse)failedResult!;
                }
            }

            throw;
        }
    }
}
