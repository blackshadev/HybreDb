var WebService = require("roadie").WebService;
var fs = require("fs");

var re = new RegExp(/\{\{(\d)\}\}/);
var template = fs.readFileSync(__dirname + "/template.html", {encoding: 'utf-8'});
var callTemp = fs.readFileSync(__dirname + "/call.html", { encoding: 'utf-8' });

module.exports = WebService.extend({
	create: function() {
		this.inherited().create.apply(this, arguments);

		this.ctx.response.header("Content-Type", "text/html");
		this.ctx.response.header("Cache-Control", "no-cache");

	},
	call: function() {
		this.body = callTemp;
		this.send();
	},
	index: function() {
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
				self.body += f_arr.map(function(e) { return "<td>" + rows[e] + "</td>"; }).join("");
				self.body += "</tr>";				
			}


			self.body += "</tbody></table>";

			self.send();
		});

	},
	send: function() {
		var t = template.replace(re, this.body);

		this.ctx.response.data(t);
		this.ctx.response.send();
	}
});