using System.ComponentModel;

namespace PosApp.ViewModels
{
    public class MainViewModel
    {
        private User _loggedInUser;

        public User LoggedInUser
        {
            get => _loggedInUser;
            set
            {
                _loggedInUser = value;
                OnPropertyChanged(nameof(LoggedInUser));
                OnPropertyChanged(nameof(DisplayLoggedInUser));
            }
        }

        public string DisplayLoggedInUser => LoggedInUser != null
            ? $"{LoggedInUser.Role}: {LoggedInUser.Username}"
            : "No user is logged in.";

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
