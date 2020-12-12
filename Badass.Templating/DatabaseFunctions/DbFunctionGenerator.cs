﻿using System;
using System.Collections.Generic;
using System.Linq;
using Badass.Model;
using Badass.Templating.DatabaseFunctions.Adapters;
using Constraint = Badass.Model.Constraint;

namespace Badass.Templating.DatabaseFunctions
{
    public class DbFunctionGenerator
    {
        public const string SqlExtension = ".sql";
        public const string SelectAllForDisplayFunctionName = "select_all_for_display";
        public const string SearchFunctionName = "search";
        public const string InsertFunctionName = "insert";
        public const string UpdateFunctionName = "update";
        public const string DeleteOperationName = "delete";


        public List<CodeFile> Generate(Domain domain, Settings settings)
        {
            Util.RegisterHelpers(domain.TypeProvider);

            var files = new List<CodeFile>();

            foreach (var type in domain.FilteredTypes)
            {
                if (!type.Ignore)
                {
                    if (settings.GenerateSecurityPolicies && type.Attributes?.createPolicy != false)
                    {
                        files.Add(GenerateSecurityPoicy(type, domain));
                    }

                    var adapter = new DbTypeAdapter(type, UpdateFunctionName, OperationType.Update, domain);
                    if (adapter.HasExcludedFields)
                    {
                        files.Add(GenerateResultType(type, domain));
                    }

                    files.Add(GenerateInsertType(type, domain));
                    
                    files.Add(GenerateInsertFunction(type, domain));

                    files.Add(GenerateSelectAllFunction(type, domain));

                    
                    if (adapter.UpdateFields.Any())
                    {
                        files.Add(GenerateUpdateFunction(adapter));
                    }

                    files.Add(GenerateDisplayType(type, domain));

                    if (type.Fields.Count(f => f.IsIdentity) == 1)
                    {
                        var identityField = type.Fields.FirstOrDefault(f => f.IsIdentity);
                        if (identityField != null)
                        {
                            files.Add(GenerateSelectByPrimaryKeyFunction(type, identityField, domain));
                            files.Add(GenerateSelectAllForDisplayByRelatedTypeFunction(type, identityField, domain));
                        }
                    }

                    if (type.Fields.Any(f => f.ReferencesType != null))
                    {
                        foreach (var field in type.Fields.Where(f => f.ReferencesType != null))
                        {
                            files.Add(GenerateSelectByRelatedTypeFunction(type, field, domain));
                            files.Add(GenerateSelectAllForDisplayByRelatedTypeFunction(type, field, domain));
                        }
                    }

                    if (type.Constraints.Any())
                    {
                        foreach (var constraint in type.Constraints)
                        {
                            files.Add(GenerateSelectByConstraint(type, constraint, domain));
                        }
                    }

                    if (type.DeleteType == DeleteType.Hard)
                    {
                        files.Add(GenerateDeleteFunction(type, domain));
                    }

                    if (type.DeleteType == DeleteType.Soft)
                    {
                        files.Add(GenerateSoftDeleteFunction(type, domain));
                    }

                    if (type.Fields.Any(f => f.IsSearch))
                    {
                        files.Add(GenerateSearchFunction(type, domain));
                    }

                    files.Add(GenerateSelectAllFunction(type, domain));
                    files.Add(GenerateSelectAllForDisplayFunction(type, domain));

                    if (!type.IsSecurityPrincipal)
                    {
                        // find all the link types that reference this type
                        var linkTypesReferencingCurrentType = domain.FilteredTypes.Where(t => t.IsLink && t.Fields.Any(f => f.HasReferenceType && f.ReferencesType == type));
                        if (linkTypesReferencingCurrentType.Any())
                        {
                            foreach (var linkingType in linkTypesReferencingCurrentType)
                            {
                                var linkAdapter = new SelectForDisplayViaLinkDbTypeAdapter(type, SelectAllForDisplayFunctionName, linkingType, domain);
                                if (linkAdapter.LinkingTypeField != null && linkAdapter.LinkTypeOtherField != null)
                                {
                                    files.Add(GenerateTemplateFromAdapter(linkAdapter, "SelectAllForDisplayViaLinkTemplate"));
                                }
                            }
                        }
                    }
                }
            }

            return files;
        }

        private CodeFile GenerateTemplateFromAdapter(DbTypeAdapter adapter, string templateName)
        {
            try
            {
                return new CodeFile
                {
                    Name = adapter.FunctionName + SqlExtension,
                    Contents = Util.GetCompiledTemplate(templateName)(adapter),
                    RelativePath = ".\\" + adapter.Name + "\\"
                };
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Unable to generate template {templateName} for type {adapter.Name}", ex);
            }
        }

