# HybreDb
Hybrid Relational Database

This is a proof of concept database which allows for fast and efficient access to relational data. The structure of the data matches that of traditional table based databases and migration is easy because of this.

HybreDb is easy to extend, new DataTypes can be added in the HybreDb.Tables.Types namespace and need to be referenced in the DataType.cs file.
Custom actions can be added with ease in the HybreDb.Actions namespace, for example look at `HybreDb/Actions/HybreDbGet.cs` or other existing actions.

More information soon.

## Requirements
 - C# runtime (mono for linux/mac)
 - msbuild (xbuild for linux/mac)
 - nodejs/npm (for GUI)

## Running HybreDb
First compile HybreDb (with msbuild/xbuild) and run it (with mono). As it is currently, it will run as a command window which doesn't show anything. In the future this will change to a service for windows, for for Linux it will stay the same because console applications can be ran as daemon on Linux.

Next run HybreDbWeb/app.js with NodeJS. Now navigate to [your localhost](http://localhost/) and enjoy. You can change the HTTP port in the app.js file.

## [Integrating HybreDb](../wiki/hybreDbComm)


# Features
 - Handling relational data
 - Plotting relational data
 - Quering relational data
 - Adding custom datatypes
 - Adding custom actions

## Future features
 - actions
   - Path finding action
   - Import/export action
 - Dynamicly adding actions within recompiling
 - Dynamicly adding datatypes within recompiling