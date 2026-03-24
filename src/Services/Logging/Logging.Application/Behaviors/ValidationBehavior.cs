using System;
using FluentValidation;
using JiraLite.Share.Common;
using MediatR;

namespace Logging.Application.Behaviors;

public sealed class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;

    public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
    {
        _validators = validators;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        if (!_validators.Any())
        {
            return await next();
        }

        var context = new ValidationContext<TRequest>(request);

        // Chạy tất cả các Validator cùng lúc (Bất đồng bộ)
        var validationResults = await Task.WhenAll(
            _validators.Select(v => v.ValidateAsync(context, cancellationToken)));

        // Gom nhóm và chuyển đổi lỗi của FluentValidation sang class Error của hệ thống
        var failures = validationResults
            .Where(r => r.Errors.Any())
            .SelectMany(r => r.Errors)
            .Select(f => new Error(f.PropertyName, f.ErrorMessage))
            .Distinct()
            .ToArray();

        // Nếu có lỗi, chặn request và trả về Result thất bại
        if (failures.Any())
        {
            return CreateValidationResult<TResponse>(failures);
        }

        // 5. Nếu dữ liệu hợp lệ, cho phép request đi tiếp vào Handler
        return await next();
    }

    private static TResponse CreateValidationResult<TResult>(Error[] failures)
    {
        // Trường hợp 1: Handler trả về Result<T> (Có mang theo object dữ liệu)
        if (typeof(TResult).IsGenericType && typeof(TResult).GetGenericTypeDefinition() == typeof(Result<>))
        {
            var resultType = typeof(TResult).GetGenericArguments()[0];

            var method = typeof(Result<>)
                .MakeGenericType(resultType)
                .GetMethod(nameof(Result<object>.ValidationFailure));

            return (TResponse)method!.Invoke(null, new object[] { failures })!;
        }

        // Trường hợp 2: Handler trả về Result (Void / Không mang theo dữ liệu)
        if (typeof(TResult) == typeof(Result))
        {
            return (TResponse)(object)Result.ValidationFailure(failures);
        }

        // Trường hợp 3: Fallback an toàn (Lập trình viên quên dùng class Result ở Handler)
        throw new ValidationException(failures.Select(f => new FluentValidation.Results.ValidationFailure(f.Code, f.Description)));
    }
}