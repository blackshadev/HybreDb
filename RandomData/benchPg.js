var rel = "./";
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
		return ["explain analyze select * from \"" + tab.table + "\" where prefix='Mister'"]
	},
	getTime: function(cb) {
		var self = this;

		var l = this.stepper.data.length -1;
		if(this.stepper.data[l] && this.stepper.data[l]["QUERY PLAN"]) {
			var res = this.stepper.data[l]["QUERY PLAN"].match(re);
			if(res) return cb(parseFloat(res[1]));
		}
		console.log(this.stepper.data);

	},
	cStepper: function(smts) {
		this.inherited().cStepper.call(this, smts);
		this.stepper.conn = this.conn;
	},
	doStep: function(cb) {
		var self = this;

		var smts = [
			"drop table if exists \"" + this.name + "\" "
		];
		smts.push.apply(smts, dat.jsonToPgSql(this.tDef, this.tabData));

		var stepper = new QueryStepper(smts, this.conn);
		stepper.onDone = cb;

		stepper.start();
	}
});


var client = new pg.Client("postgres://postgres:smurf1992@localhost/HybreDb");
var b = new QueryBenchmark({
	tableName: "people_big", 
	fileName: "results/sel_cond/Postgres.json",
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