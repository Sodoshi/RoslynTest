using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security;
using System.Security.Policy;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using RoslynDomain;
namespace RoslynCompiler
{
    public class Compiler : IDisposable
    {
        private const LanguageVersion _languageVersion = LanguageVersion.CSharp7_2;
        private static int _codeId;
        private static readonly IEnumerable<string> DefaultNamespaces =
            new[]
            {
                "System",
                "System.IO",
                "System.Net",
                "System.Linq",
                "System.Text",
                "System.Text.RegularExpressions",
                "System.Collections.Generic",
                "RoslynDomain"
            };
        private static readonly IEnumerable<string> ExcludedNamespaces =
            new[]
            {
                "Microsoft.CodeAnalysis",
                "Microsoft.CodeAnalysis.CSharp",
                "System.Collections.Immutable"
            };
        private static readonly AssemblyHash HashAlgorithm = new AssemblyHash();
        private static readonly CSharpCompilationOptions DefaultCompilationOptions =
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
                .WithOverflowChecks(true).WithOptimizationLevel(OptimizationLevel.Release)
                .WithUsings(DefaultNamespaces);
        private readonly IEnumerable<MetadataReference> _defaultReferences;
        private readonly string _localPath =
            Path.GetDirectoryName(new Uri(Assembly.GetExecutingAssembly().CodeBase).LocalPath);
        private AppDomain _appDomain;
        private AppDomainProxy _appDomainProxy;
        public Compiler()
        {
            var domain = AppDomain.CurrentDomain;
            var assemblies = domain.GetAssemblies();
            var assemblyNames = Assembly.GetExecutingAssembly().GetReferencedAssemblies();
            _defaultReferences = (from assemblyName in assemblyNames
                where !ExcludedNamespaces.Contains(assemblyName.Name)
                select assemblies.FirstOrDefault(a => a.FullName == assemblyName.FullName)
                into assembly
                select GetAssemblyBytes(assembly)).ToArray();
            ResetAppDomain();
        }
        public void Dispose()
        {
            ReleaseUnmanagedResources();
            GC.SuppressFinalize(this);
        }
        public void Compile(string code)
        {
            Compile(code, true);
        }
        private void Compile(string code, bool loadAssembly)
        {
            var parsedSyntaxTree = CSharpSyntaxTree.ParseText(
                code, CSharpParseOptions.Default.WithLanguageVersion(_languageVersion));
            var compilation
                = CSharpCompilation.Create($"CustomCode{_codeId}", new[] {parsedSyntaxTree}, _defaultReferences,
                    DefaultCompilationOptions);
            try
            {
                using (var dllStream = new MemoryStream())
                {
                    var result = compilation.Emit(dllStream);
                    if (!result.Success && loadAssembly)
                    {
                        foreach (var error in result.Diagnostics)
                        {
                            Console.WriteLine(error.ToString());
                        }
                        return;
                    }
                    if (loadAssembly)
                    {
                        _appDomainProxy.LoadAssembly(dllStream.ToArray());
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            _codeId++;
        }
        public string Execute(string nameSpace, ref ICommand command)
        {
            return _appDomainProxy.ExecuteCode(nameSpace, ref command);
        }
        ~Compiler()
        {
            ReleaseUnmanagedResources();
        }
        private static MetadataReference GetAssemblyBytes(Assembly assembly)
        {
            if (assembly == null)
            {
                return null;
            }
            return MetadataReference.CreateFromImage(new Hash(assembly).GenerateHash(HashAlgorithm));
        }
        private static PermissionSet GetPermissionSet()
        {
            var ev = new Evidence();
            ev.AddHostEvidence(new Zone(SecurityZone.MyComputer));
            return SecurityManager.GetStandardSandbox(ev);
        }
        private void ReleaseUnmanagedResources()
        {
            _appDomainProxy?.Dispose();
            if (_appDomain == null)
            {
                return;
            }
            try
            {
                AppDomain.Unload(_appDomain);
            }
            catch
            {
                // Domain already unloaded
            }
        }
        private void ResetAppDomain()
        {
            if (_appDomain != null)
            {
                AppDomain.Unload(_appDomain);
            }
            _appDomain = AppDomain.CreateDomain(Guid.NewGuid().ToString(), null,
                new AppDomainSetup {ApplicationBase = _localPath}, GetPermissionSet());
            _appDomainProxy = (AppDomainProxy) _appDomain.CreateInstanceAndUnwrap(
                typeof(AppDomainProxy).Assembly.FullName,
                typeof(AppDomainProxy).FullName ?? throw new InvalidOperationException());
            Compile(string.Empty, false);
        }
    }
}