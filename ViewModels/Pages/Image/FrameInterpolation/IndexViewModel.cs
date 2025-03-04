﻿using General.Apt.App.Utility;
using General.Apt.Service.Exceptions;
using General.Apt.Service.Models;
using General.Apt.Service.Services.Pages.Image.FrameInterpolation2;
using General.Apt.Service.Utility;
using Microsoft.Win32;
using System.Windows.Documents;
using Wpf.Ui.Controls;

namespace General.Apt.App.ViewModels.Pages.Image.FrameInterpolation
{
    public partial class IndexViewModel : ObservableValidator, INavigationAware
    {
        private bool _isInitialized = false;
        private IndexService _indexService;

        public Action<Paragraph> MessageAction { get; set; }

        [ObservableProperty]
        private string _input;

        [RelayCommand]
        private void SetInput()
        {
            var openFolderDialog = new OpenFolderDialog();
            if (!string.IsNullOrEmpty(Input)) openFolderDialog.InitialDirectory = Input;
            if (openFolderDialog.ShowDialog() is true)
            {
                Input = openFolderDialog.FolderName;
            }
        }

        [ObservableProperty]
        private string _output;

        [RelayCommand]
        private void SetOutput()
        {
            var openFolderDialog = new OpenFolderDialog();
            if (!string.IsNullOrEmpty(Output)) openFolderDialog.InitialDirectory = Output;
            if (openFolderDialog.ShowDialog() is true)
            {
                Output = openFolderDialog.FolderName;
            }
        }

        [ObservableProperty]
        private ObservableCollection<ComBoBoxItem<string>> _inputSortSource;

        [ObservableProperty]
        private ComBoBoxItem<string> _inputSortItem;

        public string InputSort
        {
            get => InputSortItem.Value;
            set => InputSortItem = InputSortSource.FirstOrDefault(e => e.Value == value);
        }

        [ObservableProperty]
        private ObservableCollection<ComBoBoxItem<string>> _sortRuleSource;

        [ObservableProperty]
        private ComBoBoxItem<string> _sortRuleItem;

        public string SortRule
        {
            get => SortRuleItem.Value;
            set => SortRuleItem = SortRuleSource.FirstOrDefault(e => e.Value == value);
        }

        [ObservableProperty]
        private ObservableCollection<ComBoBoxItem<string>> _providerSource;

        [ObservableProperty]
        private ComBoBoxItem<string> _providerItem;

        public string Provider
        {
            get => ProviderItem.Value;
            set => ProviderItem = ProviderSource.FirstOrDefault(e => e.Value == value);
        }

        [ObservableProperty]
        private ObservableCollection<ComBoBoxItem<string>> _modeSource;

        [ObservableProperty]
        private ComBoBoxItem<string> _modeItem;

        public string Mode
        {
            get => ModeItem.Value;
            set => ModeItem = ModeSource.FirstOrDefault(e => e.Value == value);
        }

        [ObservableProperty]
        private ObservableCollection<ComBoBoxItem<string>> _scaleSource;

        [ObservableProperty]
        private ComBoBoxItem<string> _scaleItem;

        public string Scale
        {
            get => ScaleItem.Value;
            set => ScaleItem = ScaleSource.FirstOrDefault(e => e.Value == value);
        }

        [ObservableProperty]
        private int _progressBarMaximum;

        [ObservableProperty]
        private int _progressBarValue;

        partial void OnProgressBarValueChanged(int value)
        {
            OnPropertyChanged(nameof(ProgressBarText));
        }

        public string ProgressBarText
        {
            get => (ProgressBarValue / (double)ProgressBarMaximum).ToString("0.00%");
        }

        [ObservableProperty]
        private bool _startEnabled;

        [RelayCommand]
        private async Task SetStart() => await Start();

        [ObservableProperty]
        private bool _stopEnabled;

        [RelayCommand]
        private void SetStop() => StopEnabled = false;

        [ObservableProperty]
        private bool _openEnabled;

        [RelayCommand]
        private void SetOpen() => System.Diagnostics.Process.Start("explorer", Output);

        public IndexViewModel()
        {
            if (!_isInitialized) InitializeViewModel();
        }

