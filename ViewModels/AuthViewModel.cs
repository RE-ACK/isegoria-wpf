using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using isegoria_wpf.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;

namespace isegoria_wpf.ViewModels
{
    public partial class AuthViewModel : ObservableObject
    {
        [ObservableProperty]
        private string _userId = string.Empty;

        [ObservableProperty]
        private string _userName = string.Empty;

        [ObservableProperty]
        private string _errorMessage = string.Empty;

        // PasswordBox는 바인딩 안되서 따로 처리
        public string Password { get; set; } = string.Empty;

        public string RegisterPassword { get; set; } = string.Empty;

        //=======================================================================//
        // Actions
        //=======================================================================//

        public Action? OnRegisterSuccess { get; set; }


        //=======================================================================//

        [RelayCommand]
        private async Task LoginAsync()
        {
            if (string.IsNullOrEmpty(UserId) || string.IsNullOrEmpty(Password))
            {
                ErrorMessage = "아이디와 비밀번호를 입력해주세요.";
                return;
            }

            var mainWindow = new MainWindow();

            var result = await ApiClient.LoginAsync(UserId, Password);

            if(result?.StatusCode == 200)
            {
                // 성공 시 메인윈도우 전환
               
                var servers = await ApiServer.GetMyServersAsync();
                if (servers != null)
                {
                    foreach (var server in servers)
                    {
                        //Debug.WriteLine($"서버: {server.Name}, iconUrl: {server.IconUrl ?? "null"}");
                        mainWindow.AddServerToList(server.IconUrl, server.Name,server.Id);
                    
                    }
                }

                mainWindow.Show();
                Application.Current.Windows[0]?.Close();
            }
            else
            {
                ErrorMessage = result?.Message ?? "로그인 실패";
            }
        }

        [RelayCommand]
        private async Task RegisterAsync()
        {

            if (string.IsNullOrEmpty(UserName) || string.IsNullOrEmpty(UserId) || string.IsNullOrEmpty(RegisterPassword))
            {
                ErrorMessage = "모든 항목을 입력해주세요.";
                return;
            }

            var result = await ApiClient.RegisterAsync(UserId, UserName, RegisterPassword);

            if (result?.StatusCode == 200)
            {
                // 성공시 로그인뷰 전환
                MessageBox.Show("성공");
                OnRegisterSuccess?.Invoke();
            }
            else
            {
                ErrorMessage = result?.Message ?? "회원가입 실패";
            }
        }

        [RelayCommand]
        private async Task FindPasswordAsync()
        {
            //@TODO REST API 비밀번호 찾기

            if (string.IsNullOrEmpty(UserId))
            {
                ErrorMessage = "아이디(이메일)을 입력해주세요";
                return;
            }

            Debug.WriteLine("userId : " + UserId);
        }


        [RelayCommand]
        private async Task UpdateUserInfoAsync()
        {
            //@TODO REST API 유저 정보 업데이트

            Debug.WriteLine("UpdateUserInfo 호출됨");
        }

    }
}
