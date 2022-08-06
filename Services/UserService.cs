namespace NepalDictServer.Services
{
    public class UserService
    {
        private DataContext _context;

        public UserService(DataContext context)
        {
            _context = context;
        }

        public async Task<UserModel> Authenticate(string userPhone, string password)
        {
            //&& x.PassWord == password

            // wrapped in "await Task.Run" to mimic fetching user from a db
            var user = await Task.Run(() => _context.Users!.SingleOrDefault(x => x.UserPhone == userPhone));

            // on auth fail: null is returned because user is not found
            // on auth success: user object is returned
            return user!;
        }
    }
}
