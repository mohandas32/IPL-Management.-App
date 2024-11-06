using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Crm.Sdk;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

namespace IPL_Mng.Plugin
{

    public abstract class PluginBase : IPlugin
    {
        /*
       * **************************
       * IMPLEMENTED IN PLUGIN-BASE
       * **************************
       * Main Logging is already handled
       * Main Tracing is already handled 
       * Try-Catch already exists
       */

        /*
        * *****************************
        * WHAT YOU GET FROM PLUGIN-BASE
        * *****************************
        * Context       = whole context for checking depth
        * Target        = Entity
        * DeletionTarget= EntityReference
        * Context       = Plugin Context
        * Service       = Organization Service
        * Tracer.Write  = Tracing the plugin execution
        * PreImage      = Provides first pre-image entity
        * PostImage     = Provides first post-image entity
        */


        public void Execute(IServiceProvider serviceProvider)
        {
            var watch = Stopwatch.StartNew();
            var context = new ContextBase(serviceProvider);

            try
            {
                Execute(context);
            }
            catch (Exception ex)
            {
                context.Trace(ex.Message, ex);
                throw;
            }
            finally
            {
                watch.Stop();
                context.Trace("Internal execution time: {0} ms", watch.ElapsedMilliseconds);
            }
        }

        public abstract void Execute(ContextBase context);
    }
}