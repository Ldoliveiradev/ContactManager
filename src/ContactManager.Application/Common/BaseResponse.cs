namespace ContactManager.Application.Common;

public abstract class BaseResponse<TData>
{
    public TData? Data { get; protected init; }
    public bool IsSuccess { get; protected init; }
    public string? Error { get; protected init; }
}
