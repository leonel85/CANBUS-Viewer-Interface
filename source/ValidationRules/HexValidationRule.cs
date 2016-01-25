using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows.Controls;

namespace CANBUSViewerInterface.ValidationRules
{
    public class HexValidationRule : ValidationRule
    {
        public int CharCount { get; set; }

        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            if (System.Text.RegularExpressions.Regex.IsMatch(value.ToString(), "^[0-9a-fA-F]{" + CharCount + "}$"))
                return ValidationResult.ValidResult;
            else
                return new ValidationResult(false, "Insert byte in Hex format");
        }

    }
}
