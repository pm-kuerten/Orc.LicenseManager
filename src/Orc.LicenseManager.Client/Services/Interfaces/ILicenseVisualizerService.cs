namespace Orc.LicenseManager;

using System.Threading.Tasks;

public interface ILicenseVisualizerService
{
    /// <summary>
    /// Shows the single license dialog including all company info.
    /// </summary>
    /// <exception cref="System.Exception">Please use the Initialize method first</exception>
    Task ShowLicenseAsync();
}
