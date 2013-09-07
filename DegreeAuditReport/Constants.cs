namespace DegreeAuditReport
{
    public static class ParserConstants
    {
        public const string NEED_PASSWORD_INPUT = "u_pw";
        public const string NEED_USERNAME_INPUT = "u_id";
        public const string SSOL_BASE_URL = "https://ssol.columbia.edu";
        public const string SSOL_MAIN_FMT = "/cgi-bin/ssol/{0}/";
        public const string DAR_PAGE = "?tran[1]_tran_name=sdar";
        public const string RANDOM_INIT_SESSION = "4dt0gqiIxdJ0zBoRoqF4Jh";
    }

    public enum ResponseType
    {
        Succeeded,
        NeedToLogIn,
    }
}