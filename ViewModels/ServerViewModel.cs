using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Threading.Tasks;

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
        public Action? OnCreateSuccess { get; set; }
        public Action? OnJoinSuccess { get; set; }

        [RelayCommand]
        private async Task CreateServerAsync()
        {
            if (string.IsNullOrEmpty(ServerName))
            {
                ErrorMessage = "서버 이름을 입력해주세요.";
                return;
            }

            //@TODO REST API 서버 생성 호출

            OnCreateSuccess?.Invoke();
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
