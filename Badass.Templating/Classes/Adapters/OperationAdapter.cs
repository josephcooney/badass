using System;
using System.Collections.Generic;
using Badass.Model;
using System.Linq;
using Badass.Templating.DatabaseFunctions;
using Serilog;

namespace Badass.Templating.Classes
{
    public class OperationAdapter
    {
        public const string HttpPostOperation = "Post";
        public const string HttpGetOperation = "Get";
        public const string HttpDeleteOperation = "Delete";
        public const string HttpPutOperation = "Put";

        protected readonly Operation _op;
        protected readonly Domain _domain;
        private readonly ApplicationType _type;

        public OperationAdapter(Operation op, Domain domain, ApplicationType type)
        {
            _op = op ?? throw new ArgumentNullException(nameof(op));
            _domain = domain ?? throw new ArgumentNullException(nameof(domain));
            _type = type ?? throw new ArgumentNullException(nameof(type));

            if (_op.Returns == null)
            {
                Log.Error("Operation {OperationName} has no return type", _op.Name);
            }
        }

        public string Name => _op.Name;

        public string BareName => _op.BareName;

        public ApplicationType RelatedType => _type;

        public string Namespace
        {
            get
            {
                if (!string.IsNullOrEmpty(_domain.DefaultNamespace) && (string.IsNullOrEmpty(_op.Namespace) || _op.Namespace == SimpleType.DefaultNamespace))
                {
                    return _domain.DefaultNamespace;
                }

                return _op.Namespace;
            }
        }

        public bool HasParameters => _op.Parameters.Any();

        public List<Parameter> Parameters => _op.Parameters;

        public List<ParameterAdapter> UserProvidedParameters
        {
            get { return _op.UserProvidedParameters.Select(p => new ParameterAdapter(_domain, p)).ToList(); }
        }

        public List<ParameterAdapter> UserEditableParameters
        {
            get { return UserProvidedParameters.Where(p => p.UserEditable).OrderBy(p => p.RelatedTypeField?.Rank).ToList(); }
        }

        public string ReturnTypeName
        {
            get
            {
                if (_op.Returns.ReturnType == ReturnType.Singular)
                {
                    return TypeMapping.GetCSharpShortTypeName(_op.Returns.SingularReturnType);
                }

                if (_op.Returns.ReturnType == ReturnType.ApplicationType || _op.Returns.ReturnType == ReturnType.CustomType)
                {
                    return Util.CSharpNameFromName(_op.Returns.SimpleReturnType.Name);
                }

                return "TODO";
            }
        }

        public string Returns
        {
            get
            {
                if (_op.Returns.ReturnType == ReturnType.None)
                {
                    return "void";
                }

                if (_op.Returns.ReturnType == ReturnType.Singular)
                {
                    if (_op.Returns.SingularReturnType == null)
                    {
                        Log.Error("Singular return type for operation {OperationName} was not defined.", _op.Name);
                        throw new InvalidOperationException($"Scalar return type for operation {_op.Name} was not defined");
                    }
                    
                    return TypeMapping.GetCSharpShortTypeName(_op.Returns.SingularReturnType);
                }

                if (_op.Returns.ReturnType == ReturnType.ApplicationType || _op.Returns.ReturnType == ReturnType.CustomType)
                {
                    if (_op.SingleResult)
                    {
                        return Util.CSharpNameFromName(_op.Returns.SimpleReturnType.Name);
                    }
                    
                    return $"List<{Util.CSharpNameFromName(_op.Returns.SimpleReturnType.Name)}>";
                }

                return "TODO";
            }
        }


        public bool SingleResult => _op.SingleResult;
        
        public string TypeScriptReturn
        {
            get
            {
                if (_op.Returns?.ReturnType == ReturnType.None)
                {
                    return "void";
                }

                if (_op.Returns?.ReturnType == ReturnType.Singular)
                {
                    return Util.GetTypeScriptTypeForClrType(_op.Returns.SingularReturnType);
                }

                if (_op.Returns?.ReturnType == ReturnType.ApplicationType || _op.Returns?.ReturnType == ReturnType.CustomType)
                {
                    if (_op.SingleResult)
                    {
                        return Util.CSharpNameFromName(_op.Returns.SimpleReturnType.Name);
                    }
                    
                    return Util.CSharpNameFromName(_op.Returns.SimpleReturnType.Name) + "[]";
                }

                return "TODO";
            }
        }

        public SimpleType SimpleReturnType => _op.Returns?.SimpleReturnType;

        public bool NoResult => _op.Returns?.ReturnType == ReturnType.None;

        public bool IsSingular => _op.Returns?.ReturnType == ReturnType.Singular;

