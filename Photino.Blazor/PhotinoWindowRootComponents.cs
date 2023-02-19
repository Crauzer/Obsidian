﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace Photino.Blazor
{
    /// <summary>
    /// Configures root components for a <see cref="BlazorWindow"/>.
    /// </summary>
    public sealed class BlazorWindowRootComponents : IJSComponentConfiguration
    {
        private readonly PhotinoWebViewManager _manager;

        internal BlazorWindowRootComponents(PhotinoWebViewManager manager, JSComponentConfigurationStore jsComponents)
        {
            _manager = manager;
            JSComponents = jsComponents;
        }

        public JSComponentConfigurationStore JSComponents { get; }

        /// <summary>
        /// Adds a root component to the window.
        /// </summary>
        /// <typeparam name="TComponent">The component type.</typeparam>
        /// <param name="selector">A CSS selector describing where the component should be added in the host page.</param>
        /// <param name="parameters">An optional dictionary of parameters to pass to the component.</param>
        public void Add(Type typeComponent, string selector, IDictionary<string, object> parameters = null) 
        {
            var parameterView = parameters == null
                ? ParameterView.Empty
                : ParameterView.FromDictionary(parameters);

            // Dispatch because this is going to be async, and we want to catch any errors
            _ = _manager.Dispatcher.InvokeAsync(async () =>
            {
                await _manager.AddRootComponentAsync(typeComponent, selector, parameterView);
            });
        }
    }
}
