﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using Badass.Model;
using HandlebarsDotNet;
using Serilog;

namespace Badass.Templating
{
    public class Util
    {
        public static string GetTemplate(string templateName)
        {
            var assembly = Assembly.GetExecutingAssembly();
            return GetTemplate(templateName, assembly);
        }

        public static string GetTemplate(string templateName, Assembly assembly)
        {
            string resourceName;
            try
            {
                resourceName = assembly.GetManifestResourceNames()
                    .Single(str => str.EndsWith(templateName + ".handlebars"));
            }
            catch
            {
                Log.Fatal("Unable to find template {TemplateName}. Make sure the build action of the template has been set to 'embedded resource'.", templateName);
                throw;
            }

            using (Stream stream = assembly.GetManifestResourceStream(resourceName))
            using (StreamReader reader = new StreamReader(stream))
            {
                return reader.ReadToEnd();
            }
        }

        public static Func<object, string> GetCompiledTemplate(string templateName)
        {
            var template = GetTemplate(templateName);
            try
            {
                return Handlebars.Compile(template);
            }
            catch
            {
                Log.Fatal("Unable to compile template {TemplateName}", templateName);
                throw;
            }
        }

        public static void RegisterHelpers(ITypeProvider typeProvider)
        {
            _typeProvider = typeProvider;
            
            Handlebars.RegisterHelper("format_clr_type", (writer, context, parameters) =>
            {
                if (parameters == null || parameters.Length == 0 || parameters[0] == null)
                {
                    writer.Write("ERROR: No type provided");
                    return;
                }

                var originalType = (System.Type) parameters[0];
                var result = FormatClrType(originalType);
                writer.Write(result);
            });

            Handlebars.RegisterHelper("escape_sql_keyword", (writer, context, parameters) =>
            {
                string name = parameters[0] as string;

                if (name == null)
                {
                    writer.Write("NULL - No parameter provided");
                    return;
                }

                var escaped = EscapeSqlReservedWord(name);
                writer.Write(escaped);
            });

            Handlebars.RegisterHelper("cs_name", (writer, context, parameters) =>
            {
                if (parameters[0] is string)
                {
                    string name = (string)parameters[0];
                    try
                    {
                        var formatted = CSharpNameFromName(name);
                        writer.Write(formatted);
                    }
                    catch (Exception ex)
                    {
                        writer.Write($"Error Formatting CS Name: {name}");
                        Log.Error("Error Formatting CSharp Name", ex);
                    }
                    return;
                }
                writer.Write(parameters[0] + "/* expected string */");
            });

            Handlebars.RegisterHelper("cml_case", (writer, context, parameters) =>
            {
                if (!(parameters[0] is string))
                {
                    writer.Write("undefined");
                    return;
                }

                string name = (string)parameters[0];
                var formatted = CamelCase(name);
                writer.Write(formatted);
            });

            Handlebars.RegisterHelper("kb_case", (writer, context, parameters) =>
            {
                if (!(parameters[0] is string))
                {
                    writer.Write("undefined");
                    return;
                }

                string name = (string)parameters[0];
                var formatted = KebabCase(name);
                writer.Write(formatted);
            });

            Handlebars.RegisterHelper("hmn", (writer, context, parameters) =>
            {
                if (parameters != null && parameters.Length > 0)
                {
                    if (parameters[0] is ApplicationType)
                    {
                        ApplicationType type = (ApplicationType)parameters[0];
                        writer.Write(HumanizeName(type.Name));
                        return;
                    }

                    if (parameters[0] is Field)
                    {
                        writer.Write(HumanizeName((Field)parameters[0]));
                        return;
                    }

                    if (parameters[0] is Parameter)
                    {
                        writer.Write(HumanizeName((Parameter)parameters[0]));
                    }

                    writer.Write(HumanizeName(parameters[0].ToString()));
                    return;
                }

                if (context is ApplicationType)
                {
                    ApplicationType type = (ApplicationType)context;
                    writer.Write(HumanizeName(type.Name));
                    return;
                }

                if (context is Field)
                {
                    writer.Write(HumanizeName((Field)context));
                    return;
                }

                if (context is Parameter)
                {
                    writer.Write(HumanizeName((Parameter)context));
                    return;
                }
            });

            Handlebars.RegisterHelper("db_type_to_cs", (writer, context, parameters) =>
            {
                string name = (string)parameters[0];
                var formatted = _typeProvider.GetCsDbTypeFromDbType(name);
                writer.Write(formatted);
            });

            Handlebars.RegisterHelper("lc", (writer, context, parameters) =>
            {
                string parameter = (string)parameters[0];
                writer.Write(parameter.ToLower());
            });

            Handlebars.RegisterHelper("get_ts_type", (writer, context, parameters) =>
            {
                if (parameters == null || parameters.Length == 0 || parameters[0] == null)
                {
                    writer.Write("ERROR: No type provided");
                    return;
                }

                System.Type type = (System.Type)parameters[0];
                var tsType = GetTypeScriptTypeForClrType(type);
                writer.Write(tsType);
            });


            Handlebars.RegisterHelper("ts_fix_http", (writer, context, parameters) =>
            {
                string httpOp = (string)parameters[0];
                var lowered = httpOp.ToLower();
                if (lowered == "delete") // delete is a reserved word in JS https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Operators/delete
                {
                    lowered = "del";
                }
                writer.Write(lowered);
            });

            Handlebars.RegisterHelper("indent", (writer, context, parameters) =>
            {
                writer.Write("\t");
            });

            Handlebars.RegisterHelper("input_type", (writer, ContextBoundObject, parameters) =>
            {
                System.Type originalType = (System.Type)parameters[0];
                var type = System.Nullable.GetUnderlyingType(originalType) ?? originalType;

                if (_inputTypes.ContainsKey(type))
                {
                    var inputType = _inputTypes[type];
                    writer.Write(inputType);
                    return;
                }

                writer.Write("text"); // fallback
            });

            Handlebars.RegisterHelper("has", (output, options, context, arguments) =>
            {
                if (arguments.Length != 1)
                {
                    throw new HandlebarsException($"{{has}} helper must be called with a single argument. Instead it received {arguments.Length} arguments.");
                }

                var arg = arguments[0];
                var hasItems = false;
                if (arg != null)
                {
                    if (arg is IEnumerable)
                    {
                        hasItems = ((IEnumerable) arg).GetEnumerator().MoveNext();
                    }
                    else
                    {
                        Log.Warning("{{has}} handlebars template received an argument but it was not IEnumerable");
                    }
                }

                if (hasItems)
                {
                    options.Template(output, context);
                }
                else
                {
                    options.Inverse(output, context);
                }
            });
        }

