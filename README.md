# WindowsPush

WindowsPush is a bare bones Windows Phone 8.1 Application which uses Apigee's BaaS to create, send, and recieve push notifications.

The application allows you to send 

#Software Needed

- Windows 8.1 or higher.
- Visual Studio 2013 or higher.
- Windows Phone 8 SDK package that comes with Visual Studio installed.

#Set Up

- In Visual Studio, open Usergrid.Notifications.sln which is located within the folder /notifications/Usergrid.Notifications/.
- Once you have the project open, navigate to and open Client/Usergrid.cs.  You will need edit the constant string values located near the top of the file to appropriate values.  (SERVER_URL,ORG_NAME,APP_NAME ect)
- You may or may not need to add a user to Apigee BaaS to fulfill all the constant string requirements.
- Click on the Project menu and select Store->Associate Your App with the Windows Phone Store.
- Login to your Microsoft account and select the app you want to associate.  You may need to create a new app which can be done by reserving an app name.
- Once you have associated your application you will need to set up a notifier on Apigee BaaS.  To do so go to https://account.live.com/developers/applications and select your application.
- On the next screen you can scroll down and find your Package SID and Client Secret you will need to use to create your push notifier.  Once you have these go to your app services page and create a notifier.
- Once the notifier has been created make sure that you have edited the constant string NOTIFIER_NAME in Client/Usergrid.cs to the name you had for the notifier you just created.
- Once all of this is done, in VS select Build->Build Solution and you should be able to then run you application.
