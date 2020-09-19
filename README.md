# Family calendar

If you have a personal account on one of Microsoft services (Skype, Outlook email, Github etc.), you can plan your events and tasks in comfortable [web interface](https://outlook.live.com/calendar/) for free. My program is a service for Raspberry Pi, that plays sounds according with iyour Outlook Calendar personal schedule. So, it can be used as an alarm clock, but not only so. The program is written for .NET Core 3.1 environment.

## Installation

Preliminary steps are:

* installation of .NET; it can be performed as is described [here](https://www.youtube.com/watch?v=WDlZ3f2xHcc);
* creating of an Azure Active Directory application according with [this article](https://docs.microsoft.com/ru-ru/graph/tutorials/dotnet-core). 

Then create a directory an enter there:

```
mkdir family-calendar
cd family-calendar
```

My service uses the `Alsa.net`, that can easy plays sound on Linux, so, clone it into `alsa` subdirectory:

```
git clone https://github.com/ZhangGaoxing/alsa.net.git alsa # Just so!
```

Then clone my application in the same subdirectory:

```
https://github.com/yababay/family-calendar.git app # Just so!
```

If you did these steps correct, you sould see `alsa` and `app` subdirectories in the `family-calendar` directory.

Go into `add` directory and run following commands:

```
dotnet add package Microsoft.Extensions.Configuration.UserSecrets --version 3.1.2
dotnet user-secrets set appId "YOUR_APP_ID_HERE"
dotnet user-secrets set scopes "User.Read;Calendars.Read"
```

Of course, the "YOUR_APP_ID_HERE" phrase must be replaced with your read application id from the Azure.

Next is to setup the application as a Linux service:

```
sudo mkdir /srv/family-calendar
sudo chown pi /srv/family-calendar
dotnet publish -c Release -o /srv/family-calendar
```

Now place some wav-files into `/srv/family-calendar/Assets` directory and edit the file `categories2sound.json`. You should create some categories in your Outlook Calendar. Every event, that has a category mentioned in this dictionary will be introbuced with the appropriate sound.

The last step - to run

```
sudo systemctl start family-calendar.service
```

It seems that the process is hang, but it only waits for authentication. So, just press `^C` immediatly and run

```
sudo systemctl status family-calendar.service
```


You will see the lines wiht such one:

```
To sign in, use a web browser to open the page https://microsoft.com/devicelogin and enter the code CLPCFM82A to authenticate.
```

Fullfill this instruction, and your service is ready to play ringtones according with your Outlook Calendar schedule. Of course, your audiodevices must be set up. It can be needed to edit settings in alsa directory, but perhaps all will work instantly.


