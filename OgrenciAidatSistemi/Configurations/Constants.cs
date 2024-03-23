namespace OgrenciAidatSistemi.Configurations
{
    public static class Constants
    {
        // TODO : read all constants (settings) from json or toml file
        public const String AppName = "OgrenciAidatSistemi";
        public const String AppVersion = "0.0.1";
        public const String PasswdSalt = "asp_19545s";
        public const short MaxUserNameLength = 124;
        public const short MinUserNameLength = 5;

        public const String EmailRegEx =
            @"\A(?:[a-z0-9!#$%&'*+/=?^_`{|}~-]+(?:\.[a-z0-9!#$%&'*+/=?^_`{|}~-]+)*@(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\.)+[a-z0-9](?:[a-z0-9-]*[a-z0-9])?)\Z";

        // this should be read from .env file
        public const String AdminPasswordSalt =
            "583b18eacfb20ee2c32aa859263e77858914d0603587032969389a6ed14a1503";
        public const String CookieName = AppName;
        public const String AuthenticationCookieName = AppName + ".Auth";
        public const String AuthenticationLoginPath = "/signIn";
        public const String AdminAuthenticationLoginPath =
            "/0e4ec65beafa9ffb05abf95f2299783ba48721134a35a7abe28aab71e0ad15c04543917869ef1ae7241186cf5bd9b28c4270031a910ca8b2d76de995ef1a73ff";

        // TODO : this should be used as const like enum but as str
        public static class userRoles
        {
            public const String SiteAdmin = "SiteAdminR";
            public const String SchoolAdmin = "SchoolAdminR";
            public const String Student = "StudentR";
        }
    }
}
