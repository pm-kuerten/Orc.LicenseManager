﻿namespace Orc.LicenseManager.Client.Example.ViewModels
{
    using System;
    using System.Threading.Tasks;
    using Catel.MVVM;
    using Catel.Services;
    using LicenseManager.ViewModels;

    /// <summary>
    /// MainWindow view model.
    /// </summary>
    public class MainWindowViewModel : ViewModelBase
    {
        private readonly ILicenseService _licenseService;
        private readonly ILicenseValidationService _licenseValidationService;
        private readonly IMessageService _messageService;
        private readonly INetworkLicenseService _networkLicenseService;
        private readonly ILicenseVisualizerService _licenseVisualizerService;
        private readonly IUIVisualizerService _uiVisualizerService;

        /// <summary>
        /// Initializes a new instance of the <see cref="MainWindowViewModel"/> class.
        /// </summary>
        public MainWindowViewModel(ILicenseService licenseService, ILicenseValidationService licenseValidationService,
            IMessageService messageService, INetworkLicenseService networkLicenseService,
            ILicenseVisualizerService licenseVisualizerService, IUIVisualizerService uiVisualizerService)
        {
            ArgumentNullException.ThrowIfNull(licenseService);
            ArgumentNullException.ThrowIfNull(licenseValidationService);
            ArgumentNullException.ThrowIfNull(messageService);
            ArgumentNullException.ThrowIfNull(networkLicenseService);
            ArgumentNullException.ThrowIfNull(licenseVisualizerService);
            ArgumentNullException.ThrowIfNull(uiVisualizerService);

            _licenseService = licenseService;
            _licenseValidationService = licenseValidationService;
            _messageService = messageService;
            _networkLicenseService = networkLicenseService;
            _licenseVisualizerService = licenseVisualizerService;
            _uiVisualizerService = uiVisualizerService;

            RemoveLicense = new TaskCommand(OnRemoveLicenseExecuteAsync);
            ValidateLicenseOnServer = new TaskCommand(OnValidateLicenseOnServerExecuteAsync, OnValidateLicenseOnServerCanExecute);
            ValidateLicenseOnLocalNetwork = new TaskCommand(OnValidateLicenseOnLocalNetworkExecuteAsync, OnValidateLicenseOnLocalNetworkCanExecute);
            ShowLicense = new TaskCommand(OnShowLicenseExecuteAsync);
            ShowLicenseUsage = new TaskCommand(OnShowLicenseUsageExecuteAsync);

            ServerUri = string.Format("http://localhost:1815/api/license/validate");
        }

        /// <summary>
        /// Gets the title of the view model.
        /// </summary>
        /// <value>The title.</value>
        public override string Title
        {
            get { return "Orc.LicenseManager example"; }
        }

        public string ServerUri { get; set; }

        public TaskCommand RemoveLicense { get; private set; }

        private async Task OnRemoveLicenseExecuteAsync()
        {
            _licenseService.RemoveLicense(LicenseMode.CurrentUser);
            _licenseService.RemoveLicense(LicenseMode.MachineWide);

            await ShowLicenseDialogAsync();
        }

        public TaskCommand ValidateLicenseOnServer { get; private set; }

        private bool OnValidateLicenseOnServerCanExecute()
        {
            if (string.IsNullOrWhiteSpace(ServerUri))
            {
                return false;
            }

            if (!_licenseService.AnyExistingLicense())
            {
                return false;
            }

            return true;
        }

        private async Task OnValidateLicenseOnServerExecuteAsync()
        {
            var licenseString = _licenseService.LoadLicense(LicenseMode.CurrentUser);

            if (string.IsNullOrWhiteSpace(licenseString))
            {
                licenseString = _licenseService.LoadLicense(LicenseMode.MachineWide);
            }

            var result = await _licenseValidationService.ValidateLicenseOnServerAsync(licenseString, ServerUri);

            await _messageService.ShowAsync(string.Format("License is {0}valid", result.IsValid ? string.Empty : "NOT "));
        }

        public TaskCommand ValidateLicenseOnLocalNetwork { get; private set; }

        private bool OnValidateLicenseOnLocalNetworkCanExecute()
        {
            if (!_licenseService.AnyExistingLicense())
            {
                return false;
            }

            return true;
        }

        private async Task OnValidateLicenseOnLocalNetworkExecuteAsync()
        {
            NetworkValidationResult validationResult = null;

            validationResult = await _networkLicenseService.ValidateLicenseAsync();

            await _messageService.ShowAsync(string.Format("License is {0}valid, using '{1}' of '{2}' licenses", validationResult.IsValid ? string.Empty : "NOT ", validationResult.CurrentUsers.Count, validationResult.MaximumConcurrentUsers));
        }

        public TaskCommand ShowLicense { get; private set; }

        private Task OnShowLicenseExecuteAsync()
        {
            return _licenseVisualizerService.ShowLicenseAsync();
        }

        public TaskCommand ShowLicenseUsage { get; set; }

        private async Task OnShowLicenseUsageExecuteAsync()
        {
            var networkValidationResult = new NetworkValidationResult();

            networkValidationResult.MaximumConcurrentUsers = 2;
            networkValidationResult.CurrentUsers.AddRange(new[]
            {
                new NetworkLicenseUsage("12", "192.168.1.100", "Jon", "Licence signature", DateTime.Now),
                new NetworkLicenseUsage("13", "192.168.1.101", "Jane", "Licence signature", DateTime.Now),
                new NetworkLicenseUsage("14", "192.168.1.102", "Samuel", "Licence signature", DateTime.Now),
                new NetworkLicenseUsage("15", "192.168.1.103", "Paula", "Licence signature", DateTime.Now)
            });

            await _uiVisualizerService.ShowDialogAsync<NetworkLicenseUsageViewModel>(networkValidationResult);
        }

        protected override async Task InitializeAsync()
        {
            _networkLicenseService.Validated += OnNetworkLicenseValidated;

            // For debug / demo / test purposes, check every 10 seconds, recommended in production is 30 seconds or higher
            await Task.Factory.StartNew(() => _networkLicenseService.Initialize(TimeSpan.FromSeconds(10)));

            if (_licenseService.AnyExistingLicense())
            {
                var licenseString = _licenseService.LoadExistingLicense();
                var licenseValidation = await _licenseValidationService.ValidateLicenseAsync(licenseString);

                if (licenseValidation.HasErrors)
                {
                    await ShowLicenseDialogAsync();
                }
            }
            else
            {
                await ShowLicenseDialogAsync();
            }
        }

#pragma warning disable AvoidAsyncVoid
        private async void OnNetworkLicenseValidated(object sender, NetworkValidatedEventArgs e)
#pragma warning restore AvoidAsyncVoid
        {
            var validationResult = e.ValidationResult;
            if (!validationResult.IsValid)
            {
                var latestUsage = validationResult.GetLatestUser();

                if (validationResult.IsCurrentUserLatestUser())
                {
                    await _messageService.ShowAsync(string.Format("License is invalid, using '{0}' of '{1}' licenses. You are the latest user, your software will be shut down", validationResult.CurrentUsers.Count, validationResult.MaximumConcurrentUsers));
                }
                else
                {
                    await _messageService.ShowAsync(string.Format("License is invalid, using '{0}' of '{1}' licenses. The latest user is '{2}' with ip '{3}', you can continue working", validationResult.CurrentUsers.Count, validationResult.MaximumConcurrentUsers, latestUsage.UserName, latestUsage.Ip));
                }
            }
        }

        private Task ShowLicenseDialogAsync()
        {
            return _licenseVisualizerService.ShowLicenseAsync();
        }
    }
}
