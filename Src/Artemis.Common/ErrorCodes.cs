using System.Collections.Generic;

namespace Com.Ctrip.Soa.Artemis.Common
{
    /// <summary>
    /// Created by fang_j 2016/08/03.
    /// </summary>
    public class ErrorCodes
    {
        public static string BAD_REQUEST = "bad-request";
        public static string RATE_LIMITED = "rate-limited";
        public static string NO_PERMISION = "no-permision";
        public static string DATA_NOT_FOUND = "data-not-found";
        public static string INTERNAL_SERVICE_ERROR = "internal-service-error";
        public static string SERVICE_UNAVAILABLE = "service-unavailable";
        public static string UNKNOWN = "unknown";

        private static HashSet<string> _rerunnabledErrorCodes = new HashSet<string>();
        private static HashSet<string> _serviceDownErrorCodes = new HashSet<string>();
        private static HashSet<string> _reregisterErrorCodes = new HashSet<string>();
        static ErrorCodes()
        {
            _rerunnabledErrorCodes.Add(RATE_LIMITED);
            _rerunnabledErrorCodes.Add(UNKNOWN);

            _serviceDownErrorCodes.Add(INTERNAL_SERVICE_ERROR);
            _serviceDownErrorCodes.Add(SERVICE_UNAVAILABLE);

            _reregisterErrorCodes.Add(DATA_NOT_FOUND);
            _reregisterErrorCodes.Add(UNKNOWN);
        }

        public static HashSet<string> RerunnableErrorCodes
        {
            get { return _rerunnabledErrorCodes; }
        }

        public static HashSet<string> ServiceDownErrorCodes
        {
            get { return _serviceDownErrorCodes; }
        }

        public static HashSet<string> ReregisterErrorCodes
        {
            get { return _reregisterErrorCodes; }
        }
    }
}
