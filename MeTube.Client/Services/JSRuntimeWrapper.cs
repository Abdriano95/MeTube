using Microsoft.JSInterop;

namespace MeTube.Client.Services
{
    public class JSRuntimeWrapper : IJSRuntimeWrapper
    {
        private readonly IJSRuntime _jsRuntime;

        public JSRuntimeWrapper(IJSRuntime jsRuntime)
        {
            _jsRuntime = jsRuntime;
        }

        public ValueTask<T> InvokeAsync<T>(string identifier, params object[] args)
        {
            return _jsRuntime.InvokeAsync<T>(identifier, args);
        }

        public ValueTask InvokeVoidAsync(string identifier, params object[] args)
        {
            return _jsRuntime.InvokeVoidAsync(identifier, args);
        }
    }
}
