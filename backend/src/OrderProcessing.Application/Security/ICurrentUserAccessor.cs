namespace OrderProcessing.Application.Security;

public interface ICurrentUserAccessor
{
    string UserId { get; }
}