        public void OnNavigatedTo()
        {
            if (!_isInitialized) InitializeViewModel();
        }

        public void OnNavigatedFrom() { }

        public void InitializeViewModel()
        {
            InputSortSource = new ObservableCollection<ComBoBoxItem<string>>()
            {
                new ComBoBoxItem<string>() { Text = Language.GetString("ImageFrameInterpolationIndexPageInputSortName"), Value = "Name" },
                new ComBoBoxItem<string>() { Text = Language.GetString("ImageFrameInterpolationIndexPageInputSortLastWriteTime"), Value = "LastWriteTime" },
                new ComBoBoxItem<string>() { Text = Language.GetString("ImageFrameInterpolationIndexPageInputSortLength"), Value = "Length" }
            };
            SortRuleSource = new ObservableCollection<ComBoBoxItem<string>>()
            {
                new ComBoBoxItem<string>() { Text = Language.GetString("ImageFrameInterpolationIndexPageInputSortRuleAsc"), Value = "Asc" },
                new ComBoBoxItem<string>() { Text = Language.GetString("ImageFrameInterpolationIndexPageInputSortRuleDesc"), Value = "Desc" }
            };
            ProviderSource = Device.Provider2;
            ModeSource = new ObservableCollection<ComBoBoxItem<string>>()
            {
                new ComBoBoxItem<string>() {  Text = Language.GetString("ImageFrameInterpolationIndexPageModeStandard"), Value = "Standard" }
            };
            ScaleSource = new ObservableCollection<ComBoBoxItem<string>>()
            {
                new ComBoBoxItem<string>() { Text = Language.GetString("ImageFrameInterpolationIndexPageScaleX2"), Value = "X2" },
                new ComBoBoxItem<string>() { Text = Language.GetString("ImageFrameInterpolationIndexPageScaleX3"), Value = "X3" },
                new ComBoBoxItem<string>() { Text = Language.GetString("ImageFrameInterpolationIndexPageScaleX4"), Value = "X4" },
                new ComBoBoxItem<string>() { Text = Language.GetString("ImageFrameInterpolationIndexPageScaleX5"), Value = "X5" },
                new ComBoBoxItem<string>() { Text = Language.GetString("ImageFrameInterpolationIndexPageScaleX6"), Value = "X6" },
                new ComBoBoxItem<string>() { Text = Language.GetString("ImageFrameInterpolationIndexPageScaleX7"), Value = "X7" },
                new ComBoBoxItem<string>() { Text = Language.GetString("ImageFrameInterpolationIndexPageScaleX8"), Value = "X8" }
            };
            ProgressBarMaximum = 1000000;
            ProgressBarValue = 0;
            StartEnabled = true;
            StopEnabled = false;
            OpenEnabled = false;

            _indexService = new IndexService();
            _indexService.ProgressMax = ProgressBarMaximum;
            _indexService.Message = async (type, message) => await Message.AddMessage(type, message, MessageAction);
            _indexService.Progress = async (process) => await AddProcess(process);
            _indexService.IsStop = () => !StopEnabled;

            _isInitialized = true;
        }

        public async Task Start()
        {
            try
            {
                StartEnabled = false;
                StopEnabled = true;
                OpenEnabled = true;

                if (!Device.VulkanEnable) throw new Exception(Language.GetString("ImageFrameInterpolationIndexPageNotSupportVulkan"));

                await _indexService.Start(Input, Output, InputSort, SortRule, Provider, Mode, Scale);

                ProgressBarValue = ProgressBarMaximum;

                Message.ShowSnackbarSuccess(Language.GetString("ImageFrameInterpolationIndexPageOperationCompleted"));

                if (Current.Config.App.IsAutoOpenOutput) SetOpen();
            }
            catch (ActivationException ex)
            {
                await Validate.ShowLicense(ex.Message);
            }
            catch (Exception ex)
            {
                Message.ShowSnackbarError(ex.Message);

                await Message.AddMessageError(ex.Message, MessageAction);
            }
            finally
            {
                ProgressBarValue = 0;
                StartEnabled = true;
                StopEnabled = false;
            }
        }

        public Task AddProcess(int process)
        {
            return Task.Run(() =>
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    ProgressBarValue = process;
                });
            });
        }
    }
}