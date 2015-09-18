var mysql = require('mysql');
var dat = require("./app.js");
var $c = require("./core.js");
var fs = require("fs");

var Stepper = $c.Object.extend({
	create: function(smts) {
		this.smts = smts;
		this.iX = -1;
	},
	start: function() {
		this.iX = -1;
		this.next();
	},
	next: function() {
		if(++this.iX >= this.smts.length) return this.done();
		if(this.onNext) this.onNext();
		var self = this;
		this.exec(this.smts[this.iX], function(err) {
			if(err) throw err;
			self.next();
		});
	},
	done: function() {
		if(this.onDone) this.onDone();
	},
	exec: function(smt, cb) {throw "Not yet implemented";}
});

var Benchmark = $c.Object.extend({
	stepperClass: Stepper,
	create: function(oPar) {
		this.stepper = null;
		this.name = oPar.tableName;
		this.tDef = oPar.tDef;

		this.rDef = oPar.rDef;
		this.relName = oPar.relName;

		this.steps = oPar.steps || [100, 1000, 10000, 100000];
		this.rep = oPar.rep || 5;
		this._mult = oPar.isSec ?  1000 : 1;
		this.fileName = oPar.fileName;

		this.lastQueryIX = 0;
		this.queryIX = 0; // count querys
		this.stepIX = -1; // count steps
		this.iterIX = -1; // count iterations

		this.data = {};
	},
	cStepper: function(smts) {
		var self = this;

		this.stepper = new this.stepperClass(smts);
		this.stepper.onNext = function() {
			self.queryIX++;
		};
		this.stepper.onDone = function() {
			self.processIter();
		};
	},
	nextStep: function() {
		if(++this.stepIX >= this.steps.length) return this.done();

		console.log("step (" + this.stepIX + "/" + this.steps.length + ")");

		this.getData();
		
		var self = this;
		this.doStep(function() {
			var smts = self.getStmts(self.tDef, self.tabData);
			self.cStepper(smts);

			self.iterIX = -1;
			self.nextIter();
		})
	},
	getData: function() {
		this.tDef.N = this.steps[this.stepIX];
		this.tabData = dat.generateTable(this.name, this.tDef);
		if(this.rDef) {
			this.rDef.N = this.steps[this.stepIX];
			this.relData = dat.generateRelation(this.relName, this.rDef, this.tDef, this.tDef);
		}
	},
	getStmts: function(tdef, tab) { throw "Not yet implemented"},

	nextIter: function() {
		//dat.reset();
		if(++this.iterIX >= this.rep) return this.nextStep();

		console.log("-- iter (" + this.iterIX + "/" + this.rep + ")");

		var self = this;
		this.doIter(function() {

			self.stepper.start();
		});
	}, 
	processIter: function() {
		var self = this;

		if(!this.data[this.steps[this.stepIX]])
			this.data[this.steps[this.stepIX]] = [];

		var dat = this.data[this.steps[this.stepIX]];
		this.getTime(function(t) {
			dat.push(t);

			self.lastQueryIX = self.queryIX;

			self.nextIter();
		});
	},
	start: function() {
		var self = this;
		this.doStart(function() {
			self.nextStep();
		});
	},
	done: function() {
		if(this.fileName)
			fs.writeFileSync(this.fileName, JSON.stringify(this.data));
	

		if(this.onDone) this.onDone(this.data);
	},
	getTime: function(cb) {
		throw new "Not yet implemented";
	},
	doIter: function(cb) {
		cb();
	},
	doStep: function(cb) {
		cb();
	},
	doStart: function(cb) {
		cb();
	}
});

function calc_avg(arr) {
	var sum = arr.reduce(function(a, b) { return a + b; });
	return sum / arr.length;
}

function calc_std(avg, arr) {
	var sq_diffs = arr.map(function(el) { return Math.pow(avg - el, 2); })
	var dev = calc_avg(sq_diffs);
	return Math.sqrt(dev);
}

module.exports = {
	Benchmark: Benchmark,
	Stepper: Stepper,
	calc_avg: calc_avg,
	calc_std: calc_std
};