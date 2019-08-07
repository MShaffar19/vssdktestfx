﻿namespace Microsoft.VisualStudio.Sdk.TestFramework
{
    using System;
    using System.Runtime.InteropServices;
    using System.Threading.Tasks;
    using Microsoft.VisualStudio.Shell;
    using Microsoft.VisualStudio.Shell.Interop;
    using Task = System.Threading.Tasks.Task;

    /// <summary>
    /// An implementation of <see cref="IAsyncServiceProvider2"/>
    /// that simply returns services from <see cref="OLE.Interop.IServiceProvider"/>.
    /// </summary>
    internal class MockAsyncServiceProvider : IAsyncServiceProvider2, Shell.Interop.IAsyncServiceProvider
    {
        private readonly OLE.Interop.IServiceProvider serviceProvider;
        private readonly IVsTaskSchedulerService taskSchedulerService;

        /// <summary>
        /// Initializes a new instance of the <see cref="MockAsyncServiceProvider"/> class.
        /// </summary>
        /// <param name="serviceProvider">The root of all services.</param>
        internal MockAsyncServiceProvider(OLE.Interop.IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));

            this.taskSchedulerService = (IVsTaskSchedulerService)serviceProvider.QueryService(typeof(SVsTaskSchedulerService).GUID);
            Assumes.Present(this.taskSchedulerService);
        }

        /// <inheritdoc/>
        public Task<object> GetServiceAsync(Type serviceType, bool swallowExceptions)
        {
            return this.GetServiceAsync(serviceType);
        }

        /// <inheritdoc/>
        public Task<object> GetServiceAsync(Type serviceType)
        {
            Requires.NotNull(serviceType, nameof(serviceType));
            return Task.FromResult(this.serviceProvider.QueryService(serviceType.GUID));
        }

        /// <inheritdoc />
        public IVsTask QueryServiceAsync(ref Guid guidService)
        {
            var completionSource = this.taskSchedulerService.CreateTaskCompletionSource();
            try
            {
                completionSource.SetResult(this.serviceProvider.QueryService(guidService));
            }
            catch (Exception ex)
            {
                completionSource.SetFaulted(Marshal.GetHRForException(ex));
            }

            return completionSource.Task;
        }
    }
}