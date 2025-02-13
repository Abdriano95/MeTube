using Microsoft.JSInterop;

public interface IJSRuntimeWrapper
{
    ValueTask<T> InvokeAsync<T>(string identifier, params object[] args);
    ValueTask InvokeVoidAsync(string identifier, params object[] args);
}
