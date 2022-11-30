namespace XinjingdailyBot.Helpers
{
    internal static class UserHelper
    {
        internal static string NickName(this User user)
        {
            return string.IsNullOrEmpty(user.LastName) ? user.FirstName : $"{user.FirstName} {user.LastName}";
        }

        internal static string UserID(this User user)
        {
            return string.IsNullOrEmpty(user.Username) ? $"#{user.Id}" : $"@{user.Username}";
        }

        internal static string UserProfile(this User user)
        {
            return $"{user.EscapedUserName()} {user.UserID()}";
        }
    }
}
