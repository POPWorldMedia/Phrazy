namespace Phrazy.Client.Services;

public interface IDeviceIDService
{
	Task<string> GetDeviceID();
}

public class DeviceIDService : IDeviceIDService
{
	private readonly ILocalStorageService _localStorageService;

	public DeviceIDService(ILocalStorageService localStorageService)
	{
		_localStorageService = localStorageService;
	}

	private const string StorageKey = "deviceid";

	public async Task<string> GetDeviceID()
	{
		var deviceID = await _localStorageService.Get<string>(StorageKey);
		if (string.IsNullOrEmpty(deviceID))
		{
			deviceID = Guid.NewGuid().ToString();
			await _localStorageService.Set(StorageKey, deviceID);
		}
		return deviceID;
	}
}