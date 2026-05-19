using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using isegoria_wpf.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using static isegoria_wpf.Models.Dtos.ServerDto;

namespace isegoria_wpf.ViewModels
{
    public partial class ServerViewModel : ObservableObject
    {
        [ObservableProperty]
        private string _serverName = string.Empty;

        [ObservableProperty]
        private string _serverCode = string.Empty;

        [ObservableProperty]
        private string _errorMessage = string.Empty;

        [ObservableProperty]
        private ObservableCollection<ServerInfo> _serverList = new();

        [ObservableProperty]
        private string _selectedImagePath = string.Empty;

        [ObservableProperty]
        private string? _uploadedImageUrl = null;

        [ObservableProperty]
        private Visibility _previewVisibility = Visibility.Collapsed;

        [ObservableProperty]
        private Visibility _defaultIconVisibility = Visibility.Visible;

        [ObservableProperty]
        private ServerInfo? _selectedServer;

        [ObservableProperty]
        private bool _isUploading = false;
        public bool CanCreate => !IsUploading;
        public string CreateButtonText => IsUploading ? "업로드 중..." : "생성";

        public Action? OnCreateSuccess { get; set; }
        public Action? OnJoinSuccess { get; set; }

        partial void OnIsUploadingChanged(bool value)
        {
            OnPropertyChanged(nameof(CanCreate));
            OnPropertyChanged(nameof(CreateButtonText));
        }

        [RelayCommand]
        private async Task SelectImageAsync()
        {
            var dialog = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "이미지 파일|*.jpg;*.jpeg;*.png;*.gif;*.webp"
            };

            if (dialog.ShowDialog() != true) return;

            SelectedImagePath = dialog.FileName;
            PreviewVisibility = Visibility.Visible;
            DefaultIconVisibility = Visibility.Collapsed;


            IsUploading = true;
            var urls = await ApiServer.UploadImagesAsync(SelectedImagePath);
            UploadedImageUrl = urls?.FirstOrDefault();
            IsUploading = false; 

            if (UploadedImageUrl == null)
            {
                ErrorMessage = "이미지 업로드에 실패했습니다.";
                SelectedImagePath = string.Empty;
                PreviewVisibility = Visibility.Collapsed;
                DefaultIconVisibility = Visibility.Visible;
            }
        }


        [RelayCommand]
        private async Task CreateServerAsync()
        {
            if (string.IsNullOrEmpty(ServerName))
            {
                ErrorMessage = "서버 이름을 입력해주세요.";
                return;
            }

            ErrorMessage = string.Empty;

            var server = await ApiServer.CreateServerAsync(ServerName, UploadedImageUrl);

            if (server != null)
            {
                ServerList.Add(server);
                SelectedServer = server;
                ServerName = string.Empty;
                SelectedImagePath = string.Empty;
                UploadedImageUrl = null;
                PreviewVisibility = Visibility.Collapsed;
                DefaultIconVisibility = Visibility.Visible;
                OnCreateSuccess?.Invoke();
            }
            else
            {
                ErrorMessage = "서버 생성에 실패했습니다.";
            }
        }

        [RelayCommand]
        private async Task JoinServerAsync()
        {
            if (string.IsNullOrEmpty(ServerCode))
            {
                ErrorMessage = "서버 코드를 입력해주세요.";
                return;
            }

            ErrorMessage = string.Empty;

            var (server, message) = await ApiServer.JoinServerAsync(ServerCode);

            if (server != null)
            {
                ServerList.Add(server);
                SelectedServer = server;
                ServerCode = string.Empty;
                OnJoinSuccess?.Invoke();
            }
            else
            {
                ErrorMessage = message ?? "서버 입장에 실패했습니다.";
            }



        }
    }
}