        public bool IsStructuredResult => _op.Returns?.ReturnType == ReturnType.ApplicationType ||
                                          _op.Returns?.ReturnType == ReturnType.CustomType;

        public bool IsSelectById => _op.IsSelectById;

        public bool IsSearch
        {
            get
            {
                if (_op.Attributes?.isSearch != null)
                {
                    return (bool) _op.Attributes?.isSearch;
                }

                var appTypeName = _op.Attributes?.applicationtype?.ToString();
                if (!string.IsNullOrEmpty(appTypeName))
                {
                    var appType = _domain.Types.FirstOrDefault(a => a.Name == appTypeName);
                    if (appType != null)
                    {
                        return _op.Name.EndsWith(DbFunctionGenerator.SearchFunctionName) && appType.Fields.Any(f => f.IsSearch);
                    }
                }

                return false;
            }
        }

        public bool IsDelete => _op.Name.EndsWith("delete") || _op.Attributes?.isDelete == true;

        public string HttpMethod
        {
            get
            {
                try
                {
                    var httpOp = _op.Attributes?.HttpMethod?.ToString();
                    if (!string.IsNullOrEmpty(httpOp))
                    {
                        return httpOp;
                    }
                
                    if (IsDelete)
                    {
                        return HttpDeleteOperation;
                    }

                    if (_op.Name.EndsWith(DbFunctionGenerator.InsertFunctionName) || _op.Name.EndsWith(DbFunctionGenerator.UpdateFunctionName) || _op.ChangesData || _op.CreatesNew)
                    {
                        return HttpPostOperation;
                    }

                    return HttpGetOperation;
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "Unexpected error getting HTTP Method for {OperationName}", _op.Name);
                    throw;
                }
            }
        }

        public bool UsesModel
        {
            get
            {
                var result = UserProvidedParameters.Count > 1 &&
                            (HttpMethod.ToLowerInvariant() == HttpPostOperation.ToLowerInvariant() || HttpMethod.ToLowerInvariant() == HttpPutOperation.ToLowerInvariant());

                return result;
            }
        }

        public bool ResultHasAnyDateFields
        {
            get
            {
                return IsStructuredResult && _op.Returns.SimpleReturnType.Fields.Any(f => f.IsDate && !f.IsExcludedFromResults);
            }
        }

        public bool ChangesData => _op.ChangesData;

        public bool CreatesNew => _op.CreatesNew;

        public string FriendlyName => _op.FriendlyName;

        public List<ClassAdapter> ParameterReferenceTypes
        {
            get
            {
                return this.UserProvidedParameters.Where(p => p.RelatedTypeField != null && p.RelatedTypeField.ReferencesType != null).Select(p => p.RelatedTypeField.ReferencesType).Distinct()
                    .Select(t => new ClassAdapter(t, _domain)).ToList();
            }
        }

        public bool GenerateUI => _op.GenerateUI;

        public bool GenerateApi => _op.GenerateApi;

        public List<ParameterAdapter> NavigationParameters
        {
            get
            {
                var navParamNames = new List<string>();
                if (ChangesData && !CreatesNew)
                {
                    navParamNames.Add(Field.IdFieldName);
                }

                var navParams = _op.Attributes?.navParams;
                if (navParams != null)
                {
                    foreach (var param in navParams)
                    {
                        navParamNames.Add(param.ToString());
                    }
                }

                return UserProvidedParameters.Where(p => navParamNames.Contains(p.Name)).ToList();
            }
        }

        public bool AllowAnon
        {
            get
            {
                var anon = _type.Attributes?.security?.anon;

                if (_op.CreatesNew)
                {
                    return anon != null && SecurityUtil.HasAddRights(anon);
                }

                if (!_op.CreatesNew && _op.ChangesData)
                {
                    return anon != null && SecurityUtil.HasEditRights(anon);
                }

                if (IsDelete)
                {
                    return anon != null && SecurityUtil.HasDeleteRights(anon);
                }

                if (_op.IsSelectById)
                {
                    return anon != null && SecurityUtil.HasReadRights(anon);
                }

                if (IsSearch || (_op.Returns.ReturnType != ReturnType.None) || (_op.Returns.ReturnType != ReturnType.Singular))
                {
                    return anon != null && SecurityUtil.HasListRights(anon);
                }

                return false;
            }
        }

        public bool ApiHooks
        {
            get { return _op.Attributes?.apiHooks == true || _type.Attributes?.apiHooks == "all" || _type.Attributes?.apiHooks == "modify" && (_op.ChangesData || _op.CreatesNew); }
        }
    }
}
