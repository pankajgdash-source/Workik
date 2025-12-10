using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using FellowOakDicom;
using FellowOakDicom.Imaging;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using PlayBook3DTSL.API.Middleware;
using PlayBook3DTSL.Model.Case;
using PlayBook3DTSL.Services.Interfaces.Case;
using System.Drawing;
using System.Drawing.Imaging;
using WebApp.Services;
namespace PlayBook3DTSL.API.Controllers;

[Route("api/{version=default}/[controller]")]
[ApiController]
[ApiVersion("v1")]
//[Authorize]
public class CaseController : BaseController
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ICaseService _caseService;
    private readonly IWebHostEnvironment _environment;
    private readonly DicomService _dicomService;

    public CaseController(IServiceProvider serviceProvider, ICaseService caseService,
         IWebHostEnvironment environment,
           DicomService dicomService)
    {
        _serviceProvider = serviceProvider;
        _caseService = caseService;
        _environment = environment;
        _dicomService = dicomService;
    }

    
    [HttpGet("GetCaseDetailById")]
    public async Task<IActionResult> GetCaseDetailById(Guid caseId)
    {
        return GenerateResponse(_caseService.GetCaseDetailById(caseId));
    }


    [HttpPost("CreateCase")]
    public async Task<IActionResult> CreateCase([FromBody] CaseCreateServiceModel caseCreateServiceModel)
    {
        return GenerateResponse(_caseService.CreateCaseAsync(caseCreateServiceModel));
    }


    [HttpPost("AddCaseImages")]
    public async Task<IActionResult> AddCaseImages([FromForm] CaseImagesServiceModel caseImagesServiceModel)
    {
        var imageListJson = Request.Form["ImageList"];

        if (!string.IsNullOrWhiteSpace(imageListJson))
        {
            caseImagesServiceModel.ImageList = JsonConvert.DeserializeObject<List<CaseImagesNameServiceModel>>(imageListJson);
        }
        return GenerateResponse(_caseService.AddCaseImages(caseImagesServiceModel));
    }


    [HttpPost("AddCaseStudy")]
    public async Task<IActionResult> AddCaseStudy(CaseStudyServiceModel caseStudyMappingServiceModel)
    {
        return GenerateResponse(_caseService.AddCaseStudy(caseStudyMappingServiceModel));
    }


    [HttpGet("GetCaseStudy")]
    public async Task<IActionResult> GetCaseStudy(Guid caseId)
    {
        return GenerateResponse(_caseService.GetCaseStudy(caseId));
    }


    [HttpPost("GetCaseImages")]
    public async Task<IActionResult> GetCaseImages(GetCaseImageDetailServiceModel getCaseImageDetailServiceModel)
    {
        return GenerateResponse(await _caseService.GetCaseImages(getCaseImageDetailServiceModel));
    }


    [HttpPost("UpdateLabellingConfigData")]
    public async Task<IActionResult> UpdateLabellingConfigData(CaseUpdateConfigServiceModel caseUpdateConfigServiceModel)
    {
        return GenerateResponse(_caseService.UpdateLabellingConfigData(caseUpdateConfigServiceModel));
    }

    [HttpGet("GetLabellingConfigData")]

    public async Task<IActionResult> GetLabellingConfigData(Guid caseResultId)
    {
        return GenerateResponse(_caseService.GetLabellingConfigData(caseResultId));
    }



    
    [HttpGet("GetCasesByHospitalId")]
    public async Task<IActionResult> GetCasesByHospitalId()
    {
        return GenerateResponse(_caseService.GetCasesByHospital());
    }


    [HttpGet("GetResultByCaseId")]
    public async Task<IActionResult> GetResultByCaseId(Guid caseId)
    {
        return GenerateResponse(_caseService.GetResultByCaseId(caseId));
    }


    [HttpGet("GetCaseMeasurementStep")]
    public async Task<IActionResult> GetCaseMeasurementStep(Guid caseId, string casePeriod)
    {
        return GenerateResponse(_caseService.GetCaseMeasurementStepsByCaseId(caseId, casePeriod));
    }


    [HttpGet("UpdateCaseMeasurementStep")]
    public async Task<IActionResult> UpdateCaseMeasurementStep(Guid caseMeasurementStepsId, bool isCompleted)
    {
        return GenerateResponse(_caseService.UpdateCaseMeasurementStepsId(caseMeasurementStepsId, isCompleted));
    }


    [HttpPost("UpdateConfigData")]
    public async Task<IActionResult> UpdateConfigData(CaseUpdateConfigServiceModel caseUpdateConfigServiceModel)
    {
        return GenerateResponse(_caseService.UpdateConfigData(caseUpdateConfigServiceModel));
    }


    [HttpGet("GetConfigurationData")]
    public async Task<IActionResult> GetConfigurationData(Guid caseResultId)
    {
        return GenerateResponse(_caseService.GetConfigurationByCaseResultId(caseResultId));
    }


    [HttpGet("GetCaseResultDetailById")]
    public async Task<IActionResult> GetCaseResultDetailById(Guid caseResultId)
    {
        return GenerateResponse(_caseService.GetCaseResultDetailById(caseResultId));
    }


    [HttpPost("Add3DTSLCalculation")]
    public async Task<IActionResult> Add3DTSLCalculation(ResultServiceModel resultServiceModel)
    {
        return GenerateResponse(_caseService.Add3DTSLCalculation(resultServiceModel));
    }

    [HttpGet("Check3DTSLDataExists")]
    public async Task<IActionResult> Check3DTSLDataExists(Guid caseResultId)
    {
        return GenerateResponse(_caseService.Check3DTSLDataExists(caseResultId));
    }


    [HttpGet("GetResultListByCaseId")]
    public async Task<IActionResult> GetResultListByCaseId(Guid caseId)
    {
        return GenerateResponse(_caseService.GetResultListByCaseId(caseId));
    }


    [HttpPost("Add3DTSLImages")]
    public async Task<IActionResult> Add3DTSLImages([FromForm] Add3DTSLImageServiceModel add3DTSLImageServiceModel)
    {
        var imageListJson = Request.Form["ImageList"];

        if (!string.IsNullOrWhiteSpace(imageListJson))
        {
            add3DTSLImageServiceModel.ImageList = JsonConvert.DeserializeObject<List<CaseImagesNameServiceModel>>(imageListJson);
        }
        return GenerateResponse(_caseService.Add3DTSLImages(add3DTSLImageServiceModel));
    }


    [HttpGet("CreateResultByCaseId")]
    public async Task<IActionResult> CreateResultByCaseId(Guid caseId, string period)
    {
        return GenerateResponse(_caseService.CreateResultByCaseIdAsync(caseId, period));
    }

    [HttpPost("GetCaseImageZip")]
    public async Task<IActionResult> GetCaseImageZip(List<Guid> caseResultIds)
    {
        var response = _caseService.GetCaseImageZip(caseResultIds);
        var mimeType = "application/zip";
        return PhysicalFile(response.Output, mimeType, "CaseResults_Data.zip");
    }

    [HttpPost("GetCaseResultsCsv")]
    public async Task<IActionResult> GetCaseResultsCsv(CaseImageDetailServiceModel caseImageDetailServiceModel)
    {
        var response = _caseService.GetCaseResultsCsv(caseImageDetailServiceModel);
        var mimeType = "text/csv";
        return PhysicalFile(response.Output, mimeType, "CaseResults_Data.csv");
    }

    [HttpPost("GetOverlayImages")]
    public async Task<IActionResult> GetOverlayImages(List<Guid> caseResultIds)
    {
        return GenerateResponse(_caseService.GetOverlayImages(caseResultIds));
    }

    [HttpGet("UpdateResultImage")]
    public async Task<IActionResult> UpdateResultImage(Guid caseResultId, string imageViewType, string imageName)
    {
        return GenerateResponse(_caseService.UpdateResultImage(caseResultId, imageViewType, imageName));
    }


    [HttpPost("GetCaseImagePDF")]
    public async Task<IActionResult> GetCaseImagePDF(List<Guid> caseResultIds)
    {
        var response = _caseService.GetCaseImagePDF(caseResultIds);
        var mimeType = "application/pdf";
        return PhysicalFile(response.Output, mimeType, "CaseResults_Data.pdf");
    }

    [HttpPost("GetCaseResultsJSON")]
    public async Task<IActionResult> GetCaseResultsJSON(List<Guid> ids)
    {
        var response = _caseService.GetCaseResultsJSON(ids);
        var mimeType = "application/json";
        return PhysicalFile(response.Output.Result, mimeType, "CaseResults_Data.json");

    }

    [HttpPost("GetCaseResultsZip")]
    public async Task<IActionResult> GetCaseResultsZip(List<Guid> ids)
    {
        var response = _caseService.GetCaseResultsZip(ids);
        var mimeType = "application/zip";
        return PhysicalFile(response.Output.Result, mimeType, "CaseResults_Data.zip");
    }

    [HttpGet("UpdateCaseMeasurementSubStep")]
    public async Task<IActionResult> UpdateCaseMeasurementSubStep(Guid caseMeasurementSubStepsId, bool isCompleted)
    {
        return GenerateResponse(_caseService.UpdateCaseMeasurementSubStepsId(caseMeasurementSubStepsId, isCompleted));
    }


    [HttpGet("GetCalibrationValue")]
    public async Task<IActionResult> GetCalibrationValue(Guid caseResultId)
    {
        return GenerateResponse(_caseService.GetCalibrationValue(caseResultId));
    }


    [HttpPost("UpdateCalibrationValue")]
    public async Task<IActionResult> UpdateCalibrationValue(CaseCalibrationModel caseCalibrationModel)
    {
        return GenerateResponse(_caseService.UpdateCalibrationValue(caseCalibrationModel));
    }

    [HttpPost("UpdateCaseName")]
    public async Task<IActionResult> UpdateCaseName(UpdateCaseNameServiceModel updateCaseNameServiceModel)
    {
        return GenerateResponse(_caseService.UpdateCaseName(updateCaseNameServiceModel));
    }



    [HttpGet("extract-image")]
    public IActionResult ExtractImage()
    {
        try
        {
            // Define input and output paths
            string inputFilePath = Path.Combine(_environment.WebRootPath, "Document", "DicomFiles", "DICOM1.DCM");
            string outputFilePath = Path.Combine(_environment.WebRootPath, "Document", "DicomFiles", "DEMO 20696_1.png");

            if (!System.IO.File.Exists(inputFilePath))
            {
                return NotFound($"DICOM file not found at path: {inputFilePath}");
            }

            // Load and process the DICOM file
            var dicomFile = DicomFile.Open(inputFilePath);
            var image = new DicomImage(dicomFile.Dataset);

            // Convert to bitmap and save as PNG
            using (var bitmap = image.RenderImage().As<Bitmap>())
            {
                bitmap.Save(outputFilePath, ImageFormat.Png);
            }

            return Ok(new
            {
                Message = "Image extracted successfully",
                InputFile = "DEMO 20696_0.dcm",
                OutputFile = "DEMO 20696_0.png",
                OutputPath = outputFilePath
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }

    [HttpGet]
    [Route("GetKeyVault")]
    public async Task<IActionResult> GetKeyVault()
    {
        const string secretName = "ConnectionStrings--DefaultConnection";

        var kvUri = $"https://pb-3dtsldev.vault.azure.net/";

        var client = new SecretClient(new Uri(kvUri), new DefaultAzureCredential());

        var secret = await client.GetSecretAsync(secretName);
        return Ok(secret);
    }
}
