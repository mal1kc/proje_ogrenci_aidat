namespace OgrenciAidatSistemi.Configurations
{
    public static class Constants
    {
        public const String AppName = "OgrenciAidatSistemi";
        public const String AppVersion = "pre-release 0.9.2";
        public const String PasswdSalt = "as1235*02xp_19545s";
        public const short MaxUserNameLength = 124;
        public const short MinUserNameLength = 5;

        public const String EmailRegEx =
            @"\A(?:[a-z0-9!#$%&'*+/=?^_`{|}~-]+(?:\.[a-z0-9!#$%&'*+/=?^_`{|}~-]+)*@(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\.)+[a-z0-9](?:[a-z0-9-]*[a-z0-9])?)\Z";
        public const String PhoneNumberRegEx = @"\d{10}";

        // this should be read from .env file
        public const String AdminPasswordSalt =
            "583b18eacfb20ee2c32aa859263e77858914d0603587032969389a6ed14a1503";
        public const String CookieName = AppName;
        public const String AuthenticationCookieName = AppName + ".Auth";
        public const String AdminAuthenticationLoginPath =
            "/e065708a77dbb08ca7e9263254223251a015563b42ad3c7092df2797f279d5e9"; // nice song
#pragma warning disable IDE1006 // Naming Styles
        public static class userRoles
#pragma warning restore IDE1006 // Naming Styles
        {
            public const String None = "None";
            public const String SiteAdmin = "SiteAdminR";
            public const String SchoolAdmin = "SchoolAdminR";
            public const String Student = "StudentR";
        }

        public const String AuthenticationAccessDeniedPath = "/Home/AccessDenied";

        // 10 MB file size
        public const long MaxFileSize = 10 * 1024 * 1024;
    }
}
