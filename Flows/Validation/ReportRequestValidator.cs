using SERVERHANGFIRE.Flows.DTOs;

namespace SERVERHANGFIRE.Flows.Validation
{
    public static class ReportRequestValidator
    {
        public static bool IsValid(ReportRequestDto request, out string errorMessage)
        {
            errorMessage = string.Empty;

            if (request.CustomerId <= 0)
            {
                errorMessage = "CustomerId debe ser mayor a cero";
                return false;
            }

            if (request.StartDate >= request.EndDate)
            {
                errorMessage = "StartDate debe ser menor a EndDate";
                return false;
            }

            return true;
        }
    }
}
