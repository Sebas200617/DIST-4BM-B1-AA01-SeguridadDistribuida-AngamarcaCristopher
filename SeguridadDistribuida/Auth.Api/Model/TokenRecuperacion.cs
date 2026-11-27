namespace Auth.Api.Model
{
    public class TokenRecuperacion
    {
        public int Id { get; set; }
        public string UserId { get; set; } = string.Empty;
        public string TokenHash { get; set; } = string.Empty;
        public DateTime FechaExpira { get; set; }
        public bool Usado { get; set; }
    }
}
