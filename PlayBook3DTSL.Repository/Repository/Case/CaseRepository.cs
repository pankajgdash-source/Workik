using AutoMapper;
using CSJ2K.j2k.quantization;
using Dapper;
using Dicom.Imaging;
using Dicom.Imaging.Codec;
using EnumsNET;
using FellowOakDicom;
using iTextSharp.text;
using iTextSharp.text.pdf;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using PlayBook3DTSL.Database.DataContext;
using PlayBook3DTSL.Database.Entities;
using PlayBook3DTSL.Model.Case;
using PlayBook3DTSL.Model.Cloud;
using PlayBook3DTSL.Model.Common;
using PlayBook3DTSL.Repository.Interfaces.Case;
using PlayBook3DTSL.Repository.Interfaces.CloudStorage;
using PlayBook3DTSL.Repository.PacsServer.PacsServerFactory;
using PlayBook3DTSL.Utilities.Constant;
using PlayBook3DTSL.Utilities.Enum;
using PlayBook3DTSL.Utilities.Helpers;
using Serilog;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO.Compression;
using System.Net;
using System.Text;

using static PlayBook3DTSL.Utilities.Constant.MessageHelper;
using static PlayBook3DTSL.Utilities.Helpers.ServiceResponse;







namespace PlayBook3DTSL.Repository.Repository.Case
{
    public class CaseRepository : ICaseRepository
    {
        private readonly PlayBook3DTSLDbContext _context;
        private readonly IMapper _mapper;
        private readonly ConcurrencyHelpers _concurrencyHelpers;
        private readonly AppConfiguration _appconfiguration;
        private readonly IHostEnvironment _environment;
        private readonly CloudStorageFactory _couldFactory;
        private readonly PacsServerFactoryService _pacsServerFactory;
        private readonly IConfiguration _configuration;
        private string _csvFolderPath;

        public CaseRepository(PlayBook3DTSLDbContext context, IMapper mapper, ConcurrencyHelpers concurrencyHelpers, IHostEnvironment environment, CloudStorageFactory couldFactory, IConfiguration configuration)
        {
            _context = context;
            _mapper = mapper;
            _environment = environment;
            _concurrencyHelpers = concurrencyHelpers;
            _configuration = configuration;

            _couldFactory = couldFactory;
        }

        public ServiceResponseGeneric<CaseResultListServiceModel> CreateCaseAsync(CaseCreateServiceModel caseCreateServiceModel)
        {
            //throw new NotImplementedException();
            return new ServiceResponseGeneric<CaseResultListServiceModel>(async () =>
            {
                try
                {
                    var userId = _concurrencyHelpers.GetLoggedInUserId();
                    //var cData = new Database.Entities.Case()
                    //{
                    //    CaseName = caseCreateServiceModel.CaseName,
                    //    Period = caseCreateServiceModel.Period,
                    //    HospitalId = caseCreateServiceModel.HospitalId,
                    //    CreatedOn = System.DateTime.UtcNow,
                    //    CreatedBy = userId,
                    //};
                    //_context.Cases.Add(cData);
                    //await _context.SaveChangesAsync();

                    var caseSqlParam = new DynamicParameters();
                    caseSqlParam.Add("@CaseName", caseCreateServiceModel.CaseName, DbType.String, ParameterDirection.Input);
                    caseSqlParam.Add("@Period", caseCreateServiceModel.Period, DbType.String, ParameterDirection.Input);
                    caseSqlParam.Add("@UserId", userId, DbType.Int32, ParameterDirection.Input);
                    var cData = _context.GetResultFromStoredProc<CaseDetailServiceModel>(
                         DbStoredProcedure.InsertCase,
                         caseSqlParam,
                         CommandType.StoredProcedure
                     ).FirstOrDefault();



                    var sqlParam = new DynamicParameters();
                    sqlParam.Add("@CaseId", cData.NewResultId, DbType.Guid, ParameterDirection.Input);
                    sqlParam.Add("@Period", caseCreateServiceModel.Period, DbType.String, ParameterDirection.Input);
                    sqlParam.Add("@UserId", userId, DbType.Int32, ParameterDirection.Input);

                    var resultInsertedData = _context.GetResultFromStoredProc<CaseDetailServiceModel>(
                         DbStoredProcedure.InsertCaseresult,
                         sqlParam,
                         CommandType.StoredProcedure
                     ).ToList();

                    //var resultServiceModel = new ResultServiceModel()
                    //{
                    //    CaseId = cData.Id,
                    //    Period = caseCreateServiceModel.Period,
                    //    CreatedOn = System.DateTime.UtcNow,
                    //    CreatedBy = -1,
                    //};

                    //var rData = _mapper.Map<CaseResult>(resultServiceModel);
                    //_concurrencyHelpers.SetDefaultValueInsert(rData);
                    //_context.CaseResults.Add(rData);
                    //_context.SaveChanges();



                    if (caseCreateServiceModel != null && caseCreateServiceModel.CaseStudyMappingServiceModel != null
           && caseCreateServiceModel.CaseStudyMappingServiceModel.StudyIds.Any())
                    {
                        caseCreateServiceModel.CaseStudyMappingServiceModel.CaseId = cData.NewResultId;
                        AddCaseStudy(caseCreateServiceModel.CaseStudyMappingServiceModel);
                    }


                    // return  cData.Id;
                    var rId = resultInsertedData.ToList().FirstOrDefault().NewResultId;
                    var sqlParamN = new DynamicParameters();
                    sqlParamN.Add("@CaseResultId", rId, DbType.Guid, ParameterDirection.Input);
                    sqlParamN.Add("@UserId", userId, DbType.Int64, ParameterDirection.Input);

                    var resultNameData = _context.GetResultFromStoredProc<CaseResultDetailServiceModel>(
                          DbStoredProcedure.GetCaseResultDetailById,
                          sqlParamN,
                          CommandType.StoredProcedure
                      ).FirstOrDefault();

                    // Configured the default case measurement steps
                    CreateDefaultCaseMeasurementSteps(cData.NewResultId, caseCreateServiceModel.Period);

                    if (resultNameData != null)
                    {
                        var resultData = new CaseResultListServiceModel
                        {
                            Id = resultNameData.Id,
                            CaseId = cData.NewResultId,
                            CaseName = caseCreateServiceModel.CaseName,
                            Period = caseCreateServiceModel.Period,
                            CreatedOn = DateTime.UtcNow,
                            CreatedBy = userId,
                            CaseResultName = resultNameData.CaseResultName,
                        };
                        return resultData;
                    }
                    else
                    {
                        return new CaseResultListServiceModel();
                    }
                }
                catch (Exception ex)
                {
                    return new CaseResultListServiceModel();

                }
            });
        }


        public ServiceResponseGeneric<List<CaseDetailServiceModel>> GetCasesByHospital()
        {
            return new ServiceResponseGeneric<List<CaseDetailServiceModel>>(() =>
            {
                var userId = _concurrencyHelpers.GetLoggedInUserId();

                var sqlParam = new DynamicParameters();
                sqlParam.Add("@UserId", userId, DbType.Int32, ParameterDirection.Input);

                var returnData = _context.GetResultFromStoredProc<CaseDetailServiceModel>(
                    DbStoredProcedure.GetCasesByHospital, sqlParam, CommandType.StoredProcedure);

                return returnData.Any() ? returnData : new List<CaseDetailServiceModel>();
            });
        }



        // TODO - Add feature to remove the case study mapping
        public ServiceResponseGeneric<Task<bool>> AddCaseStudy(CaseStudyServiceModel caseStudyMappingServiceModel)
        {
            return new ServiceResponseGeneric<Task<bool>>(async () =>
            {
                var userId = _concurrencyHelpers.GetLoggedInUserId();
                var caseStudyMapping = caseStudyMappingServiceModel.StudyIds.Select(d => new CaseStudyMapping
                {
                    CaseId = caseStudyMappingServiceModel.CaseId,
                    StudyId = d,
                    CreatedBy = userId,
                    CreatedOn = DateTime.UtcNow,
                    PatientId = caseStudyMappingServiceModel.PatientId,

                }).ToList();
                _context.CaseStudyMappings.AddRange(caseStudyMapping);
                await _context.SaveChangesAsync();
                return true;
            });
        }

