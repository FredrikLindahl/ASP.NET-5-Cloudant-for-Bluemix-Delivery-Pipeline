# ASP .NET 5 Cloudant Sample

This application demonstrates how to use the Bluemix Cloudant NoSQL DB Service in an ASP.NET 5 application.

[![Deploy to Bluemix](https://bluemix.net/deploy/button.png)](https://bluemix.net/deploy)

## Run the app locally

1. Install ASP.NET 5 by following the [Getting Started][] instructions
2. Clone this app
3. cd into the app directory and then `src/dotnetCloudantWebstarter`
4. Copy the value for the VCAP_SERVICES envirionment variable from the application running in Bluemix and paste it in the config.json file
5. Run `dnu restore`
6. Run `dnx kestrel`
7. Access the running app in a browser at http://localhost:5004

[Getting Started]: http://docs.asp.net/en/latest/getting-started/index.html



