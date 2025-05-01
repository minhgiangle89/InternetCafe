using Microsoft.AspNetCore.Mvc;

namespace InternetCafe.API.Common
{
    /// <summary>
    /// Generic API Response wrapper
    /// </summary>
    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public T Data { get; set; }
        public IDictionary<string, string[]> Errors { get; set; }

        public ApiResponse()
        {
        }

        public ApiResponse(T data, string message = null)
        {
            Success = true;
            Message = message;
            Data = data;
        }

        public ApiResponse(string message)
        {
            Success = false;
            Message = message;
        }
    }

    /// <summary>
    /// Non-generic API Response for operations without return data
    /// </summary>
    public class ApiResponseBase
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public IDictionary<string, string[]> Errors { get; set; }

        public ApiResponseBase()
        {
            Success = true;
        }

        public ApiResponseBase(string message, bool success = false)
        {
            Success = success;
            Message = message;
        }
    }

    /// <summary>
    /// API Response Factory for creating response objects
    /// </summary>
    public static class ApiResponseFactory
    {
        public static ApiResponse<T> Success<T>(T data, string message = null)
        {
            return new ApiResponse<T>
            {
                Success = true,
                Data = data,
                Message = message
            };
        }

        public static ApiResponse<T> Fail<T>(string message, IDictionary<string, string[]> errors = null)
        {
            return new ApiResponse<T>
            {
                Success = false,
                Message = message,
                Errors = errors
            };
        }

        public static ApiResponseBase Success(string message = null)
        {
            return new ApiResponseBase
            {
                Success = true,
                Message = message
            };
        }

        public static ApiResponseBase Fail(string message, IDictionary<string, string[]> errors = null)
        {
            return new ApiResponseBase
            {
                Success = false,
                Message = message,
                Errors = errors
            };
        }
    }

    /// <summary>
    /// Extension methods for Controller responses
    /// </summary>
    public static class ApiResponseExtensions
    {
        public static ActionResult<ApiResponse<T>> ToApiResponse<T>(this T data, string message = null)
        {
            return new ApiResponse<T>(data, message);
        }

        public static ActionResult<ApiResponse<T>> ToApiResponse<T>(this ActionResult<T> actionResult, string message = null)
        {
            if (actionResult.Result is ObjectResult objectResult)
            {
                return new ApiResponse<T>((T)objectResult.Value, message);
            }
            return new ApiResponse<T>(actionResult.Value, message);
        }

        public static ActionResult ToApiResponse(this ControllerBase controller, string message = null)
        {
            return controller.Ok(new ApiResponseBase { Success = true, Message = message });
        }

        public static ActionResult ToApiErrorResponse(this ControllerBase controller, string message, int statusCode = 400)
        {
            return new ObjectResult(new ApiResponseBase { Success = false, Message = message })
            {
                StatusCode = statusCode
            };
        }
    }
}
