# Google Custom Search - Parallel Fetching Demo

ASP.NET Core application demonstrating parallel API calls to Google Custom Search with real-time filtering.

## Quick Start

### 1. Get Your Free API Keys

**Create API Key:**
- Go to: https://console.cloud.google.com/apis/library/customsearch.googleapis.com
- Click Enable
- Go to: https://console.cloud.google.com/apis/credentials
- Click + Create Credentials → API Key
- Copy your API Key

**Create Custom Search Engine:**
- Go to: https://cse.google.com/cse/all
- Click + Create
- Enter search engine name
- Add any website (e.g., github.com)
- Click Create
- Copy the Search Engine ID

### 2. Configure Application

Open `FetchApplication/appsettings.json` and replace the placeholders:

```json
{
  "GoogleCustomSearch": {
    "ApiKey": "YOUR_API_KEY_HERE",
    "CxId": "YOUR_CX_ID_HERE"
  }
}
```

#### Why I Didn't Include My API Keys

I didn't leave my API keys because it's bad security practice. Please make your own free API keys and copy them to `appsettings.json`

### Parallel Processing

Application gets 100 results per search by calling API 10 times (each returns max 10 results). 
Google Custom Search API free version has **100 queries/day limit**, so you can only search **10 times per day** (each search = 10 API calls).
Sequential fetching would take 2.5+ seconds for 10 queries, while parallel fetching completes in ~0.8 seconds - **3x performance improvement.**

