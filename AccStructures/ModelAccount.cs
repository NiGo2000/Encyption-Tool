using Encrypter.Utilities;

namespace Encrypter.AccStructures 
{
    public class ModelAccount : ModelBase
    {
        private string websitePath;
        private string email;
        private string username;
        private string password;

        public string WebsitePath { get => websitePath; set => RaisePropertyChanged(ref websitePath, value); }
        public string Email { get => email; set => RaisePropertyChanged(ref email, value); }
        public string Username { get => username; set => RaisePropertyChanged(ref username, value); }
        public string Password { get => password; set => RaisePropertyChanged(ref password, value); }
    }
}
