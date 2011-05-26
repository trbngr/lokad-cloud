﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Lokad.Cloud.Provisioning.AzureManagement
{
    // NOTE (ruegg, 2011-05-24): Clone from Microsoft.WindowsAzure.StorageClient.ShouldRetry.
    // Justification: Avoid reference to the storage client library just for this delegate type
    public delegate bool ShouldRetry(int retryCount, Exception lastException, out TimeSpan delay);
}