        public static string FormatClrType(Type originalType)
        {
            var type = Nullable.GetUnderlyingType(originalType) ?? originalType;

            var shortName = TypeMapping.GetCSharpShortTypeName(type);
            if (shortName != null)
            {
                if (originalType != type)
                {
                    shortName += "?";
                }

                return shortName;
            }

            if (type.Namespace == "System")
            {
                if (originalType != type)
                {
                    return type.Name + "?";
                }
                else
                {
                    return type.Name;
                }
            }

            return type.ToString();
        }

        public static string EscapeSqlReservedWord(string name)
        {
            return _typeProvider.EscapeReservedWord(name);
        }

        public static string CSharpNameFromName(string name)
        {
            var parts = name.Split('_');
            return string.Join("", parts.Select(p => char.ToUpperInvariant(p[0]) + p.Substring(1)));
        }

        public static string CamelCase(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                return name;
            }
            
            if (name.IndexOf('_') > -1)
            {
                name = CSharpNameFromName(name);
            }

            if (name.ToUpperInvariant() == name)
            {
                return name.ToLowerInvariant();
            }
            
            return Char.ToLowerInvariant(name[0]) + name.Substring(1);
        }

        public static string KebabCase(string name)
        {
            if (IsPascalCase(name))
            {
                var sentence = PascalCaseToSentenceCase(name).ToLower();
                return sentence.Replace(' ', '-');
            }

            return name.Replace('_', '-');
        }

