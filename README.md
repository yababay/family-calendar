# Family calendar

If you have a personal account on one of Microsoft services (Skype, Outlook email, Github etc.), you can plan your events and tasks in comfortable [web interface](https://outlook.live.com/calendar/) for free. My program is a service for Raspberry Pi, that plays sounds according with your Outlook Calendar personal schedule. So, it can be used as an alarm clock, but not only so. The program is written with .NET Core 3.1 SDK.

## Installation

Preliminary steps:

* installation of .NET (is described [here](https://www.youtube.com/watch?v=WDlZ3f2xHcc), for example);
* creating of an Azure Active Directory application according with [this article](https://docs.microsoft.com/ru-ru/graph/tutorials/dotnet-core). 

Then create a directory an enter there:

```
mkdir family-calendar
cd family-calendar
```

My service uses the `Alsa.net` library, that can plays sound on Linux, so, clone it into `alsa` subdirectory:

```
git clone https://github.com/ZhangGaoxing/alsa.net.git alsa # Just so!
```

Then clone my repository into the same subdirectory:

```
https://github.com/yababay/family-calendar.git app
```

If you did these steps correct, you sould see `alsa` and `app` subdirectories in the `family-calendar` directory.

Go into `app` directory and run following commands:

```
dotnet add package Microsoft.Extensions.Configuration.UserSecrets --version 3.1.2
dotnet user-secrets set appId "YOUR_APP_ID_HERE"
dotnet user-secrets set scopes "User.Read;Calendars.Read"
```

Of course, the "YOUR_APP_ID_HERE" must be replaced in the second command with your read application id from the Azure (see link above).

Next is to setup the application as a Linux service:

```
sudo mkdir /srv/family-calendar
sudo chown pi /srv/family-calendar
dotnet publish -c Release -o /srv/family-calendar
```

Now place some wav-files into `/srv/family-calendar/Assets` directory and edit the file `categories2sound.json`. You also have to create some categories in your Outlook Calendar. Every event, that has a category mentioned in the dictionary will be introduced with the appropriate sound.

The last step is to run the service:

```
sudo systemctl start family-calendar.service
```

It seems that the process is hang, but it only waits for authentication. So, just press `^C` immediatly and run

```
sudo systemctl status family-calendar.service
```

You will see several lines wiht such one among them:

```
To sign in, use a web browser to open the page https://microsoft.com/devicelogin and enter the code CLPCFM82A to authenticate.
```

Fullfill this instruction, and your service is ready to play ringtones according with your Outlook Calendar schedule. Of course, your audiodevices must be set up. Perhaps It will be needed to edit settings in alsa directory, but I belive that all will work instantly.


