﻿using System;
using System.IO;
using System.Net;
using System.Net.Http;

namespace Lokad.Cloud.Provisioning
{
    public static class ProvisioningErrorHandling
    {
        private static Random _random = new Random();

        public static bool RetryOnTransientErrors(int currentRetryCount, Exception lastException, out TimeSpan retryInterval)
        {
            if (IsTransientError(lastException) && currentRetryCount <= 30)
            {
                lock (_random)
                {
                    retryInterval = TimeSpan.FromMilliseconds(_random.Next(Math.Min(10000, 10 + currentRetryCount * currentRetryCount * 10)));
                }

                return true;
            }

            retryInterval = TimeSpan.Zero;
            return false;
        }

        public static bool IsTransientError(Exception exception)
        {
            HttpStatusCode httpStatus;
            if (TryGetHttpStatusCode(exception, out httpStatus))
            {
                // For HTTP Errors only retry on Server Errors: 5xx
                return (int)httpStatus >= 500 && (int)httpStatus < 600;
            }

            WebExceptionStatus webStatus;
            if (TryGetWebStatusCode(exception, out webStatus))
            {
                switch(webStatus)
                {
                    case WebExceptionStatus.Timeout:
                    case WebExceptionStatus.ConnectionClosed:
                    case WebExceptionStatus.ProtocolError:
                    case WebExceptionStatus.ConnectFailure:
                    case WebExceptionStatus.ReceiveFailure:
                    case WebExceptionStatus.SendFailure:
                    case WebExceptionStatus.PipelineFailure:
                    case WebExceptionStatus.SecureChannelFailure:
                    case WebExceptionStatus.ServerProtocolViolation:
                    case WebExceptionStatus.KeepAliveFailure:
                    case WebExceptionStatus.Pending:
                    case WebExceptionStatus.UnknownError:
                        return true;
                    default:
                        return false;
                }
            }

            return exception is IOException;
        }

        public static bool TryGetHttpStatusCode(Exception exception, out HttpStatusCode httpStatusCode)
        {
            var aggregateException = exception as AggregateException;
            if (aggregateException != null)
            {
                exception = aggregateException.GetBaseException();
            }

            var httpException = exception as HttpException;
            if (httpException == null)
            {
                httpStatusCode = default(HttpStatusCode);
                return false;
            }

            var webException = httpException.InnerException as WebException;
            if (webException == null)
            {
                httpStatusCode = default(HttpStatusCode);
                return false;
            }

            var httpWebResponse = webException.Response as HttpWebResponse;
            if (httpWebResponse == null)
            {
                httpStatusCode = default(HttpStatusCode);
                return false;
            }

            httpStatusCode = httpWebResponse.StatusCode;
            return true;
        }

        public static bool TryGetWebStatusCode(Exception exception, out WebExceptionStatus webStatusCode)
        {
            var aggregateException = exception as AggregateException;
            if (aggregateException != null)
            {
                exception = aggregateException.GetBaseException();
            }

            var httpException = exception as HttpException;
            if (httpException == null)
            {
                webStatusCode = default(WebExceptionStatus);
                return false;
            }

            var webException = httpException.InnerException as WebException;
            if (webException == null)
            {
                webStatusCode = default(WebExceptionStatus);
                return false;
            }

            webStatusCode = webException.Status;
            return true;
        }
    }
}
