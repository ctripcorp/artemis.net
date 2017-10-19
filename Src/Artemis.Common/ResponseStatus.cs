namespace Com.Ctrip.Soa.Artemis.Common
{
    public class ResponseStatus
    {
        public class STATUS
        {

            public const string SUCCESS = "success";
            public const string FAIL = "fail";
            public const string PARTIAL_FAIL = "partial_fail";
            public const string UKNOWN = "unknown";

        }
        public string Status { get; set; }
        public string ErrorCode { get; set; }
        public string Message { get; set; }
    }

    public static class ResponseStatusExtension
    {
        public static bool IsFail(this ResponseStatus responseStatus)
        {
            if (responseStatus == null)
            {
                return false;
            }
            return ResponseStatus.STATUS.FAIL.Equals(responseStatus.Status);
        }

        public static bool isPartialFail(this ResponseStatus responseStatus)
        {
            if (responseStatus == null)
            {
                return false;
            }
            return ResponseStatus.STATUS.PARTIAL_FAIL.Equals(responseStatus.Status);
        }

        public static bool IsServiceDown(this ResponseStatus status)
        {
            if (status == null)
            {
                return false;
            }
            return status.IsFail() && ErrorCodes.ServiceDownErrorCodes.Contains(status.ErrorCode);
        }

        public static bool IsSuccess(this ResponseStatus status)
        {
            if (status == null)
            {
                return false;
            }
            return ResponseStatus.STATUS.SUCCESS.Equals(status.Status);
        }

        public static bool IsRateLimited(this ResponseStatus status)
        {
            if (status == null)
            {
                return false;
            }
            return status.IsFail() && ErrorCodes.RATE_LIMITED.Equals(status.ErrorCode);
        }

        public static bool IsRerunnable(this ResponseStatus status)
        {
            if (status == null)
            {
                return false;
            }
            return status.IsFail() && ErrorCodes.RerunnableErrorCodes.Contains(status.ErrorCode);
        }
    }
}
