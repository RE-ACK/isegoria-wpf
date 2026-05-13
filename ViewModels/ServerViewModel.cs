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

        public Action? OnCreateSuccess { get; set; }
        public Action? OnJoinSuccess { get; set; }

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

            // 선택하자마자 바로 업로드
            Debug.WriteLine($"업로드 시도: {SelectedImagePath}");

            var urls = await ApiServer.UploadImagesAsync(SelectedImagePath);

            UploadedImageUrl = urls?.FirstOrDefault(); 

            Debug.WriteLine($"urls 자체: {urls}");
            Debug.WriteLine($"urls 개수: {urls?.Count}");
            Debug.WriteLine($"첫번째 URL: {urls?.FirstOrDefault()}");

            if (UploadedImageUrl == null)
            {
                ErrorMessage = "이미지 업로드에 실패했습니다.";
                // 실패하면 다시 기본 아이콘으로
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

            //@TODO REST API 서버 생성 호출
            ErrorMessage = string.Empty;

            // 이미 업로드된 URL 사용
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

            //@TODO REST API 서버 참여 호출 

            OnJoinSuccess?.Invoke();

        }
    }
}
