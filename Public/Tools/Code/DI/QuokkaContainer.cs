using Autofac;
using Autofac.Features.ResolveAnything;
using Quokka.Public.Content;
using Quokka.Public.Logging;
using Quokka.Public.Profiling;
using Quokka.Public.Transformation;
using System;

namespace Quokka.Public.Tools
{
    public class QuokkaContainerScopeFactory
    {
        private ILifetimeScope _scope { get; set; }

        public QuokkaContainerScopeFactory(ILifetimeScope scope)
        {
            _scope = scope;
        }

        public QuokkaContainer CreateScope()
        {
            return new QuokkaContainer(_scope.BeginLifetimeScope());
        }
    }

    public class ClassFactory
    {
        private ILifetimeScope _scope { get; set; }

        public ClassFactory(ILifetimeScope scope)
        {
            _scope = scope;
        }

        public T Create<T>()
        {
            return _scope.Resolve<T>();
        }
    }

    public class QuokkaContainer : IDisposable 
    {
        private readonly ContainerBuilder _builder = new ContainerBuilder();
        private ILifetimeScope _container;

        public QuokkaContainer()
        {
            _builder = new ContainerBuilder();

            _builder.RegisterSource(new AnyConcreteTypeNotAlreadyRegisteredSource());

            WithContentDomain(eContentDomain.Public);

            WithLogStream<ConsoleLogStream>();

            WithLicensingOptions<DemoLicensingOptions>();

            WithRuntimeConfiguration(new RuntimeConfiguration());

            _builder.RegisterType<QuokkaContainerScopeFactory>().InstancePerLifetimeScope();
            _builder.RegisterType<ClassFactory>().InstancePerDependency();

            _builder
                .RegisterType<VirtualFS>()
                .InstancePerLifetimeScope();

            _builder
                .RegisterType<ProfilerFactory>()
                .As<IProfilerFactory>()
                .InstancePerLifetimeScope();
        }

        internal QuokkaContainer(ILifetimeScope _scope)
        {
            _container = _scope;
        }

        private void EnsureNotInitialized()
        {
            if( _container != null)
            {
                throw new InvalidOperationException("Container already initialized");
            }
        }

        public QuokkaContainer WithContentDomain(eContentDomain contentDomain)
        {
            EnsureNotInitialized();

            _builder
                .RegisterInstance(new ContentDomainProvider(contentDomain))
                .AsImplementedInterfaces()
                .SingleInstance();

            return this;
        }

        public QuokkaContainer WithRuntimeConfiguration(RuntimeConfiguration runtimeConfiguration)
        {
            EnsureNotInitialized();

            _builder
                .RegisterInstance(runtimeConfiguration)
                .SingleInstance();

            return this;
        }

        public QuokkaContainer WithLogStream<T>() where T : ILogStream
        {
            EnsureNotInitialized();

            _builder
                .RegisterType<T>().As<ILogStream>()
                .InstancePerLifetimeScope();

            return this;
        }

        public QuokkaContainer WithLicensingOptions<T>() where T : ILicensingOptions
        {
            EnsureNotInitialized();

            _builder
                .RegisterType<T>().As<ILicensingOptions>()
                .SingleInstance();

            return this;
        }

        public QuokkaContainer WithProjectTransformation<T>() where T : IQuokkaProjectTransformation
        {
            EnsureNotInitialized();

            _builder
                .RegisterType<T>()
                .As<IQuokkaProjectTransformation>()
                .InstancePerDependency();

            return this;
        }

        public T Resolve<T>()
        {
            if( _container == null )
                _container = _builder.Build();

            return _container.Resolve<T>();
        }

        #region IDisposable Support
        private bool disposedValue = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    _container.Dispose();
                }

                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }
        #endregion
    }
}
