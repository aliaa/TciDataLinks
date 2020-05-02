using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;
using MongoDB.Bson;
using System;
using System.Threading.Tasks;

namespace TciDataLinks
{
    public class ObjectIdModelBinder : IModelBinder
    {
        public Task BindModelAsync(ModelBindingContext bindingContext)
        {
            var result = bindingContext.ValueProvider.GetValue(bindingContext.ModelName);
            if (result.Length == 0)
                return Task.CompletedTask;
            if (ObjectId.TryParse(result.FirstValue, out ObjectId objId))
                bindingContext.Result = ModelBindingResult.Success(objId);
            else
                bindingContext.Result = ModelBindingResult.Failed();
            return Task.CompletedTask;
        }
    }

    public class ObjectIdModelBinderProvider : IModelBinderProvider
    {
        public IModelBinder GetBinder(ModelBinderProviderContext context)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));

            if (context.Metadata.ModelType == typeof(ObjectId))
            {
                return new BinderTypeModelBinder(typeof(ObjectIdModelBinder));
            }

            return null;
        }
    }
}
