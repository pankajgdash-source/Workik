namespace PlayBook3DTSL.Model.PacsServer
{
    public class PacsServerModel
    {
        public class CFindRequestServiceModel : CFindRequestModel
        {
            public string StudyInstanceUID { get; set; } = string.Empty;
            public string SeriesInstanceUID { get; set; } = string.Empty;
            public string SOPInstanceUID { get; set; } = string.Empty;
            public Guid HospitalId { get; set; }
            //public Func<Guid, HospitalModel>? GetPACSDetail { get; set; }
            public PACSRequest PACSRequest { get; set; }
        }

        public class CFindRequestModel
        {
            public string PatientName { get; set; } = string.Empty;
            public string PatientId { get; set; } = string.Empty;
            public Guid HospitalId { get; set; }
            public bool IsExactMatch { get; set; }
            public bool IsSearchedOnId { get; set; }
            public byte PACSServiceType { get; set; }

        }

        public class CFindPatientResponse
        {
            public string PatientName { get; set; } = string.Empty;
            public string PatientId { get; set; } = string.Empty;
            public DateTime PatientBirthDate { get; set; }
            public string PatientSex { get; set; } = string.Empty;
            public string PatientAge { get; set; } = string.Empty;
            public string StudyDescription { get; set; } = string.Empty;
            public DateTime? StudyDate { get; set; }
            public string Modality { get; set; } = string.Empty;
            public string NumberOfStudyRelatedSeries { get; set; } = string.Empty;
            public string AccessionNumber { get; set; } = string.Empty;
            public string PixelSpacing { get; set; }
            public string StudyInstanceUID { get; set; } = string.Empty;
            public string SeriesInstanceUID { get; set; } = string.Empty;
            public string SOPInstanceUID { get; set; } = string.Empty;
            public string SeriesNumber { get; set; } = string.Empty;
            public string InstanceNumber { get; set; } = string.Empty;
            public string NumberOfSeriesRelatedInstances { get; set; } = string.Empty;
            public string NumberOfPatientRelatedStudies { get; set; } = string.Empty;
            public string NumberOfFrames { get; set; } = string.Empty;
            public string AnatomicalOrientationType { get; set; } = string.Empty;
            public string TrackSetAnatomicalTypeCodeSequence { get; set; } = string.Empty;
            public DateTime StartAcquisitionDateTime { get; set; }
            public DateTime EndAcquisitionDateTime { get; set; }
        }

        public class PacsPatientDetail
        {
            public string PatientName { get; set; } = string.Empty;
            public string PatientId { get; set; } = string.Empty;
            public DateTime PatientBirthDate { get; set; }

            public List<CFindPatientResponse> CFindOtherDetail { get; set; }

            public bool IsExactMatch { get; set; }
        }

        public class CGetRequestServiceModel
        {
            public string PatientName { get; set; } = string.Empty;
            public string PatientId { get; set; } = string.Empty;
            public string StudyInstanceUID { get; set; } = string.Empty;
            public string SeriesInstanceUID { get; set; } = string.Empty;
            public string SOPInstanceUID { get; set; } = string.Empty;
            public PACSRequest PACSRequest { get; set; }
            public Guid HospitalId { get; set; }
            public byte PACSServiceType { get; set; }
        }

        public class CGetPatientResponse
        {
            public string PatientName { get; set; } = string.Empty;
            public string PatientId { get; set; } = string.Empty;
            public DateTime PatientBirthDate { get; set; }
            public string StudyDescription { get; set; } = string.Empty;
            public DateTime StudyDate { get; set; }
            public string Modality { get; set; } = string.Empty;
            public string NumberOfStudyRelatedSeries { get; set; } = string.Empty;
            public string AccessionNumber { get; set; } = string.Empty;
            public string StudyInstanceUID { get; set; } = string.Empty;
            public string SeriesInstanceUID { get; set; } = string.Empty;
            public string SOPInstanceUID { get; set; } = string.Empty;
            public string SeriesNumber { get; set; } = string.Empty;
            public string InstanceNumber { get; set; } = string.Empty;
            public string NumberOfSeriesRelatedInstances { get; set; } = string.Empty;
            public string NumberOfPatientRelatedStudies { get; set; } = string.Empty;
            public string NumberOfFrames { get; set; } = string.Empty;
            public string AnatomicalOrientationType { get; set; } = string.Empty;
            public string TrackSetAnatomicalTypeCodeSequence { get; set; } = string.Empty;
            public DateTime StartAcquisitionDateTime { get; set; }
            public DateTime EndAcquisitionDateTime { get; set; }
            public string SaveImagePath { get; set; }
        }



        public enum PACSRequest
        {
            Patient = 0,
            Study = 1,
            Image = 2
        }
    }
}
