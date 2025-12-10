//using DICOMcloud.Extensions;
using Serilog;
using System.Net;

namespace PlayBook3DTSL.Utilities.Helpers
{
    public class ServiceResponse
    {
        public class FileResponse
        {
            private bool? _success;
            private string? fileStream;
            public FileResponse()
                : this((FileResponse)null)
            {
            }

            public FileResponse(ErrorInfo error)
                : this(new[] { error })
            {
            }


            public FileResponse(IEnumerable<ErrorInfo> errors)
                : this((FileResponse)null)
            {
                foreach (var errorInfo in errors)
                    Errors.Add(errorInfo);
            }

            public FileResponse(FileResponse result)
            {
                if (result != null)
                {
                    Success = result.Success;
                    Errors = new List<ErrorInfo>(result.Errors);
                    StatusCode = result.StatusCode;
                    FileStream = result.fileStream;
                }
                else
                {
                    Errors = new List<ErrorInfo>();
                    //Messages = new List<InfoMessage>();
                    StatusCode = result.StatusCode;
                }
            }

            /// <summary>
            ///     Indicates if result is successful.
            /// </summary>
            public bool Success
            {
                get => _success ?? Errors.Count == 0;
                set => _success = value;
            }
            public HttpStatusCode StatusCode { get; set; }

            /// <summary>
            ///     Gets a list of errors.
            /// </summary>
            public IList<ErrorInfo> Errors { get; }
            public string FileStream { get; set; }

        }

        public class ServiceResponseGeneric
        {
            private bool? _success;
        }



        public class ErrorInfo
        {
            public ErrorInfo()
               : this(HttpStatusCode.BadRequest, string.Empty)
            {
            }

            public ErrorInfo(string errorMessage)
                : this(HttpStatusCode.BadRequest, errorMessage)
            {
            }


            public ErrorInfo(HttpStatusCode statusCode, string errorMessage)
            {
                StatusCode = statusCode;
                ErrorMessage = errorMessage;
            }

            public HttpStatusCode StatusCode { get; set; } = HttpStatusCode.BadRequest;

            public string ErrorMessage { get; set; }

            public override string ToString()
            {
                return string.Format("{0}. Key: '{1}', ErrorMessage: '{2}'", base.ToString(), StatusCode, ErrorMessage);
            }
        }

        public class ServiceResponseGeneric<T>
        {
            private bool? _success;
            //private List<AccountView> userlist;

            public bool Success
            {
                get => _success ?? Errors.Count == 0;
                set => _success = value;
            }
            public HttpStatusCode StatusCode { get; set; }
            public List<ErrorInfo> Errors { get; } = new List<ErrorInfo>();
            public ServiceResponseGeneric(string msg, HttpStatusCode httpStatusCode)
            {
                StatusCode = httpStatusCode;
                Errors.Add(new ErrorInfo
                {
                    ErrorMessage = msg,
                    StatusCode = httpStatusCode
                });
            }

            public ServiceResponseGeneric(Func<Task<T>> result)
            {
                InitializeAsync(result).GetAwaiter().GetResult();
            }
            private async Task InitializeAsync(Func<Task<T>> result)
            {
                try
                {
                    Output = await result.Invoke();
                }
                catch (ServiceResponseExceptionHandle ex)
                {
                    Errors.Add(new ErrorInfo { ErrorMessage = ex.Exception, StatusCode = ex.HttpStatusCode });
                }
                catch (Exception ex)
                {
                    Log.Error($"ErrorData = {result.Target}, " +
                              $"StackTrace = {ex.StackTrace}, " +
                              $"Message = {ex.Message}, " +
                              $"InnerException = {ex.InnerException}, " +
                              $"ex.InnerException.Message = {ex.InnerException?.Message} ");
                    Errors.Add(new ErrorInfo { ErrorMessage = ex.Message, StatusCode = HttpStatusCode.BadRequest });
                }
            }

            public ServiceResponseGeneric(Func<T> result)
            {
                try
                {

                    Output = result.Invoke();
                }
                catch (ServiceResponseExceptionHandle ex)
                {
                    Errors.Add(new ErrorInfo { ErrorMessage = ex.Exception, StatusCode = ex.HttpStatusCode });
                }
                catch (Exception ex)
                {
                    Log.Error($"StackTrace = {ex.StackTrace}, " +
                               $"Message = {ex.Message}, " +
                                $" InnerException = {ex.InnerException}, " +
                                $" ex.InnerException.Message = {ex.InnerException?.Message} ");
                    Errors.Add(new ErrorInfo { ErrorMessage = ex.Message, StatusCode = HttpStatusCode.BadRequest });
                }
            }

            //public ServiceResponseGeneric(List<AccountView> userlist)
            //{
            //    this.userlist = userlist;
            //}