        public ServiceResponseGeneric<Task<bool>> AddCaseImages(CaseImagesServiceModel caseImagesServiceModel)
        {

            return new ServiceResponseGeneric<Task<bool>>(async () =>
            {
                var userId = _concurrencyHelpers.GetLoggedInUserId();
                var caseImageDetails = new List<CaseImage>();
                var studyInstanceUID = Guid.NewGuid();

                var cloudServiceModel = new List<CloudServiceModel>();
                string destinationPath =
                    $"{ApplicationHelpers.GetDocuemnt}{(Utilities.Enum.EntityType.Case).AsString(EnumFormat.Name)}/{caseImagesServiceModel.CaseId}";
                string casePath =
                    $"{_environment.ContentRootPath}{ApplicationHelpers.GetDocuemnt}{(Utilities.Enum.EntityType.Case).AsString(EnumFormat.Name)}";
                if (!Directory.Exists(casePath))
                {
                    Directory.CreateDirectory(casePath);
                }

                var caseDetails =
                    _context.Cases.FirstOrDefault(d => d.Id.Equals(caseImagesServiceModel.CaseId));
                if (caseDetails == null)
                {
                    throw new ServiceResponseExceptionHandle(MessageHelper.NoDataFound, HttpStatusCode.NotFound);
                }

                foreach (var file in caseImagesServiceModel.FilePath)
                {
                    var sopinstanceUid = Guid.NewGuid();
                    var seriesInstanceUID = Guid.NewGuid();
                    var randomFileName = $"{FileHelper.GetRandomFileName()}_{file.FileName}";
                    var fileExtension = Path.GetExtension(file.FileName).TrimStart('.');

                    var isDocumentUpload =
                        FileExtension.DocumentFileExtensions.Any(d => d.Equals(fileExtension.ToLower()));
                    var isImageFileUpload =
                        FileExtension.ImageFileExtensions.Any(d => d.Equals(fileExtension.ToLower()));
                    var isDicomFileUpload =
                        FileExtension.DicomFileExtensions.Any(d => d.Equals(fileExtension.ToLower()));
                    var isTiffFileUpload = fileExtension == "tif" || fileExtension == "tiff";

                    // JPG, PNG File Upload
                    if (isImageFileUpload || isTiffFileUpload)
                    {
                        await ImageFileUpload(cloudServiceModel,
                            destinationPath,
                            casePath,
                            caseDetails, file,
                            sopinstanceUid,
                            caseImageDetails,
                            caseImagesServiceModel.CaseId,
                            caseImagesServiceModel.CasePeriod,
                            studyInstanceUID,
                            seriesInstanceUID);
                    }

                    var imageData = caseImagesServiceModel.ImageList.FirstOrDefault(d => d.ImageName.Equals(file.FileName));
                    if (imageData != null)
                    {
                        imageData.ImageName = $"{sopinstanceUid}_image.png";
                        imageData.DICOMImage = $"{sopinstanceUid}.DCM";
                    }

                    if (isDicomFileUpload)
                    {
                        var newStudyInstanceUID = Guid.NewGuid();
                        var newSeriesInstanceUID = Guid.NewGuid();
                        var filePath = Path.Combine(casePath, randomFileName);

                        try
                        {
                            using (var fileStream = new FileStream(filePath, FileMode.Create))
                            {
                                await file.CopyToAsync(fileStream);
                            }

                        }
                        catch (Exception ex)
                        {
                            throw new ServiceResponseExceptionHandle(ex.Message, HttpStatusCode.InternalServerError);
                        }

                        string capturedImageName = $"{sopinstanceUid}_image.png";
                        string dcmFileName = $"{sopinstanceUid}.DCM";
                        string outputPath = $"{_environment.ContentRootPath}{ApplicationHelpers.GetDocuemnt}{(Utilities.Enum.EntityType.Case).AsString(EnumFormat.Name)}/{capturedImageName}";


                        try
                        {
                            var dicomFile = DicomFile.Open(filePath);
                            var images = new FellowOakDicom.Imaging.DicomImage(dicomFile.Dataset);

                            using (var bitmap = images.RenderImage().As<Bitmap>())
                            {
                                bitmap.Save(outputPath, ImageFormat.Png);
                            }

                        }
                        catch (Exception ex)
                        {
                            throw new ServiceResponseExceptionHandle(ex.Message, HttpStatusCode.InternalServerError);
                        }

                        cloudServiceModel.Add(new CloudServiceModel
                        {
                            File = file,
                            FileName = capturedImageName,
                            CoudPath = destinationPath,
                            IsFromSourcePath = true,
                            SourcePath = outputPath
                        });

                        cloudServiceModel.Add(new CloudServiceModel
                        {
                            File = file,
                            FileName = dcmFileName,
                            CoudPath = destinationPath,
                            SourcePath = Path.Combine(casePath, dcmFileName)
                        });

                        caseImageDetails.Add(new CaseImage
                        {
                            FileName = dcmFileName,
                            SeriesName = Path.GetFileNameWithoutExtension(file.FileName),
                            CaseId = caseImagesServiceModel.CaseId,
                            StudyInstanceUid = newStudyInstanceUID.ToString(),
                            SeriesInstanceUid = newSeriesInstanceUID.ToString(),
                            SopinstanceUid = sopinstanceUid.ToString(),
                            StudyDate = DateTime.UtcNow,
                            StudyDescription = Path.GetFileNameWithoutExtension(file.FileName),
                            CreatedBy = userId,
                            CreatedOn = DateTime.UtcNow,
                            CasePeriod = caseImagesServiceModel.CasePeriod,
                        });

                    }
                   

                    //PDF File Upload
                    if (isDocumentUpload)
                    {
                        // Combine the folder path and the file name of uploded pdf
                        var filePath = Path.Combine(casePath, randomFileName);

                        // Save the pdf file to the folder
                        using (var fileStream = new FileStream(filePath, FileMode.Create))
                        {
                            await file.CopyToAsync(fileStream);
                        }

                        List<PDFToImageDetailServiceModel> pdfToImageDetailServiceModel = new();

                        using (var document = PdfiumViewer.PdfDocument.Load(filePath))
                        {
                            string outputPath =
                                $"{_environment.ContentRootPath}{ApplicationHelpers.GetDocuemnt}{(Utilities.Enum.EntityType.Case).AsString(EnumFormat.Name)}";
                            string localImagePath = string.Empty;

                            for (int i = 0; i < document.PageCount; i++)
                            {
                                var newSopinstanceUid = Guid.NewGuid();
                                var imageFileName = $"{newSopinstanceUid}_image.png";
                                var dcmFileName = $"{newSopinstanceUid}.DCM";
                                localImagePath = Path.Combine(outputPath, imageFileName);
                                var image = document.Render(i, 300, 300, true);
                                image.Save(localImagePath, ImageFormat.Jpeg);
                                pdfToImageDetailServiceModel.Add(new PDFToImageDetailServiceModel
                                {
                                    ImageFileName = imageFileName,
                                    ImagePath = localImagePath,
                                    SopinstanceUID = newSopinstanceUid,
                                    DCMFileName = dcmFileName,
                                    Order = i
                                });
                            }
                        }

                        foreach (var imageDetail in pdfToImageDetailServiceModel.OrderByDescending(d => d.Order))
                        {
                            // Combine the folder path and the file name
                            var dicomFilePath = ConvertImageToDicomFile(caseDetails,
                                imageDetail.DCMFileName, imageDetail.ImagePath);
                            cloudServiceModel.Add(new CloudServiceModel
                            {
                                CoudPath = destinationPath,
                                FileName = imageDetail.DCMFileName,
                                IsFromSourcePath = true,
                                SourcePath = dicomFilePath
                            });

                            cloudServiceModel.Add(new CloudServiceModel
                            {
                                CoudPath = destinationPath,
                                FileName = imageDetail.ImageFileName,
                                IsFromSourcePath = true,
                                SourcePath = Path.Combine(casePath, imageDetail.ImageFileName)
                            });

                            caseImageDetails.Add(new CaseImage
                            {
                                FileName = imageDetail.DCMFileName,
                                SeriesName = file.FileName,
                                CaseId = caseImagesServiceModel.CaseId,
                                StudyInstanceUid = studyInstanceUID.ToString(),
                                SeriesInstanceUid = seriesInstanceUID.ToString(),
                                SopinstanceUid = imageDetail.SopinstanceUID.ToString(),
                                StudyDate = DateTime.UtcNow,
                                StudyDescription = "Uploaded Image",
                                CreatedBy = userId,
                                CreatedOn = DateTime.UtcNow,
                                Id = Guid.NewGuid(),
                                CasePeriod = caseImagesServiceModel.CasePeriod,
                            });
                        }
                    }
                }

                // create update config data
                CreateConfigData(caseImagesServiceModel);

                caseImageDetails.ForEach(d => { d.StudyDate = DateTime.UtcNow; });
                _context.CaseImages.AddRange(caseImageDetails);
                _context.SaveChanges();
                await _couldFactory.GetCloudService(CloudStorageName.Azure).UploadeFileAsync(cloudServiceModel);


                var caseImageDetailList = _context.CaseImages
                    .Where(d =>
                        d.CaseId.Equals(caseImagesServiceModel.CaseId)
                        && d.StudyDescription.Equals("Uploaded Image"))
                    .ToList();

                caseImageDetailList.ForEach(d => d.StudyDate = DateTime.UtcNow);
                _context.CaseImages.UpdateRange(caseImageDetailList);
                _context.SaveChanges();

                // Remove existing configuration
                var existingConfig = _context.CaseConfigurations
                    .Where(d => d.CaseId.Equals(caseImagesServiceModel.CaseId)
                    && d.CaseResultId.Equals(caseImagesServiceModel.CaseResultId))
                    .ToList();

                if (existingConfig != null && existingConfig.Any())
                {

                    var existingSteps = _context.CaseMeasurementSteps
                                                .Include(d => d.SubSteps)
                                                .Where(d => d.CaseId.Equals(caseImagesServiceModel.CaseId))
                                                .ToList();

                    caseImagesServiceModel.ImageList.ForEach(d =>
                    {
                        var configData = existingConfig.FirstOrDefault(x => x.ImageViewType == d.ImageViewType);
                        if (configData != null)
                        {
                            configData.CaseResultImageDetail = null;
                            configData.CaseResultRotateState = null;
                            configData.CaseResultToolState = null;
                            configData.CalibrationValue = null;
                            _context.CaseConfigurations.Update(configData);
                        }

                        var steps = existingSteps.Where(x => x.ImageViewType == d.ImageViewType);
                        if (steps != null && steps.Any())
                        {
                            steps.Each(d =>
                            {
                                d.IsCompleted = false;
                                d.SubSteps.Each(d => d.IsCompleted = false);
                            });
                            _context.CaseMeasurementSteps.UpdateRange(steps);
                        }
                    });
                    await _context.SaveChangesAsync();
                }



                return true;
            });
        }

        private void CreateConfigData(CaseImagesServiceModel caseImagesServiceModel)
        {
            var userId = _concurrencyHelpers.GetLoggedInUserId();
            foreach (var item in caseImagesServiceModel.ImageList)
            {
                var existingConfig = _context.CaseConfigurations.FirstOrDefault(x => x.CaseId == caseImagesServiceModel.CaseId
                && x.CaseResultId == caseImagesServiceModel.CaseResultId && x.ImageViewType == item.ImageViewType);

                if (existingConfig == null)
                {
                    var configData = new CaseConfiguration
                    {
                        CaseId = caseImagesServiceModel.CaseId,
                        CaseResultId = caseImagesServiceModel.CaseResultId,
                        ImageName = item.ImageName,
                        DICOMImage = item.DICOMImage,
                        ImageViewType = item.ImageViewType,
                        CreatedBy = userId,
                        CreatedOn = DateTime.UtcNow,
                        LastUpdatedBy = userId,
                        LastUpdatedOn = DateTime.UtcNow,
                        Id = Guid.NewGuid()
                    };
                    _context.CaseConfigurations.Add(configData);

                }
                else
                {
                    existingConfig.ImageName = item.ImageName;
                    if (!string.IsNullOrEmpty(item.DICOMImage))
                    {
                        existingConfig.DICOMImage = item.DICOMImage;
                    }
                    existingConfig.LastUpdatedBy = -1;
                    existingConfig.LastUpdatedOn = DateTime.UtcNow;
                    _context.CaseConfigurations.Update(existingConfig);

                }
                _context.SaveChanges();
            }


        }
        private async Task ImageFileUpload(List<CloudServiceModel> cloudServiceModel,
            string destinationPath,
            string casePath,
            Database.Entities.Case? caseDetails,
            IFormFile file, Guid sopinstanceUid,
            List<CaseImage> caseImageDetails,
            Guid caseId,
            string casePeriod,
            Guid studyInstanceUID,
            Guid seriesInstanceUID)

