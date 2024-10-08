namespace Orc.LicenseManager;

using System;
using System.Threading.Tasks;
using Catel.Logging;
using Catel.Services;
using ViewModels;

public class DialogLicenseVisualizerService : ILicenseVisualizerService
{
    private static readonly ILog Log = LogManager.GetCurrentClassLogger();

    private readonly IUIVisualizerService _uiVisualizerService;
    private readonly ILicenseInfoService _licenseInfoService;

    public DialogLicenseVisualizerService(IUIVisualizerService uiVisualizerService, ILicenseInfoService licenseInfoService)
    {
        ArgumentNullException.ThrowIfNull(uiVisualizerService);
        ArgumentNullException.ThrowIfNull(licenseInfoService);

        _uiVisualizerService = uiVisualizerService;
        _licenseInfoService = licenseInfoService;
    }

    /// <summary>
    /// Shows the single license dialog including all company info. You will see the about box.
    /// </summary>
    public async Task ShowLicenseAsync()
    {
        Log.Debug("Showing license dialog with company info");
        var licenseInfo = _licenseInfoService.GetLicenseInfo();
        await _uiVisualizerService.ShowDialogAsync<LicenseViewModel>(licenseInfo);
    }
}
