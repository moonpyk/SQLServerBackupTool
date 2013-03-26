# SQLServerBackupTool #

A poor man's *Microsoft SQL Server* database backup utility.

Free versions of *Microsoft SQL Server* lack the SQLServer Agent, among the gigantic amount of functionalities provided by the agent, ability to make automatic backups of databases was a problem for me.

A wrote this simple program to be able to schedule automatic backups of my databases in an easy way. 

In my setup, the program is automatically started all nights by the Windows Task Scheduler. 

## Features ##

- Backup any number of databases
- Automatic zip creation of backups
- **TBD** Automatic copy of each backup to a configured Network share / FTP Server / Cloud service

## Configuration ##

TBD, for now, take **App.exemple.config**, and copy it to **App.config**, the things are pretty easy, you should able to guess how things are working.

A more detailed documentation/tutorial will come later.