// <copyright file="ValidatePostcode.cs" company="">
// Copyright (c) 2019 All Rights Reserved
// </copyright>
// <author></author>
// <date>1/16/2019 3:13:11 PM</date>
// <summary>Implements the ValidatePostcode Plugin.</summary>
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.1
// </auto-generated>

using System;
using System.ServiceModel;
using Microsoft.Xrm.Sdk;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;

namespace CRMTraining2.Plugins
{

    /// <summary>
    /// ValidatePostcode Plugin.
    /// </summary>    
    public class ValidatePostcode: PluginBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ValidatePostcode"/> class.
        /// </summary>
        /// <param name="unsecure">Contains public (unsecured) configuration information.</param>
        /// <param name="secure">Contains non-public (secured) configuration information. 
        /// When using Microsoft Dynamics 365 for Outlook with Offline Access, 
        /// the secure string is not passed to a plug-in that executes while the client is offline.</param>
        public ValidatePostcode(string unsecure, string secure)
            : base(typeof(ValidatePostcode))
        {
            
           // TODO: Implement your custom configuration handling.
        }


        /// <summary>
        /// Main entry point for he business logic that the plug-in is to execute.
        /// </summary>
        /// <param name="localContext">The <see cref="LocalPluginContext"/> which contains the
        /// <see cref="IPluginExecutionContext"/>,
        /// <see cref="IOrganizationService"/>
        /// and <see cref="ITracingService"/>
        /// </param>
        /// <remarks>
        /// For improved performance, Microsoft Dynamics 365 caches plug-in instances.
        /// The plug-in's Execute method should be written to be stateless as the constructor
        /// is not called for every invocation of the plug-in. Also, multiple system threads
        /// could execute the plug-in at the same time. All per invocation state information
        /// is stored in the context. This means that you should not use global variables in plug-ins.
        /// </remarks>
        protected override void ExecuteCrmPlugin(LocalPluginContext localContext)
        {
            var trace = localContext.TracingService;
            trace.Trace("Starting Validate postcode plugin");
            if (localContext == null)
            {
                throw new InvalidPluginExecutionException("localContext");
            }

            if (localContext.PluginExecutionContext.InputParameters.ContainsKey("Target") &&
                localContext.PluginExecutionContext.InputParameters["Target"] is Entity)
            {
                trace.Trace("Target is preset and an entity");
                var contact = (Entity) localContext.PluginExecutionContext.InputParameters["Target"];

                if (contact.LogicalName == "contact")
                {
                    trace.Trace("Target is a contact");

                    if (contact.Attributes.ContainsKey("address1_postalcode"))
                    {
                        var postcode = contact.GetAttributeValue<string>("address1_postalcode");
                        trace.Trace($"Found postcode value {postcode}");

                        if (!IsValidPostcode(postcode, s => trace.Trace(s)))
                        {
                            trace.Trace($"Invalid postcode: {postcode}");
                            throw new Exception("This is not a valid postcode");
                        }
                    }
                }
            }

        }

        public bool IsValidPostcode(string postcode,  Action<string> tracer)
        {
            var httpClient = System.Net.WebRequest.Create($"https://api.postcodes.io/postcodes/{postcode}");
            tracer($"Lookup postcode");

            try
            {
                var response = httpClient.GetResponse();
                tracer($"Response was {response}");
            }
            catch (WebException e)
            {
                tracer($"Exception thrown: {e.Message}");
                // Lookup failed - bad postcode
                return false;
            }

            return true;


        }
    }
}
