// Copyright (c) Microsoft and contributors.  All rights reserved.
//
// This source code is licensed under the MIT license found in the
// LICENSE file in the root directory of this source tree.

namespace Microsoft.Azure.Management.ANF.Samples
{
    using Microsoft.Azure.Management.NetApp;
    using Microsoft.Azure.Management.NetApp.Models;
    using System.Threading.Tasks;

    /// <summary>
    /// Includes Updates functionalities for ANF resources
    /// </summary>
    public class Update
    {
        /// <summary>
        /// Performs pool change for given volume
        /// </summary>
        /// <param name="anfClient">ANF object</param>
        /// <param name="resourceGroupName">Resource group name</param>
        /// <param name="accountName">Azure NetApp Files Account name</param>
        /// <param name="poolName">Azure NetApp Files Capacity Pool name</param>
        /// <param name="volumeName">Azure NetApp Files Volume name</param>
        /// <param name="newPoolResourceId">New Capacity Pool resource Id</param>
        /// <returns></returns>
        public static async Task ChangeVolumeCapacityPoolAsync(AzureNetAppFilesManagementClient anfClient, string resourceGroupName, string accountName, string poolName, string volumeName, string newPoolResourceId)
        {
            PoolChangeRequest changeRequestBody = new PoolChangeRequest()
            {
                NewPoolResourceId = newPoolResourceId
            };
            await anfClient.Volumes.PoolChangeAsync(resourceGroupName, accountName, poolName, volumeName, changeRequestBody);
        }
    }
}
