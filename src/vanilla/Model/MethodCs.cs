// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using AutoRest.Core.Model;
using AutoRest.Core.Utilities;
using AutoRest.Extensions;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Newtonsoft.Json;

namespace AutoRest.CSharp.Model
{
    public class MethodCs : Method
    {
        public MethodCs() { }
        
        public bool IsCustomBaseUri
            => CodeModel.Extensions.ContainsKey(SwaggerExtensions.ParameterizedHostExtension);

        public SyncMethodsGenerationMode SyncMethods { get; set; }

        public bool ExcludeFromInterface { get; set; }

        /// <summary>
        /// Get the predicate to determine of the http operation status code indicates failure
        /// </summary>
        public string FailureStatusCodePredicate
        {
            get
            {
                if (Responses.Any())
                {
                    List<string> predicates = new List<string>();
                    foreach (var responseStatus in Responses.Keys)
                    {
                        predicates.Add(string.Format(CultureInfo.InvariantCulture,
                            "(int)_statusCode != {0}", GetStatusCodeReference(responseStatus)));
                    }

                    return string.Join(" && ", predicates);
                }
                return "!_httpResponse.IsSuccessStatusCode";
            }
        }

        /// <summary>
        /// Generate the method parameter declaration for sync methods and extensions
        /// </summary>
        /// <param name="addCustomHeaderParameters">If true add the customHeader to the parameters</param>
        /// <returns>Generated string of parameters</returns>
        public virtual string GetSyncMethodParameterDeclaration(bool addCustomHeaderParameters, bool addCancellationToken)
        {
            List<string> declarations = new List<string>();
            foreach (var parameter in LocalParameters)
            {
                string format = (parameter.IsRequired ? "{0} {1}" : "{0} {1} = {2}");
                declarations.Add(string.Format(CultureInfo.InvariantCulture,
                    format, parameter.ModelTypeName, parameter.Name, parameter.ActualDefaultValue));
            }

            if (addCustomHeaderParameters)
            {
                declarations.Add("System.Collections.Generic.Dictionary<string, System.Collections.Generic.List<string>> customHeaders = null");
            }

            if (addCancellationToken)
            {
                declarations.Add("CancellationToken cancellationToken = default");
            }

            return string.Join(", ", declarations);
        }

        /// <summary>
        /// Generate the method parameter declaration for async methods and extensions
        /// </summary>
        /// <param name="addCustomHeaderParameters">If true add the customHeader to the parameters</param>
        /// <returns>Generated string of parameters</returns>
        public virtual string GetAsyncMethodParameterDeclaration(bool addCustomHeaderParameters, bool addCancellationToken)
        {
            var declarations = this.GetSyncMethodParameterDeclaration(addCustomHeaderParameters, addCancellationToken);

            return declarations;
        }

        /// <summary>
        /// Arguments for invoking the method from a synchronous extension method
        /// </summary>
        public string SyncMethodInvocationArgs => string.Join(", ", LocalParameters.Select(each => each.Name));

        /// <summary>
        /// Get the invocation args for an invocation with an async method
        /// </summary>
        public string GetAsyncMethodInvocationArgs() => 
            string.Join(", ", LocalParameters.Select(each => (string)each.Name));

        /// <summary>
        /// Get the parameters that are actually method parameters in the order they appear in the method signature
        /// exclude global parameters
        /// </summary>
        [JsonIgnore]
        public IEnumerable<ParameterCs> LocalParameters
        {
            get
            {
                return
                    Parameters.Where(parameter =>
                        parameter != null &&
                        !parameter.IsClientProperty &&
                        !string.IsNullOrWhiteSpace(parameter.Name) &&
                        !parameter.IsConstant)
                        .OrderBy(item => !item.IsRequired).Cast<ParameterCs>();
            }
        }

        /// <summary>
        /// Get the return type name for the underlying interface method
        /// </summary>
        public virtual string OperationResponseReturnTypeString => $"Result<{ResponseReturnTypeString}, ErrorResult>";

