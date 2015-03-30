/* Creates and initializes the database
 **/

var cnf = require("./config.json");
var Adapter = require("./Adapter.js");

var init_sql = [
		"create schema if not exists relDb",
		"create table if not exists reldb.tables (" +
			"  table_name	varchar(128)	primary key		not null" +
			", pk_fields	varchar(256)					not null" +
		")",
		"drop table reldb.relations; create table if not exists reldb.relations (" + 
			"  master_table	varchar(128)	not null" + 
			", master_field varchar(128)	not null" +
			", join_table   varchar(128)    not null" +
			", child_name 	varchar(128)	not null" +
			", child_field	varchar(128)	not null" +
		")"
	];


module.exports = (function() {

	function initialize(cb, iX) {
		if(iX >= init_sql.length) return cb();
		iX = iX || 0;

		d.exec(init_sql[iX], null, function(err, d) {
			if(err) { console.log("error"); throw err; }
			initialize(cb, iX + 1);
		})
	}


	var db = Adapter.Types[cnf.database.type];

	var d = new db(cnf.database);
	d.connect();

	d.on("connected", function() {
		initialize(function() {
			console.log("Done, database ready")
		});
	});
	d.on("error", function(src, err) {
		console.log("error", err);
	});

	function prepareFieldJSON(j) {
		if(!(j.fields && typeof(j.fields) === "object" ) ) return false;

		var str = "uid serial primary key";
		for(var k in j.fields) {
			str +=  ", " + k + " " + j.fields[k].type + (j.fields[k].length ? "(" + j.fields[k].length + ")" : "") ;
		}

		return str;
	}

	return  {
		getTable: function(t, cb) {
			d.exec("select * from \"" + t + "\"", null, cb);
		},
		getTables: function(cb) {
			d.exec("select * from reldb.tables", null, cb)
		},
		createTable: function(t, b, cb) {
			var fields = prepareFieldJSON(b);
			
			if(!fields) return cb({error: "No fields given"}, null);

			var sql = [
				"insert into relDb.tables (table_name, pk_fields) values ({:tab}, 'uid')",
				"create table " + t + " (" + fields + ")"
			];

			d.exec(sql.join(";"), {tab: t} , cb);
		},
		deleteTable: function(t, cb) {
			var sql = [
				"delete from relDb.tables where table_name={:tab}",
				"drop table " + t
			];
			d.exec(sql.join(";"), {tab:t}, cb); 
		}
	};
})();