using System.ComponentModel;

namespace PosApp
{
    public static class UserSession
    {
        private static User _currentUser;

        public static User CurrentUser
        {
            get => _currentUser;
            set
            {
                _currentUser = value;
                OnPropertyChanged(nameof(DisplayLoggedInUser));
            }
        }

        public static bool IsLoggedIn => CurrentUser != null;

        public static string DisplayLoggedInUser =>
            IsLoggedIn ? $"Acc : {CurrentUser.Username}" : "No user acc";

        public static event PropertyChangedEventHandler PropertyChanged;

        private static void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(null, new PropertyChangedEventArgs(propertyName));
        }
    }
}
