var rel = "./";
process.chdir(__dirname + "/" + rel);
var dat = require(rel + "./app.js");
var $b = require(rel + "./bench.js");

var mysql = require('mysql');
var fs = require("fs");

var QueryStepper = $b.Stepper.extend({
	create: function (smts, conn) {
		this.inherited().create.call(this, smts);
		
		this.conn = conn;
	},
	exec: function(smt, cb) {
		this.conn.query(smt, function(err) {
			if(err) throw err;
			cb();
		});
	}
});

var QueryBenchmark = $b.Benchmark.extend({ 
	stepperClass: QueryStepper,
	create: function(oPar) {
		this.inherited().create.call(this, oPar);
		this.conn = oPar.connection;
	},
	getStmts: function(tdef, tab) {
		var name = dat.chance.name();
		var key = dat.getRandomId(tdef);
		return ["select * from `" + tab.table + "` where prefix='Mister'"];
	},
	getTime: function(cb) {
		var self = this;
		this.conn.query("show profiles", function(err, rows) {
			if(err) throw err;

			var tot = 0, found = 0;
			rows.forEach(function(r) {
				if(r.Query_ID >= self.lastQueryIX && r.Query_ID <= self.queryIX) {
					tot += r.Duration * self._mult;
					found++;
				}
			});

			if(found < self.stepper.smts.length) throw "Missing records";

			cb(tot);
		});
	},
	cStepper: function(smts) {
		this.inherited().cStepper.call(this, smts);
		this.stepper.conn = this.conn;
	},
	doStart: function(cb) {
		this.conn.query("set profiling = 1", function(err) {
			if(err) throw err;
			cb();
		});
	},
	doStep: function(cb) {
		var self = this;

		var smts = [
			"drop table if exists `" + this.name + "` "
		];
		smts.push.apply(smts, dat.jsonToSql(this.tDef, this.tabData));

		var stepper = new QueryStepper(smts, this.conn);
		stepper.onDone = cb;
		stepper.onNext = function() {
			self.lastQueryIX++;
			self.queryIX++;
		};

		stepper.start();

	}
});


var connection = mysql.createConnection({
  host     : 'localhost',
  user     : 'root',
  password : 'smurf1992'
});
connection.connect();

connection.query("use HybreDb");


var b = new QueryBenchmark({
	tableName: "people_big", 
	fileName: "results/sel_cond/MySQL.json",
	tDef: dat.table_defs.people_big, 
	connection: connection,
	isSec: true,
	rep: 20,
	steps: [10, 100, 500, 1000, 5000, 10000, 25000, 50000, 75000, 100000]
});
b.onDone = function() { connection.end(); };
b.start();