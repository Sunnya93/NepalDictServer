namespace NepalDictServer.Services
{
    public class UserService
    {
        private DataContext _context;

        public UserService(DataContext context)
        {
            _context = context;
        }

        public async Task<UserModel?> Authenticate(string userPhone, string password)
        {
            UserModel? model = await _context.Users!.SingleOrDefaultAsync(x => x.UserPhone == userPhone);
            if(model is null || !BCrypt.Net.BCrypt.Verify(password, model.PassWord)) return null;
            return model;
        }
    }
}
