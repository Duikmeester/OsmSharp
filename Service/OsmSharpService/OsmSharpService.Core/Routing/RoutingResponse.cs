﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OsmSharp.Routing.Core.Route;

namespace OsmSharpService.Core.Routing
{
    /// <summary>
    /// A generic response for a routing request.
    /// </summary>
    public class RoutingResponse
    {
        /// <summary>
        /// Holds the routing status.
        /// </summary>
        public RoutingResponseStatus Status { get; set; }

        /// <summary>
        /// Holds a message with more details about the status.
        /// </summary>
        public string StatusMessage { get; set; }

        /// <summary>
        /// Holds the calculated weights.
        /// </summary>
        public double[][] Weights { get; set; }

        /// <summary>
        /// The resulting route.
        /// </summary>
        public OsmSharpRoute Route { get; set; }

        /// <summary>
        /// Returns all hooks that have not been routed.
        /// </summary>
        public RoutingHook[] UnroutableHooks { get; set; }

        /// <summary>
        /// Returns a description for this response.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return string.Format("Response: {0}:{1}", this.Status.ToString(), this.StatusMessage);
        }
    }

    /// <summary>
    /// An enumeration representing status.
    /// </summary>
    public enum RoutingResponseStatus
    {
        /// <summary>
        /// The routing request failed.
        /// </summary>
        Failed,
        /// <summary>
        /// The request was a succes!
        /// </summary>
        Success
    }
}
