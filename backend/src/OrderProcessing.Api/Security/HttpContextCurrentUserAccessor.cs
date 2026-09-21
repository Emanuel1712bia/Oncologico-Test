using OrderProcessing.Application.Security;

namespace OrderProcessing.Api.Security;

public sealed class HttpContextCurrentUserAccessor : ICurrentUserAccessor
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public HttpContextCurrentUserAccessor(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public string UserId
    {
        get
        {
            var subjectClaim = _httpContextAccessor.HttpContext?.User.FindFirst("sub");
            if (subjectClaim is null || string.IsNullOrWhiteSpace(subjectClaim.Value))
            {
                throw new InvalidOperationException("The current request does not have an authenticated user.");
            }

            return subjectClaim.Value;
        }
    }
}
