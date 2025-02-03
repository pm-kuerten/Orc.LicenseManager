namespace Orc.LicenseManager.Views;

using System.Windows;
using ViewModels;

public sealed partial class LicenseView
{
    public LicenseView()
    {
        InitializeComponent();
    }

    public bool ShowAbout
    {
        get { return (bool)GetValue(ShowAboutProperty); }
        set { SetValue(ShowAboutProperty, value); }
    }

    public static readonly DependencyProperty ShowAboutProperty = DependencyProperty.Register(nameof(ShowAbout), typeof(bool), 
        typeof(LicenseView), new PropertyMetadata(true));

    private void OnGridDrop(object sender, DragEventArgs e)
    {
        if (ViewModel is not LicenseViewModel vm) return;
        if (e.Data.GetData(DataFormats.FileDrop) is not string[] files || files.Length == 0) return;
        vm.PasteFromFile.Execute(files[0]);
    }
}
