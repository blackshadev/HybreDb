var rel = "../../";
process.chdir(__dirname + "/" + rel);
var dat = require(rel + "./app.js");
var $b = require(rel + "./bench.js");

var pg = require('pg');
var fs = require("fs");

var QueryStepper = $b.Stepper.extend({
	create: function (smts, conn) {
		this.inherited().create.call(this, smts);
		
		this.data = [];
		this.conn = conn;
	},
	exec: function(smt, cb) {
		var self = this;
		this.conn.query(smt, function(err, res) {
			if(err) throw err;

			self.data = res.rows;

			cb();
		});
	}
});



var re = /Execution time\: (\d+\.\d+) ms/;
var QueryBenchmark = $b.Benchmark.extend({ 
	stepperClass: QueryStepper,
	create: function(oPar) {
		this.inherited().create.call(this, oPar);
		this.conn = oPar.connection;
	},
	getStmts: function(tdef, tab) {
		return dat.jsonToPgSql(tdef, tab);
	},
	getTime: function(cb) {
		var self = this;

		if(this.stepper.data[3] && this.stepper.data[3]["QUERY PLAN"]) {
			var res = this.stepper.data[3]["QUERY PLAN"].match(re);
			if(res) cb(parseFloat(res[1]));
		}

	},
	cStepper: function(smts) {
		this.inherited().cStepper.call(this, smts);
		this.stepper.conn = this.conn;
	},
	doIter: function(cb) {
		var self = this;
		this.conn.query("drop table if exists \"" + this.name + "\" ", function(err) {
			if(err) throw err;

			cb();
		});
	}
});


var client = new pg.Client("postgres://postgres:smurf1992@localhost/HybreDb");
var b = new QueryBenchmark({
	tableName: "people_big", 
	fileName: "results/ins/Postgres.json",
	tDef: dat.table_defs.people_big, 
	connection: client,
	isSec: true,
	rep: 20,
	steps: [10, 100, 500, 1000, 5000, 10000, 25000, 50000, 75000, 100000, 250000, 500000]
});
b.onDone = function() { client.end(); };

client.connect(function(err) {
	if(err) throw err;
	b.start();
});