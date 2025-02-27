using System.Diagnostics.Contracts;
using System.Text.Json.Serialization;

namespace LegacyLauncher.Models {
    public class Config {
        [JsonPropertyName("email")]
        public string Email {get; set;}

        [JsonPropertyName("password")]
        public string Password {get; set;}

        [JsonPropertyName("fortnitePath")]
        public string FortnitePath {get; set;}

        public Config(string Email, string Password, string FortnitePath) {
            this.Email = Email;
            this.Password = Password;
            this.FortnitePath = FortnitePath;
        }
    }
}