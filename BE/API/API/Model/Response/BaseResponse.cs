using System;

namespace API.Model.Response
{
    public class BaseResponse
    {
        public bool Success { get; set; }
        public string ErrorMessage { get; set; }
        public string ErrorCode { get; set; }
        public object Data { get; set; }

        public BaseResponse()
        {
            Success = true;
        }

        public void OnSuccess(object data)
        {
            Success = true;
            Data = data;
        }

        public void OnException(Exception ex)
        {
            Success = false;
            ErrorMessage = ex.Message;
        }
    }
}