        private CodeFile GenerateUpdateFunction(DbTypeAdapter adapter)
        {
            return GenerateTemplateFromAdapter(adapter, "UpdateTemplate");
        }

        private CodeFile GenerateSelectByRelatedTypeFunction(ApplicationType type, Field field, Domain domain)
        {
            var adapter = new SelectByFieldsDbTypeAdapter(type, $"select_by_{field.Name}", new List<Field> { field }, OperationType.Select, domain, false);
            return GenerateTemplateFromAdapter(adapter, "SelectByForeignKeyTemplate");
        }

        private CodeFile GenerateSelectAllForDisplayByRelatedTypeFunction(ApplicationType type, Field field, Domain domain)
        {
            var adapter = new SelectByFieldsForDisplayDbTypeAdapter(type, $"select_for_display_by_{field.Name}", new List<Field> { field }, domain);
            return GenerateTemplateFromAdapter(adapter, "SelectAllForDisplayByForeignKeyTemplate");
        }

        private CodeFile GenerateSelectByPrimaryKeyFunction(ApplicationType type, Field field, Domain domain)
        {
            var adapter = new SelectByFieldsDbTypeAdapter(type, $"select_by_{field.Name}", new List<Field> {field}, OperationType.Select, domain, true);
            return GenerateTemplateFromAdapter(adapter, "SelectByForeignKeyTemplate");
        }

        private CodeFile GenerateSelectByConstraint(ApplicationType type, Constraint constraint, Domain domain)
        {
            var adapter = new SelectByFieldsDbTypeAdapter(type, $"select_by_{constraint.Name}", constraint.Fields, OperationType.Select, domain, false);
            return GenerateTemplateFromAdapter(adapter, "SelectByForeignKeyTemplate");
        }

        private CodeFile GenerateInsertFunction(ApplicationType applicationType, Domain domain)
        {
            var adapter = new DbTypeAdapter(applicationType, "insert", OperationType.Insert, domain);
            return GenerateTemplateFromAdapter(adapter, "InsertTemplate");
        }

        private CodeFile GenerateSelectAllFunction(ApplicationType applicationType, Domain domain)
        {
            var adapter = new DbTypeAdapter(applicationType, "select_all", OperationType.Select, domain);
            return GenerateTemplateFromAdapter(adapter, "SelectAllTemplate");
        }

        private CodeFile GenerateDisplayType(ApplicationType type, Domain domain)
        {
            var adapter = new SelectForDisplayDbTypeAdapter(type, "display", domain);
            return GenerateTemplateFromAdapter(adapter, "DisplayType");
        }

        private CodeFile GenerateResultType(ApplicationType applicationType, Domain domain)
        {
            var adapter = new DbTypeAdapter(applicationType, "result", OperationType.None, domain);
            return GenerateTemplateFromAdapter(adapter, "ResultType");
        }
        
        private CodeFile GenerateInsertType(ApplicationType applicationType, Domain domain)
        {
            var adapter = new DbTypeAdapter(applicationType, "new", OperationType.Insert, domain);
            return GenerateTemplateFromAdapter(adapter, "InsertType");
        }

        private CodeFile GenerateSelectAllForDisplayFunction(ApplicationType applicationType, Domain domain)
        {
            var adapter = new SelectForDisplayDbTypeAdapter(applicationType, SelectAllForDisplayFunctionName, domain);
            return GenerateTemplateFromAdapter(adapter, "SelectAllForDisplayTemplate");
        }

        private CodeFile GenerateSearchFunction(ApplicationType applicationType, Domain domain)
        {
            var adapter = new SelectForDisplayDbTypeAdapter(applicationType, SearchFunctionName, domain);
            return GenerateTemplateFromAdapter(adapter, "SearchTemplate");
        }

        private CodeFile GenerateDeleteFunction(ApplicationType applicationType, Domain domain)
        {
            var adapter = new DbTypeAdapter(applicationType, DeleteOperationName, OperationType.Delete, domain);
            return GenerateTemplateFromAdapter(adapter, "DeleteTemplate");
        }

        private CodeFile GenerateSoftDeleteFunction(ApplicationType type, Domain domain)
        {
            var adapter = new DbTypeAdapter(type, DeleteOperationName, OperationType.Delete, domain);
            return GenerateTemplateFromAdapter(adapter, "DeleteSoftTemplate");
        }

        private CodeFile GenerateSecurityPoicy(ApplicationType type, Domain domain)
        {
            var adapter = new SecureDbTypeAdapter(type, domain);
            return new CodeFile
            {
                Name = type.Name + "_policy" + SqlExtension,
                Contents = Util.GetCompiledTemplate("SecurityPolicyTemplate")(adapter),
                RelativePath = type.Name
            };
        }
    }
}