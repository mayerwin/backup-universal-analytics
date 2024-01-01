# Google Analytics / Universal Analytics Data Backup And Export

## About This Script
Use this script to easily back up and export your legacy data from Google Analytics (Universal Analytics) into a single CSV file per views, using the Google Analytics API. 

You'll have a local copy of your valuable historical Google Analytics data, that can be easily loaded into any data visualization tool.

## Step-by-Step Usage Guide

### Step 1: Create Your Google Cloud Platform (GCP) Project
1. **Visit [Google Cloud Console](https://console.cloud.google.com/).**
2. **Create a new project.** Give it a simple name like "Universal Analytics Export".
3. **Navigate to the 'APIs & Services' dashboard.** Enable both the "Google Analytics API" and "Analytics Reporting API".

### Step 2: Generate Your Credentials
1. **In your GCP project, head over to 'Credentials'.**
2. **Click on 'Create credentials'.** Choose 'OAuth client ID'.
3. **Configure the consent screen.** You'll need to set up an OAuth consent screen. It's a bit like introducing your app to Google. Make sure to add your email address as a test user (of the Google account you're using to access Analytics).
4. **Set the application type to 'Desktop app'.**
5. **Name your OAuth client ID and click 'Create'.**
6. **Take note of the generated Client ID and Client Secret.** You'll need these next.

### Step 3: Clone and Set Up the Script
1. **Clone this repository to your local machine.**
2. **Open the script and look for the section where you can enter your credentials.** It'll be near the top (search for "new ClientSecrets").
3. **Replace the placeholders with your Client ID and Client Secret.**

### Step 4: Run and Revel
1. **Execute the script.** If it's your first run, Google will ask you to authorize your app.
3. **Sit back and watch the magic happen.** The script will fetch the data for each view, and save it to a CSV file in the same folder as the executable.

## Customizing Dimensions and Metrics
To tailor the data export to your specific needs, you can customize the dimensions and metrics that the script retrieves from your Google Analytics account. This allows you to focus on the data most relevant to your analysis.

### Steps to Customize Dimensions and Metrics:
1. **Explore Available Options**:
    - Visit the [Google Analytics Query Explorer](https://ga-dev-tools.web.app/query-explorer/).
    - Here, you can experiment with different dimensions and metrics to see what data is available for extraction.

2. **Select Your Dimensions and Metrics**:
    - Identify the dimensions and metrics that are most relevant to your needs.
    - Make a note of their exact names as they appear in the Query Explorer (e.g., `ga:sessions`, `ga:pageviews`).

3. **Update the Script**:
    - Open the script in your preferred text editor.
    - Locate the section where dimensions and metrics are defined for the `ReportRequest` object.
    - Replace or add the dimensions and metrics in the script with those you've selected.
    - For example:
      ```csharp
      Dimensions = new List<Dimension> {
          new() { Name = "ga:date" },
          new() { Name = "ga:sourceMedium" },
          // Add or replace dimensions here
      },
      Metrics = new List<Metric> {
          new() { Expression = "ga:users" },
          new() { Expression = "ga:newUsers" },
          // Add or replace metrics here
      }
      ```

4. **Run the Script with Custom Dimensions/Metrics**:
    - Save the changes to the script.
    - Run the script as usual. The data exported will now reflect the custom dimensions and metrics you've specified.

### Tips for Customization:
- **Balance the Data**: Be mindful of the volume of data you're requesting. More dimensions and metrics can lead to larger datasets, which might take longer to process.
- **Combinations**: Some dimensions and metrics can only be queried together in certain combinations. The Query Explorer is a great tool to validate these combinations before updating the script.
- **API Limits**: Keep in mind the API limits. Excessive data requests might lead to hitting the quota limits.

By customizing dimensions and metrics, you can fine-tune the data extraction to align with your analysis requirements, making the script a powerful tool for your specific data needs.

## Your Contribution
Found a bug? Got an idea for an improvement? Feel like adding a cherry on top? Your contributions are welcome! Fork it, branch it, push it, and make a pull request. Let's make Google Analytics data archival a piece of cake for everyone! 🍰

## Background
With the transition from Universal Analytics to Google Analytics 4 (GA4), Google horribly decided not to migrate existing user data to GA4, nor to keep a way to consult it online in a legacy way, nor to at least offer a simple way to download the raw data so it could be loaded into another program.

I tried all the solutions I could find to archive this data, such as the Google Analytics and SyncWith add-ons for Google Sheets, Airbyte through their cloud service, Restack.io (to export the data to BigQuery), with Docker locally, Supermetrics and Dataddo.
Unfortunately, each time it was either too unreliable, cumbersome, slow, or didn't work at all. The Google Analytics add-on for Google Sheets for example is currently impossible to install as [Google recently started blocking it](https://groups.google.com/g/google-analytics-spreadsheet-add-on/c/dT7496g9Fe0) for security reasons... 

So I ended up writing this script with ChatGPT's help, which does the job neatly and quickly, without running into any API limitation issues.

## License
This script is released under the [MIT License](LICENSE). Feel free to use it for your personal or commercial needs, and share it with your friends.
