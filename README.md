# CRMTraining
Solution from CRM Training

This solution is produced as an aid to trainees on the QACRMDEV course.

- Primarily is is a working example of the Microsoft Dynamics 365 Template toolkit for Visual Studio.
- It includes worked examples of many of the LAB exercises

Note that the Mirosoft Dynamics 365 Developer Toolit requires that you download and install the Microsoft [Dynamics SDK](https://www.microsoft.com/en-us/download/details.aspx?id=50032)
Note that the v8 SDK as downloaded is exactly what is required, even if you are working with online CRM v9.
Do not try to use nuget to patch the SDK to v9 as this will not work with the developer toolkit.

Nuget is the preferred way to get access to the Microsoft SDK Dll's from within visual studio. (nuget:Microsoft.CRMSDK.CodeAssemblies et al)
You will need a v9 copy of the Plugin registration tool to interact with v9 CRM. (nuget: Microsoft.CRMSDK.XrmTooling.PluginRegistrationTool)
