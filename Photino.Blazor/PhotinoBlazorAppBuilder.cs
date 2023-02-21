using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Photino.Blazor
{
    public class PhotinoBlazorAppBuilder
    {
        internal PhotinoBlazorAppBuilder()
        {
            RootComponents = new RootComponentList();
            Services = new ServiceCollection();
        }

        public static PhotinoBlazorAppBuilder CreateDefault(string[] args = default)
        {
            // We don't use the args for anything right now, but we want to accept them
            // here so that it shows up this way in the project templates.
            // var jsRuntime = DefaultWebAssemblyJSRuntime.Instance;
            var builder = new PhotinoBlazorAppBuilder();
            builder.Services.AddBlazorDesktop();

            // Right now we don't have conventions or behaviors that are specific to this method
            // however, making this the default for the template allows us to add things like that
            // in the future, while giving `new BlazorDesktopHostBuilder` as an opt-out of opinionated
            // settings.
            return builder;
        }

        public RootComponentList RootComponents { get; }

        public IServiceCollection Services { get; }

        public PhotinoBlazorApp Build(Action<IServiceProvider> serviceProviderOptions = null)
        {
            // register root components with DI container
            // Services.AddSingleton(RootComponents);

            var sp = Services.BuildServiceProvider();
            var app = sp.GetRequiredService<PhotinoBlazorApp>();

            serviceProviderOptions?.Invoke(sp);

            app.Initialize(sp, RootComponents);
            return app;
        }
    }

    public class RootComponentList : IEnumerable<(Type, string)>
    {
        private readonly List<(Type componentType, string domElementSelector)> components = new List<(Type componentType, string domElementSelector)>();

        public void Add<TComponent>(string selector) where TComponent : IComponent
        {
            components.Add((typeof(TComponent), selector));
        }

        public IEnumerator<(Type, string)> GetEnumerator()
        {
            return components.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return components.GetEnumerator();
        }
    }
}
