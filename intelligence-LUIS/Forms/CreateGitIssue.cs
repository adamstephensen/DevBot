using Microsoft.Bot.Builder.FormFlow;
using System;

namespace formflow.FormFlow
{
    //The questions are structured for the Form Bot using public properties on the class. 
    //You can see here the Prompt attribute is set and the question options can be complex types like String or enums.

    [Serializable]
    public class CreateGitIssue
    {
        [Prompt("Who is the repository owner?")]
        public string Message { get; set; }
        [Prompt("Repository Name?")]
        public string Number { get; set; }
        [Prompt("What is the title of your issue?")]
        public string Title { get; set; }
        [Prompt("Please describe your issue in more detail")]
        public string Description { get; set; }



        public static IForm<CreateGitIssue> BuildEnquiryForm()
        {
            return new FormBuilder<CreateGitIssue>()
                .AddRemainingFields()
                .Build();
        }
    }


}