        private string ResponseReturnTypeString
        {
            get
            {
                // autorest treats all documented return types as successful (even BadRequest and InternalServerError), so we'll just fix that
                var successResponse = Responses[HttpStatusCode.OK];
                if (successResponse != null)
                {
                    if (successResponse.Body != null && successResponse.Headers != null)
                    {
                        return $"{successResponse.Body.AsNullableType(HttpMethod != HttpMethod.Head && IsXNullableReturnType)},{successResponse.Headers.AsNullableType(HttpMethod != HttpMethod.Head)}";
                    }
                    if (successResponse.Body != null)
                    {
                        return successResponse.Body.AsNullableType(HttpMethod != HttpMethod.Head && IsXNullableReturnType);
                    }
                    if (successResponse.Headers != null)
                    {
                        return successResponse.Headers.AsNullableType(HttpMethod != HttpMethod.Head);
                    }
                }

                return "Ok";
            }
        }

        /// <summary>
        /// Get the return type for the async extension method
        /// </summary>
        public virtual string TaskExtensionReturnTypeString
        {
            get
            {
                if (ReturnType.Body != null)
                {
                    return string.Format(CultureInfo.InvariantCulture,
                        "System.Threading.Tasks.Task<{0}>", ReturnType.Body.AsNullableType(HttpMethod != HttpMethod.Head && IsXNullableReturnType));
                }
                else if (ReturnType.Headers != null)
                {
                    return string.Format(CultureInfo.InvariantCulture,
                        "System.Threading.Tasks.Task<{0}>", ReturnType.Headers.AsNullableType(HttpMethod != HttpMethod.Head));
                }
                else
                {
                    return "System.Threading.Tasks.Task";
                }
            }
        }

        /// <summary>
        /// Get the type for operation exception
        /// </summary>
        public virtual string OperationExceptionTypeString
        {
            get
            {
                if (this.DefaultResponse.Body is CompositeType)
                {
                    CompositeType type = this.DefaultResponse.Body as CompositeType;
                    if (type.Extensions.ContainsKey(SwaggerExtensions.NameOverrideExtension))
                    {
                        var ext = type.Extensions[SwaggerExtensions.NameOverrideExtension] as Newtonsoft.Json.Linq.JContainer;
                        if (ext != null && ext["name"] != null)
                        {
                            return ext["name"].ToString();
                        }
                    }
                    return type.Name + "Exception";
                }
                else
                {
                    return "Microsoft.Rest.HttpOperationException";
                }
            }
        }

        /// <summary>
        /// Get the expression for exception initialization with message.
        /// </summary>
        public virtual string InitializeExceptionWithMessage => string.Empty;

        /// <summary>
        /// Get the expression for exception initialization with message.
        /// </summary>
        public virtual string InitializeException => string.Empty;

        /// <summary>
        /// Gets the expression for response body initialization.
        /// </summary>
        public virtual string InitializeResponseBody => string.Empty;

        /// <summary>
        /// Gets the expression for default header setting.
        /// </summary>
        public virtual string SetDefaultHeaders => string.Empty;

        /// <summary>
        /// Get the type name for the method's return type
        /// </summary>
        public virtual string ReturnTypeString
        {
            get
            {
                // autorest treats all documented return types as successful (even BadRequest and InternalServerError), so we'll just fix that
                var successResponse = Responses[HttpStatusCode.OK];
                if (successResponse != null)
                {
                    if (successResponse.Body != null)
                    {
                        return successResponse.Body.AsNullableType(HttpMethod != HttpMethod.Head && IsXNullableReturnType);
                    }
                    if (successResponse.Headers != null)
                    {
                        return successResponse.Headers.AsNullableType(HttpMethod != HttpMethod.Head);
                    }
                }

                return "Ok";
            }
        }

        /// <summary>
        /// Get the method's request body (or null if there is no request body)
        /// </summary>
        [JsonIgnore]
        public ParameterCs RequestBody => Body as ParameterCs;

        [JsonIgnore]
        public string AccessModifier => Hidden ? "internal" : "public";

        /// <summary>
        /// Generate a reference to the ServiceClient
        /// </summary>
        [JsonIgnore]
        public string ClientReference => Group.IsNullOrEmpty() ? "this" : "this.Client";