        {
            var userId = _concurrencyHelpers.GetLoggedInUserId();
            string imageFileName = $"{sopinstanceUid}_image.png";
            string dicomFileName = $"{sopinstanceUid}.DCM";
            var randomFileName = $"{FileHelper.GetRandomFileName()}_{file.FileName}";
            var fileExtension = Path.GetExtension(file.FileName).TrimStart('.');

            var isTiffFileUpload = fileExtension == "tif" || fileExtension == "tiff";

            var filePath = Path.Combine(casePath, imageFileName);


            if (isTiffFileUpload)
            {

                string tiffPath = Path.Combine(casePath, randomFileName);
                string pngPath = Path.Combine(casePath, imageFileName);

                using (var stream = new FileStream(tiffPath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                using (var image = System.Drawing.Image.FromFile(tiffPath))
                {
                    image.Save(pngPath, ImageFormat.Png);
                }

            }
            else
            {
                // Save the file to the folder to snap from dicom file
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(fileStream);
                    fileStream.Close();
                }
            }
            // Combine the folder path and the file name
            var dicomFilePath = ConvertImageToDicomFile(caseDetails, dicomFileName, filePath);
            cloudServiceModel.Add(new CloudServiceModel
            {
                CoudPath = destinationPath,
                FileName = dicomFileName,
                IsFromSourcePath = true,
                SourcePath = dicomFilePath
            });

            cloudServiceModel.Add(new CloudServiceModel
            {
                //File = file,
                FileName = imageFileName,
                CoudPath = destinationPath,
                SourcePath = Path.Combine(casePath, imageFileName),
                IsFromSourcePath = true
            });


            caseImageDetails.Add(new CaseImage
            {
                FileName = dicomFileName,
                SeriesName = file.FileName,
                CaseId = caseId,
                StudyInstanceUid = studyInstanceUID.ToString(),
                SeriesInstanceUid = seriesInstanceUID.ToString(),
                SopinstanceUid = sopinstanceUid.ToString(),
                StudyDate = DateTime.UtcNow,
                StudyDescription = "Uploaded Image",
                CreatedBy = userId,
                CreatedOn = DateTime.UtcNow,
                CasePeriod = casePeriod,
                Id = Guid.NewGuid(),

            });
        }

        private string ConvertImageToDicomFile(Database.Entities.Case? caseDetails,
          string randomFileName,
          string sourcePath)
        {
            using (Bitmap originalBitmap = new(sourcePath))
            {
                // Ensure square dimensions for circular images
                int maxDimension = Math.Max(originalBitmap.Width, originalBitmap.Height);
                using (Bitmap squareBitmap = new Bitmap(maxDimension, maxDimension))
                {
                    using (Graphics g = Graphics.FromImage(squareBitmap))
                    {
                        // Set high quality rendering
                        g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                        g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                        g.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;

                        // Fill background with white to avoid transparency issues
                        // g.Clear(Color.White);

                        // Calculate centering position
                        int x = (maxDimension - originalBitmap.Width) / 2;
                        int y = (maxDimension - originalBitmap.Height) / 2;

                        // Draw the original image centered in the square bitmap
                        g.DrawImage(originalBitmap, x, y, originalBitmap.Width, originalBitmap.Height);
                    }

                    // Create a new DICOM dataset
                    var dataset = new Dicom.DicomDataset();

                    // Set required DICOM attributes
                    dataset.AddOrUpdate(Dicom.DicomTag.SOPClassUID, Dicom.DicomUID.SecondaryCaptureImageStorage);

                    // Patient information
                    dataset.AddOrUpdate(Dicom.DicomTag.PatientName, $"{caseDetails?.CaseName}");
                    dataset.AddOrUpdate(Dicom.DicomTag.PatientID, $"{caseDetails?.CaseId}");

                    // Study and series information
                    dataset.AddOrUpdate(Dicom.DicomTag.StudyInstanceUID, Dicom.DicomUID.Generate());
                    dataset.AddOrUpdate(Dicom.DicomTag.SeriesInstanceUID, Dicom.DicomUID.Generate());
                    dataset.AddOrUpdate(Dicom.DicomTag.SOPInstanceUID, Dicom.DicomUID.Generate());

                    // Image-related attributes for RGB color
                    dataset.AddOrUpdate(Dicom.DicomTag.Rows, (ushort)squareBitmap.Height);
                    dataset.AddOrUpdate(Dicom.DicomTag.Columns, (ushort)squareBitmap.Width);
                    dataset.AddOrUpdate(Dicom.DicomTag.BitsAllocated, (ushort)8);
                    dataset.AddOrUpdate(Dicom.DicomTag.BitsStored, (ushort)8);
                    dataset.AddOrUpdate(Dicom.DicomTag.HighBit, (ushort)7);
                    dataset.AddOrUpdate(Dicom.DicomTag.PixelRepresentation, (ushort)0);
                    dataset.AddOrUpdate(Dicom.DicomTag.PhotometricInterpretation, "RGB");
                    dataset.AddOrUpdate(Dicom.DicomTag.SamplesPerPixel, (ushort)3);
                    dataset.AddOrUpdate(Dicom.DicomTag.PlanarConfiguration, (ushort)0);

                    // Set equal pixel spacing to maintain circular shape
                    float spacing = 0.30f;
                    dataset.AddOrUpdate(Dicom.DicomTag.PixelSpacing, new float[] { spacing, spacing });

                    // Additional Tags
                    dataset.AddOrUpdate(Dicom.DicomTag.Modality, "XA");
                    dataset.AddOrUpdate(Dicom.DicomTag.ReferencedFileID, $"CAPYURE");
                    dataset.AddOrUpdate(Dicom.DicomTag.StudyTime, DateTime.Now);
                    dataset.AddOrUpdate(Dicom.DicomTag.ConversionType, "WSD");
                    dataset.AddOrUpdate(Dicom.DicomTag.PatientBirthDate, DateTime.Now);
                    dataset.AddOrUpdate(Dicom.DicomTag.InstanceNumber, "1");
                    dataset.AddOrUpdate(Dicom.DicomTag.LossyImageCompression, "01");
                    dataset.AddOrUpdate(Dicom.DicomTag.LossyImageCompressionRatio, "230.216");
                    dataset.AddOrUpdate(Dicom.DicomTag.LossyImageCompressionMethod, "ISO_10918_1");

                    // Convert the image to RGB bytes
                    byte[] pixels = ImageToRGBBytes(squareBitmap);

                    // Create a DICOM Pixel Data element
                    var pixelData = Dicom.Imaging.DicomPixelData.Create(dataset, true);
                    pixelData.AddFrame(new Dicom.IO.Buffer.MemoryByteBuffer(pixels));

                    // Save the DICOM dataset to a file
                    string dicomFilePathDir = $"{_environment.ContentRootPath}{ApplicationHelpers.GetDocuemnt}{"NewDicomFile"}";
                    if (!Directory.Exists(dicomFilePathDir))
                    {
                        Directory.CreateDirectory(dicomFilePathDir);
                    }

                    string dicomFilePath = $"{_environment.ContentRootPath}{ApplicationHelpers.GetDocuemnt}{"NewDicomFile"}/{randomFileName}";
                    var dicomFile = new Dicom.DicomFile(dataset);
                    var compressedFile = dicomFile.Clone(Dicom.DicomTransferSyntax.RLELossless);
                    compressedFile.Save(dicomFilePath);
                    return dicomFilePath;
                }
            }
        }

        static byte[] ImageToRGBBytes(Bitmap image)
        {
            int width = image.Width;
            int height = image.Height;
            byte[] rgbPixels = new byte[width * height * 3]; // 3 bytes per pixel for RGB

            // Lock the bitmap's bits
            System.Drawing.Rectangle rect = new System.Drawing.Rectangle(0, 0, width, height);
            System.Drawing.Imaging.BitmapData bmpData = image.LockBits(rect,
                System.Drawing.Imaging.ImageLockMode.ReadOnly,
                System.Drawing.Imaging.PixelFormat.Format24bppRgb);

            try
            {
                unsafe
                {
                    byte* ptr = (byte*)bmpData.Scan0;
                    int stride = bmpData.Stride;
                    int rgbIndex = 0;

                    // Process each pixel
                    for (int y = 0; y < height; y++)
                    {
                        for (int x = 0; x < width; x++)
                        {
                            int idx = y * stride + x * 3;
                            // BGR to RGB conversion (Windows stores in BGR format)
                            rgbPixels[rgbIndex++] = ptr[idx + 2]; // R
                            rgbPixels[rgbIndex++] = ptr[idx + 1]; // G
                            rgbPixels[rgbIndex++] = ptr[idx];     // B
                        }
                    }
                }
            }
            finally
            {
                image.UnlockBits(bmpData);
            }

            return rgbPixels;
        }

        public ServiceResponseGeneric<List<ResultServiceModel>> GetResultByCaseId(Guid caseId)
        {
            return new ServiceResponseGeneric<List<ResultServiceModel>>(() =>
            {
                var returnData = new List<ResultServiceModel>();


                var userId = _concurrencyHelpers.GetLoggedInUserId();

                var sqlParam = new DynamicParameters();
                sqlParam.Add("@CaseId", caseId, DbType.Guid, ParameterDirection.Input);
                sqlParam.Add("@UserId", userId, DbType.Int32, ParameterDirection.Input);

                returnData = _context.GetResultFromStoredProc<ResultServiceModel>(
                    DbStoredProcedure.GetResultByCaseId,
                    sqlParam,
                    CommandType.StoredProcedure
                );

                if (!returnData.Any())
                {
                    return new List<ResultServiceModel>();
                }

                return returnData;
            });
        }


        public ServiceResponseGeneric<List<CaseResultListServiceModel>> GetResultListByCaseId(Guid caseId)
        {
            return new ServiceResponseGeneric<List<CaseResultListServiceModel>>(() =>
            {
                var returnData = new List<CaseResultListServiceModel>();
                var userId = _concurrencyHelpers.GetLoggedInUserId();
                var sqlParam = new DynamicParameters();
                sqlParam.Add("@CaseId", caseId, DbType.Guid, ParameterDirection.Input);
                sqlParam.Add("@UserId", userId, DbType.Int32, ParameterDirection.Input);

                returnData = _context.GetResultFromStoredProc<CaseResultListServiceModel>(
                    DbStoredProcedure.GetResultByCaseId,
                    sqlParam,
                    CommandType.StoredProcedure
                );

                if (!returnData.Any())
                {
                    return new List<CaseResultListServiceModel>();
                }

                return returnData;
            });
        }


        public async Task<ServiceResponseGeneric<List<CaseImageDetailResponse>>> GetCaseImages(GetCaseImageDetailServiceModel getCaseImageDetailServiceModel)
        {

            return new ServiceResponseGeneric<List<CaseImageDetailResponse>>(async () =>
            {
                var userId = _concurrencyHelpers.GetLoggedInUserId();
                //var caseStudyMappingList = _context.CaseStudyMappings
                //                              .Where(d => d.CaseId.Equals(getCaseImageDetailServiceModel.CaseId))
                //                              .ToList();

                #region Get Instance list from PACS Server.
                //var existingImageDetails = _context.CaseImages
                //                              .Where(d => d.CaseId.Equals(getCaseImageDetailServiceModel.CaseId))
                //                              .ToList();

                //var patientService = _pacsServerFactory.GetService(PACSRequest.Image);

                //List<CFindPatientResponse> dicomServerImageList = new List<CFindPatientResponse>();
                //foreach (var caseStudy in caseStudyMappingList.Where(d => !string.IsNullOrWhiteSpace(d.PatientId)))
                //{
                //    var response = await patientService.CFind<CFindPatientResponse>(new CFindRequestServiceModel
                //    {
                //        PatientId = caseStudy.PatientId,
                //        StudyInstanceUID = caseStudy.StudyId,
                //        HospitalId = getCaseImageDetailServiceModel.HospitalId,
                //        IsExactMatch = true,
                //        PACSServiceType = (byte)PACSRequest.Image,
                //    }, d => { return _hospitalService.GetHospitalPACSDetail(d).Output; });

                //    dicomServerImageList.AddRange(response);
                //}
                //#endregion

                //#region Check Exising Images and Dicom Images SeriesInstanceUID

                //var existingInstaceIds = existingImageDetails.Select(d => d.SopinstanceUid).ToArray();
                //var dicomServerInstanceIds = dicomServerImageList.Select(d => d.SOPInstanceUID).ToArray();
                //var newInstanceUID = dicomServerInstanceIds.Except(existingInstaceIds).ToList();

                //#region If New Image found then store into database and blob storage

                //if (newInstanceUID.Any())
                //{
                //    var newUploadAndStoreImageDetails = dicomServerImageList.Where(d => newInstanceUID.Contains(d.SOPInstanceUID)).ToList();

                //    var newCaseImageDetails = newUploadAndStoreImageDetails.Select(d => new CaseImage
                //    {
                //        CaseId = getCaseImageDetailServiceModel.CaseId,
                //        SeriesInstanceUid = d.SeriesInstanceUID,
                //        SopinstanceUid = d.SOPInstanceUID,
                //        StudyDescription = d.StudyDescription,
                //        StudyDate = d.StudyDate,
                //        StudyInstanceUid = d.StudyInstanceUID,
                //        CreatedBy = ApplicationHelpers.LoggedInUserId,
                //        CreatedOn = DateTime.UtcNow,
                //        FileName = $"{d.SOPInstanceUID}.DCM",
                //        PixelSpacing = d.PixelSpacing.Contains('\\') ? d.PixelSpacing.Split(@"\")[0] : d.PixelSpacing,
                //    }).ToList();

                //    if (newUploadAndStoreImageDetails.Any(d => d?.StudyDate == null))
                //    {
                //        throw new ServiceResponseExceptionHandle(string.Format(MessageHelper.Invalid, "Study Date"), HttpStatusCode.BadRequest);
                //    }

                //    _context.CaseImages.AddRange(newCaseImageDetails);
                //    _context.SaveChanges();

                //    List<CGetPatientResponse> cGetPatientResponse = new();

                //    var tasks = newUploadAndStoreImageDetails.Select(async imageDetails =>
                //    {
                //        var response = await patientService.GetDetails<CGetPatientResponse>(new CGetRequestServiceModel
                //        {
                //            PatientId = imageDetails.PatientId,
                //            PatientName = imageDetails.PatientName,
                //            SeriesInstanceUID = imageDetails.SeriesInstanceUID,
                //            SOPInstanceUID = imageDetails.SOPInstanceUID,
                //            StudyInstanceUID = imageDetails.StudyInstanceUID,
                //            HospitalId = getCaseImageDetailServiceModel.HospitalId,
                //        }, d => { return _hospitalService.GetHospitalPACSDetail(d).Output; });

                //        cGetPatientResponse.AddRange(response);
                //    });
                //    await Task.WhenAll(tasks);

                //    //});
                //    string destinationPath = $"{ApplicationHelpers.GetDocuemnt}{(Utilities.Enum.EntityType.Case).AsString(EnumFormat.Name)}/{getCaseImageDetailServiceModel.CaseId}";

                //    var tasksForUpload = cGetPatientResponse.Select(async cGetPatientFile =>
                //    {
                //        List<CloudServiceModel> cloudServiceModel = new List<CloudServiceModel>();
                //        List<CloudServiceModel> cloudServiceDCMModel = new List<CloudServiceModel>();
                //        //// Read the file into a byte array
                //        byte[] fileBytes = System.IO.File.ReadAllBytes(cGetPatientFile.SaveImagePath);


                //        //DCM file jpg screenshot
                //        var image = new DicomImage(cGetPatientFile.SaveImagePath);
                //        var sharpimage = image.RenderImage().AsSharpImage();
                //        using var stream = new MemoryStream();
                //        sharpimage.Save(stream, new JpegEncoder());
                //        string casePath = $"{_environment.WebRootPath}{ApplicationHelpers.GetDocuemnt}{(Utilities.Enum.EntityType.Case).AsString(EnumFormat.Name)}";
                //        Log.Logger.Write(LogEventLevel.Information, "casePath :" + casePath);
                //        if (!Directory.Exists(casePath))
                //        {
                //            Directory.CreateDirectory(casePath);
                //        }
                //        string outputPath = $"{_environment.WebRootPath}{ApplicationHelpers.GetDocuemnt}{(Utilities.Enum.EntityType.Case).AsString(EnumFormat.Name)}/{cGetPatientFile.SOPInstanceUID}_image.jpg";
                //        await System.IO.File.WriteAllBytesAsync(outputPath, stream.ToArray());
                //        Log.Logger.Write(LogEventLevel.Information, "outputPath :" + outputPath);

                //        cloudServiceDCMModel.Add(new CloudServiceModel
                //        {
                //            CoudPath = destinationPath,
                //            FileName = $"{cGetPatientFile.SOPInstanceUID}.DCM",
                //            IsFromSourcePath = true,
                //            SourcePath = cGetPatientFile.SaveImagePath,
                //        });

                //        cloudServiceModel.Add(new CloudServiceModel
                //        {
                //            CoudPath = destinationPath,
                //            FileName = $"{cGetPatientFile.SOPInstanceUID}_image.jpg",
                //            IsFromSourcePath = true,
                //            SourcePath = outputPath
                //        });
                //        await _couldFactory.GetCloudService(CloudStorageName.Azure).UploadeFileAsync(cloudServiceModel);
                //        _ = _couldFactory.GetCloudService(CloudStorageName.Azure).UploadeFileAsync(cloudServiceDCMModel);
                //    });
                //    await Task.WhenAll(tasksForUpload);
                //}
                //#endregion

                #endregion

                #region Return Image Details
                var caseImageResponse = _context.CaseImages
                  .Where(d => d.CaseId.Equals(getCaseImageDetailServiceModel.CaseId) && d.CreatedBy == userId && d.CasePeriod == getCaseImageDetailServiceModel.CasePeriod)
                  .ToList();

                var grpDetails = caseImageResponse.GroupBy(d => new { d.StudyDate.Value.Date, d.StudyDescription })
                  .Select(d => new CaseImageDetailResponse
                  {
                      StudyDate = d.Key.Date,
                      StudyDescription = d.Key.StudyDescription,
                      NumberOfInstance = d.GroupBy(x => x.SeriesInstanceUid).Count(),
                      CaseStudyList = d.GroupBy(x => x.SeriesInstanceUid)
                          .Select(x => new CaseImageList
                          {
                              CaseImageSeries = x.OrderByDescending(d => d.CreatedOn).ToList()
                          })
                          .OrderByDescending(x => x.CaseImageSeries.FirstOrDefault()?.CreatedOn)
                          .ToList(),
                  })
                  .OrderByDescending(d => d.StudyDate)
                  .ToList();
                #endregion

                return grpDetails;
            });
        }

        #region Case Measurement Steps
        public ServiceResponseGeneric<List<CaseMeasurementStepsSericeModel>> GetCaseMeasurementStepsByCaseId(Guid caseId, string casePeriod)
        {
            return new ServiceResponseGeneric<List<CaseMeasurementStepsSericeModel>>(() =>
            {
                var userId = _concurrencyHelpers.GetLoggedInUserId();
                var steps = _context.CaseMeasurementSteps
                    .Where(d => d.CaseId == caseId && d.CreatedBy == userId && d.CasePeriod == casePeriod)
                    .ToList();

                var stepIds = steps.Select(s => s.Id).ToList();

                var subSteps = _context.CaseMeasurementSubSteps
                    .Where(ss => ss.CaseMeasurementStepsId != null && stepIds.Contains(ss.CaseMeasurementStepsId.Value))
                    .ToList();

                var mappedSteps = _mapper.Map<List<CaseMeasurementStepsSericeModel>>(steps);
                var mappedSubSteps = _mapper.Map<List<CaseMeasurementSubStepServiceModel>>(subSteps);

                foreach (var step in mappedSteps)
                {
                    step.SubSteps = mappedSubSteps
                        .Where(ss => ss.CaseMeasurementStepsId == step.Id)
                        .OrderBy(x => x.SortOrder).ToList();
                }

                return mappedSteps;
            });
        }

        public ServiceResponseGeneric<Task<bool>> UpdateCaseMeasurementStepsId(Guid caseMeasurementStepsId, bool isCompleted)
        {
            return new ServiceResponseGeneric<Task<bool>>(async () =>
            {
                var userId = _concurrencyHelpers.GetLoggedInUserId();
                var caseMeasurementSteps = _context.CaseMeasurementSteps.FirstOrDefault(d => d.Id.Equals(caseMeasurementStepsId) && d.CreatedBy == userId);

                if (caseMeasurementSteps == null)
                {
                    throw new ServiceResponseExceptionHandle(MessageHelper.NoDataFound, HttpStatusCode.NotFound);
                }
                caseMeasurementSteps.IsCompleted = isCompleted;
                _context.Update(caseMeasurementSteps);
                await _context.SaveChangesAsync();
                return true;
            });
        }

        public ServiceResponseGeneric<Task<bool>> UpdateCaseMeasurementSubStepsId(Guid caseMeasurementSubStepsId, bool isCompleted)
        {
            return new ServiceResponseGeneric<Task<bool>>(async () =>
            {
                var caseMeasurementSubSteps = _context.CaseMeasurementSubSteps.FirstOrDefault(d => d.Id.Equals(caseMeasurementSubStepsId));

                if (caseMeasurementSubSteps == null)
                {
                    throw new ServiceResponseExceptionHandle(MessageHelper.NoDataFound, HttpStatusCode.NotFound);
                }
                caseMeasurementSubSteps.IsCompleted = isCompleted;
                _context.Update(caseMeasurementSubSteps);
                await _context.SaveChangesAsync();
                return true;
            });
        }

        private ServiceResponseGeneric<bool> CreateDefaultCaseMeasurementSteps(Guid caseId, string casePeriod)
        {
            return new ServiceResponseGeneric<bool>(() =>
            {
                var userId = _concurrencyHelpers.GetLoggedInUserId();
                var caseMeasurementSteps = new List<CaseMeasurementStep>();
                var sqlParam = new DynamicParameters();
                sqlParam.Add("@CaseId", null, DbType.Guid, ParameterDirection.Input);
                sqlParam.Add("@UserId", userId, DbType.Int32, ParameterDirection.Input);


                caseMeasurementSteps = _context.GetResultFromStoredProc<CaseMeasurementStep>(
                    DbStoredProcedure.GetCasesMeasurementByCaseId,
                    sqlParam,
                    CommandType.StoredProcedure
                );

                // var caseMeasurementSteps = _context.CaseMeasurementSteps.Where(d => d.CaseId == null).ToList();

                if (caseMeasurementSteps == null)
                {
                    throw new ServiceResponseExceptionHandle(MessageHelper.NoDataFound, HttpStatusCode.NotFound);
                }
                var newCaseMeasurementSteps = caseMeasurementSteps.Select(d => new CaseMeasurementStep
                {
                    CaseId = caseId,
                    StepName = d.StepName,
                    Description = d.Description,
                    IsCompleted = d.IsCompleted,
                    CreatedBy = userId,
                    CreatedOn = d.CreatedOn,
                    LastUpdatedBy = userId,
                    LastUpdatedOn = d.LastUpdatedOn,
                    ImageViewType = d.ImageViewType,
                    IsLEVDropDown = d.IsLEVDropDown,
                    IsLILDropDown = d.IsLILDropDown,
                    SortOrder = d.SortOrder,
                    CasePeriod = casePeriod,

                }).ToList();
                _context.CaseMeasurementSteps.AddRange(newCaseMeasurementSteps);
                _context.SaveChanges();

                //Create default SubSteps
                var apId = _context.CaseMeasurementSteps.FirstOrDefault(x => x.StepName == MessageHelper.APSubStepSelection
                && x.CaseId == caseId && x.ImageViewType == ImageViewType.AP.ToString() && x.CasePeriod == casePeriod)?.Id;

                var latId = _context.CaseMeasurementSteps.FirstOrDefault(x => x.StepName == MessageHelper.LATSubStepSelection
                && x.CaseId == caseId && x.ImageViewType == ImageViewType.LATERAL.ToString() && x.CasePeriod == casePeriod)?.Id;

                CreateDefaultCaseMeasurementSubSteps(latId, apId);
                return true;
            });
        }

        private bool CreateDefaultCaseMeasurementSubSteps(Guid? latId, Guid? apId)
        {


            var defaultSubSteps = _context.CaseMeasurementSubSteps.Where(x => x.CaseMeasurementStepsId == null).ToList();

            if (defaultSubSteps == null)
            {
                throw new ServiceResponseExceptionHandle(MessageHelper.NoDataFound, HttpStatusCode.NotFound);
            }
            var newCaseMeasurementSubStepsLat = defaultSubSteps.Select(d => new CaseMeasurementSubSteps
            {
                CaseMeasurementStepsId = latId,
                StepName = d.StepName,
                Description = d.Description,
                IsCompleted = d.IsCompleted,
                SortOrder = d.SortOrder
            }).ToList();
            _context.CaseMeasurementSubSteps.AddRange(newCaseMeasurementSubStepsLat);
            var newCaseMeasurementSubStepsAP = defaultSubSteps.Select(d => new CaseMeasurementSubSteps
            {
                CaseMeasurementStepsId = apId,
                StepName = d.StepName,
                Description = d.Description,
                IsCompleted = d.IsCompleted,
                SortOrder = d.SortOrder
            }).ToList();
            _context.CaseMeasurementSubSteps.AddRange(newCaseMeasurementSubStepsAP);
            _context.SaveChanges();

            return true;

        }

        public ServiceResponseGeneric<List<CaseStudyMappingServiceModel>> GetCaseStudy(Guid caseId)
        {
            return new ServiceResponseGeneric<List<CaseStudyMappingServiceModel>>(() =>
            {
                var userId = _concurrencyHelpers.GetLoggedInUserId();

                var caseStudyMappings = _context.CaseStudyMappings
                    .Where(d => d.CaseId.Equals(caseId) && d.CreatedBy == userId)
                    .ToList();

                return _mapper.Map<List<CaseStudyMappingServiceModel>>(caseStudyMappings);
            });
        }

        #endregion Case Measurement Steps


        public ServiceResponseGeneric<Task<bool>> UpdateLabellingConfigData(CaseUpdateConfigServiceModel caseUpdateConfigServiceModel)
        {
            return new ServiceResponseGeneric<Task<bool>>(async () =>
            {
                var userId = _concurrencyHelpers.GetLoggedInUserId();
                var caseResultData = _context.CaseResults.FirstOrDefault(x =>
                    x.Id == caseUpdateConfigServiceModel.CaseResultId);

                if (caseResultData == null)
                {
                    throw new ServiceResponseExceptionHandle(MessageHelper.NoDataFound, HttpStatusCode.NotFound);
                }
                caseUpdateConfigServiceModel.casePeriod = caseUpdateConfigServiceModel.casePeriod.ToLower();
                var apStepId = _context.CaseMeasurementSteps
                    .FirstOrDefault(x => x.CaseId == caseUpdateConfigServiceModel.CaseId && x.StepName == MessageHelper.APSubStepSelection
                    && x.CasePeriod.ToLower().Equals(caseUpdateConfigServiceModel.casePeriod))?.Id;

                var latStepId = _context.CaseMeasurementSteps
                    .FirstOrDefault(x => x.CaseId == caseUpdateConfigServiceModel.CaseId && x.StepName == MessageHelper.LATSubStepSelection
                    && x.CasePeriod.ToLower().Equals(caseUpdateConfigServiceModel.casePeriod))?.Id;

                await RemoveSubsteps(caseUpdateConfigServiceModel, apStepId, latStepId);
                await AddSubsteps(caseUpdateConfigServiceModel, apStepId, latStepId);

                if (apStepId.HasValue && latStepId.HasValue)
                {
                    var sqlParam = new DynamicParameters();
                    sqlParam.Add("@APStepId", apStepId.Value, DbType.Guid, ParameterDirection.Input);
                    sqlParam.Add("@LATStepId", latStepId.Value, DbType.Guid, ParameterDirection.Input);

                    _context.GetResultFromStoredProc<bool>(
                        DbStoredProcedure.UpdateSubStepSortOrder,
                        sqlParam,
                        CommandType.StoredProcedure
                    );
                }
                caseResultData.LabelingConfig = caseUpdateConfigServiceModel.ConfigData;
                await _context.SaveChangesAsync();

                return true;
            });
        }


        private async Task RemoveSubsteps(CaseUpdateConfigServiceModel model, Guid? apStepId, Guid? latStepId)
        {
            if (model.RemovedSubStepsLabel == null || !model.RemovedSubStepsLabel.Any()) return;

            foreach (var stepId in new[] { apStepId, latStepId })
            {
                if (stepId == null) continue;

                var substepsToRemove = await _context.CaseMeasurementSubSteps
                    .Where(s => s.CaseMeasurementStepsId == stepId &&
                                model.RemovedSubStepsLabel.Contains(s.Description))
                    .ToListAsync();

                _context.CaseMeasurementSubSteps.RemoveRange(substepsToRemove);
            }

            await _context.SaveChangesAsync();
        }

        private async Task AddSubsteps(CaseUpdateConfigServiceModel model, Guid? apStepId, Guid? latStepId)
        {
            if (model.AddedSubStepsLabel == null || !model.AddedSubStepsLabel.Any()) return;

            foreach (var stepId in new[] { apStepId, latStepId })
            {
                if (stepId == null) continue;

                var existingSubsteps = await _context.CaseMeasurementSubSteps
                    .Where(s => s.CaseMeasurementStepsId == stepId)
                    .ToListAsync();

                foreach (var label in model.AddedSubStepsLabel)
                {
                    var alreadyExists = existingSubsteps.Any(s =>
                        s.Description.Equals(label, StringComparison.OrdinalIgnoreCase));

                    if (!alreadyExists)
                    {
                        var substepName = $"Click at the middle of the top of the {label} endplate";

                        var newSubstep = new CaseMeasurementSubSteps
                        {
                            Id = Guid.NewGuid(),
                            CaseMeasurementStepsId = stepId.Value,
                            StepName = substepName,
                            Description = label,
                        };

                        _context.CaseMeasurementSubSteps.Add(newSubstep);
                    }
                }
            }

            await _context.SaveChangesAsync();
        }





        public ServiceResponseGeneric<Task<string>> GetLabellingConfigData(Guid caseResultId)
        {
            return new ServiceResponseGeneric<Task<string>>(async () =>
            {
                var userId = _concurrencyHelpers.GetLoggedInUserId();

                var caseResultData = await _context.CaseResults.FirstOrDefaultAsync(x =>
                    x.Id == caseResultId && x.CreatedBy == userId);

                if (caseResultData == null)
                {
                    throw new ServiceResponseExceptionHandle(MessageHelper.NoDataFound, HttpStatusCode.NotFound);
                }

                return caseResultData.LabelingConfig;
            });
        }


        public ServiceResponseGeneric<Task<bool>> UpdateConfigData(CaseUpdateConfigServiceModel caseUpdateConfigServiceModel)
        {
            return new ServiceResponseGeneric<Task<bool>>(async () =>
            {

                string imageViewTypeString = caseUpdateConfigServiceModel.ImageViewType.ToString();
                var caseData = _context.CaseConfigurations.FirstOrDefault(x =>
                    x.CaseId == caseUpdateConfigServiceModel.CaseId &&
                    x.CaseResultId == caseUpdateConfigServiceModel.CaseResultId &&
                    x.ImageViewType == imageViewTypeString);


                if (caseData == null)
                {
                    // Create a new Case Configuration
                    caseData = new CaseConfiguration
                    {
                        Id = Guid.NewGuid(),
                        CaseId = caseUpdateConfigServiceModel.CaseId,
                        CaseResultId = caseUpdateConfigServiceModel.CaseResultId,
                        ImageViewType = caseUpdateConfigServiceModel.ImageViewType.ToString()
                    };

                    _concurrencyHelpers.SetDefaultValueInsert(caseData);
                    _context.CaseConfigurations.Add(caseData);
                    await _context.SaveChangesAsync();
                }
                else
                {
                    _concurrencyHelpers.SetDefaultValueUpdate(caseData);
                }
                caseData.CaseResultImageDetail = caseUpdateConfigServiceModel.ConfigData;
                caseData.CaseResultRotateState = caseUpdateConfigServiceModel.RotateState;
                caseData.CaseResultToolState = caseUpdateConfigServiceModel.ToolState;



                await _context.SaveChangesAsync();
                return true;
            });
        }



        public ServiceResponseGeneric<List<ConfigureViewModel>> GetConfigurationByCaseResultId(Guid caseResultId)
        {
            return new ServiceResponseGeneric<List<ConfigureViewModel>>(() =>
            {
                var userId = _concurrencyHelpers.GetLoggedInUserId();

                var sqlParam = new DynamicParameters();
                sqlParam.Add("@CaseResultId", caseResultId, DbType.Guid, ParameterDirection.Input);
                sqlParam.Add("@UserId", userId, DbType.Int32, ParameterDirection.Input);

                var returnData = _context.GetResultFromStoredProc<ConfigureViewModel>(
                    DbStoredProcedure.GetConfigurationByCaseResultId,
                    sqlParam,
                    CommandType.StoredProcedure
                );

                return returnData.ToList();
            });
        }



        public ServiceResponseGeneric<CaseResultDetailServiceModel> GetCaseResultDetailById(Guid caseResultId)
        {
            return new ServiceResponseGeneric<CaseResultDetailServiceModel>(() =>
            {
                var userId = _concurrencyHelpers.GetLoggedInUserId();

                var sqlParam = new DynamicParameters();
                sqlParam.Add("@CaseResultId", caseResultId, DbType.Guid, ParameterDirection.Input);
                sqlParam.Add("@UserId", userId, DbType.Int32, ParameterDirection.Input);

                var returnData = _context.GetResultFromStoredProc<CaseResultDetailServiceModel>(
                    DbStoredProcedure.GetCaseResultDetailById,
                    sqlParam,
                    CommandType.StoredProcedure
                );

                return returnData.Any() ? returnData.First() : new CaseResultDetailServiceModel();
            });
        }


        public ServiceResponseGeneric<Guid> Add3DTSLCalculation(ResultServiceModel resultServiceModel)
        {
            return new ServiceResponseGeneric<Guid>(async () =>
            {
                var resultCaseId = resultServiceModel.Id;
                var prevresultLabelingData = _context.CaseResults.FirstOrDefault(x => x.Id == resultCaseId).LabelingConfig;
                var resultNameData = _context.CaseResults.FirstOrDefault(x => x.Id == resultCaseId).CaseResultName;
                var prevresultCalibrationData = _context.CaseConfigurations.FirstOrDefault(x => x.CaseResultId == resultCaseId).CalibrationValue;
                var prevResult = _context.CaseResults.FirstOrDefault(c =>
                c.Id == resultServiceModel.Id &&
               (c.SspineLength == null && c.CspineLength == null));
                if(prevResult != null)
                {
                    resultCaseId = prevResult.Id;
                }

                var trackedEntity = _context.ChangeTracker.Entries<CaseResult>()
                            .FirstOrDefault(e => e.Entity.Id == resultCaseId);

                if (trackedEntity != null)
                {
                    trackedEntity.State = EntityState.Detached;
                }
                

                if (_context.CaseResults.Any(c =>
                c.Id == resultServiceModel.Id &&
               (c.SspineLength == null && c.CspineLength == null)))
                {
                    var cData = _mapper.Map<CaseResult>(resultServiceModel);
                    _concurrencyHelpers.SetDefaultValueInsert(cData);
                    cData.LabelingConfig = prevresultLabelingData;
                    cData.CaseResultName = resultNameData;
                    _context.CaseResults.Update(cData);
                    _context.SaveChanges();

                    //Add previous config data to new result
                    var sqlParam = new DynamicParameters();
                    sqlParam.Add("@CaseId", resultServiceModel.CaseId, DbType.Guid, ParameterDirection.Input);
                    sqlParam.Add("@Period", cData.Period, DbType.String, ParameterDirection.Input);
                    sqlParam.Add("@UserId", cData.CreatedBy, DbType.Int32, ParameterDirection.Input);

                    var resultInsertedData = _context.GetResultFromStoredProc<CaseDetailServiceModel>(
                         DbStoredProcedure.InsertCaseresult,
                         sqlParam,
                         CommandType.StoredProcedure
                     ).ToList();
                    var rId = resultInsertedData.ToList().FirstOrDefault().NewResultId;

                    var caseConfigData = _context.CaseConfigurations.Where(x =>
                   x.CaseId == resultServiceModel.CaseId &&
                   x.CaseResultId == resultCaseId);

                    foreach (var item in caseConfigData)
                    {
                        var configdata = new CaseConfiguration
                        {
                            CaseId = item.CaseId,
                            CaseResultId = rId,
                            ImageViewType = item.ImageViewType,
                            ImageName = item.ImageName,
                            DICOMImage = item.DICOMImage,
                            CaseResultImageDetail = item.CaseResultImageDetail,
                            CaseResultRotateState = item.CaseResultRotateState,
                            CaseResultToolState = item.CaseResultToolState,
                            CalibrationValue = prevresultCalibrationData,

                            Id = Guid.NewGuid()
                        };
                        _concurrencyHelpers.SetDefaultValueInsert(configdata);
                        _context.CaseConfigurations.Add(configdata);
                    }
                    _context.SaveChanges();

                    return rId;
                }
                else
                {
                    resultServiceModel.Id = Guid.NewGuid();
                    var cData = _mapper.Map<CaseResult>(resultServiceModel);
                    _concurrencyHelpers.SetDefaultValueInsert(cData);
                    cData.LabelingConfig = prevresultLabelingData;
                    _context.CaseResults.Add(cData);
                    _context.SaveChanges();


                    //Add previous config data to new result
                    var sqlParam = new DynamicParameters();
                    sqlParam.Add("@CaseId", resultServiceModel.CaseId, DbType.Guid, ParameterDirection.Input);
                    sqlParam.Add("@Period", cData.Period, DbType.String, ParameterDirection.Input);
                    sqlParam.Add("@UserId", cData.CreatedBy, DbType.Int32, ParameterDirection.Input);

                    var resultInsertedData = _context.GetResultFromStoredProc<CaseDetailServiceModel>(
                         DbStoredProcedure.InsertCaseresult,
                         sqlParam,
                         CommandType.StoredProcedure
                     ).ToList();
                    var rId = resultInsertedData.ToList().FirstOrDefault().NewResultId;

                    var caseConfigData = _context.CaseConfigurations.Where(x =>
                   x.CaseId == resultServiceModel.CaseId &&
                   x.CaseResultId == resultCaseId);

                    foreach (var item in caseConfigData)
                    {
                        var configdata = new CaseConfiguration
                        {
                            CaseId = item.CaseId,
                            CaseResultId = rId,
                            ImageViewType = item.ImageViewType,
                            ImageName = item.ImageName,
                            DICOMImage = item.DICOMImage,
                            CaseResultImageDetail = item.CaseResultImageDetail,
                            CaseResultRotateState = item.CaseResultRotateState,
                            CaseResultToolState = item.CaseResultToolState,
                            CalibrationValue = prevresultCalibrationData,

                            Id = Guid.NewGuid()
                        };
                        _concurrencyHelpers.SetDefaultValueInsert(configdata);
                        _context.CaseConfigurations.Add(configdata);
                    }
                    _context.SaveChanges();

                    return rId;
                }
            });
        }
        public ServiceResponseGeneric<Task<bool>> Check3DTSLDataExists(Guid resultId)
        {
            return new ServiceResponseGeneric<Task<bool>>(async () =>
            {
                var userId = _concurrencyHelpers.GetLoggedInUserId();
                bool exists = _context.CaseResults.Any(c =>
                    c.Id == resultId &&
                    c.SspineLength != null &&
                    c.CspineLength != null && c.CreatedBy == userId);

                return exists;
            });
        }


        public ServiceResponseGeneric<CaseDetailServiceModel> GetCaseDetailById(Guid caseId)
        {
            return new ServiceResponseGeneric<CaseDetailServiceModel>(() =>
            {
                var userId = _concurrencyHelpers.GetLoggedInUserId();
                var caseDetail = _context.Cases.FirstOrDefault(d => d.Id.Equals(caseId) && d.CreatedBy == userId);
                if (caseDetail == null)
                {
                    throw new ServiceResponseExceptionHandle(MessageHelper.NoDataFound, HttpStatusCode.NotFound);
                }
                return new CaseDetailServiceModel
                {
                    Id = caseDetail.Id,
                    CaseId = caseDetail.CaseId,
                    CaseName = caseDetail.CaseName,
                };
            });
        }

        public ServiceResponseGeneric<Task<bool>> Add3DTSLImages(Add3DTSLImageServiceModel add3DTSLImageServiceModel)
        {

            return new ServiceResponseGeneric<Task<bool>>(async () =>
            {
                var userId = _concurrencyHelpers.GetLoggedInUserId();
                var caseImageDetails = new List<CaseImage>();
                var studyInstanceUID = Guid.NewGuid();

                var cloudServiceModel = new List<CloudServiceModel>();
                string destinationPath =
                    $"{ApplicationHelpers.GetDocuemnt}{(Utilities.Enum.EntityType.Case).AsString(EnumFormat.Name)}/{add3DTSLImageServiceModel.CaseId}";
                string casePath =
                    $"{_environment.ContentRootPath}{ApplicationHelpers.GetDocuemnt}{(Utilities.Enum.EntityType.Case).AsString(EnumFormat.Name)}";
                if (!Directory.Exists(casePath))
                {
                    Directory.CreateDirectory(casePath);
                }

                var caseDetails =
                    _context.Cases.FirstOrDefault(d => d.Id.Equals(add3DTSLImageServiceModel.CaseId) && d.CreatedBy == userId);
                if (caseDetails == null)
                {
                    throw new ServiceResponseExceptionHandle(MessageHelper.NoDataFound, HttpStatusCode.NotFound);
                }

                foreach (var file in add3DTSLImageServiceModel.FilePath)
                {
                    var sopinstanceUid = Guid.NewGuid();
                    var seriesInstanceUID = Guid.NewGuid();
                    var randomFileName = $"{FileHelper.GetRandomFileName()}_{file.FileName}";
                    var fileExtension = Path.GetExtension(file.FileName).TrimStart('.');

                    var isDocumentUpload =
                        FileExtension.DocumentFileExtensions.Any(d => d.Equals(fileExtension.ToLower()));
                    var isImageFileUpload =
                        FileExtension.ImageFileExtensions.Any(d => d.Equals(fileExtension.ToLower()));
                    var isDicomFileUpload =
                        FileExtension.DicomFileExtensions.Any(d => d.Equals(fileExtension.ToLower()));

                    if (isImageFileUpload)
                    {
                        await ImageFileUpload(cloudServiceModel,
                            destinationPath,
                            casePath,
                            caseDetails, file,
                            sopinstanceUid,
                            caseImageDetails,
                            add3DTSLImageServiceModel.CaseId,
                            add3DTSLImageServiceModel.CasePeriod,
                            studyInstanceUID,
                            seriesInstanceUID);

                        var imageData = add3DTSLImageServiceModel.ImageList.FirstOrDefault(d => d.ImageName.Equals(file.FileName));
                        if (imageData != null)
                        {
                            imageData.ImageName = $"{sopinstanceUid}_image.png";
                            imageData.DICOMImage = string.Empty;
                        }
                    }
                }

                CreateConfigData(add3DTSLImageServiceModel);

                await _couldFactory.GetCloudService(CloudStorageName.Azure).UploadeFileAsync(cloudServiceModel);
                return true;

            });
        }

        private void CreateConfigData(Add3DTSLImageServiceModel add3DTSLImageServiceModel)
        {
            var userId = _concurrencyHelpers.GetLoggedInUserId();
            foreach (var item in add3DTSLImageServiceModel.ImageList)
            {
                var existingConfig = _context.CaseConfigurations.FirstOrDefault(x => x.CaseId == add3DTSLImageServiceModel.CaseId
                && x.CaseResultId == add3DTSLImageServiceModel.CaseResultId && x.ImageViewType == item.ImageViewType);

                if (existingConfig == null)
                {
                    var configData = new CaseConfiguration
                    {
                        CaseId = add3DTSLImageServiceModel.CaseId,
                        CaseResultId = add3DTSLImageServiceModel.CaseResultId,
                        ImageName = item.ImageName,
                        DICOMImage = item.DICOMImage,
                        ImageViewType = item.ImageViewType,
                        CreatedBy = userId,
                        CreatedOn = DateTime.UtcNow
                    };
                    _context.CaseConfigurations.Add(configData);
                }
                else
                {
                    existingConfig.ImageName = item.ImageName;
                    if (!string.IsNullOrEmpty(item.DICOMImage))
                    {
                        existingConfig.DICOMImage = item.DICOMImage;
                    }
                    existingConfig.LastUpdatedBy = userId;
                    existingConfig.LastUpdatedOn = DateTime.UtcNow;
                    _context.CaseConfigurations.Update(existingConfig);
                }

            }
            _context.SaveChanges();

        }

        public ServiceResponseGeneric<CaseResultListServiceModel> CreateResultByCaseIdAsync(Guid caseId, string period)
        {
            return new ServiceResponseGeneric<CaseResultListServiceModel>(async () =>
            {
                var userId = _concurrencyHelpers.GetLoggedInUserId();
                // Step 1: Create result service model
                var resultServiceModel = new ResultServiceModel
                {
                    Id = Guid.NewGuid(),
                    CaseId = caseId,
                    Period = period,
                    CreatedOn = DateTime.UtcNow,
                    CreatedBy = userId


                };

                CreateDefaultCaseMeasurementSteps(caseId, period);

                // Step 2: Map and insert CaseResult
                var caseResultEntity = _mapper.Map<CaseResult>(resultServiceModel);
                _concurrencyHelpers.SetDefaultValueInsert(caseResultEntity);
                _context.CaseResults.Add(caseResultEntity);
                _context.SaveChanges();

                resultServiceModel.Id = caseResultEntity.Id;

                // Step 3: Fetch case details
                var caseDetails = _context.Cases.FirstOrDefault(d => d.Id == caseId);
                if (caseDetails == null)
                {
                    throw new Exception($"Case with ID {caseId} not found.");
                }
                //  caseResultEntity.CaseResultName = $"{caseDetails.CaseName} - {period}";

                // Step 4: Prepare result model

                var caseResultData = _context.CaseResults.FirstOrDefault(d => d.Id == caseResultEntity.Id);
                return new CaseResultListServiceModel
                {
                    Id = caseResultEntity.Id,
                    CreatedOn = resultServiceModel.CreatedOn,
                    Period = period,
                    CaseName = caseDetails.CaseName,
                    CaseResultName = caseResultData.CaseResultName,
                    CaseId = caseId
                };
            });
        }


        //public ServiceResponseGeneric<CaseResultListServiceModel> CreateResultByCaseIdAsync(Guid caseId, string period)
        //{
        //    return new ServiceResponseGeneric<CaseResultListServiceModel>(async () =>
        //    {
        //        var userId = _concurrencyHelpers.GetLoggedInUserId();

        //        // Step 1: Fetch case details
        //        var caseDetails = _context.Cases.FirstOrDefault(d => d.Id == caseId);
        //        if (caseDetails == null)
        //        {
        //            throw new Exception($"Case with ID {caseId} not found.");
        //        }

        //        // Step 2: Create result service model
        //        var resultServiceModel = new ResultServiceModel
        //        {
        //            Id = Guid.NewGuid(),
        //            CaseId = caseId,
        //            Period = period,
        //            CreatedOn = DateTime.UtcNow,
        //            CreatedBy = userId,

        //            //  Set CaseResultName here to ensure it's always populated
        //            CaseResultName = $"{caseDetails.CaseName} - {period}"
        //        };

        //        // Step 3: Create default steps
        //        CreateDefaultCaseMeasurementSteps(caseId, period);

        //        // Step 4: Map and insert CaseResult
        //        var caseResultEntity = _mapper.Map<CaseResult>(resultServiceModel);
        //        _concurrencyHelpers.SetDefaultValueInsert(caseResultEntity);
        //        _context.CaseResults.Add(caseResultEntity);
        //        _context.SaveChanges();

        //        // Step 5: Prepare response model
        //        return new CaseResultListServiceModel
        //        {
        //            Id = caseResultEntity.Id,
        //            CreatedOn = resultServiceModel.CreatedOn,
        //            Period = period,
        //            CaseName = caseDetails.CaseName,
        //            CaseResultName = caseResultEntity.CaseResultName,
        //            CaseId = caseId
        //        };
        //    });
        //}



        public ServiceResponseGeneric<string> GetCaseImageZip(List<Guid> caseResultIds)
        {
            return new ServiceResponseGeneric<string>(async () =>
            {
                var userId = _concurrencyHelpers.GetLoggedInUserId();
                if (caseResultIds == null || !caseResultIds.Any())
                {
                    throw new ServiceResponseExceptionHandle(MessageHelper.InvalidImage, HttpStatusCode.BadRequest);
                }

                var imageRecords = await _context.CaseConfigurations
                    .Where(x => caseResultIds.Contains(x.CaseResultId) && x.CreatedBy == userId)
                    .ToListAsync();

                if (!imageRecords.Any())
                {
                    throw new ServiceResponseExceptionHandle(MessageHelper.InvalidImage, HttpStatusCode.NotFound);
                }

                string mediaPath = _configuration["AzureConfiguration:MediaPath"];
                string zipFolderPath = Path.Combine(_environment.ContentRootPath, "Document", "ZipCaseImages");
                string zipFileName = $"CaseImages_{Guid.NewGuid()}.zip";
                string zipFilePath = Path.Combine(zipFolderPath, zipFileName);

                if (!Directory.Exists(zipFolderPath))
                {
                    Directory.CreateDirectory(zipFolderPath);
                }

                if (File.Exists(zipFilePath))
                {
                    File.Delete(zipFilePath);
                }

                using (HttpClient httpClient = new HttpClient())
                using (var zipArchive = ZipFile.Open(zipFilePath, ZipArchiveMode.Create))
                {
                    foreach (var image in imageRecords)
                    {
                        string caseId = image.CaseId.ToString().ToLower();
                        string blobUrl = $"{mediaPath}Case/{caseId}/{image.ImageName}";

                        var response = await httpClient.GetAsync(blobUrl);
                        if (response.IsSuccessStatusCode)
                        {
                            byte[] imageBytes = await response.Content.ReadAsByteArrayAsync();
                            var zipEntry = zipArchive.CreateEntry(image.ImageName, CompressionLevel.Optimal);
                            using (var entryStream = zipEntry.Open())
                            {
                                await entryStream.WriteAsync(imageBytes, 0, imageBytes.Length);
                            }
                        }
                    }
                }
                return zipFilePath;
            });
        }


        public ServiceResponseGeneric<string> GetCaseResultsCsv(CaseImageDetailServiceModel caseImageDetailServiceModel)
        {
            return new ServiceResponseGeneric<string>(async () =>
            {
                var userId = _concurrencyHelpers.GetLoggedInUserId();
                if (caseImageDetailServiceModel.CaseResultIds == null || !caseImageDetailServiceModel.CaseResultIds.Any())
                {
                    throw new ServiceResponseExceptionHandle(MessageHelper.InvalId, HttpStatusCode.BadRequest);
                }

                var data = _context.CaseResults
                    .Where(x => caseImageDetailServiceModel.CaseResultIds.Contains(x.Id) && x.CreatedBy == userId).OrderByDescending(x => x.CreatedOn)
                    .ToList();

                if (!data.Any())
                {
                    throw new ServiceResponseExceptionHandle(MessageHelper.NoDataFound, HttpStatusCode.NotFound);
                }

                var resultModels = _mapper.Map<List<CaseResultJsonModel>>(data);
                var allProps = typeof(CaseResultJsonModel).GetProperties();

                var inputCols = string.IsNullOrWhiteSpace(caseImageDetailServiceModel.Columns)
                    ? null
                    : caseImageDetailServiceModel.Columns.Split(',')
                        .Select(c => c.Trim().ToLower().Replace(" ", "").Replace("-", "").Replace("_", ""))
                        .ToList();

                var selectedProps = inputCols == null
                    ? allProps
                    : allProps.Where(p =>
                    {
                        var jsonProp = p.GetCustomAttributes(typeof(JsonPropertyAttribute), true)
                                        .Cast<JsonPropertyAttribute>()
                                        .FirstOrDefault();

                        var jsonName = jsonProp?.PropertyName?.ToLower().Replace(" ", "").Replace("-", "").Replace("_", "") ?? "";
                        var propName = p.Name.ToLower();

                        return inputCols.Contains(propName) || inputCols.Contains(jsonName);
                    }).ToArray();

                if (!selectedProps.Any())
                {
                    throw new ServiceResponseExceptionHandle("No valid columns specified.", HttpStatusCode.BadRequest);
                }

                string csvFolderPath = Path.Combine(_environment.ContentRootPath, "Document", "CsvCaseResults");
                if (!Directory.Exists(csvFolderPath))
                {
                    Directory.CreateDirectory(csvFolderPath);
                }

                string csvFilePath = Path.Combine(csvFolderPath, $"CaseResults_{DateTime.UtcNow:yyyyMMddHHmmss}.csv");

                using (var writer = new StreamWriter(csvFilePath, false, Encoding.UTF8))
                {
                    // Header row
                    writer.WriteLine(string.Join(",", selectedProps.Select(p =>
                    {
                        var jsonProp = p.GetCustomAttributes(typeof(JsonPropertyAttribute), true)
                                        .Cast<JsonPropertyAttribute>()
                                        .FirstOrDefault();
                        return jsonProp?.PropertyName ?? p.Name;
                    })));

                    // Data rows
                    foreach (var item in resultModels)
                    {
                        var values = selectedProps.Select(p =>
                        {
                            var val = p.GetValue(item)?.ToString() ?? "";
                            return val.Contains(",") ? $"\"{val}\"" : val;
                        });

                        writer.WriteLine(string.Join(",", values));
                    }
                }

                return csvFilePath;
            });
        }


        public ServiceResponseGeneric<List<CaseOverlayImagesServiceModel>> GetOverlayImages(List<Guid> caseResultIds)
        {
            return new ServiceResponseGeneric<List<CaseOverlayImagesServiceModel>>(() =>
            {
                var userId = _concurrencyHelpers.GetLoggedInUserId();
                DataTable table = new DataTable();
                table.Columns.Add("Guid", typeof(Guid));

                foreach (var guid in caseResultIds)
                {
                    table.Rows.Add(guid);
                }
                var sqlParam = new DynamicParameters();
                sqlParam.Add("@Guids", table.AsTableValuedParameter("dbo.CaseresultGuidListType"));
                sqlParam.Add("@UserId", userId, DbType.Int32, ParameterDirection.Input);

                var returnData = _context.GetResultFromStoredProc<CaseOverlayImagesServiceModel>(
                    DbStoredProcedure.GetOverlayImages,
                    sqlParam,
                    CommandType.StoredProcedure
                );

                return returnData.ToList();
            });
        }

        public ServiceResponseGeneric<Task<bool>> UpdateResultImage(Guid caseResultId, string imageViewType, string imageName)
        {
            return new ServiceResponseGeneric<Task<bool>>(async () =>
            {
                var userId = _concurrencyHelpers.GetLoggedInUserId();
                var existingConfig = await _context.CaseConfigurations
                    .FirstOrDefaultAsync(x => x.CaseResultId == caseResultId && x.ImageViewType == imageViewType && x.CreatedBy == userId);

                if (existingConfig == null)
                {
                    throw new ServiceResponseExceptionHandle("No matching record found.", HttpStatusCode.NotFound);
                }

                existingConfig.DICOMImage = imageName;
                existingConfig.LastUpdatedBy = userId;
                existingConfig.LastUpdatedOn = DateTime.UtcNow;

                _context.CaseConfigurations.Update(existingConfig);
                await _context.SaveChangesAsync();

                return true;
            });
        }

        public ServiceResponseGeneric<string> GetCaseImagePDF(List<Guid> caseResultIds)
        {
            return new ServiceResponseGeneric<string>(async () =>
            {
                var userId = _concurrencyHelpers.GetLoggedInUserId();
                if (caseResultIds == null || !caseResultIds.Any())
                {
                    throw new ServiceResponseExceptionHandle(MessageHelper.InvalidImage, HttpStatusCode.BadRequest);
                }

                DataTable table = new DataTable();
                table.Columns.Add("Guid", typeof(Guid));

                foreach (var guid in caseResultIds)
                {
                    table.Rows.Add(guid);
                }
                var sqlParam = new DynamicParameters();
                sqlParam.Add("@Guids", table.AsTableValuedParameter("dbo.CaseresultGuidListType"));
                sqlParam.Add("@userId", userId);

                var imageRecords = _context.GetResultFromStoredProc<CaseOverlayImagesServiceModel>(
                    DbStoredProcedure.GetOverlayImages,
                    sqlParam,
                    CommandType.StoredProcedure
                );

                if (!imageRecords.Any())
                {
                    throw new ServiceResponseExceptionHandle(MessageHelper.InvalidImage, HttpStatusCode.NotFound);
                }

                string mediaPath = _configuration["AzureConfiguration:MediaPath"];
                string pdfFolderPath = Path.Combine(_environment.ContentRootPath, "Document", "PdfCaseImages");
                string pdfFileName = $"CaseImages_{Guid.NewGuid()}.pdf";
                string pdfFilePath = Path.Combine(pdfFolderPath, pdfFileName);

                if (!Directory.Exists(pdfFolderPath))
                {
                    Directory.CreateDirectory(pdfFolderPath);
                }

                if (File.Exists(pdfFilePath))
                {
                    File.Delete(pdfFilePath);
                }

                using (var stream = new FileStream(pdfFilePath, FileMode.Create, FileAccess.Write, FileShare.None))
                using (var doc = new iTextSharp.text.Document(PageSize.A4))
                {
                    PdfWriter.GetInstance(doc, stream);
                    doc.Open();

                    using (HttpClient httpClient = new HttpClient())
                    {
                        foreach (var image in imageRecords)
                        {
                            string caseId = image.CaseId.ToString().ToLower();
                            string blobUrl = $"{mediaPath}Case/{caseId}/{image.ImageName}";

                            var response = await httpClient.GetAsync(blobUrl);
                            if (response.IsSuccessStatusCode)
                            {
                                // Add the image title (this could be ImageName, CaseId, or any other property)
                                string title = image.CasePeriod + " - " + image.ImageViewType; // or any other title you want
                                doc.Add(new Paragraph(title) { Alignment = Element.ALIGN_CENTER });

                                byte[] imageBytes = await response.Content.ReadAsByteArrayAsync();

                                iTextSharp.text.Image pdfImage = iTextSharp.text.Image.GetInstance(imageBytes);

                                float maxWidth = doc.PageSize.Width - 40; // 40 for margins
                                float maxHeight = doc.PageSize.Height - 40; // 40 for margins

                                float scaleFactor = Math.Min(maxWidth / pdfImage.Width, maxHeight / pdfImage.Height);

                                if (scaleFactor < 1)
                                {
                                    pdfImage.ScalePercent(scaleFactor * 100); // Scale image down if needed
                                }
                                else
                                {
                                    if (maxHeight > 700)
                                    {
                                        maxHeight = 700;
                                    }
                                    pdfImage.ScaleToFit(maxWidth, maxHeight); // Otherwise, just scale to fit
                                }

                                //pdfImage.ScaleToFit(doc.PageSize.Width - 40, doc.PageSize.Height - 40);



                                pdfImage.Alignment = Element.ALIGN_CENTER;
                                doc.Add(pdfImage);
                                //doc.Add(new Paragraph("\n"));
                                doc.NewPage();
                            }
                        }
                    }

                    doc.Close();
                }

                return pdfFilePath;
            });
        }


        public ServiceResponseGeneric<Task<string>> GetCaseResultsJSON(List<Guid> ids)
        {
            return new ServiceResponseGeneric<Task<string>>(async () =>
            {
                var userId = _concurrencyHelpers.GetLoggedInUserId();
                if (ids == null || !ids.Any())
                {
                    throw new ServiceResponseExceptionHandle(MessageHelper.InvalId, HttpStatusCode.BadRequest);
                }

                var data = _context.CaseResults
                    .Where(x => ids.Contains(x.Id) && x.CreatedBy == userId)
                    .ToList();

                if (!data.Any())
                {
                    throw new ServiceResponseExceptionHandle(MessageHelper.NoDataFound, HttpStatusCode.NotFound);
                }

                var mapped = _mapper.Map<List<CaseResultJsonModel>>(data);

                string jsonFolderPath = Path.Combine(_environment.ContentRootPath, "Document", "JsonCaseResults");
                if (!Directory.Exists(jsonFolderPath))
                {
                    Directory.CreateDirectory(jsonFolderPath);
                }

                string jsonFilePath = Path.Combine(jsonFolderPath, $"CaseResults_{DateTime.UtcNow:yyyyMMddHHmmss}.json");

                string jsonData = JsonConvert.SerializeObject(new { CaseResults = mapped }, Formatting.Indented);
                await File.WriteAllTextAsync(jsonFilePath, jsonData);

                return jsonFilePath;
            });
        }


        public ServiceResponseGeneric<Task<string>> GetCaseResultsZip(List<Guid> ids)
        {
            return new ServiceResponseGeneric<Task<string>>(async () =>
            {
                var userId = _concurrencyHelpers.GetLoggedInUserId();
                if (ids == null || !ids.Any())
                {
                    throw new ServiceResponseExceptionHandle(MessageHelper.InvalId, HttpStatusCode.BadRequest);
                }

                var data = _context.CaseResults.Where(x => ids.Contains(x.Id) && x.CreatedBy == userId).ToList();

                if (!data.Any())
                {
                    throw new ServiceResponseExceptionHandle(MessageHelper.NoDataFound, HttpStatusCode.NotFound);
                }

                var resultModels = _mapper.Map<List<CaseResultJsonModel>>(data);
                var properties = typeof(CaseResultJsonModel).GetProperties();

                string basePath = Path.Combine(_environment.ContentRootPath, "Document", "CaseResultsZip");
                if (!Directory.Exists(basePath)) Directory.CreateDirectory(basePath);

                string timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
                string jsonFilePath = Path.Combine(basePath, $"CaseResults_{timestamp}.json");
                string csvFilePath = Path.Combine(basePath, $"CaseResults_{timestamp}.csv");
                string zipFilePath = Path.Combine(basePath, $"CaseResults_{timestamp}.zip");

                string jsonData = JsonConvert.SerializeObject(new { CaseResults = resultModels }, Formatting.Indented);
                await File.WriteAllTextAsync(jsonFilePath, jsonData);

                var csvLines = new List<string>
        {
            string.Join(",", properties.Select(p =>
            {
                var jsonProp = p.GetCustomAttributes(typeof(JsonPropertyAttribute), true)
                                .Cast<JsonPropertyAttribute>()
                                .FirstOrDefault();
                return jsonProp?.PropertyName ?? p.Name;
            }))
        };

                csvLines.AddRange(resultModels.Select(item =>
                    string.Join(",", properties.Select(p =>
                    {
                        var value = p.GetValue(item);
                        var text = value?.ToString() ?? "";
                        return text.Contains(",") ? $"\"{text}\"" : text;
                    }))
                ));

                await File.WriteAllLinesAsync(csvFilePath, csvLines);

                using (var zip = ZipFile.Open(zipFilePath, ZipArchiveMode.Create))
                {
                    zip.CreateEntryFromFile(jsonFilePath, Path.GetFileName(jsonFilePath));
                    zip.CreateEntryFromFile(csvFilePath, Path.GetFileName(csvFilePath));
                }

                File.Delete(jsonFilePath);
                File.Delete(csvFilePath);

                return zipFilePath;
            });
        }

        public ServiceResponseGeneric<List<CaseCalibrationModel>> GetCalibrationValue(Guid caseResultId)
        {
            return new ServiceResponseGeneric<List<CaseCalibrationModel>>(() =>
            {
                var caseDetail = _context.CaseConfigurations.Where(d => d.CaseResultId.Equals(caseResultId)).ToList();
                return caseDetail.Select(d => new CaseCalibrationModel
                {
                    Id = d.Id,
                    CaseResultId = d.CaseResultId,
                    ImageViewType = d.ImageViewType,
                    CalibrationValue = d.CalibrationValue,
                }).ToList();
            });
        }

       public ServiceResponseGeneric<bool> UpdateCaseName(UpdateCaseNameServiceModel updateCaseNameServiceModel)
       {
            return new ServiceResponseGeneric<bool>(() =>
            {
                var caseDetail = _context.Cases.FirstOrDefault(d => d.Id.Equals(updateCaseNameServiceModel.CaseId));
                if (caseDetail == null)
                {
                    throw new ServiceResponseExceptionHandle(MessageHelper.NoDataFound, HttpStatusCode.NotFound);
                }
                caseDetail.CaseName = updateCaseNameServiceModel.CaseName;
                _context.Cases.Update(caseDetail);
                _context.SaveChanges();
                return true;
            });
       }


        public ServiceResponseGeneric<bool> UpdateCalibrationValue(CaseCalibrationModel caseCalibrationModel)
        {
            return new ServiceResponseGeneric<bool>(() =>
            {
                var caseDetail = _context
                                    .CaseConfigurations
                                    .FirstOrDefault(d =>
                                            d.CaseResultId.Equals(caseCalibrationModel.CaseResultId) &&
                                            d.ImageViewType.Equals(caseCalibrationModel.ImageViewType)
                                    );
                if (caseDetail == null)
                {
                    throw new ServiceResponseExceptionHandle(MessageHelper.NoDataFound, HttpStatusCode.NotFound);
                }
                caseDetail.CalibrationValue = caseCalibrationModel.CalibrationValue;
                _context.CaseConfigurations.Update(caseDetail);
                _context.SaveChanges();
                return true;
            });
        }
    }

}
