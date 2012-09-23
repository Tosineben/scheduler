namespace Columbia.Scheduler
{
    public static class ParserConstants
    {
        public const string FormStart = "/cgi-bin/ssol/";
        public const string FormName = "Search";
        public const string FinalAddFormName = "query";
        public const string OpenClass = "cls0";
        public const string RestrictedClass = "cls1";
        public const string BlockedClass = "cls2";
        public const string NoAvailSections = "no available sections found";
        public const string SysNotAvail = "System not available";
        public const string NeedsPasswordInput = "u_pw";
        public const string NeedsUsernameInput = "u_id";
        public const string AddSubmitKey = "tran[1]_CALLNUM";
        public const string NeedInstructorPermissionToAddKey = "tran[1]_INSTRPERM";
        public const string SelectPassFailKey = "tran[1]_PASSFAIL";
        public const string PassFailResponse = "N"; //taking class for credit, not pass fail
        public const string RootUrlFmt = "https://ssol.columbia.edu{0}?";
        public const string RandomInitSession = "60gyrn1NvIBN8dtDJfB7rx";
        public const string InitParamsFmt = "p_r_id=V8RgGDrj9CkrBe2agfy7fh&p_t_id=1&tran%5B1%5D_entry=student&tran%5B1%5D_tran_name=sregs&tran%5B1%5D_ss={0}&tran%5B1%5D_act=Search+Class";
    }
}