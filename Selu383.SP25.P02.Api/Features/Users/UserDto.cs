namespace Selu383.SP25.P02.Api.Features.Users
{
    public class UserDto
    {
        public required int Id { get; set; }  
        public required string UserName { get; set; }
        public required string[] Roles { get; set; }  
    }
}