            public T Output { get; set; }
        }


        public class ServiceResponseExceptionHandle : Exception
        {
            public string Exception { get; set; }
            public HttpStatusCode HttpStatusCode { get; set; }
            public ServiceResponseExceptionHandle()
            {

            }
            public ServiceResponseExceptionHandle(string msg, HttpStatusCode httpStatusCode)
            {
                Exception = msg;
                HttpStatusCode = httpStatusCode;
            }
            public List<ErrorInfo> Errors { get; } = new List<ErrorInfo>();
        }







        /// <summary>
        /// Represents result of an action.
        /// </summary>
        public class ExecutionResult
        {
            private bool? _success;

            #region Constructors

            /// <summary>
            /// Default constructor
            /// </summary>
            public ExecutionResult() : this((ExecutionResult)null)
            { }

            /// <summary>
            /// Initialize execution result with one error
            /// </summary>
            public ExecutionResult(ErrorInfo error) : this(new[] { error })
            { }

            /// <summary>
            /// Initialize execution result with one information message
            /// </summary>
            public ExecutionResult(InfoMessage message) : this(new[] { message })
            { }

            /// <summary>
            /// Initialize execution result with error list
            /// </summary>
            public ExecutionResult(IEnumerable<ErrorInfo> errors) : this((ExecutionResult)null)
            {
                foreach (ErrorInfo errorInfo in errors)
                {
                    Errors.Add(errorInfo);
                }
            }

            /// <summary>
            /// Initialize execution result with information message list
            /// </summary>
            public ExecutionResult(IEnumerable<InfoMessage> messages) : this((ExecutionResult)null)
            {
                foreach (InfoMessage message in messages)
                {
                    Messages.Add(message);
                }
            }

            /// <summary>
            /// Main constructor
            /// </summary>
            public ExecutionResult(ExecutionResult result)
            {
                if (result != null)
                {
                    Success = result.Success;
                    Errors = new List<ErrorInfo>(result.Errors);
                    Messages = new List<InfoMessage>(result.Messages);
                }
                else
                {
                    Errors = new List<ErrorInfo>();
                    Messages = new List<InfoMessage>();
                }
            }

            #endregion

            #region Properties

            /// <summary>
            /// Indicates if result is successful.
            /// </summary>
            public bool Success
            {
                get => _success ?? Errors.Count == 0;
                set => _success = value;
            }

            /// <summary>
            /// Errors collection
            /// </summary>
            public IList<ErrorInfo> Errors { get; }

            /// <summary>
            /// Info messages collection
            /// </summary>
            public IList<InfoMessage> Messages { get; }

            //public HttpStatusCode StatusCode { get; set; } = HttpStatusCode.BadRequest;

            #endregion
        }

        /// <summary>
        /// Represents result of an action that returns any value
        /// </summary>
        /// <typeparam name="T">Type of value to be returned with action</typeparam>
        public class ExecutionResult<T> : ExecutionResult
        {
            /// <summary>
            /// Default constructor
            /// </summary>
            public ExecutionResult() : this((ExecutionResult)null)
            { }

            public ExecutionResult(T result) : this((ExecutionResult)null)
            {
                Output = result;
            }

            public ExecutionResult(T result, InfoMessage message) : this((ExecutionResult)null)
            {
                Output = result;
                Messages.Add(message);
            }

            public ExecutionResult(ExecutionResult result) : base(result)
            {
                if (result is ExecutionResult<T> r) // make sure result is not null and cast to ExecutionResult
                {
                    Output = r.Output;
                }
            }

            public ExecutionResult(ErrorInfo error) : this(new[] { error })
            { }

            public ExecutionResult(InfoMessage message) : this(new[] { message })
            { }

            public ExecutionResult(IEnumerable<ErrorInfo> errors) : this((ExecutionResult)null)
            {
                foreach (ErrorInfo errorInfo in errors)
                {
                    Errors.Add(errorInfo);
                }
            }

            public ExecutionResult(IEnumerable<InfoMessage> messages) : this((ExecutionResult)null)
            {
                foreach (InfoMessage message in messages)
                {
                    Messages.Add(message);
                }
            }

            public T Output { get; set; }
        }



        public class InfoMessage
        {
            /// <summary>
            /// Default constructor
            /// </summary>
            public InfoMessage() : this(string.Empty, string.Empty)
            { }

            public InfoMessage(string message)
                : this("", message)
            { }

            /// <summary>
            /// Main constructor
            /// </summary>
            public InfoMessage(string key, string message)
            {
                Title = key;
                MessageText = message;
            }

            public string Title { get; set; }

            public string MessageText { get; set; }

            public override string ToString()
            {
                return string.Format("{0}. Key: '{1}', Message: '{2}'", base.ToString(), Title, MessageText);
            }

        }
    }
}
