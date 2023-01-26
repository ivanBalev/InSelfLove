namespace InSelfLove.Services.Data.Recaptcha
{
    using Google.Api.Gax.ResourceNames;
    using System.Threading.Tasks;

    using Google.Cloud.RecaptchaEnterprise.V1;
    using System.Collections.Generic;
    using Newtonsoft.Json;

    public class RecaptchaService : IRecaptchaService
    {
        private const string ProjectId = "inselflove";
        private const string SiteKey = "6LdSQIIfAAAAAO787M08KaNncgzfLpOO6VknjOeF";

        public async Task<string> VerifyAsync(string recaptchaResponse, string expectedAction)
        {
            RecaptchaEnterpriseServiceClient client = RecaptchaEnterpriseServiceClient.Create();
            ProjectName projectName = new ProjectName(ProjectId);

            CreateAssessmentRequest createAssessmentRequest = new CreateAssessmentRequest()
            {
                Assessment = new Assessment()
                {
                    Event = new Event()
                    {
                        SiteKey = SiteKey,
                        Token = recaptchaResponse,
                        ExpectedAction = expectedAction,
                    },
                },
                ParentAsProjectName = projectName,
            };

            Assessment response = await client.CreateAssessmentAsync(createAssessmentRequest);

            // Check if the token is valid.
            if (response.TokenProperties.Valid == false)
            {
                return "The CreateAssessment call failed because the token was: " +
                    response.TokenProperties.InvalidReason.ToString();
            }

            // Check if the expected action was executed.
            if (response.TokenProperties.Action != expectedAction)
            {
                return ("The action attribute in reCAPTCHA tag is: " +
                    response.TokenProperties.Action.ToString()) +
                " The action attribute in the reCAPTCHA tag does not " +
                    "match the action you are expecting to score";
            }

            // Get the risk score and the reason(s).
            if (response.RiskAnalysis.Score < 0.5)
            {
                return "The reCAPTCHA score is too low: " + ((decimal)response.RiskAnalysis.Score);
            }

            var riskAnalysisString = string.Empty;

            foreach (RiskAnalysis.Types.ClassificationReason reason in response.RiskAnalysis.Reasons)
            {
                riskAnalysisString += reason.ToString() + ". ";
            }

            return riskAnalysisString;
        }
    }
}
