# HybreDb
Hybrid Relational Database

This is a proof of concept database which allows for fast and efficient access to relational data. The structure of the data matches that of traditional table based databases and migration is easy because of this.

HybreDb is easy to extend, new DataTypes can be added in the HybreDb.Tables.Types namespace and need to be referenced in the DataType.cs file.
Custom actions can be added with ease in the HybreDb.Actions namespace, for example look at `HybreDb/Actions/HybreDbGet.cs` or other existing actions.

More information soon.