        /// <summary>
        /// Returns serialization settings reference.
        /// </summary>
        /// <param name="serializationType"></param>
        /// <returns></returns>
        public string GetSerializationSettingsReference(IModelType serializationType)
        {
            if (serializationType.IsOrContainsPrimaryType(KnownPrimaryType.Date))
            {
                return "new Microsoft.Rest.Serialization.DateJsonConverter()";
            }
            else if (serializationType.IsOrContainsPrimaryType(KnownPrimaryType.DateTimeRfc1123))
            {
                return "new Microsoft.Rest.Serialization.DateTimeRfc1123JsonConverter()";
            }
            else if (serializationType.IsOrContainsPrimaryType(KnownPrimaryType.Base64Url))
            {
                return "new Microsoft.Rest.Serialization.Base64UrlJsonConverter()";
            }
            else if (serializationType.IsOrContainsPrimaryType(KnownPrimaryType.UnixTime))
            {
                return "new Microsoft.Rest.Serialization.UnixTimeJsonConverter()";
            }
            return ClientReference + ".SerializationSettings";
        }

        /// <summary>
        /// Returns deserialization settings reference.
        /// </summary>
        /// <param name="deserializationType"></param>
        /// <returns></returns>
        public string GetDeserializationSettingsReference(IModelType deserializationType)
        {
            if (deserializationType.IsOrContainsPrimaryType(KnownPrimaryType.Date))
            {
                return "new Microsoft.Rest.Serialization.DateJsonConverter()";
            }
            else if (deserializationType.IsOrContainsPrimaryType(KnownPrimaryType.Base64Url))
            {
                return "new Microsoft.Rest.Serialization.Base64UrlJsonConverter()";
            }
            else if (deserializationType.IsOrContainsPrimaryType(KnownPrimaryType.UnixTime))
            {
                return "new Microsoft.Rest.Serialization.UnixTimeJsonConverter()";
            }
            return ClientReference + ".DeserializationSettings";
        }

        public string GetExtensionParameters(string methodParameters)
        {
            string operationsParameter = "this I" + MethodGroup.TypeName + " operations";
            return string.IsNullOrWhiteSpace(methodParameters)
                ? operationsParameter
                : operationsParameter + ", " + methodParameters;
        }

