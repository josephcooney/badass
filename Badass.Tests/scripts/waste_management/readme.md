## Configure Azure AD ##

## Update Client ##
`npm install @azure/msal-browser`
`npm install @azure/msal-react`

## Add Nuget Packages ##
Hangfire
Hangfire.PostgreSql

## Register Hangfire Background Worker in Autofac ##
- add to ConfigureContainer in startup.cs
`builder.RegisterType<BackgroundJobClient>().AsImplementedInterfaces();`

## Set Startup Args  
`"commandLineArgs": "-u -c bins -x hangfire -del --no-policy"`