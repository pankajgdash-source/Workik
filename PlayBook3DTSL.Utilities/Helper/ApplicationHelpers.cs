using System.Diagnostics.CodeAnalysis;

namespace PlayBook3DTSL.Utilities.Helpers
{
    [ExcludeFromCodeCoverage]
    public static class ApplicationHelpers
    {
        public static string NewID = "00000000-0000-0000-0000-000000000000";
        public static long LoggedInUserId;
        public static Guid? LoggedInHospitalId;
        public static string UserName;
        public static string? LoggedinUserRole;
        public static string UplodedMessage = "Uploaded successfully";
        public static string NewUserMessage = " has been added to your hospital. You may take action from SCMS if needed.";
        public static string RedoxUrl = "https://api.redoxengine.com/";
        public static string GetDocuemnt = "/Document/";
        public static string DefaultProcedureStepName = "Auto Generated Timer Step";
        public static bool IsMirrorModeOn;

    }
}
