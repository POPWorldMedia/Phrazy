using Microsoft.JSInterop;

namespace Phrazy.Client.Services;

public interface IAlertService
{
	Task PopAlert(string text);
}

public class AlertService : IAlertService
{
	private readonly IJSRuntime _jsRuntime;

	public AlertService(IJSRuntime jsRuntime)
	{
		_jsRuntime = jsRuntime;
	}

	public async Task PopAlert(string text)
	{
		await _jsRuntime.InvokeVoidAsync("alert", text);
	}
}