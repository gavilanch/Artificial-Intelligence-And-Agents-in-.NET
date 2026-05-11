using BlazorAI.DTOs;

namespace BlazorAI.Services.RAG
{
    public class DocumentsFromMemoryService
    {
        public List<Document> GetDocuments() =>
            [
                new Document
                {
                    Title = "Vacation Policy",
                    Content = """
            Every employee is entitled to 14 working days of vacation after completing one year at the company.
            The request must be submitted at least 15 days in advance.
            The direct manager must approve the request before it is taken.
            Vacation cannot be split into blocks smaller than 2 days, unless specially approved by Human Resources.
            """
                },
        new Document
        {
                    Title = "Remote Work",
            Content = """
            Employees may work remotely up to 3 days per week.
            They must attend in-person strategic meetings when required.
            The employee must ensure a stable internet connection and a suitable environment for video calls.
            Company-provided equipment is for work use only.
            """
        },
        new Document
        {
                    Title = "Equipment Requests",
            Content = """
            To request a new laptop or accessory, the employee must open a ticket with the help desk.
            The ticket must include business justification and supervisor approval.
            The estimated delivery time for in-stock equipment is 3 working days.
            If the equipment is not available, the Purchasing department will provide an estimated restock date.
            """
        },
        new Document
        {
                    Title = "Technical Support",
            Content = """
            Technical support hours are Monday through Friday from 8:00 a.m. to 6:00 p.m.
            Critical incidents are high priority and must be reported through the emergency channel.
            Passwords must not be shared under any circumstances.
            Password resets can be requested through the self-service portal.
            """
        }];
    }
}
