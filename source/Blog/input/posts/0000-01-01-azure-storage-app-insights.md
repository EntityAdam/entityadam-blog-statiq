---
Title: "Azure Storage Static Website - Adding Application Insights"
Published: 2020-01-1 6:00:00 -0500
Image: "/posts/img/adam-avatar-final-trans-400.png"
Tags: [Azure, DevOps]
IsPost: false
---

There's a slew of info on setting up, using and monitoring with Azure Application Insights, but most of the time they assume you're working with a dynamic web application. I firmly believe that static websites are not second class citizens. You can still get conversions from a static site, so we need to be able to track that, right?  
# Where's Waldo?
Since we'll be working with a static, or static generated site our focus is on HTML, CSS and JavaScript.  So we'll need to locate an SDK for JavaScript and not .NET, NodeJS etc..  
  
This was a bit tricky to find but here is where the JavaScript SDK for Application Insights lives.
[https://github.com/Microsoft/ApplicationInsights-JS](https://github.com/Microsoft/ApplicationInsights-JS)

# Create an Application Insights resource
### Create a new resource from your resource group
![alt text][create-new]  
 - Click the add button  

### Search for 'Application Insights'
![alt text][search-appinsights]
 - Start typing 'Application Insights' in the search bar
 - Select the 'Application Insights' resource

### Click 'Create'
![alt text][appinsights-create]
 - Click the 'Create' button

### Example configuration
![alt text][appinsights-config]
  - Enter a name for your Application Insights resource
  - Select a Region, everything in your Resource Group should ideally be in the same region

### Locate your Instrumentation Key
![alt text][instrumentationkey]
 - You'll need your Instrumentation Key in the next step in this article.

# Integrating with your static site

On the ApplicationInsights GitHub README there's a bunch of blahblah instructions. If you want to read it.

For the TL;DR, copy pasta the below script into the `<head>` of every HTML page on your static site, then add in your Application Insights instrumentation key.  




```js
<script type="text/javascript"> var sdkInstance="appInsightsSDK";window[sdkInstance]="appInsights";var aiName=window[sdkInstance],aisdk=window[aiName]||function(e){ function n(e){t[e]=function(){var n=arguments;t.queue.push(function(){t[e].apply(t,n)})}}var t={config:e};t.initialize=!0;var i=document,a=window;setTimeout(function(){var n=i.createElement("script");n.src=e.url||"https://az416426.vo.msecnd.net/next/ai.2.min.js",i.getElementsByTagName("script")[0].parentNode.appendChild(n)});try{t.cookie=i.cookie}catch(e){}t.queue=[],t.version=2;for(var r=["Event","PageView","Exception","Trace","DependencyData","Metric","PageViewPerformance"];r.length;)n("track"+r.pop());n("startTrackPage"),n("stopTrackPage");var s="Track"+r[0];if(n("start"+s),n("stop"+s),n("setAuthenticatedUserContext"),n("clearAuthenticatedUserContext"),n("flush"),!(!0===e.disableExceptionTracking||e.extensionConfig&&e.extensionConfig.ApplicationInsightsAnalytics&&!0===e.extensionConfig.ApplicationInsightsAnalytics.disableExceptionTracking)){n("_"+(r="onerror"));var o=a[r];a[r]=function(e,n,i,a,s){var c=o&&o(e,n,i,a,s);return!0!==c&&t["_"+r]({message:e,url:n,lineNumber:i,columnNumber:a,error:s}),c},e.autoExceptionInstrumented=!0}return t }({ instrumentationKey:"xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxx" }); window[aiName]=aisdk,aisdk.queue&&0===aisdk.queue.length&&aisdk.trackPageView({}); </script>
```

# What's next?

Now that we have Application Insights set up we can monitor the website!

There's a lot of things we can track
- Funnels
- Cohorts
- Usage Impact
- Retention
- User Flows

> Light reading: [https://docs.microsoft.com/en-us/azure/azure-monitor/app/usage-overview](https://docs.microsoft.com/en-us/azure/azure-monitor/app/usage-overview)

If you're just getting started with Application Insights [User Flows](https://docs.microsoft.com/en-us/azure/azure-monitor/app/usage-flows) is an easy  place to start since it gives you a visual representation of how your site is being used.

[create-new]: /img/posts/20200101-001-az-portal-create-new.png "Resource Group - Create new resource"
[search-appinsights]: /img/posts/20200101-002-az-portal-marketplace-search-appinsights.png "Search marketplace for Application Insights"
[appinsights-create]: /img/posts/20200101-003-az-portal-appinsights-create.png "Click the create button"
[appinsights-config]: /img/posts/20200101-004-az-portal-appinsights-example.png "Example Application Insights configuration"
[instrumentationkey]: /img/posts/20200101-005-az-portal-appinsights-instrumentationkey.png "Find the Instrumentation Key"