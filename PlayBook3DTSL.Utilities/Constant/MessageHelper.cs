using System.Diagnostics.CodeAnalysis;

namespace PlayBook3DTSL.Utilities.Constant
{
    [ExcludeFromCodeCoverage]
    public static class MessageHelper
    {

        public const string GenericErrorMessage = "Oops, Something went wrong please try again or contact support";
        public const string Invalid = "Invalid {0}";
        public const string NotFound = "{0} not found";
        public const string ResourseNotExist = "'{0}' with provided {1} doesn't exists";
        public const string ResourceExists = "{0} already exists in the system";
        public const string SuccessMessage = "{0} {1} successfully";
        public const string Anonymous = "Anonymous";
        public const string NoDataFound = "Data Not Found.";
        public const string NotValidPhoneOrEmail = "Not a valid phone or email";
        public const string NotValidPhone = "Not a valid phone number";
        public const string SurgeryNotOpen = "Surgery is not in open state.";
        public const string InvalidImage = "No Image found for this caseResultId";
        public const string InvalId = "No valid IDs provided.";
        public const string APSubStepSelection = "AP Substep Selection";
        public const string LATSubStepSelection = "LAT Substep Selection";



        #region AzureCommand
        public static class AzureCommand
        {
            public const string EndPoint = "You can add only one Endpoint.";
            public const string NotFount = "Endpoint not found.";
        }
        #endregion

        [ExcludeFromCodeCoverage]
        public static class FileExtension
        {
            public static List<string> ImageFileExtensions = new() {
            "jpg","png","jpeg",  "bmp","svg", "psd"
        };
            public static List<string> VideoFileExtensions = new() {
            "mp4", "mpeg", "webm", "ogg", "avi","mov"
        };
            public static List<string> DocumentFileExtensions = new() {
            "pdf"
        };
            public static List<string> DicomFileExtensions = new() {
            "dcm"
        };
        }
    }
}

