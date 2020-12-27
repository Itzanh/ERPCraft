# ERPCraft

ERPCraft is a management software that allows you to control your Minecraft world from the real world. This software is based in a web application where all the user interaction happens, connected to a powerful backend with a PostgreSQL database.

**-- CURRENTLY UNDER CONSTRUCTION --**

This is just a beta version that is still under under development. If you encounter an error, please submit an issue.

## The architecture

![ERPCraft schema](https://izcloud.tk/erpcraft/ERPCraft-schema.jpg) 

The central piece of this software is the ERPCraft server. It requires .NET Framework, a web server, and a PostgreSQL database to work.

The UI is a React web application, that you can host with an apache or nginx web server. When running the app, it will attempt to connect via WebSocket to the backend, on the same host as the webpage has loaded.

The communication with the Minecraft world will be done via the "Internet card" on the OpenComputers mod. A script will allow the real server to send and receive modem messages to the Minecraft world.

The game server will be connected to the real world ERPCraft server using a socket connection on the port 32325, and the web, when loaded, will attempt a WebSocket connection with the host the web was loaded from, at the port 32324. The server requires a PostgreSQL database to work.

## Features

### Control your robots' status
You can view a Robot's GPS position, battery level, inventory contents, mining status, etc. in real time using the web interface.

### Monitor your power grids
Control in real time your IndustrialCraft 2 battery levels and generators' status.

### Organize the mining jobs
Add and manage mining orders for your robots, control what resources are being used, and what resources you are getting.

## How to install

### Install PostgreSQL
If you don't have a PostgreSQL server installed yet, install one in your machine from the [official web page](https://www.postgresql.org/download/).

### Setup DB connection
Configure the database so the program can connect to it.
#### Create user
Create a user for the database that we are going to create. We suggest using a random password for that user. When runned for the first time, it will need admin privileges to install the **pg_trgm EXTENSION** on the first run. The command that the program attemps to execute on the first run is:
```
CREATE EXTENSION IF NOT EXISTS pg_trgm WITH SCHEMA public;
```

![ERPCraft Setup 1](https://izcloud.tk/erpcraft/how_to_install/setup1.PNG)
![ERPCraft Setup 2](https://izcloud.tk/erpcraft/how_to_install/setup2.PNG)

#### Create the database
Create a new blank database and grant the permissions for the user that you just created.

![ERPCraft Setup 3](https://izcloud.tk/erpcraft/how_to_install/setup3.PNG)
![ERPCraft Setup 4](https://izcloud.tk/erpcraft/how_to_install/setup4.PNG)

### Download the program files and run for the first time
Download the zip file and run the .exe. Double click on Windows or mono ... .exe in Linux. When runned for the first time, it will generate the config.json file.
Change the db host, database name, user and password to enable the ERPCraft server to connect to the database, with the name of the user and the password that you just created. Example configuration:
```
{"dbhost":"127.0.0.1","dbname":"ERPCraft","dbuser":"erpcraft","dbpassword":"erpcraft"}
```

Run the software again, and check that it creates the database tables and the initial data without any errors. You should see an output like this:
```
*** ERPCRAFT - Control your Minecraft world from the real world, anywhere, realtime. ***
[[[ https://github.com/Itzanh/ERPCraft ]]]

*** THE DATABASE IS EMPTY. CREATING THE SCHEMA INTO THE DATABASE. ***
*** DB CREATED OK ***
Created the default system user 'admin' for web access. Check the 'How to install' page to learn more.
Esperant connexions de OC Servers
WebSocket server listening.
```

### Set up the web
Unzip the file with the contents of the web page on the root of your web server, and open it with a web browser. When loaded, the web will attemps to connect to the ERPCraft Server with a WebSocket connection, to the same host where the web was loaded from, at the port 32324. If you successfully connect, you can log in using the default credentials:
```
User: admin
Password: admin1234
```

Once you have logged in, go to the Users tab and change your password.

### Set up the game connection
#### Create the server and the credentials
On the settings dropdown, go the the Servers tab, and create a new server. Add a descriptive name and set the UUID of the OpenComputers computer's network card that is going to have the Internet card and connect to the ERPCraft server. That computer/server need and Internet card and a wired network card (can be connected to a relay to allow wireless connection).

Next, on the settings dropdown, go to the API Keys tab, and create and API Key. Add a descriptive name, and an UUID will be generated. This will be the secret that the in-game server need to know to connect to the ERPCraft server.

#### Install the server software in-game
Copy the **server.lua** file in a in-game computer, and change the part where is send the server ID and secret. This string is formed by the server UUID and API Key appended. For example:
```
-- ID del servidor + Clave secreta
handle:write("dc38400f-7514-45ec-9161-b20c65330576e4f76e04-6816-44a6-9aa4-a42df874bcd9")
```

#### Set up the robots
Go to the Robots tab, and add a new Robot with the UUID of the network card of the robot that you are going to connect.

Finally, install the **dig.lua** file on the robots that are going to be mining for you. Just change the UUID of the in-game server that the Robot has to connect to, and run the program. Example configuration:
```
SERVER_ADDR = "086c88d8-6dbc-4ae2-9136-311bdd180482"
```

## Screenshots

![ERPCraft Screeshot 1](https://izcloud.tk/erpcraft/screenshots/ERPCRaft_1.PNG)
![ERPCraft Screeshot 2](https://izcloud.tk/erpcraft/screenshots/ERPCRaft_2.PNG)
![ERPCraft Screeshot 3](https://izcloud.tk/erpcraft/screenshots/ERPCRaft_3.PNG)
![ERPCraft Screeshot 4](https://izcloud.tk/erpcraft/screenshots/ERPCRaft_4.PNG)
![ERPCraft Screeshot 5](https://izcloud.tk/erpcraft/screenshots/ERPCRaft_5.PNG)

## Troubleshooting
If you encounter and error, please, reboot the ERPCraft Server :)

If the problem persists, please submit an issue providing the traceback of the error.

## Contributing
Images and icons by: [loading.io](https://loading.io/)

## License
[MIT](https://choosealicense.com/licenses/mit/)