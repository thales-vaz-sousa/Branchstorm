using System.ComponentModel.DataAnnotations;

namespace BranchstormDomain.Domain
{
    public static class DomainValidationExtensions
    {
        public static void ValidateDomain<T>(this T value) where T : class
        {
            var validationContext = new ValidationContext(value);
            var validationResults = new List<ValidationResult>();

            // com base em notations [Required], [EmailAddress], etc...
            bool isValid = Validator.TryValidateObject(value, validationContext, validationResults, true);

            if (!isValid)
            {
                // primeira mensagem de erro ou junta todas
                var errorMessages = string.Join("; ", validationResults.Select(r => r.ErrorMessage));

                // lança uma exceção de argumento inválido (ou uma exceção de domínio personalizada nossa)
                throw new ArgumentException($"Erro de validação do domínio: {errorMessages}");
            }
        }
    }
}