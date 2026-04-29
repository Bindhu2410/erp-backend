using System;

namespace ERP.API.Models
{
    public class ApiResponse<T>
    {
        public string Message { get; set; }
        public int StatusCode { get; set; }
        public T Data { get; set; }

        public ApiResponse(T data, string message = null, int statusCode = 200)
        {
            Message = message ?? "Success";
            StatusCode = statusCode;
            Data = data;
        }

        public static ApiResponse<T> Success(T data, string message = null)
        {
            return new ApiResponse<T>(data, message ?? "Success", 200);
        }

        public static ApiResponse<T> Failure(string message, int statusCode = 500)
        {
            return new ApiResponse<T>(default, message, statusCode);
        }
    }
}
