// Copyright (c) Microsoft and contributors.  All rights reserved.
//
// This source code is licensed under the MIT license found in the
// LICENSE file in the root directory of this source tree.

namespace Microsoft.Azure.Management.ANF.Samples
{
    using Microsoft.Azure.Management.NetApp;
    using Microsoft.Azure.Management.NetApp.Models;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    /// <summary>
    /// Includes Creation functionality for ANF resources
    /// </summary>
    public class Creation
    {
        /// <summary>
        /// Creates or Updates Azure NetApp Files Account
        /// </summary>
        /// <param name="anfClient">ANF client object</param>
        /// <param name="resourceGroupName">Resource group name</param>
        /// <param name="location">Azure location</param>
        /// <param name="accountName">Azure NetApp Files Account name</param>
        /// <returns>NetApp Account object</returns>
        public static async Task<NetAppAccount> CreateOrUpdateANFAccountAsync(AzureNetAppFilesManagementClient anfClient, string resourceGroupName, string location, string accountName)
        {
           NetAppAccount anfAccountBody = new NetAppAccount(location, null, accountName);
           return await anfClient.Accounts.CreateOrUpdateAsync(anfAccountBody, resourceGroupName, accountName);           
        }

        /// <summary>
        /// Creates or Updates Azure NetApp Files Capacity Pool
        /// </summary>
        /// <param name="anfClient">ANF client object</param>
        /// <param name="resourceGroupName">Resource group name</param>
        /// <param name="location">Azure location</param>
        /// <param name="accountName">Azure NetApp Files Account name</param>
        /// <param name="poolName">Azure NetApp Files Capacity Pool name</param>
        /// <param name="poolSize">Azure NetApp Files Capacity Pool size</param>
        /// <param name="serviceLevel">Service Level</param>
        /// <returns>Azure NetApp Files Capacity Pool</returns>
        public static async Task<CapacityPool> CreateOrUpdateANFCapacityPoolAsync(AzureNetAppFilesManagementClient anfClient, string resourceGroupName, string location, string accountName, string poolName, long poolSize, string serviceLevel)
        {
            CapacityPool primaryCapacityPoolBody = new CapacityPool()
            {
                Location = location.ToLower(), // Important: location needs to be lower case
                ServiceLevel = serviceLevel, //Service level can be one of three levels -> { Standard, Premium, Ultra }
                Size = poolSize
            };
            return await anfClient.Pools.CreateOrUpdateAsync(primaryCapacityPoolBody, resourceGroupName, accountName, poolName);
        }

        /// <summary>
        /// Creates Or Updates Azure NetApp Files Volume
        /// </summary>
        /// <param name="anfClient">ANF client object</param>
        /// <param name="resourceGroupName">Resource group name</param>
        /// <param name="location">Azure location</param>
        /// <param name="accountName">Azure NetApp Files Account name</param>
        /// <param name="poolName">Azure NetApp Files Capacity Pool name</param>
        /// <param name="serviceLevel">Service Level</param>
        /// <param name="volumeName">Azure NetApp Files Volume name</param>
        /// <param name="subnetId">Subnet Id</param>
        /// <param name="volumeSize">Azure NetApp Files Volume size</param>
        /// <returns>Azure NetApp Files Volume</returns>
        public static async Task<Volume> CreateOrUpdateANFVolumeAsync(AzureNetAppFilesManagementClient anfClient, string resourceGroupName, string location, string accountName, string poolName, string serviceLevel, string volumeName, string subnetId, long volumeSize)
        {
            // Creating export policy object
            VolumePropertiesExportPolicy exportPolicies = new VolumePropertiesExportPolicy()
            {
                Rules = new List<ExportPolicyRule>
                {
                    new ExportPolicyRule() {
                        AllowedClients = "0.0.0.0",
                        Cifs = false,
                        Nfsv3 = false,
                        Nfsv41 = true,
                        RuleIndex = 1,
                        UnixReadOnly = false,
                        UnixReadWrite = true
                    }
                }
            };

            Volume primaryVolumeBody = new Volume()
            {
                ExportPolicy = exportPolicies,
                Location = location.ToLower(),
                ServiceLevel = serviceLevel, //Service level can be one of three levels -> { Standard, Premium, Ultra }
                CreationToken = volumeName,
                SubnetId = subnetId,
                UsageThreshold = volumeSize,
                ProtocolTypes = new List<string>() { "NFSv4.1" }
            };

            return await anfClient.Volumes.CreateOrUpdateAsync(primaryVolumeBody, resourceGroupName, accountName, poolName, volumeName);
        }
    }
}
