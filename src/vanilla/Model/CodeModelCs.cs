// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using AutoRest.Core.Model;
using AutoRest.Core.Utilities;
using AutoRest.Core.Utilities.Collections;
using AutoRest.Extensions;
using Newtonsoft.Json;
using static AutoRest.Core.Utilities.DependencyInjection;

namespace AutoRest.CSharp.Model
{
    public class MainClientType : CodeModel
    {
        public string MainClientName { get; set; }
        public IList<string> ClientNames { get; set; }
    }

    public class CodeModelCs : CodeModel
    {
        [JsonIgnore]
        public IEnumerable<MethodGroupCs> AllOperations => Operations.Where( operation => !operation.Name.IsNullOrEmpty()).Cast<MethodGroupCs>();

        public bool IsCustomBaseUri => Extensions.ContainsKey(SwaggerExtensions.ParameterizedHostExtension);

        public virtual bool HaveModelNamespace => Enumerable.Empty<ModelType>()
            .Concat(ModelTypes)
            .Concat(HeaderTypes)
            .Concat(EnumTypes)
            .Any(m => !m.Extensions.ContainsKey(SwaggerExtensions.ExternalExtension));

        public virtual IEnumerable<string> Usings
        {
            get
            {
                if (HaveModelNamespace)
                {
                    yield return ModelsName;
                }
            }
        }

        [JsonIgnore]
        public bool ContainsCredentials => Properties.Any(p => p.ModelType.IsPrimaryType(KnownPrimaryType.Credentials));

        [JsonIgnore]
        public string ConstructorVisibility
            => Singleton<GeneratorSettingsCs>.Instance.InternalConstructors ? "internal" : "public";

        [JsonIgnore]
        public string RequiredConstructorParameters
        {
            get
            {
                var requireParams = new List<string>();
                Properties.Where(p => p.IsRequired && p.IsReadOnly)
                    .ForEach(p => requireParams.Add(string.Format(CultureInfo.InvariantCulture, 
                        "{0} {1}", 
                        p.ModelType.Name, 
                        p.Name.ToCamelCase())));
                return string.Join(", ", requireParams);
            }
        }

        [JsonIgnore]
        public bool NeedsTransformationConverter => ModelTypes.Any(m => m.Properties.Any(p => p.WasFlattened()));

        /// <summary>
        /// Returns the list of names that this element is reserving
        /// (most of the time, this is just 'this.Name' )
        /// </summary>
        public override IEnumerable<string> MyReservedNames
            => base.MyReservedNames.ConcatSingleItem(Namespace.Else("").Substring(Namespace.Else("").LastIndexOf('.') + 1)).Where( each => !each.IsNullOrEmpty());

        public string ClientName { get; set; }

        public bool FilterMethodsByClientName { get; set; } = true;

        public IEnumerable<Method> FilteredMethods => GetFilteredMethods(ClientName);

        public IEnumerable<Method> GetFilteredMethods(string clientName)
        {
            var nonDeprecatedMethodsByClientName = Methods
                .Select(m => (MethodCs)m)
                .Where(m =>
            {
                var matchesClientName = !FilterMethodsByClientName || (m.ClientName + "Client").Equals(clientName);
                return !m.Deprecated && matchesClientName;
            }).ToList();
            var versionedRoutes = nonDeprecatedMethodsByClientName.Where(m => Regex.IsMatch(m.Url, "^/v([0-9]+.[0-9]+)")).ToList();
            if (versionedRoutes.Any())
            {
                return versionedRoutes;
            }
            return nonDeprecatedMethodsByClientName.Where(m => !m.Name.Equals("GetVersions", StringComparison.OrdinalIgnoreCase));

        }
    }
}