        public static string GetStatusCodeReference(HttpStatusCode code)
        {
            return ((int)code).ToString(CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Generates a code fragment like `.ProfileOperations.Create` or `.GetOperations`
        /// representing how to reach this method given a client instance.
        /// </summary>
        public virtual string MethodReference
            => $"{ClientReference}{(MethodGroup?.Name.IsNullOrEmpty() != false ? "" : "." + MethodGroup.NameForProperty)}.{Name}";

        /// <summary>
        /// Generate code to build the URL from a url expression and method parameters
        /// </summary>
        /// <param name="variableName">The variable to store the url in.</param>
        /// <returns></returns>
        public virtual string BuildUrl(string variableName)
        {
            var builder = new IndentedStringBuilder();

            foreach (var pathParameter in this.LogicalParameters.Where(p => p.Location == ParameterLocation.Path))
            {
                string replaceString = "requestUri.ReplaceUrlSegment(\"{{{1}}}\", {2});";
                var urlPathName = pathParameter.SerializedName;
                if (pathParameter.ModelType is SequenceType)
                {
                    builder.AppendLine(replaceString,
                    variableName,
                    urlPathName,
                    pathParameter.GetFormattedReferenceValue(ClientReference));
                }
                else
                {
                    builder.AppendLine(replaceString,
                    variableName,
                    urlPathName,
                    pathParameter.Name);
                }
            }
            if (this.LogicalParameters.Any(p => p.Location == ParameterLocation.Query))
            {
                foreach (var queryParameter in this.LogicalParameters.Where(p => p.Location == ParameterLocation.Query))
                {
                    var replaceString = "requestUri.AddQueryParameter(\"{0}\", {1});";

                    if (queryParameter.CollectionFormat == CollectionFormat.Multi)
                    {
                        builder.AppendLine(replaceString,
                            queryParameter.SerializedName, queryParameter.Name);
                    }
                    else
                    {
                        builder.AppendLine(replaceString,
                                queryParameter.SerializedName, queryParameter.GetFormattedReferenceValue(ClientReference));
                    }
                }
            }

            return builder.ToString();
        }

        /// <summary>
        /// Generates input mapping code block.
        /// </summary>
        /// <returns></returns>
        public virtual string BuildInputMappings()
        {
            var builder = new IndentedStringBuilder();
            foreach (var transformation in InputParameterTransformation)
            {
                var compositeOutputParameter = transformation.OutputParameter.ModelType as CompositeType;
                if (transformation.OutputParameter.IsRequired && compositeOutputParameter != null)
                {
                    builder.AppendLine("{0} {1} = new {0}();",
                        transformation.OutputParameter.ModelTypeName,
                        transformation.OutputParameter.Name);
                }
                else
                {
                    builder.AppendLine("{0} {1} = default({0});",
                        transformation.OutputParameter.ModelTypeName,
                        transformation.OutputParameter.Name);
                }
                var nullCheck = BuildNullCheckExpression(transformation);
                if (!string.IsNullOrEmpty(nullCheck))
                {
                    builder.AppendLine("if ({0})", nullCheck)
                       .AppendLine("{").Indent();
                }

                if (transformation.ParameterMappings.Any(m => !string.IsNullOrEmpty(m.OutputParameterProperty)) &&
                    compositeOutputParameter != null && !transformation.OutputParameter.IsRequired)
                {
                    builder.AppendLine("{0} = new {1}();",
                        transformation.OutputParameter.Name,
                        transformation.OutputParameter.ModelType.Name);
                }

                foreach (var mapping in transformation.ParameterMappings)
                {
                    builder.AppendLine("{0};", mapping.CreateCode(transformation.OutputParameter));
                }

                if (!string.IsNullOrEmpty(nullCheck))
                {
                    builder.Outdent()
                       .AppendLine("}");
                }
            }

            return builder.ToString();
        }

        private static string BuildNullCheckExpression(ParameterTransformation transformation)
        {
            if (transformation == null)
            {
                throw new ArgumentNullException("transformation");
            }

            return string.Join(" || ",
                transformation.ParameterMappings
                    .Where(m => m.InputParameter.IsNullable())
                    .Select(m => m.InputParameter.Name + " != null"));
        }

        public string GetHttpMethod(HttpMethod httpMethod)
        {
	        var genericType = string.Empty;
	        if (ReturnTypeString != "void")
	        {
		        genericType = $"<{ReturnTypeString}>";
            }

            var bodyParameter = LocalParameters.SingleOrDefault(p => p.Location == ParameterLocation.Body);

            switch (httpMethod)
	        {
		        case HttpMethod.Get: return $"GetAsync{genericType}";
                case HttpMethod.Post:
                    // if we have a post with nothing being passed in the body, we are going to pass null to PostAsync in GetPostParameter,
                    //   so the type needs to be specified
                    return bodyParameter == null ? $"PostAsync<string, {ResponseReturnTypeString}>" : $"PostAsync<{bodyParameter.ModelTypeName}, {ResponseReturnTypeString}>";
                case HttpMethod.Put:
                    // if we have a post with nothing being passed in the body, we are going to pass null to PutAsync in GetPostParameter,
                    //   so the type needs to be specified
                    return bodyParameter == null ? $"PutAsync<string, {ResponseReturnTypeString}>" : $"PutAsync<{bodyParameter.ModelTypeName}, {ResponseReturnTypeString}>";
		        case HttpMethod.Delete: return $"DeleteAsync{genericType}";
		        default:
			        throw new ArgumentException($"HttpMethod: {httpMethod} is not supported.");
	        }
        }

        public string RequiredScope => $"\"{Extensions.GetValue<string>("x-required-scope")}\"";
        public string CorrectedName
        {
            get
            {
                var correctedName = Regex.Replace(Name, "V([0-9]+)$", "").TrimEnd('1');
                return correctedName + (correctedName.EndsWith("Async", StringComparison.OrdinalIgnoreCase) ? "" : "Async");
            }
        }

        public string ClientName => Extensions.GetValue<string>("x-client-name");


        public string GetPostParameter(HttpMethod httpMethod)
        {
            if (httpMethod != HttpMethod.Post && httpMethod != HttpMethod.Put) return string.Empty;
            var bodyParameter = LocalParameters.SingleOrDefault(p => p.Location == ParameterLocation.Body);
            return bodyParameter == null ? ", null" : $", {bodyParameter.Name}";
        }
    }
}
