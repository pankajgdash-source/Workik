using FellowOakDicom;
using FellowOakDicom.Imaging;
using FellowOakDicom.Imaging.NativeCodec;
using System.Drawing;
using System.Drawing.Imaging;

namespace WebApp.Services
{
    public class DicomService
    {
        public DicomService()
        {
            // Initialize DICOM with codec support
            new DicomSetupBuilder()
                .RegisterServices(s => s.AddFellowOakDicom()
                                      .AddTranscoderManager<NativeTranscoderManager>()
                                      .AddImageManager<WinFormsImageManager>())
                .Build();
        }

      }
} 