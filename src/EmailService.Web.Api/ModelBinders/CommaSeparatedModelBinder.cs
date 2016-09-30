using Microsoft.AspNetCore.Mvc.ModelBinding;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace EmailService.Web.Api.ModelBinders
{
    public class CommaSeparatedModelBinder : IModelBinder
    {
        private static readonly char[] Splitters = { ',' };
        private static readonly MethodInfo ToArrayMethod = typeof(Enumerable).GetMethod("ToArray");

        public Task BindModelAsync(ModelBindingContext bindingContext)
        {
            var type = bindingContext.ModelType.GetTypeInfo();
            var name = bindingContext.FieldName;
            
            object result = null;

            if (type.GetInterface(typeof(IEnumerable).Name) != null)
            {
                var valueType = type.GetElementType() ?? type.GetGenericArguments().FirstOrDefault();
                if (valueType?.GetTypeInfo().GetInterface(typeof(IConvertible).Name) != null)
                {
                    var attempted = bindingContext.ValueProvider.GetValue(name).FirstValue;

                    if (!string.IsNullOrWhiteSpace(attempted))
                    {
                        var list = (IList)Activator.CreateInstance(typeof(List<>).MakeGenericType(valueType));
                        foreach (var splitValue in attempted?.Split(Splitters, StringSplitOptions.RemoveEmptyEntries))
                        {
                            list.Add(Convert.ChangeType(splitValue, valueType));
                        }

                        if (type.IsArray)
                        {
                            result = ToArrayMethod.MakeGenericMethod(valueType).Invoke(this, new[] { list });
                        }
                        else
                        {
                            result = list;
                        }
                    }
                }
            }

            bindingContext.Result = ModelBindingResult.Success(result);
            return Task.FromResult(0);
        }
    }
}
