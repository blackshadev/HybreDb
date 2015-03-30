var $o = require("./core.js");
var mysql = require("mysql");
// var Maria = require("maria");
var pg = require("pg");
var EventEmitter = require("events").EventEmitter; 

module.exports = (function() {

	// Generic adapter for a sql database
	var Adapter = $o.Object.extend({
		cnf: null, // object with host, port, username, password
		isConnected: false,
		lastError: null,
		events: null, // event emitter, emits `conntected` `error` `closed`.
		_conn: null, // native connection object
		create: function(cnf) {
			this.cnf = cnf;
			this.events = new EventEmitter();

			this.on("error", function(src, err) { src.lastError = err; });
		},
		// Connects to the database, emits `connected` when connected
		connect: function() {
			this.isConnected = true;
			this.emit("connected", this);
		},
		// Disconnects from the host, emits `close` when disconnected
		disconnect: function() {
			this.isConnected = false;
			this._conn = null;
			this.emit("close", this);
		},
		// execute given sql with pars and calls 
		// sql: string, sql to execute
		// pars: array, of parameters in sql
		// cb: callback called with (err, rows, fields)
		exec: function(sql, pars, cb) { },
		prepare: function(sql, pars) {
			
			return sql.replace(/\{\:(\w+)\}/g, function(m, name) {
				return "'" + pars[name].replace("'", "''") + "'";
			})
		},

		// Event methods
		on: function(n, cb) { this.events.on(n, cb); },
		once: function(n, cb) { this.events.once(n, cb); },
		emit: function() { this.events.emit.apply(this.events, arguments); },
	});

	var Mysql = Adapter.extend({
		connect: function() {
			this._conn = mssql.createConnect(this.cnf);

			var self = this;
			this._conn.connect(function(err) {
				if(err) self.emit("error", self, err);
				self.isConnected = true;
				self.emit("connected", self);
			});
		},
		disconnect: function() {
			var self = this;
			this._conn.end(function(err) {
				if(err) self.emit("error", self, err);
				self.isConnected = false;
				self.emit("closed", self);
			});
		},
		exec: function(sql, params, cb) {
			this._conn.query(this.prepare(sql, params), cb);
		}
	});


	// var Maria = Adapter.extend({
	// 	create: function(cnf) {
	// 		this.inherited().create.call(this, cnf);
	// 		this._conn = new Maria();

	// 		// reroute events
	// 		this.events = this._conn;
	// 	},
	// 	connect: function() {
	// 		this._conn.connect(this.cnf);
	// 		self.isConnected = true;
	// 	},
	// 	disconnect: function() {
	// 		this._conn.end();
	// 	},
	// 	exec: function(sql, params, cb) {
	// 		var r = this._conn.query(_sql, params, true);
	// 		r.on("result", function(res) {
	// 			console.log(res);
	// 		});
	// 		 .on("error", function(err) { cb(err, null, null); })

	// 	} 

	// });

	var Postgres = Adapter.extend({
		connStr: function() {
			return "postgres://" + (this.cnf.user || this.cnf.username) + ":" + (this.cnf.password) 
				+ "@" + (this.cnf.host || "localhost") + "/" + (this.cnf.database || this.cnf.db);
		},
		connect: function() {
			var self = this;
			
			self._conn = new pg.Client(this.connStr());
			self._conn.connect(function(err) {
				if(err) return self.emit("error", this, err);
				self.isConnected = true;
				self.emit("connected", this);
			});
		},
		disconnect: function() {
			self._conn.end();
			this.isConnected = false;
			this.emit("closed", this);
		},
		exec: function(sql, params, cb) {
			var s = this.prepare(sql, params);
			this._conn.query(s, function(err, res) {
				if(err) return cb(err, null, null);
				cb(err, res.fields, res.rows);
			});
		}
	});


	Adapter.Types = { "mysql": Mysql, "postgres": Postgres };

	return Adapter;
})();