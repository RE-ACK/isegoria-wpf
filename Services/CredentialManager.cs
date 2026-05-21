using CredentialManagement;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Text;

namespace isegoria_wpf.Services
{
    public static class CredentialManager
    {
        private const string AccessTokenKey = "isegoria_access_token";
        private const string RefreshTokenKey = "isegoria_refresh_token";
        private const string SessionTokenKey = "isegoria_session_token";

        public static void SaveToken(string accessToken, string refreshToken)
        {
            using var accessCred = new Credential
            {
                Target = AccessTokenKey,
                Username = AccessTokenKey,
                Password = accessToken,
                PersistanceType = PersistanceType.LocalComputer
            };
            accessCred.Save();

            using var refreshCred = new Credential
            {
                Target = RefreshTokenKey,
                Username = RefreshTokenKey,
                Password = refreshToken,
                PersistanceType = PersistanceType.LocalComputer
            };
            refreshCred.Save();
        }

        public static void SaveSessionToken(string sessionToken)
        {
            using var sessionCred = new Credential
            {
                Target = SessionTokenKey,
                Username = SessionTokenKey,
                Password = sessionToken,
                PersistanceType = PersistanceType.LocalComputer
            };
            sessionCred.Save();
        }

        public static string? GetAccessToken(string accessToken)
        {
            using var cred = new Credential { Target = AccessTokenKey };
            return cred.Load() ? cred.Password : null;
        }

        public static string? GetRefreshToken(string refreshToken)
        {
            using var cred = new Credential { Target = RefreshTokenKey };
            return cred.Load() ? cred.Password : null;
        }

        public static void ClearTokens()
        {
            using var accessCred = new Credential { Target = AccessTokenKey };
            using var refreshCred = new Credential { Target = RefreshTokenKey };

            accessCred.Delete();
            refreshCred.Delete();
        }
    }
}
