// Copyright (c) Microsoft and contributors.  All rights reserved.
//
// This source code is licensed under the MIT license found in the
// LICENSE file in the root directory of this source tree.

namespace Microsoft.Azure.Management.ANF.Samples
{
    using Microsoft.Azure.Management.ANF.Samples.Common;
    using Microsoft.Azure.Management.NetApp;
    using Microsoft.Azure.Management.NetApp.Models;
    using Microsoft.Rest;
    using System;
    using System.Threading.Tasks;
    using static Microsoft.Azure.Management.ANF.Samples.Common.Utils;

    class Program
    {
        //------------------------------------------IMPORTANT------------------------------------------------------------------
        // Setting variables necessary for resources creation - change these to appropriated values related to your environment
        // Please NOTE: Resource Group and VNETs need to be created prior to run this code
        //----------------------------------------------------------------------------------------------------------------------

        // Subscription - Change SubId below
        const string subscriptionId = "<Subscription ID>";

        const string resourceGroupName = "<Resource Group Name>";
        const string location = "westus";
        const string subnetId = "<subnet ID>";
        const string anfAccountName = "anftestaccount";
        const string anfVolumeName = "anftestvolume";

        //Primary Capacity pool
        const string primaryCapacityPoolName = "anfprimarypool";
        const string primaryCapacityPoolServiceLevel = "Premium";

        //Secondary Capacity pool
        const string secondaryCapacityPoolName = "anfsecondarypool";
        const string SecondaryCapacityPoolServiceLevel = "Standard";


        // Shared Capacity Pool Properties
        const long capacitypoolSize = 4398046511104;  // 4TiB which is minimum size
        const long volumeSize = 107374182400;  // 100GiB - volume minimum size

        // If resources should be cleaned up
        static readonly bool shouldCleanUp = false;

        private static ServiceClientCredentials Credentials { get; set; }

        /// <summary>
        /// Sample console application that changes a volume from one Capacity Pool to another in different service level tier
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
            DisplayConsoleAppHeader();
            try
            {
                CreateANFAsync().GetAwaiter().GetResult();
                WriteConsoleMessage("Sample application successfuly completed execution.");
            }
            catch (Exception ex)
            {
                WriteErrorMessage(ex.Message);
            }
        }

        private static async Task CreateANFAsync()
        {
            //----------------------------------------------------------------------------------------
            // Authenticating using service principal, refer to README.md file for requirement details
            //----------------------------------------------------------------------------------------
            WriteConsoleMessage("Authenticating...");
            Credentials = await ServicePrincipalAuth.GetServicePrincipalCredential("AZURE_AUTH_LOCATION");

            //------------------------------------------
            // Instantiating a new ANF management client
            //------------------------------------------
            WriteConsoleMessage("Instantiating a new Azure NetApp Files management client...");
            AzureNetAppFilesManagementClient anfClient = new AzureNetAppFilesManagementClient(Credentials)
            {
                SubscriptionId = subscriptionId
            };
            WriteConsoleMessage($"\tApi Version: {anfClient.ApiVersion}");

            //----------------------
            // Creating ANF Account
            //----------------------            
            WriteConsoleMessage($"Requesting ANF Account to be created in {location}");
            var newAccount = await Creation.CreateOrUpdateANFAccountAsync(anfClient, resourceGroupName, location, anfAccountName);
            WriteConsoleMessage($"\tAccount Resource Id: {newAccount.Id}");

            //----------------------
            // Creating ANF Primary Capacity Pool
            //----------------------            
            WriteConsoleMessage($"Requesting ANF Primary Capacity Pool to be created in {location}");
            var newPrimaryPool = await Creation.CreateOrUpdateANFCapacityPoolAsync(anfClient, resourceGroupName, location, anfAccountName, primaryCapacityPoolName, capacitypoolSize, primaryCapacityPoolServiceLevel);
            WriteConsoleMessage($"\tAccount Resource Id: {newPrimaryPool.Id}");

            //----------------------
            // Creating ANF Secondary Capacity Pool
            //----------------------            
            WriteConsoleMessage($"Requesting ANF Secondary Capacity Pool to be created in {location}");
            var newSecondaryPool = await Creation.CreateOrUpdateANFCapacityPoolAsync(anfClient, resourceGroupName, location, anfAccountName, secondaryCapacityPoolName, capacitypoolSize, SecondaryCapacityPoolServiceLevel);
            WriteConsoleMessage($"\tAccount Resource Id: {newSecondaryPool.Id}");

            //----------------------
            // Creating ANF Volume 
            //----------------------            
            WriteConsoleMessage($"Requesting ANF Volume to be created in {location}");
            var newVolume = await Creation.CreateOrUpdateANFVolumeAsync(anfClient, resourceGroupName, location, anfAccountName, primaryCapacityPoolName, primaryCapacityPoolServiceLevel, anfVolumeName, subnetId, volumeSize);
            WriteConsoleMessage($"\tAccount Resource Id: {newVolume.Id}");

            WriteConsoleMessage($"Waiting for {newVolume.Id} to be available...");
            await ResourceUriUtils.WaitForAnfResource<Volume>(anfClient, newVolume.Id);

            //----------------------------------
            // Change ANF Volume's Capacity Pool 
            //----------------------------------            
            WriteConsoleMessage("Performing Pool Change. Updating volume...");
            await Update.ChangeVolumeCapacityPoolAsync(anfClient, resourceGroupName, anfAccountName, primaryCapacityPoolName, anfVolumeName, newSecondaryPool.Id);
            WriteConsoleMessage($"\tPool change is successful. Moved volume from {primaryCapacityPoolName} to {secondaryCapacityPoolName}");

            //--------------------
            // Clean Up Resources
            //--------------------

            if (shouldCleanUp)
            {
                WriteConsoleMessage("-------------------------");
                WriteConsoleMessage("Cleaning up ANF resources");
                WriteConsoleMessage("-------------------------");

                // Deleting Volume in the secondary pool first
                WriteConsoleMessage("Deleting Volume in the secondary Pool...");
                var volume = await anfClient.Volumes.GetAsync(resourceGroupName, anfAccountName, secondaryCapacityPoolName, anfVolumeName);
                await Deletion.DeleteANFVolumeAsync(anfClient, resourceGroupName, anfAccountName, secondaryCapacityPoolName, anfVolumeName);
                await ResourceUriUtils.WaitForNoAnfResource<Volume>(anfClient, volume.Id);

                // Deleting Primary Pool
                WriteConsoleMessage("Deleting primary Pool...");
                await Deletion.DeleteANFCapacityPoolAsync(anfClient, resourceGroupName, anfAccountName, primaryCapacityPoolName);
                await ResourceUriUtils.WaitForNoAnfResource<CapacityPool>(anfClient, newPrimaryPool.Id);

                // Deleting Secondary pool
                WriteConsoleMessage("Deleting secondary Pool...");
                await Deletion.DeleteANFCapacityPoolAsync(anfClient, resourceGroupName, anfAccountName, secondaryCapacityPoolName);
                await ResourceUriUtils.WaitForNoAnfResource<CapacityPool>(anfClient, newSecondaryPool.Id);

                //Deleting Account
                WriteConsoleMessage("Deleting Account ...");
                await Deletion.DeleteANFAccountAsync(anfClient, resourceGroupName, anfAccountName);
                await ResourceUriUtils.WaitForNoAnfResource<NetAppAccount>(anfClient, newAccount.Id);
            }
        }
    }
}
