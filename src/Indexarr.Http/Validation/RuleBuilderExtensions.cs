using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using FluentValidation;
using FluentValidation.Validators;

namespace Prowlarr.Http.Validation
{
    public static class RuleBuilderExtensions
    {
        public static IRuleBuilderOptions<T, Guid> ValidId<T>(this IRuleBuilder<T, Guid> ruleBuilder)
        {
            return ruleBuilder.SetValidator(new NotEqualValidator(Guid.Empty));
        }

        public static IRuleBuilderOptions<T, Guid> IsZero<T>(this IRuleBuilder<T, Guid> ruleBuilder)
        {
            return ruleBuilder.SetValidator(new EqualValidator(Guid.Empty));
        }

        public static IRuleBuilderOptions<T, string> HaveHttpProtocol<T>(this IRuleBuilder<T, string> ruleBuilder)
        {
            return ruleBuilder.SetValidator(new RegularExpressionValidator("^http(s)?://", RegexOptions.IgnoreCase)).WithMessage("must start with http:// or https://");
        }

        public static IRuleBuilderOptions<T, string> NotBlank<T>(this IRuleBuilder<T, string> ruleBuilder)
        {
            return ruleBuilder.SetValidator(new NotNullValidator()).SetValidator(new NotEmptyValidator(""));
        }

        public static IRuleBuilderOptions<T, IEnumerable<TProp>> EmptyCollection<T, TProp>(this IRuleBuilder<T, IEnumerable<TProp>> ruleBuilder)
        {
            return ruleBuilder.SetValidator(new EmptyCollectionValidator<TProp>());
        }

        public static IRuleBuilderOptions<T, int> IsValidRssSyncInterval<T>(this IRuleBuilder<T, int> ruleBuilder)
        {
            return ruleBuilder.SetValidator(new RssSyncIntervalValidator());
        }

        public static IRuleBuilderOptions<T, int> IsValidImportListSyncInterval<T>(this IRuleBuilder<T, int> ruleBuilder)
        {
            return ruleBuilder.SetValidator(new ImportListSyncIntervalValidator());
        }
    }
}
