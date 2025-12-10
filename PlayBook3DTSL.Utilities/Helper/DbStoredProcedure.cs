using System.Diagnostics.CodeAnalysis;

namespace PlayBook3DTSL.Utilities.Helpers
{
    [ExcludeFromCodeCoverage]
    public static class DbStoredProcedure
    {
        public static string GetOpenRequestList = "SP_OpenRequestList";
        public static string GetOpenRequestCommentList = "SP_OpenRequestCommentList";
        public static string GetSurgeryByHospitalList = "SP_GetSurgeryByHospital";
        public static string GetHospitalWiseStaff = "SP_GetHospitalWiseStaff";
        public static string GetUserRoleById = "SP_GetUserRoleById";
        public static string GetHospitalWiseSalary = "SP_GetHospitalWiseSalary";
        public static string GetCasesByHospital = "SP_GetCasesByHospital";
        public static string GetResultByCaseId = "SP_GetResultByCaseId";
        public static string GetConfigurationByCaseResultId = "SP_GetConfigurationByCaseResultId";
        public static string GetCaseResultDetailById = "SP_GetCaseResultDetailById";
        public static string GetCasesMeasurementByCaseId = "SP_GetCasesMeasurementByCaseId";
        public static string GetOverlayImages = "SP_GetOverlayImages";
        public static string InsertCaseresult = "SP_InsertCaseresult";
        public static string UpdateSubStepSortOrder = "SP_UpdateSubStepSortOrder";
        public static string InsertCase = "SP_InsertCase";

    }
}
