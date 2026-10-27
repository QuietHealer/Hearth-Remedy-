using RemedyLib.Data;


namespace HearthRemedies; // use your app's namespace

public class PreferencesDeviceIdProvider : IDeviceIdProvider
{
    public string GetDeviceId()
    {
        var id = Preferences.Default.Get("device_id", "");
        if (string.IsNullOrEmpty(id))
        {
            id = Guid.NewGuid().ToString("N");
            Preferences.Default.Set("device_id", id);
        }
        return id;
    }
}