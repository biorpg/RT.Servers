﻿
namespace Servers
{
    /// <summary>
    /// Contains definitions for all the HTTP status codes defined in HTTP/1.1.
    /// </summary>
    public enum HTTPStatusCode
    {
        None = 0,

        // Informational 1xx
        _100_Continue = 100,
        _101_SwitchingProtocols = 101,

        // Successful 2xx
        _200_OK = 200,
        _201_Created = 201,
        _202_Accepted = 202,
        _203_NonAuthoritativeInformation = 203,
        _204_NoContent = 204,
        _205_ResetContent = 205,
        _206_PartialContent = 206,

        // Redirection 3xx
        _300_MultipleChoices = 300,
        _301_MovedPermanently = 301,
        _302_Found = 302,
        _303_SeeOther = 303,
        _304_NotModified = 304,
        _305_UseProxy = 305,
        _306__Unused = 306,
        _307_TemporaryRedirect = 307,

        // Client Error 4xx
        _400_BadRequest = 400,
        _401_Unauthorized = 401,
        _402_PaymentRequired = 402,
        _403_Forbidden = 403,
        _404_NotFound = 404,
        _405_MethodNotAllowed = 405,
        _406_NotAcceptable = 406,
        _407_ProxyAuthenticationRequired = 407,
        _408_RequestTimeout = 408,
        _409_Conflict = 409,
        _410_Gone = 410,
        _411_LengthRequired = 411,
        _412_PreconditionFailed = 412,
        _413_RequestEntityTooLarge = 413,
        _414_RequestURITooLong = 414,
        _415_UnsupportedMediaType = 415,
        _416_RequestedRangeNotSatisfiable = 416,
        _417_ExpectationFailed = 417,

        // Server Error 5xx
        _500_InternalServerError = 500,
        _501_NotImplemented = 501,
        _502_BadGateway = 502,
        _503_ServiceUnavailable = 503,
        _504_GatewayTimeout = 504,
        _505_HTTPVersionNotSupported = 505
    }
}
