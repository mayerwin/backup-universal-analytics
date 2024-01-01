using Google.Apis.Analytics.v3;
using Google.Apis.Analytics.v3.Data;
using Google.Apis.AnalyticsReporting.v4;
using Google.Apis.AnalyticsReporting.v4.Data;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using CsvHelper;
using System.Globalization;

class Program {
    static void Main(string[] args) {
        try {
            UserCredential credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
                new ClientSecrets {
                    ClientId = "xxx",
                    ClientSecret = "xxx"
                },
                new[] { AnalyticsService.Scope.AnalyticsReadonly },
                "user",
                CancellationToken.None).Result;

            var analyticsService = new AnalyticsService(new BaseClientService.Initializer() {
                HttpClientInitializer = credential,
                ApplicationName = "Google Analytics API Console App",
            });

            // Get Account Summaries
            var accountSummariesRequest = analyticsService.Management.AccountSummaries.List();
            AccountSummaries accountSummaries = accountSummariesRequest.Execute();
            var total = accountSummaries.Items.SelectMany(e => e.WebProperties.SelectMany(p => p.Profiles)).Count();

            var counter = 1;
            foreach (var account in accountSummaries.Items) {
                foreach (var property in account.WebProperties.OrderByDescending(p => p.Name)) {
                    foreach (var view in property.Profiles) {
                        Console.WriteLine($"Retrieving data for view {counter}/{total}: {property.Name}-{view.Name}-{property.WebsiteUrl}...");

                        // Create Reporting Service
                        var reportingService = new AnalyticsReportingService(new BaseClientService.Initializer() {
                            HttpClientInitializer = credential,
                            ApplicationName = "Google Analytics Reporting API Console App",
                        });

                        List<ReportRequest> requests = [];
                        // Add user-defined dimensions and metrics here

                        var reportRequest = new ReportRequest {
                            ViewId = view.Id,
                            DateRanges = new List<DateRange> {
                                new() { StartDate = "2005-01-01", EndDate = "2023-08-31" }
                            },
                            Dimensions = new List<Dimension> {
                                    new() { Name = "ga:date" },
                                    new() { Name = "ga:sourceMedium" },
                                    new() { Name = "ga:keyword" },
                                    new() { Name = "ga:countryIsoCode" },
                                    new() { Name = "ga:fullReferrer" },
                                    new() { Name = "ga:city" },
                                    new() { Name = "ga:userType" },
                                    new() { Name = "ga:deviceCategory" }
                            },
                            Metrics = new List<Metric> {
                                    new() { Expression = "ga:users" },
                                    new() { Expression = "ga:newUsers" },
                                    new() { Expression = "ga:pageviews" },
                                    new() { Expression = "ga:uniquePageviews" },
                                    new() { Expression = "ga:sessions" },
                                    new() { Expression = "ga:bounceRate" },
                                    new() { Expression = "ga:avgSessionDuration" },
                                    new() { Expression = "ga:goalCompletionsAll" }
                            }
                        };

                        // Execute requests and write to CSV
                        GetReportsAndWriteToCsv(reportingService, reportRequest, view, property);
                    }
                }
            }
        }
        catch (Exception ex) {
            Console.WriteLine("Error: " + ex.Message);
        }
    }

    static void GetReportsAndWriteToCsv(AnalyticsReportingService service, ReportRequest request, ProfileSummary view, WebPropertySummary property) {
        string domainName = new Uri(property.WebsiteUrl).Host;
        string fileName = ReplaceInvalidChars($"{property.Id}.{view.Id}+{domainName}+{property.Name}+{view.Name}.csv");

        bool fileExists = File.Exists(fileName);
        if (fileExists) {
            Console.WriteLine($"File {fileName} already exists. Delete and recreate? (y/n)");
            if (!Console.ReadLine().Equals("y", StringComparison.CurrentCultureIgnoreCase)) {
                Console.WriteLine("View skipped.");
                return;
            }
            File.Delete(fileName);
        }

        int retryCount = 0;
        bool successful = false;
        int startIndex = 0;
        int pageSize = 1000; // Default page size for Google Analytics Reporting API

        while (!successful && retryCount < 100) {
            try {
                var response = service.Reports.BatchGet(new GetReportsRequest { ReportRequests = new List<ReportRequest> { request } }).Execute();

                using (var writer = new StreamWriter(fileName, append: true))
                using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture)) {
                    if (startIndex == 0) {
                        // Write headers
                        foreach (var header in response.Reports.First().ColumnHeader.Dimensions) {
                            csv.WriteField(header);
                        }
                        foreach (var header in response.Reports.First().ColumnHeader.MetricHeader.MetricHeaderEntries) {
                            csv.WriteField(header.Name);
                        }
                        csv.NextRecord();
                    }

                    foreach (var report in response.Reports) {
                        foreach (var row in report.Data.Rows) {
                            foreach (var dimension in row.Dimensions) {
                                csv.WriteField(dimension);
                            }
                            foreach (var metric in row.Metrics) {
                                foreach (var value in metric.Values) {
                                    csv.WriteField(value);
                                }
                            }
                            csv.NextRecord();
                        }
                    }
                }

                startIndex += pageSize;
                Console.WriteLine($"Fetched {Math.Min(startIndex, response.Reports.First().Data.RowCount.Value)}/{response.Reports.First().Data.RowCount.Value} rows.");

                if (response.Reports.Single().Data.RowCount < pageSize || startIndex >= response.Reports.First().Data.RowCount) {
                    successful = true; // No more pages
                    Console.WriteLine("All data fetched successfully.");
                }
                else {
                    request.PageToken = startIndex.ToString();
                }
            }
            catch (Google.GoogleApiException ex) when (ex.HttpStatusCode == System.Net.HttpStatusCode.TooManyRequests) {
                Console.WriteLine("Quota Error: Daily request limit exceeded. Retrying in 10 minutes.");
                Thread.Sleep(TimeSpan.FromMinutes(10));
            }
            catch (Exception ex) {
                Console.WriteLine($"Error on attempt {retryCount + 1}: {ex.Message}");
                retryCount++;
            }
        }

        if (!successful) {
            Console.WriteLine("Max retry count reached. Do you want to continue? (y/n)");
            if (!Console.ReadLine().Equals("y", StringComparison.CurrentCultureIgnoreCase)) {
                throw new Exception("Operation aborted by user.");
            }
        }
    }

    static string ReplaceInvalidChars(string filename) {
        return string.Join("_", filename.Split(Path.GetInvalidFileNameChars()));
    }
}
