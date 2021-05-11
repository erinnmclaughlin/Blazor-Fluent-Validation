using FluentValidation;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using System;
using System.Linq;

namespace BlazorFluentValidation
{
    /*
     * This was created using https://blog.stevensanderson.com/2019/09/04/blazor-fluentvalidation/ as a reference
     * Needed to fix ValidateModel method as it was outdated - found fix here https://blog.stevensanderson.com/2019/09/04/blazor-fluentvalidation/
     * Also added _hasErrors field to prevent validation from occuring before the user submits the form for the first time
     */
    public class FluentValidator<TValidator> : ComponentBase where TValidator : IValidator, new()
    {
        private readonly static char[] separators = new[] { '.', '[' };
        private TValidator validator;
        private bool _hasErrors = false;

        [CascadingParameter] private EditContext EditContext { get; set; }

        protected override void OnInitialized()
        {
            validator = new TValidator();
            var messages = new ValidationMessageStore(EditContext);

            // Revalidate when any field changes, or if the entire form requests validation
            // (e.g., on submit)

            EditContext.OnFieldChanged += (sender, eventArgs) => {
                if (_hasErrors)
                    ValidateModel((EditContext)sender, messages);
            };

            EditContext.OnValidationRequested += (sender, eventArgs)
                => ValidateModel((EditContext)sender, messages);
        }

        private void ValidateModel(EditContext editContext, ValidationMessageStore messages)
        {
            var validationContext = new ValidationContext<object>(EditContext.Model);
            var validationResult = validator.Validate(validationContext);

            messages.Clear();
            foreach (var error in validationResult.Errors)
            {
                var fieldIdentifier = ToFieldIdentifier(editContext, error.PropertyName);
                messages.Add(fieldIdentifier, error.ErrorMessage);
            }
            editContext.NotifyValidationStateChanged();
            _hasErrors = editContext.GetValidationMessages().Any();
        }

        private static FieldIdentifier ToFieldIdentifier(EditContext editContext, string propertyPath)
        {
            // This method parses property paths like 'SomeProp.MyCollection[123].ChildProp'
            // and returns a FieldIdentifier which is an (instance, propName) pair. For example,
            // it would return the pair (SomeProp.MyCollection[123], "ChildProp"). It traverses
            // as far into the propertyPath as it can go until it finds any null instance.

            var obj = editContext.Model;

            while (true)
            {
                var nextTokenEnd = propertyPath.IndexOfAny(separators);
                if (nextTokenEnd < 0)
                {
                    return new FieldIdentifier(obj, propertyPath);
                }

                var nextToken = propertyPath.Substring(0, nextTokenEnd);
                propertyPath = propertyPath.Substring(nextTokenEnd + 1);

                object newObj;
                if (nextToken.EndsWith("]"))
                {
                    // It's an indexer
                    // This code assumes C# conventions (one indexer named Item with one param)
                    nextToken = nextToken.Substring(0, nextToken.Length - 1);
                    var prop = obj.GetType().GetProperty("Item");
                    var indexerType = prop.GetIndexParameters()[0].ParameterType;
                    var indexerValue = Convert.ChangeType(nextToken, indexerType);
                    newObj = prop.GetValue(obj, new object[] { indexerValue });
                }
                else
                {
                    // It's a regular property
                    var prop = obj.GetType().GetProperty(nextToken);
                    if (prop == null)
                    {
                        throw new InvalidOperationException($"Could not find property named {nextToken} on object of type {obj.GetType().FullName}.");
                    }
                    newObj = prop.GetValue(obj);
                }

                if (newObj == null)
                {
                    // This is as far as we can go
                    return new FieldIdentifier(obj, nextToken);
                }

                obj = newObj;
            }
        }
    }
}
