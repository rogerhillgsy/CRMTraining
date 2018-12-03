REM
REM Use CRMSVCUtil to regenerate the Early Bindings.
..\coretools\crmsvcutil.exe /url:https://rbh2.api.crm11.dynamics.com/XRMServices/2011/Organization.svc /out:Entities.cs /username:roger@rbh2.onmicrosoft.com /password:Password1 /namespace:DynamicsLinq /serviceContextName:OrgServiceContext
	