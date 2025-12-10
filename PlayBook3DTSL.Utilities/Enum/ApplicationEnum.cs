using System.ComponentModel;

namespace PlayBook3DTSL.Utilities.Enum
{

    public enum CloudStorageName
    {
        Azure = 1
    }
    
    public enum ConfigType
    {
        Labelling,
        Annotation
    }

    public enum ImageViewType
    {
        AP,
        LATERAL
    }

    public enum EntityType
    {
        User = 1,
        Hospital = 2,
        Company = 3,
        Patient = 4,
        Supply = 5,
        Instrument = 6,
        Medication = 7,
        Tray = 8,
        Profile = 9,
        CheckList = 10,
        PreOp = 11,
        EquipmentNeeds = 12,
        Case = 13
    }
}
