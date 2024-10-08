namespace Orc.LicenseManager;

using System.Threading.Tasks;

public class EmptyLicenseVisualizerService : ILicenseVisualizerService
{
    public Task ShowLicenseAsync()
    {
        // Empty for a reason
        return Task.CompletedTask;
    }
}
