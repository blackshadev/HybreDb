var WebService = require("roadie").WebService;
var fs = require("fs");

var re = new RegExp(/\{\{(\d)\}\}/);
var template = fs.readFileSync(__dirname + "/template.html", {encoding: 'utf-8'});
var callTemp = fs.readFileSync(__dirname + "/call.html", { encoding: 'utf-8' });

var formatRe = /\{(\d+)\}/g
String.prototype.format = function(arr) {
	this.replace(formatRe, function(m, iX) {
		return arr[iX];
	});
}

module.exports = WebService.extend({
	create: function() {
		this.inherited().create.apply(this, arguments);

		this.ctx.response.header("Content-Type", "text/html");
		this.ctx.response.header("Cache-Control", "no-cache");

	},
	send: function() {
		var t = template.replace(re, this.body);

		this.ctx.response.data(t);
		this.ctx.response.send();
	},

	index: function() {
		this.send();
	},
	call: function() {
		this.body = callTemp;
		this.send();
	},
	tables: function() {
		var self = this;
		self.body = "";
		this.ctx._server.db.getTables(function(err, f, tabs) {
			if(err) return self.ctx.error(err);
			
			self.body += "<table class='table table-hover'>";
			self.body += "<thead><tr><td>Table name</td></tr></thead><tbody>";

			for(var i = 0; i < tabs.length; i++) {
				var t = tabs[i].table_name;
				self.body += "<tr><td><a href=\"/table/" + t + "\">" + t + "</a></td></tr>";
			}
			
			self.body += "</tbody></table>";
			self.send();
		});
	},
	table: function() {
		var t = this.ctx.request.parameter("table");
		var self = this;
		
		self.body = "<h2>" + t + "</h2>";
		this.ctx._server.db.getTable(t, function(err, f, rows) {
			if(err) return self.ctx.error(err);

			self.body += "<table class='table table-hover'>";
			self.body += "<thead><tr>";

			var f_arr = f.map(function(e) { return e.name; });
			self.body += f_arr.map(function(e) { return "<td>" + e + "</td>"; }).join("");


			self.body += "</tr></thead><tbody>";

			for(var i = 0; i < rows.length; i++) {
				self.body += "<tr>";
				self.body += f_arr.map(function(e) { return "<td>" + rows[i][e] + "</td>"; }).join("");
				self.body += "</tr>";	
			}


			self.body += "</tbody></table>";

			self.send();
		});

	},
	relations: function() {
		var self = this;
		this.ctx._server.db.getRelations(function(err, f, rels) {
			if(err) return self.ctx.error(err);
			
			self.body += "<table class='table table-hover'>";
			self.body += "<thead><tr><td>Relation name</td><td>Master table</td><td>Child Table</td></tr></thead><tbody>";

			for(var i = 0; i < rels.length; i++) {
				self.body += "<tr>";

				self.body += "<td>{0}</td><td>{1}</td><td>{2}</td>".format([rels[i].relation_name, rels[i].master_name, rels[i].child_name])

				self.body += "</ tr>";				
			}

			self.body += "</tbody></table>";

			self.send();
		});
	}

});