        public static string BareName(string itemName, string typeName)
        {
            if (typeName != null)
            {
                return itemName.Replace(typeName, "").Trim('_').Replace("__", "_");
            }

            return null;
        }
        
        private static bool IsPascalCase(string name)
        {
            //this is very naive
            return (name[0] == char.ToUpperInvariant(name[0]));
        }

        public static string TypescriptFileName(string name)
        {
            return CamelCase(name);
        }

        public static string HumanizeName(string name)
        {
            var parts = name.Split('_');
            return string.Join(" ", parts.Select(p => char.ToUpperInvariant(p[0]) + p.Substring(1)));
        }

        public static string PascalCaseToSentenceCase(string str)
        {
            return Regex.Replace(str, "[a-z][A-Z]", m => $"{m.Value[0]} {char.ToLower(m.Value[1])}");
        }

        public static string HumanizeName(Field field)
        {
            // we might want to read an attribute later on here 
            var name = HumanizeName(field.Name);

            if (field.HasReferenceType && name.EndsWith(" Id"))
            {
                name = name.Substring(0, name.Length - " Id".Length);
            }

            return name;
        }

        public static string HumanizeName(Parameter parameter)
        {
            var name = HumanizeName(parameter.Name);

            if (parameter.RelatedTypeField?.HasReferenceType == true && name.EndsWith(" Id"))
            {
                name = name.Substring(0, name.Length - " Id".Length);
            }

            return name;
        }

        public static string GetTypeScriptTypeForClrType(System.Type clrType)
        {
            var type = System.Nullable.GetUnderlyingType(clrType) ?? clrType;
            if (_typeScriptTypes.ContainsKey(type))
            {
                return _typeScriptTypes[type];
            }

            return "any";
        }

        public static string RemoveSuffix(string name, string suffix)
        {
            if (name.EndsWith(suffix))
            {
                return name.Substring(0, name.Length - suffix.Length);
            }

            return suffix;
        }

        public static string CanonicalizeName(string name)
        {
            return name.ToLowerInvariant().Replace(" ", "").Replace("-", "").Replace("_", "");
        }

        private static ITypeProvider _typeProvider;

        private static Dictionary<System.Type, string> _typeScriptTypes;

        private static Dictionary<System.Type, string> _inputTypes;

        static Util()
        {
            _typeScriptTypes = new Dictionary<Type, string>()
            {
                [typeof(string)] = "string",
                [typeof(char)] = "string",
                [typeof(byte)] = "number",
                [typeof(sbyte)] = "number",
                [typeof(short)] = "number",
                [typeof(ushort)] = "number",
                [typeof(int)] = "number",
                [typeof(uint)] = "number",
                [typeof(long)] = "number",
                [typeof(ulong)] = "number",
                [typeof(float)] = "number",
                [typeof(double)] = "number",
                [typeof(decimal)] = "number",
                [typeof(bool)] = "boolean",
                [typeof(object)] = "any",
                [typeof(void)] = "void",
                [typeof(DateTime)] = "Date",
                [typeof(byte[])] = "File",
            };

            _inputTypes = new Dictionary<Type, string>()
            {
                [typeof(string)] = "text",
                [typeof(char)] = "text",
                [typeof(short)] = "number",
                [typeof(ushort)] = "number",
                [typeof(int)] = "number",
                [typeof(uint)] = "number",
                [typeof(long)] = "number",
                [typeof(ulong)] = "number",
                [typeof(float)] = "number",
                [typeof(double)] = "number",
                [typeof(decimal)] = "number",
                [typeof(bool)] = "checkbox",
                [typeof(byte[])] = "file",
            };
        }
    }
}
